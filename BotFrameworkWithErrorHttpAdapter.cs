// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with EmptyBot .NET Template version v4.4.3
using Microsoft.Bot.Builder.Integration.AspNet.Core;

namespace ReservationAssistant
{
    public class BotFrameworkWithErrorHttpAdapter : BotFrameworkHttpAdapter
    {
        public BotFrameworkWithErrorHttpAdapter()
        {
            OnTurnError = async (turnContext, exception) =>
            {

                await turnContext.SendActivityAsync("Sorry, Looks like there is some error! Try after some time");
            };
        }
    }
}