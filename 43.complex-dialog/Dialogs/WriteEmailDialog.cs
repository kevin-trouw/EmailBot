using ChatBotEmail;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace Microsoft.BotBuilderSamples
{
    public class WriteEmailDialog : ComponentDialog
    {
        // Define a Done response for the selection prompt
        private const string DoneOption = "Done";

        // Define value names for values tracked inside the dialogs
        private const string EmailInfo = "value-emailInfo";

        public WriteEmailDialog()
            : base(nameof(WriteEmailDialog))
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                ReceiverStepAsync,
                SubjectStepAsync,
                TextStepAsync,
                ConfirmStepAsync,
                ConfirmSelectionStepAsync,
                AcknowledgementStepAsync
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private static async Task<DialogTurnResult> ReceiverStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Create an object in which to collect the email's information within the dialog
            stepContext.Values[EmailInfo] = new EmailProfile();

            var promptOptions = new PromptOptions { Prompt = MessageFactory.Text("Enter the receiver of the email") };

            // Ask the user to enter the receiver
            return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> SubjectStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var emailProfile = (EmailProfile)stepContext.Values[EmailInfo];
            emailProfile.Receiver = (string)stepContext.Result;

            var promptOptions = new PromptOptions { Prompt = MessageFactory.Text("Enter the subject of the email.") };

            // Ask the user for the subject of the email
            return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> TextStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var emailProfile = (EmailProfile)stepContext.Values[EmailInfo];
            emailProfile.Receiver = (string)stepContext.Result;
            emailProfile.Subject = (string)stepContext.Result;

            var promptOptions = new PromptOptions { Prompt = MessageFactory.Text("Enter the text of the email.") };

            // Ask the user for the text of the email
            return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> ConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var emailProfile = (EmailProfile)stepContext.Values[EmailInfo];
            emailProfile.Confirm = (string)stepContext.Result;

            var promptOptions = new PromptOptions { Prompt = MessageFactory.Text("Are you sure you want to send the email? (Y = Yes - N = No") };

            // Ask the user for the confirmation of the email
            return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> ConfirmSelectionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var emailProfile = (EmailProfile)stepContext.Values[EmailInfo];
            emailProfile.Confirm = (string)stepContext.Result;

            if (emailProfile.Confirm == "Y")
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("Email is sended."), cancellationToken);
                return await stepContext.BeginDialogAsync(nameof(TopLevelDialog), null, cancellationToken);
            }

            else
            {
                return await stepContext.BeginDialogAsync(nameof(TopLevelDialog), null, cancellationToken);
            }
        }


        private async Task<DialogTurnResult> AcknowledgementStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Set email's information
            var emailProfile = (EmailProfile)stepContext.Values[EmailInfo];

            //Ask for confirmation
            await stepContext.Context.SendActivityAsync(
                MessageFactory.Text($"Receiver: {((EmailProfile)stepContext.Values[EmailInfo]).Subject}"), cancellationToken);

            return await stepContext.EndDialogAsync(stepContext.Values[EmailInfo], cancellationToken);
        }
    }
}
