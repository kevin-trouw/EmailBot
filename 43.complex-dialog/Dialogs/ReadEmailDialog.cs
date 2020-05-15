using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace Microsoft.BotBuilderSamples
{
    public class ReadEmailDialog : ComponentDialog
    {
        // Define a Done response for the selection prompt
        private const string DoneOption = "Done";

        // Define value names for values tracked inside the dialogs
        private const string EmailInfo = "value-emailInfo";

        private const string MenuSelected = "value-menuSelected";

        // Define the option choices for the selection prompt
        private readonly string[] _menuOptions = new string[]
        {
            "Last 10", "Last 15","All",
        };

        private readonly string[] _emailOptions = new string[]
        {
            "Email 1", "Email 2", "Email 3", "Email 4", "Email 5", "Email 6", "Email 7", "Email 8", "Email 9", "Email 10",
             "Email 11", "Email 12", "Email 13", "Email 14", "Email 15", "Email 16", "Email 17", "Email 18", "Email 19", "Email 20",
        };

        public ReadEmailDialog()
            : base(nameof(ReadEmailDialog))
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[] {
                StartSelectionStepAsync,
                LoopStepAsync,
                //AcknowledgementStepAsync
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> StartSelectionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var list = stepContext.Options as List<string> ?? new List<string>();
            stepContext.Values[MenuSelected] = list;

            // Create a prompt message
            string message;
            if (list.Count is 0)
            {
                message = $"Please choice a option, or `{DoneOption}` to finish";
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

        private async Task<DialogTurnResult> LoopStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Retrieve their selection
            var list = stepContext.Values[MenuSelected] as List<string>;
            var choice = (FoundChoice)stepContext.Result;
            var done = choice.Value == DoneOption;

            if(choice.Value == "Last 10")
            {
                // Create the list of emails
                var emails = _emailOptions.ToList();
                emails.Add(DoneOption);
                List<string> last10 = emails.GetRange(0, 10);

                var promptOptions = new PromptOptions
                {
                    Prompt = MessageFactory.Text("Choose a email you want to read."),
                    RetryPrompt = MessageFactory.Text("Please choose an option from the list."),
                    Choices = ChoiceFactory.ToChoices(last10)
                };
                // Promp the user for a choice
                return await stepContext.PromptAsync(nameof(ChoicePrompt), promptOptions, cancellationToken);

            }
            else if (choice.Value == "Last 15")
            {
                // Create the list of emails
                var emails = _emailOptions.ToList();
                emails.Add(DoneOption);
                List<string> last15 = emails.GetRange(0, 15);

                var promptOptions = new PromptOptions
                {
                    Prompt = MessageFactory.Text("Choose a email you want to read."),
                    RetryPrompt = MessageFactory.Text("Please choose an option from the list."),
                    Choices = ChoiceFactory.ToChoices(last15)
                };

                // Promp the user for a choice
                return await stepContext.PromptAsync(nameof(ChoicePrompt), promptOptions, cancellationToken);

            }

            else
            {
                var emails = _emailOptions.ToList();
                emails.Add(DoneOption);
                var promptOptions = new PromptOptions
                {
                    Prompt = MessageFactory.Text("Choose a email you want to read."),
                    RetryPrompt = MessageFactory.Text("Please choose an option from the list."),
                    Choices = ChoiceFactory.ToChoices(emails)
                };

                // Promp the user for a choice
                return await stepContext.PromptAsync(nameof(ChoicePrompt), promptOptions, cancellationToken);
            }
            
        }

        
    }
}
