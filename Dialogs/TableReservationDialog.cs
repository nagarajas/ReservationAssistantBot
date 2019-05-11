using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder;
using System.Data;
using Microsoft.Bot.Builder.Dialogs.Choices;
using System.Collections.Generic;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;
using System.Linq;
using ReservationAssistant.Model;
using Microsoft.Bot.Schema;

namespace ReservationAssistant.Dialogs
{
    public class TableReservationDialog : ComponentDialog
    {
        private const string ReservedBy = "ReservedBy";
        private const string TotalOccupants = "TotalOccupants";
        private const string ReservationDate = "ReservationDate";
        private const string ConfirmBooking = "ConfirmTableReservationSteps";

        public TableReservationDialog(string dialogId)
            : base(dialogId)
        {
            AddDialog(new WaterfallDialog(nameof(TableReservationDialog), new WaterfallStep[]{
                PromptNameAsync,
                GetNameAndPromptOccupantsAsync,
                GetOccupantsAndPromptDateAsync,
                GetDateAndConfirmAsync,
                ReserveOrCancelTableAsync
            }));
            AddDialog(new TextPrompt(ReservedBy));
            AddDialog(new NumberPrompt<int>(TotalOccupants));
            AddDialog(new DateTimePrompt(ReservationDate));
            AddDialog(new ChoicePrompt(ConfirmBooking));

            InitialDialogId = nameof(TableReservationDialog);
        }

        private async Task<DialogTurnResult> ReserveOrCancelTableAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var result = (FoundChoice)stepContext.Result;

            if (result.Value == "Yes")
            {
                var tblReservationModel = (TableReservationModel)stepContext.Values["tbleReservationModel"];
                var reply = stepContext.Context.Activity.CreateReply();
                reply.Attachments.Add(new HeroCard()
                {
                    Title = "Reservation Details",
                    Subtitle = $"{tblReservationModel.ReservedBy} Reservation Reference",
                    Text = $"Reservation is done for total occupants: {tblReservationModel.TotalOccupants} on {tblReservationModel.ReservationDate.ToString("F")} "
                }.ToAttachment());
                await stepContext.Context.SendActivityAsync(reply, cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Ok, Cancelled the reservation"), cancellationToken);
            }

            return await stepContext.ReplaceDialogAsync(nameof(ReservationOptionsDialog), cancellationToken);
        }

        private async Task<DialogTurnResult> GetDateAndConfirmAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            DateTime reservationDate = default(DateTime);
            if (stepContext.Result is IList<DateTimeResolution> datetimes)
            {
                reservationDate = TimexHelpers.DateFromTimex(new TimexProperty(datetimes.First().Timex));
            }
            var tbleReservation = new TableReservationModel()
            {
                ReservedBy = stepContext.Values[ReservedBy].ToString(),
                TotalOccupants = (int)stepContext.Values[TotalOccupants],
                ReservationDate = reservationDate
            };

            stepContext.Values.Clear();
            stepContext.Values["tbleReservationModel"] = tbleReservation;
            return await stepContext.PromptAsync(ConfirmBooking, new PromptOptions()
            {
                Prompt = MessageFactory.Text($"Alright {tbleReservation.ReservedBy}, I am reservaing for {tbleReservation.TotalOccupants} on {reservationDate.ToString("MMM/dd/yyyy hh:mm tt")}, could you please confirm booking?"),
                Choices = ChoiceFactory.ToChoices(new List<string> { "Yes", "No" })

            }, cancellationToken);
        }

        private async Task<DialogTurnResult> GetOccupantsAndPromptDateAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values[TotalOccupants] = stepContext.Result;

            return await stepContext.PromptAsync(ReservationDate, new PromptOptions()
            {
                Prompt = MessageFactory.Text($"Ok, total {stepContext.Result} members. When do you want me to reserve?")
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> GetNameAndPromptOccupantsAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values[ReservedBy] = stepContext.Result;

            return await stepContext.PromptAsync(TotalOccupants, new PromptOptions()
            {
                Prompt = MessageFactory.Text($"Hello {stepContext.Result}, what would be the total occupants?")
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> PromptNameAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(ReservedBy, new PromptOptions()
            {
                Prompt = MessageFactory.Text("What's your good name?")
            }, cancellationToken);
        }
    }
}
