using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Collections.Generic;

namespace EduBot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        private enum State
        {
            DEFAULT = 0,
            STATUS = 1
        }

        private const string statusCommand = "status";
        private const string notifyCommand = "notify";

        private State state = State.DEFAULT;

        private const string assignmentsStatus = "Assignments";
        private List<string> statuses = new List<string> { assignmentsStatus };

        private string studentQueryId { get; set; }

        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            string[] sentence = activity.Text.Split(' ');

            if (sentence.Length >= 1)
            {
                string command = sentence[0];

                if (state == State.DEFAULT)
                {



                    if (command == notifyCommand) // show commands
                    {
                        string personName = "Fred";
                        string assignmentName = "Design Universe";

                        await context.PostAsync($"{personName} completed assignment: '{assignmentName}'");

                    }
                    else if (command == statusCommand) // send message
                    {
                        // Send command requires multiple parameters
                        if (sentence.Length >= 2)
                        {
                            string paramater = sentence[1];

                            studentQueryId = paramater;
                            state = State.STATUS;

                            await context.PostAsync("What would you like to know about {studentName}?");
                            await context.PostAsync(GenerateOptions(context, statuses));
                        }
                        else
                        {
                            // No message
                            await context.PostAsync("Please provide the student id you wish to query");
                        }
                    }
                }
                else if (state == State.STATUS)
                {
                    if (command == notifyCommand)
                    {

                    }
                }
            }

            context.Wait(MessageReceivedAsync);
        }

        /// <summary>
        /// Generates list of all configuration options to send to user
        /// Based on the different types of bots that have been configured
        /// </summary>
        private Activity GenerateOptions(IDialogContext context, List<string> options)
        {
            Activity reply = ((Activity)context.Activity).CreateReply();

            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            reply.Attachments = new List<Attachment>();


            // Configuration options
            foreach (string status in options)
            {

                List<CardAction> cardButtons = new List<CardAction>();

                CardAction button = new CardAction()
                {
                    Value = status,
                    Type = ActionTypes.ImBack,
                    Title = "Select"
                };

                cardButtons.Add(button);

                HeroCard card = new HeroCard()
                {
                    Title = status,
                    Buttons = cardButtons
                };

                Attachment attachment = card.ToAttachment();
                reply.Attachments.Add(attachment);
            }

            return reply;
        }
    }
}