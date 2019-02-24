using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using AlexaFunctions.Interfaces;
using AlexaFunctions.Models;
using AlexaFunctions.Services;
using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AlexaFunctions
{
    public class Function
    {
        public ICalendarService CalendarService { get; }
        public IDateTimeService DateTimeService { get; }

        public Function(ICalendarService calendarService, IDateTimeService dateTimeService)
        {
            CalendarService = calendarService;
            DateTimeService = dateTimeService;
        }

        // Poor man's dependency injection
        public Function() : this(new CalendarService(), new DateTimeService())
        {
        }

        public SkillResponse FunctionHandler(SkillRequest input, ILambdaContext context)
        {
            var logger = context.Logger;

            switch (input.Request)
            {
                case LaunchRequest launchRequest:
                    return HandleLaunchRequest(launchRequest, logger);
                case IntentRequest intentRequest:
                    return HandleIntentRequest(intentRequest, logger);
                default:
                    throw new NotImplementedException($"Non-implemented request type: {input.Request.GetType().Name}");
            }
        }

        private SkillResponse HandleLaunchRequest(LaunchRequest launchRequest, ILambdaLogger logger)
        {
            var response = ResponseBuilder.Tell(new PlainTextOutputSpeech
            {
                Text = "Welcome! I can tell you the events that are coming up on the City of Plano Calendar. Just say, when is the next meeting?",
            });

            return response;
        }

        private SkillResponse HandleIntentRequest(IntentRequest intentRequest, ILambdaLogger logger)
        {
            if (intentRequest.Intent.Name == "next_event")
            {
                return HandleNextEventRequest(intentRequest, logger);
            }

            throw new NotImplementedException($"Non-implemented intent request type: {intentRequest.Intent.Name}");
        }

        private SkillResponse HandleNextEventRequest(IntentRequest intentRequest, ILambdaLogger logger)
        {
            string message;

            if (intentRequest.Intent.Slots.TryGetValue("calendar_name", out var calendarNameSlot)
                && !String.IsNullOrWhiteSpace(calendarNameSlot?.Value)
                && Enum.TryParse(typeof(CalendarType), calendarNameSlot?.Value, out var tempCalendarType)
                && (CalendarType)tempCalendarType != CalendarType.None)
            {
                Calendar calendar = CalendarService.GetCalendar((CalendarType)tempCalendarType)
                    .ConfigureAwait(false).GetAwaiter().GetResult();

                CalendarEntry nextEvent = CalendarService.GetFutureCalendarEntries(calendar)
                    .FirstOrDefault();

                if (nextEvent != null)
                {
                    string ssml = $"<s>The next event in the {calendar.Name} calendar is titled: \"{nextEvent.Name}\".</s>"
                        + $"<s>It starts at <say-as interpret-as=\"time\">{nextEvent.Start.ToString("HH:mm")}</say-as> ";
                    if (DateTimeService.NowCentral.Date == nextEvent.Start.Date)
                    {
                        ssml += "<emphasis level=\"strong\">today</emphasis>";
                    }
                    else if (DateTimeService.NowCentral.Date.AddDays(1) == nextEvent.Start.Date)
                    {
                        ssml += "tomorrow";
                    }
                    else
                    {
                        ssml += $"on <say-as interpret-as=\"date\" format=\"md\">{nextEvent.Start.Date.ToString("MMdd")}</say-as>";
                    }
                    ssml += ".</s>";

                    ssml += $"<s>It lasts until <say-as interpret-as=\"time\">{nextEvent.End.ToString("HH:mm")}</say-as>.</s>";

                    if (!String.IsNullOrWhiteSpace(nextEvent.Description))
                    {
                        ssml += $"<s>Here's the description.</s><s>{nextEvent.Description}</s>";
                    }

                    return ResponseBuilder.Tell(new SsmlOutputSpeech
                    {
                        Ssml = ssml
                    });
                }
                else
                {
                    message = $"I'm sorry. I couldn't find any future calendar entries in the {calendar.Name} calendar.";
                }
            }
            else
            {
                message = "I'm sorry. I wasn't able to understand that calendar name.";
            }

            return ResponseBuilder.Tell(new PlainTextOutputSpeech
            {
                Text = message
            });
        }
    }
}
