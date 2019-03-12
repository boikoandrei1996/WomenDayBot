using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace WomenDay.Dialogs
{
  public sealed class MainDialogSet : DialogSet
  {
    public MainDialogSet(IStatePropertyAccessor<DialogState> dialogState) : base(dialogState)
    {
      // Greeting dialog
      Add(new TextPrompt(GreetingDialog.NamePromt, new UserNameValidator().ValidateAsync));
      Add(new ChoicePrompt(GreetingDialog.RoomPromt));
      Add(new WaterfallDialog(GreetingDialog.Id, new GreetingDialog().GetWaterfallSteps()));

      // Category choose dialog
      Add(new ChoicePrompt(CategoryChooseDialog.OrderCategoryPromt));
      Add(new WaterfallDialog(CategoryChooseDialog.Id, new CategoryChooseDialog().GetWaterfallSteps()));
    }
  }
}
