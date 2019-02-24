﻿using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace WomanDayBot
{
    /// <summary>Defines a dialog for collecting a user's name.</summary>
    public class GreetingsDialog : DialogSet
    {
        /// <summary>The ID of the main dialog.</summary>
        public const string MainDialog = "main";

        /// <summary>The ID of the text prompt to use in the dialog.</summary>
        private const string TextPrompt = "textPrompt";

        /// <summary>Creates a new instance of this dialog set.</summary>
        /// <param name="dialogState">The dialog state property accessor to use for dialog state.</param>
        public GreetingsDialog(IStatePropertyAccessor<DialogState> dialogState)
            : base(dialogState)
        {
            // Add the text prompt to the dialog set.
            Add(new TextPrompt(TextPrompt));

            // Define the main dialog and add it to the set.
            Add(new WaterfallDialog(MainDialog, new WaterfallStep[]
            {
            async (stepContext, cancellationToken) =>
            {
                // Ask the user for their name.
                return await stepContext.PromptAsync(TextPrompt, new PromptOptions
                {
                    Prompt = MessageFactory.Text("What is your room?"),
                }, cancellationToken);
            },
            async (stepContext, cancellationToken) =>
            {
                // Assume that they entered their name, and return the value.
                return await stepContext.EndDialogAsync(stepContext.Result, cancellationToken);
            },
            }));
        }
    }
}
