using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xunit;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;

using AlexaFunctions;
using Alexa.NET.Response;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using AlexaFunctions.Services;
using AlexaFunctions.Tests.Helpers;

namespace AlexaFunctions.Tests
{
    public class FunctionTest
    {
        private const string EXAMPLE_FILE = "CityCouncilMeetings.xml";
        private readonly DateTime EXAMPLE_DATETIME = new DateTime(2019, 2, 25, 19, 0, 0);

        [Fact]
        public void Returns_Next_Event()
        {
            // Invoke the lambda function and confirm the string was upper cased.
            var function = new Function(CreateCalendarService(), new DummyDateTimeService(EXAMPLE_DATETIME));
            var context = new TestLambdaContext();
            SkillRequest request = new SkillRequest();
            var intentRequest = new IntentRequest();
            request.Request = intentRequest;
            intentRequest.Intent = new Intent { Name = "next_event", Slots = new Dictionary<string, Slot>() };
            intentRequest.Intent.Slots.Add("calendar_name", new Slot { Name = "calendar_name", Value = "Meetings" }); 

            SkillResponse response = function.FunctionHandler(request, context);

            // TODO: Add more tests here
            Assert.NotNull(response);
        }

        private CalendarService CreateCalendarService()
        {
            var service = new CalendarService(new DummyHttpService(EXAMPLE_FILE), new DummyDateTimeService(EXAMPLE_DATETIME));
            return service;
        }
    }
}
