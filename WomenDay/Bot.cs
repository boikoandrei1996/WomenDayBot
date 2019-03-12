using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
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
    private readonly BotAccessors _accessors;
    private readonly MainDialogSet _mainDialogSet;
    private readonly ICardService _cardService;
    private readonly OrderRepository _orderRepository;

    public Bot(
      BotAccessors botAccessors,
      ICardService cardService,
      OrderRepository orderRepository)
    {
      _accessors = botAccessors ?? throw new ArgumentNullException(nameof(botAccessors));
      _cardService = cardService ?? throw new ArgumentNullException(nameof(cardService));
      _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));

      _mainDialogSet = new MainDialogSet(_accessors.DialogStateAccessor);
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
          await this.StartGreetingDialogAsync(dialogContext, cancellationToken);
        }
        else if (turnContext.Activity.Value == null)
        {
          await this.StartCategoryChooseDialogAsync(dialogContext, userData, cancellationToken);
        }
        else
        {
          await this.RegisterOrderAsync(turnContext, turnContext.Activity.Value, userData, cancellationToken);
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

          await this.StartCategoryChooseDialogAsync(dialogContext, userData, cancellationToken);
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

    private Task StartGreetingDialogAsync(DialogContext dialogContext, CancellationToken cancellationToken)
    {
      return dialogContext.BeginDialogAsync(GreetingDialog.Id, null, cancellationToken);
    }

    private async Task StartCategoryChooseDialogAsync(DialogContext dialogContext, UserData userData, CancellationToken cancellationToken)
    {
      await dialogContext.Context.SendActivityAsync($"Хаю хай {userData.Name} из {userData.Room}.", cancellationToken: cancellationToken);
      await dialogContext.BeginDialogAsync(CategoryChooseDialog.Id, null, cancellationToken);
    }

    private async Task RegisterOrderAsync(
      ITurnContext turnContext,
      object value,
      UserData userData,
      CancellationToken cancellationToken)
    {
      var order = JsonConvert.DeserializeObject<Order>(value.ToString());
      order.DocumentId = Guid.NewGuid();
      order.OrderId = Guid.NewGuid();
      order.RequestTime = DateTime.Now;
      order.UserData = userData;

      await _orderRepository.CreateDocumentAsync(order);

      await turnContext.SendActivityAsync("Мы уже летим, красотка. брымбрымбрым....", cancellationToken: cancellationToken);
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
