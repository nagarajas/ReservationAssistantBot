using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace ReservationAssistant.Dialogs
{
    public class ReservationOptionsDialog : ComponentDialog
    {
        public ReservationOptionsDialog(string dialogId) : base(dialogId)
        {
            AddDialog(new WaterfallDialog(nameof(ReservationOptionsDialog), new WaterfallStep[]
            {
                ShowChoicesAsync,
                ConfirmSelectedOptionAsync
            }));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));


            InitialDialogId = nameof(ReservationOptionsDialog);
        }

        private async Task<DialogTurnResult> ConfirmSelectedOptionAsync(WaterfallStepContext stepContext,
                                                                  CancellationToken cancellationToken)
        {
            var result = (FoundChoice)stepContext.Result;
            await stepContext.Context.SendActivityAsync($"Ok, I will assist you in reserving the {result.Value}");

            if (result.Value == "Table")
                return await stepContext.ReplaceDialogAsync(nameof(TableReservationDialog), cancellationToken);
            else
            {
                await stepContext.Context.SendActivityAsync("Not implemented yet");
                return await stepContext.EndDialogAsync(cancellationToken);
            }
        }

        private async Task<DialogTurnResult> ShowChoicesAsync(WaterfallStepContext stepContext,
                                                     CancellationToken cancellationToken)
        {
            var options = new PromptOptions()
            {
                Prompt = MessageFactory.Text("Please choose which kind of reservation you would like proceed with"),
                RetryPrompt = MessageFactory.Text("Invalid option, please choose the valid option"),
                Choices = ChoiceFactory.ToChoices(new List<string>() { "Table", "Room" })
            };

            return await stepContext.PromptAsync(nameof(ChoicePrompt), options, cancellationToken);
        }


    }
}
