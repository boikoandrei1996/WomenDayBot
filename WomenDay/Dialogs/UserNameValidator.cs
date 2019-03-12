using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace WomenDay.Dialogs
{
  public class UserNameValidator
  {
    public async Task<bool> ValidateAsync(
      PromptValidatorContext<string> promptContext,
      CancellationToken cancellationToken = default(CancellationToken))
    {
      if (promptContext.Recognized.Succeeded == false)
      {
        await promptContext.Context.SendActivityAsync(
          "Ты че-то ввела не те букавки.",
          cancellationToken: cancellationToken);

        return false;
      }

      var value = promptContext.Recognized.Value ?? string.Empty;

      var regex = new Regex(@"(\w)+");

      if (regex.IsMatch(value))
      {
        return true;
      }

      await promptContext.Context.SendActivitiesAsync(new[]
      {
        MessageFactory.Text("Введи имечко."),
        promptContext.Options.RetryPrompt
      },
      cancellationToken);

      return false;
    }
  }
}
