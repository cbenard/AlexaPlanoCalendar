using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
                case SessionEndedRequest sessionEndedRequest:
                    return HandleSessionEndedRequest(sessionEndedRequest, logger);
                default:
                    throw new NotImplementedException($"Non-implemented request type: {input.Request.GetType().Name}");
            }
        }

        private SkillResponse HandleLaunchRequest(Request launchRequest, ILambdaLogger logger)
        {
            var response = ResponseBuilder.Tell(new PlainTextOutputSpeech
            {
                Text = "Welcome! I can tell you the events that are coming up on the City of Plano Calendar. Just say, when is the next meeting?",
            });

            response.Response.ShouldEndSession = false;

            return response;
        }

        private SkillResponse HandleIntentRequest(IntentRequest intentRequest, ILambdaLogger logger)
        {
            if (intentRequest.Intent.Name == "next_event")
            {
                return HandleNextEventRequest(intentRequest, logger);
            }
            else if (intentRequest.Intent.Name == "AMAZON.HelpIntent"
                || intentRequest.Intent.Name == "AMAZON.NavigateHomeIntent")
            {
                return HandleLaunchRequest(intentRequest, logger);
            }

            throw new NotImplementedException($"Non-implemented intent request type: {intentRequest.Intent.Name}");
        }

        private SkillResponse HandleSessionEndedRequest(Request sessionEndedRequest, ILambdaLogger logger)
        {
            var response = ResponseBuilder.Tell(new PlainTextOutputSpeech
            {
                Text = "Goodbye.",
            });

            response.Response.ShouldEndSession = true;

            return response;
        }

        private SkillResponse HandleNextEventRequest(IntentRequest intentRequest, ILambdaLogger logger)
        {
            string message;

            if (intentRequest.Intent.Slots.TryGetValue("calendar_name", out var calendarNameSlot)
                && TryParseCalendarName(calendarNameSlot, out CalendarType? calendarType, logger)
                && calendarType != CalendarType.None)
            {
                Calendar calendar = CalendarService.GetCalendar(calendarType.Value)
                    .ConfigureAwait(false).GetAwaiter().GetResult();

                CalendarEntry nextEvent = CalendarService.GetFutureCalendarEntries(calendar)
                    .FirstOrDefault();

                if (nextEvent != null)
                {
                    string ssml = CreateSsmlForEvent(calendar, nextEvent);

                    logger.Log($"Created response SSML: {ssml}");

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

            var response = ResponseBuilder.Tell(new PlainTextOutputSpeech
            {
                Text = message
            });

            response.Response.ShouldEndSession = true;

            return response;
        }

        private bool TryParseCalendarName(Slot calendarNameSlot, out CalendarType? calendarType, ILambdaLogger logger)
        {
            string calendarTypeText = calendarNameSlot.Value;
            var firstResolution = calendarNameSlot.Resolution?.Authorities?.FirstOrDefault();
            var firstResolutionStatusCode = firstResolution?.Status?.Code;
            var firstResolutionValue = firstResolution?.Values?.FirstOrDefault()?.Value?.Name;

            if (firstResolutionStatusCode == "ER_SUCCESS_MATCH"
                && !String.IsNullOrEmpty(firstResolutionValue))
            {
                calendarTypeText = firstResolutionValue;
            }

            logger.Log($"Received request for calendar: {calendarNameSlot.Value} - Resolution code: {firstResolutionStatusCode} - Resolution value: {firstResolutionValue} - Resolved to: {calendarTypeText}");

            if (Enum.TryParse(typeof(CalendarType), calendarTypeText, out var tempCalendarType))
            {
                calendarType = (CalendarType)tempCalendarType;
                return true;
            }

            calendarType = null;
            return false;
        }

        private string CreateSsmlForEvent(Calendar calendar, CalendarEntry nextEvent)
        {
            string ssml = "<speak>"; 

            ssml += $"<s>The next event in the {EscapeSsmlSpeech(calendar.Name)} calendar is titled: \"{EscapeSsmlSpeech(nextEvent.Name)}\".</s> ";
            ssml += $"<s>It starts at {CreateSsmlForTime(nextEvent.Start)} {CreateSsmlForDate(nextEvent.Start)}.</s> ";
            ssml += $"<s>It lasts until {CreateSsmlForTime(nextEvent.End)}.</s> ";

            if (!String.IsNullOrWhiteSpace(nextEvent.Description))
            {
                ssml += $"<s>Here's the description:</s> <s>{EscapeSsmlSpeech(nextEvent.Description)}</s>";
            }

            ssml += "</speak>";

            return ssml;
        }

        private string CreateSsmlForTime(DateTime time)
        {
            string timeString = time.ToString("hh:mm tt");

            if (timeString == "11:59 PM" || timeString == "12:00 AM")
            {
                timeString = "midnight";
            }
            else if (timeString == "12:00 PM")
            {
                timeString = "noon";
            }

            timeString = $"<say-as interpret-as=\"time\">{timeString}</say-as>";

            return timeString;
        }

        private string CreateSsmlForDate(DateTime date)
        {
            string ssml;

            if (DateTimeService.NowCentral.Date == date.Date)
            {
                ssml = "today";
            }
            else if (DateTimeService.NowCentral.Date.AddDays(1) == date.Date)
            {
                ssml = "tomorrow";
            }
            else
            {
                ssml = $"on <say-as interpret-as=\"date\" format=\"md\">{date.Date.ToString("MMdd")}</say-as>";
            }

            return ssml;
        }

        internal string EscapeSsmlSpeech(string input)
        {
            string escaped = input;

            escaped = WebUtility.HtmlEncode(escaped);

            return escaped;
        }
    }
}
