// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with EmptyBot .NET Template version v4.4.3

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder.Dialogs;
using ReservationAssistant.Dialogs;

namespace ReservationAssistant.Bot
{
    public class ReservationAssistantBot : ActivityHandler
    {
        readonly UserState userState;
        readonly ConversationState conversationState;
        readonly ReservationOptionsDialog reservationOptionsDialog;
        readonly DialogSet dialogSet;

        public ReservationAssistantBot(ConversationState conversationState,
                                   UserState userState,
                                   ReservationOptionsDialog reservationOptionsDialog,
                                   TableReservationDialog tableReservationDialog)
        {
            this.reservationOptionsDialog = reservationOptionsDialog;
            this.conversationState = conversationState;
            this.userState = userState;

            var dialogSetStateAccessor = this.conversationState.CreateProperty<DialogState>(nameof(DialogSet));
            dialogSet = new DialogSet(dialogSetStateAccessor);
            dialogSet.Add(reservationOptionsDialog);
            dialogSet.Add(tableReservationDialog);
        }

        public async override Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            await conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await userState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected async override Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            await base.OnMessageActivityAsync(turnContext, cancellationToken);
            await HandleDialogNavigationAsync(turnContext, cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await HandleDialogNavigationAsync(turnContext,cancellationToken);
                }
            }
        }

        private async Task HandleDialogNavigationAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var dialogContext = await dialogSet.CreateContextAsync(turnContext, cancellationToken);
            var result = await dialogContext.ContinueDialogAsync(cancellationToken);

            if (result.Status == DialogTurnStatus.Empty)
            {
                await dialogContext.BeginDialogAsync(reservationOptionsDialog.Id, cancellationToken);
            }
        }
    }
}
