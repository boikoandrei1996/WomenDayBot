using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using WomenDay.Models;

namespace WomenDay.Dialogs
{
  public sealed class GreetingDialog
  {
    public const string Id = "GreetingDialogId";

    public const string NamePromt = "NamePromt";
    public const string RoomPromt = "RoomPromt";

    // Define keys for tracked values within the dialog.
    private const string NameKey = "NameKey";
    private const string RoomKey = "RoomKey";

    public WaterfallStep[] GetWaterfallSteps()
    {
      return new WaterfallStep[]
      {
        PromtForNameAsync,
        PromtForRoomAsync,
        EndDialogAsync
      };
    }

    private async Task<DialogTurnResult> PromtForNameAsync(
      WaterfallStepContext stepContext,
      CancellationToken cancellationToken = default(CancellationToken))
    {
      return await stepContext.PromptAsync(
        NamePromt,
        new PromptOptions
        {
          Prompt = MessageFactory.Text("Не то, чтобы я хотел подкатить, но как тебя зовут, Принцесса?"),
          RetryPrompt = MessageFactory.Text("Да ладно, ну скажи имечко?")
        },
        cancellationToken);
    }

    private async Task<DialogTurnResult> PromtForRoomAsync(
      WaterfallStepContext stepContext,
      CancellationToken cancellationToken = default(CancellationToken))
    {
      // Record the name information in the current dialog state.
      stepContext.Values[NameKey] = (string)stepContext.Result;

      return await stepContext.PromptAsync(
        RoomPromt,
        new PromptOptions
        {
          Prompt = MessageFactory.Text("Мы уже почти на одной волне. Черкани адресок: я заеду."),
          RetryPrompt = MessageFactory.Text("Да не домашний адрес. В офисе комнату напиши."),
          Choices = ChoiceFactory.ToChoices(new List<string> { "701", "702", "801", "802", "803", "806", "807", "808" })
        },
        cancellationToken);
    }

    private async Task<DialogTurnResult> EndDialogAsync(
      WaterfallStepContext stepContext,
      CancellationToken cancellationToken = default(CancellationToken))
    {
      stepContext.Values[RoomKey] = (stepContext.Result as FoundChoice).Value;

      var userData = new UserData
      {
        Name = (string)stepContext.Values[NameKey],
        Room = (string)stepContext.Values[RoomKey]
      };

      return await stepContext.EndDialogAsync(userData, cancellationToken);
    }
  }
}
