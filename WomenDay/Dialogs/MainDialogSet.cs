using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace WomenDay.Dialogs
{
  public sealed class MainDialogSet : DialogSet
  {
    public MainDialogSet(
      IStatePropertyAccessor<DialogState> dialogState,
      GreetingDialog greetingDialog,
      CategoryChooseDialog categoryChooseDialog,
      UserNameValidator userNameValidator) : base(dialogState)
    {
      // Greeting dialog
      Add(new TextPrompt(GreetingDialog.NamePromt, userNameValidator.ValidateAsync));
      Add(new ChoicePrompt(GreetingDialog.RoomPromt));
      Add(new WaterfallDialog(GreetingDialog.Id, greetingDialog.GetWaterfallSteps()));

      // Category choose dialog
      Add(new ChoicePrompt(CategoryChooseDialog.OrderCategoryPromt));
      Add(new WaterfallDialog(CategoryChooseDialog.Id, categoryChooseDialog.GetWaterfallSteps()));
    }
  }
}
