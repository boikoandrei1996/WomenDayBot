using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Configuration;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WomenDay.Models;
using WomenDay.Repositories;
using WomenDay.Services;

namespace WomenDay
{
  public class Startup
  {
    private readonly ILogger<Bot> _logger;

    public IConfiguration Configuration { get; }

    public IHostingEnvironment HostingEnvironment { get; }

    public Startup(IHostingEnvironment env, ILoggerFactory loggerFactory)
    {
      _logger = loggerFactory.CreateLogger<Bot>();

      HostingEnvironment = env;

      Configuration = new ConfigurationBuilder()
        .SetBasePath(env.ContentRootPath)
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
        .AddEnvironmentVariables()
        .Build();
    }

    public void ConfigureServices(IServiceCollection services)
    {
      var botSettings = Configuration.GetSection("BotSettings").Get<BotSettings>();

      if (File.Exists(botSettings.FilePath) == false)
      {
        throw new FileNotFoundException($"The .bot configuration file was not found. botFilePath: {botSettings.FilePath}");
      }

      // Loads .bot configuration file
      BotConfiguration botConfig = null;
      try
      {
        botConfig =
          BotConfiguration.Load(botSettings.FilePath, botSettings.FileSecret) ??
          throw new InvalidOperationException($"The .bot configuration file '{botSettings.FilePath}' could not be loaded.");
      }
      catch
      {
        // Please ensure you have valid botFilePath and botFileSecret set for your environment.
        // You can find the botFilePath and botFileSecret in the Azure App Service application settings.
        // If you are running this bot locally, consider adding a appsettings.json file with botFilePath and botFileSecret.
        // See https://aka.ms/about-bot-file to learn more about .bot file its use and bot configuration.
        throw new InvalidOperationException("Error reading .bot file.");
      }

      services.AddSingleton<BotConfiguration>(botConfig);

      // Add BotServices singleton.
      // Create the connected services from .bot file.
      services.AddSingleton<BotServices>(new BotServices(botConfig));

      var isProduction = HostingEnvironment.IsProduction();

      // Retrieve current endpoint.
      var environment = isProduction ? "production" : "development";
      var service = botConfig.Services.FirstOrDefault(s => s.Type == "endpoint" && s.Name == environment);
      if (service == null && isProduction)
      {
        // Attempt to load development environment
        service = botConfig.Services.FirstOrDefault(s => s.Type == "endpoint" && s.Name == "development");
      }

      if (!(service is EndpointService endpointService))
      {
        throw new InvalidOperationException($"The .bot file does not contain an endpoint with name '{environment}'.");
      }

      var cosmosdbSettings = Configuration.GetSection("CosmosDB").Get<CosmosDBSettings>();
      var cosmosDbStorageOptions = new CosmosDbStorageOptions
      {
        DatabaseId = cosmosdbSettings.DatabaseId,
        CollectionId = cosmosdbSettings.CollectionId,
        CosmosDBEndpoint = new Uri(cosmosdbSettings.EndpointUri),
        AuthKey = cosmosdbSettings.AuthenticationKey
      };

      services.AddSingleton<CosmosDbStorageOptions>(cosmosDbStorageOptions);

      // Register state models
      var dataStore = new CosmosDbStorage(cosmosDbStorageOptions);
      var conversationState = new ConversationState(dataStore);
      services.AddSingleton(conversationState);
      var userState = new UserState(dataStore);
      services.AddSingleton(userState);

      // Register repositories
      services.AddSingleton<OrderRepository>();
      services.AddSingleton<CardConfigurationRepository>();

      // Register services
      services.AddSingleton<ICardConfigurationService, CardConfigurationService>();
      services.AddSingleton<ICardService, CardService>();

      services.AddBot<Bot>(options =>
      {
        options.CredentialProvider = new SimpleCredentialProvider(endpointService.AppId, endpointService.AppPassword);
        options.OnTurnError = async (context, exception) =>
        {
          _logger.LogError(exception, "Unhandled exception");

          await context.SendActivityAsync("Черт, эти программисты опять налажали! Неведома ошибка");
        };
      });

      services.AddSingleton<BotAccessors>(new BotAccessors(userState, conversationState)
      {
        UserDataAccessor = userState.CreateProperty<UserData>("WomenDayBot.UserData"),
        DialogStateAccessor = conversationState.CreateProperty<DialogState>("WomenDayBot.DialogState")
      });

      services
        .AddMvc()
        .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

      // In production, the React files will be served from this directory
      services.AddSpaStaticFiles(configuration =>
      {
        configuration.RootPath = "ClientApp/build";
      });
    }

    public void Configure(IApplicationBuilder app)
    {
      var development = HostingEnvironment.IsDevelopment();
      if (development)
      {
        app.UseDeveloperExceptionPage();
      }
      else
      {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
      }

      app
        .UseDefaultFiles()
        .UseStaticFiles()
        .UseBotFramework()
        .UseHttpsRedirection()
        .UseSpaStaticFiles();

      app.UseMvc(routes =>
      {
        routes.MapRoute(
          name: "default",
          template: "{controller}/{action=Index}/{id?}");
      });

      app.UseSpa(spa =>
      {
        spa.Options.SourcePath = "ClientApp";

        if (development)
        {
          spa.UseReactDevelopmentServer(npmScript: "start");
        }
      });
    }
  }
}
