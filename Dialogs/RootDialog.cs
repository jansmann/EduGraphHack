using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Collections.Generic;
using AdaptiveCards;
using Newtonsoft.Json;
using EduBot.Service.GraphAssignmentsService;

namespace EduBot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        private enum State
        {
            DEFAULT = 0,
            STATUS = 1,
            CREATE = 2,
        }

        private const string statusCommand = "status";
        private const string notifyCommand = "notify";
        private const string createCommand = "create";

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



            if (state == State.DEFAULT)
            {
                string[] sentence = activity.Text.Split(' ');

                if (sentence.Length >= 1)
                {
                    string command = sentence[0];


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

                            //await context.PostAsync("What would you like to know about {studentName}?");
                            var reply = activity.CreateReply("What would you like to know about {studentName}?");
                            reply.Type = ActivityTypes.Message;
                            reply.TextFormat = TextFormatTypes.Plain;

                            reply.SuggestedActions = new SuggestedActions()
                            {
                                Actions = new List<CardAction>()
                            };

                            foreach (string status in statuses)
                            {
                                reply.SuggestedActions.Actions.Add(new CardAction() { Title = status, Type = ActionTypes.ImBack, Value = status });
                            }

                            await context.PostAsync(reply);
                            //await context.PostAsync(GenerateOptions(context, statuses));
                        }
                        else
                        {
                            // No message
                            await context.PostAsync("Please provide the student id you wish to query");
                        }
                    }
                    else if (command == createCommand)
                    {
                        Activity reply = activity.CreateReply("Create your assignment:");
                        reply.Attachments = new List<Attachment>();

                        AdaptiveCard card = new AdaptiveCard();

                        card.Body.Add(new TextInput()
                        {
                            Id = "displayName",
                            Placeholder = "enter assignment title"
                        });

                        card.Body.Add(new ToggleInput()
                        {
                            IsRequired = true,
                            Id = "distributeForStudentWork",
                            Title = "Distribute for student work"
                        });


                        card.Actions.Add(new SubmitAction()
                        {
                            Title = "Submit",
                        });

                        // Create the attachment.
                        Attachment attachment = new Attachment()
                        {
                            ContentType = AdaptiveCard.ContentType,
                            Content = card
                        };

                        reply.Attachments.Add(attachment);

                        await context.PostAsync(reply);

                        state = State.CREATE;
                    }
                }
            }
            else if (state == State.STATUS)
            {
                string[] sentence = activity.Text.Split(' ');

                if (sentence.Length >= 1)
                {
                    string command = sentence[0];
                    if (command == assignmentsStatus)
                    {
                        List<AssignmentJSON> assignments = new List<AssignmentJSON>();

                        DateTime time = DateTime.Now.AddDays(-2);
                        for (int i = 0; i < 6; i++)
                        {
                            AssignmentJSON x = new AssignmentJSON()
                            {
                                displayName = "Assignment " + i,
                                dueDate = time
                            };

                            time.AddDays(1);
                        }
                    }
                }
            }
            else if (state == State.CREATE)
            {
                AssignmentJSON x = JsonConvert.DeserializeObject<AssignmentJSON>(activity.Value.ToString());

                if (x != null)
                {
                    Assignment assignment = new Assignment()
                    {
                        distributeForStudentWork = x.distributeForStudentWork,
                        id = Guid.NewGuid().ToString(),
                        resource = new Resource()
                        {
                            createdBy = "me",
                            createdDateTime = DateTime.Now.ToString(),
                            displayName = x.displayName,
                            lastModifiedBy = "me",
                            lastModifiedDateTime = DateTime.Now.ToString()
                        }
                    };


                    GraphAssignmentsService graphService = new GraphAssignmentsService();
                    graphService.AddToAssignments(assignment);


                }

                state = State.DEFAULT;
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



        public class AssignmentJSON
        {
            public string displayName { get; set; }
            public bool distributeForStudentWork { get; set; }
            public DateTime dueDate { get; set; }
            public bool completed { get; set; }
        }
    }
}