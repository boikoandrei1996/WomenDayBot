﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WomenDay.Dialogs;
using WomenDay.Models;
using WomenDay.Repositories;
using WomenDay.Services;

namespace WomenDay
{
  /// <summary>
  /// Main entry point and orchestration for bot.
  /// </summary>
  public class Bot : IBot
  {
    private readonly ILogger<Bot> _logger;
    private readonly BotAccessors _accessors;
    private readonly MainDialogSet _mainDialogSet;
    private readonly UserState _userState;
    private readonly ConversationState _conversationState;
    private readonly ICardService _cardService;
    private readonly OrderRepository _orderRepository;

    public Bot(
      BotAccessors botAccessors,
      UserState userState,
      ConversationState conversationState,
      ICardService cardService,
      OrderRepository orderRepository,
      ILogger<Bot> logger)
    {
      _accessors = botAccessors ?? throw new ArgumentNullException(nameof(botAccessors));
      _userState = userState ?? throw new ArgumentNullException(nameof(userState));
      _conversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
      _cardService = cardService ?? throw new ArgumentNullException(nameof(cardService));
      _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
      _logger = logger;

      _mainDialogSet = new MainDialogSet(_accessors.DialogStateAccessor, new GreetingDialog(), new CategoryChooseDialog(), new UserNameValidator());
    }

    public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken)
    {
      // Retrieve user data from state.
      var userData = await _accessors.UserDataAccessor.GetAsync(turnContext, () => new UserData(), cancellationToken);

      // Establish context for our dialog from the turn context.
      var dialogContext = await _mainDialogSet.CreateContextAsync(turnContext, cancellationToken);

      var dialogTurnResult = await dialogContext.ContinueDialogAsync(cancellationToken);

      if (dialogTurnResult.Status == DialogTurnStatus.Empty)
      {
        if (string.IsNullOrEmpty(userData.Name) || string.IsNullOrEmpty(userData.Room))
        {
          // Start greeting dialog
          await dialogContext.BeginDialogAsync(GreetingDialog.Id, null, cancellationToken);
        }
        else if (turnContext.Activity.Value == null)
        {
          // Start category choose dialog
          await turnContext.SendActivityAsync($"Хаю хай {userData.Name} из {userData.Room}.", cancellationToken: cancellationToken);
          await dialogContext.BeginDialogAsync(CategoryChooseDialog.Id, null, cancellationToken);
        }
        else
        {
          // Register order
          await this.RegisterOrderAsync(turnContext.Activity.Value, userData, cancellationToken);
          await turnContext.SendActivityAsync("Мы уже летим, красотка. брымбрымбрым....", cancellationToken: cancellationToken);
        }
      }
      else if (dialogTurnResult.Status == DialogTurnStatus.Complete)
      {
        if (dialogTurnResult.Result is UserData)
        {
          // Greeting dialog is completed
          userData = (UserData)dialogTurnResult.Result;

          await _accessors.UserDataAccessor.SetAsync(
            turnContext,
            userData,
            cancellationToken);

          await turnContext.SendActivityAsync($"Добро пожаловать, {userData.Name} из {userData.Room}.", cancellationToken: cancellationToken);
          await dialogContext.BeginDialogAsync(CategoryChooseDialog.Id, null, cancellationToken);
        }
        else if (dialogTurnResult.Result is OrderCategory)
        {
          // Category choose dialog is completed
          var category = (OrderCategory)dialogTurnResult.Result;

          await this.ShowMenuAsync(turnContext, category, cancellationToken);
        }
      }

      // Persist any changes to storage.
      await _accessors.UserState.SaveChangesAsync(turnContext, false, cancellationToken);
      await _accessors.ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
    }

    private async Task RegisterOrderAsync(
      object value,
      UserData userData,
      CancellationToken cancellationToken)
    {
      var order = JsonConvert.DeserializeObject<Order>(value.ToString());
      order.DocumentId = Guid.NewGuid();
      order.OrderId = Guid.NewGuid();
      order.RequestTime = DateTime.Now;
      order.UserData = userData;

      var document = await _orderRepository.CreateDocumentAsync(order);
      _logger.LogDebug("Created {orderDoc}", document);
    }

    private async Task ShowMenuAsync(
      ITurnContext turnContext,
      OrderCategory category,
      CancellationToken cancellationToken)
    {
      var attachments = await _cardService.CreateAttachmentsAsync(category);
      if (attachments.Any())
      {
        var reply = turnContext.Activity.CreateReply();

        reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
        reply.Attachments = attachments;

        await turnContext.SendActivityAsync(reply, cancellationToken);
      }
      else
      {
        await turnContext.SendActivityAsync("В этой категории нет элементов.", cancellationToken: cancellationToken);
      }
    }
  }
}
