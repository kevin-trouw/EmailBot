using System.Threading;
using System.Threading.Tasks;
using ChatBotEmail;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace Microsoft.BotBuilderSamples
{
    public class TopLevelDialog : ComponentDialog
    {
        // Define a "done" response
        private const string DoneOption = "Done";

        // Define value names for values tracked inside the dialogs
        private const string UserInfo = "value-userInfo";

        private const string MenuSelected = "value-menuSelected";

        // Define the option choices for the selection prompt
        private readonly string[] _menuOptions = new string[]
        {
            "Write email", "Read email",
        };

        public TopLevelDialog()
            : base(nameof(TopLevelDialog))
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));

            AddDialog(new WriteEmailDialog());
            AddDialog(new ReadEmailDialog());

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]{
                EmailaddressStepAsync,
                PasswordStepAsync,
                StartSelectionStepAsync,
                LoopStepAsync,
                AcknowledgementStepAsync
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private static async Task<DialogTurnResult> EmailaddressStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Create an object in which to collect the user's information within the dialog.
            stepContext.Values[UserInfo] = new UserProfile();

            var promptOptions = new PromptOptions { Prompt = MessageFactory.Text("Please enter your emailaddress.") };

            // Ask the user to enter their emailaddress
            return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> PasswordStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Set the user's emailaddress to what they entered in response to the name prompt
            var userProfile = (UserProfile)stepContext.Values[UserInfo];
            userProfile.Password = (string)stepContext.Result;

            var promptOptions = new PromptOptions { Prompt = MessageFactory.Text("Please enter your password.") };

            // Ask the user to enter their password
            return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> StartSelectionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Set the user's password to what they entered in response to the password prompt
            var userProfile = (UserProfile)stepContext.Values[UserInfo];
            userProfile.Password = (string)stepContext.Result;

            if (userProfile.Password != "test123")
            {
                // If password is incorrect
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("Emailaddress or password is incorrect"), cancellationToken);

                return await stepContext.NextAsync(new List<string>(), cancellationToken);
            }

            else
            {
                var list = stepContext.Options as List<string> ?? new List<string>();
                stepContext.Values[MenuSelected] = list;

                // Create a prompt message
                string message;
                if (list.Count is 0)
                {
                    message = $"Please choose a option, or `{DoneOption}` to finish.";
                }
                else
                {
                    message = $"You have selected **{list[0]}**.";
                }

                // Create a list of options to choose from
                var options = _menuOptions.ToList();
                options.Add(DoneOption);

                var promptOptions = new PromptOptions
                {
                    Prompt = MessageFactory.Text(message),
                    RetryPrompt = MessageFactory.Text("Please choose an option from the list."),
                    Choices = ChoiceFactory.ToChoices(options)
                };

                // Prompt the user for a choice
                return await stepContext.PromptAsync(nameof(ChoicePrompt), promptOptions, cancellationToken);
                
            }
        }

        private async Task<DialogTurnResult> LoopStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Retrieve their selection
            var list = stepContext.Values[MenuSelected] as List<string>;
            var choice = (FoundChoice)stepContext.Result;
            string stringChoice = choice.ToString();
            var done = choice.Value == DoneOption;

            if(choice.Value == "Write email")
            {
                return await stepContext.BeginDialogAsync(nameof(WriteEmailDialog), null, cancellationToken);
            }

            else
            {
                return await stepContext.BeginDialogAsync(nameof(ReadEmailDialog), null, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> AcknowledgementStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(
                MessageFactory.Text($"Thanks for using the emailbot, {((UserProfile)stepContext.Values[UserInfo]).Emailadress}"),
                cancellationToken);

            // Exit the dialog, returning collected information
            return await stepContext.EndDialogAsync(stepContext.Values[UserInfo], cancellationToken);
        }
    }
}
