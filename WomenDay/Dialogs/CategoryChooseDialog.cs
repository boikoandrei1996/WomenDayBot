using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using WomenDay.Models;

namespace WomenDay.Dialogs
{
  public sealed class CategoryChooseDialog
  {
    public const string Id = "CategoryChooseDialogId";

    public const string OrderCategoryPromt = "OrderCategoryPromt";

    public WaterfallStep[] GetWaterfallSteps()
    {
      return new WaterfallStep[]
      {
        PromtForCategoryAsync,
        EndDialogAsync
      };
    }

    private async Task<DialogTurnResult> PromtForCategoryAsync(
      WaterfallStepContext stepContext,
      CancellationToken cancellationToken = default(CancellationToken))
    {
      var categories = Enum.GetNames(typeof(OrderCategory));

      return await stepContext.PromptAsync(
        OrderCategoryPromt,
        new PromptOptions
        {
          Prompt = MessageFactory.Text("Выбирай категорию"),
          RetryPrompt = MessageFactory.Text("Повтори-ка"),
          Choices = ChoiceFactory.ToChoices(categories)
        },
        cancellationToken);
    }

    private async Task<DialogTurnResult> EndDialogAsync(
      WaterfallStepContext stepContext,
      CancellationToken cancellationToken = default(CancellationToken))
    {
      var choiceValue = (stepContext.Result as FoundChoice).Value;
      var category = Enum.Parse<OrderCategory>(choiceValue);

      return await stepContext.EndDialogAsync(category, cancellationToken);
    }
  }
}
