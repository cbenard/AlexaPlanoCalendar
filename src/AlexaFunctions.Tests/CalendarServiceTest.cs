using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xunit;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;

using AlexaFunctions;
using AlexaFunctions.Services;
using AlexaFunctions.Tests.Helpers;
using AlexaFunctions.Models;

namespace AlexaFunctions.Tests
{
    public class CalendarServiceTest
    {
        private const string EXAMPLE_FILE = "CityCouncilMeetings.xml";

        [Fact]
        public async Task Parses_Calendar()
        {
            var service = new CalendarService(new DummyHttpService(EXAMPLE_FILE));

            Calendar calendar = await service.GetCalendar(CalendarType.Meetings);

            Assert.NotNull(calendar);
            Assert.NotNull(calendar.Entries);
            Assert.Equal("City Council & Commissions Meetings", calendar.Name);
            Assert.Equal(5, calendar.Entries.Length);
            Assert.Equal("City Council Meeting", calendar.Entries.First().Name);
            Assert.Equal(new DateTime(2019, 2, 25, 19, 0, 0), calendar.Entries.First().Start);
            Assert.Equal(new DateTime(2019, 2, 25, 23, 59, 0), calendar.Entries.First().End);
            Assert.Equal("Parks and Recreation Planning Board - Notice of Tour", calendar.Entries.Last().Name);
            Assert.Equal(new DateTime(2019, 3, 5, 15, 0, 0), calendar.Entries.Last().Start);
            Assert.Equal(new DateTime(2019, 3, 5, 23, 59, 0), calendar.Entries.Last().End);
        }

        [Fact]
        public void Parses_Calendar_Name()
        {
            var service = new CalendarService(new DummyHttpService(EXAMPLE_FILE));
            string input = "Plano, TX - Calendar - City Council & Commissions Meetings";

            string output = service.ParseCalendarName(input);

            Assert.Equal("City Council & Commissions Meetings", output);
        }

        [Fact]
        public void Parses_Event_Name()
        {
            var service = new CalendarService(new DummyHttpService(EXAMPLE_FILE));
            string input = "Board of Adjustment";

            string output = service.ParseEventName(input);

            Assert.Equal("Board of Adjustment", output);
        }

        [Fact]
        public void Parses_Event_Name_Cancellation()
        {
            var service = new CalendarService(new DummyHttpService(EXAMPLE_FILE));
            string input = "Board of Adjustment - Cancellation";

            string output = service.ParseEventName(input);

            Assert.Null(output);
        }

        [Fact]
        public void Parses_Two_Times()
        {
            var service = new CalendarService(new DummyHttpService(EXAMPLE_FILE));
            string dateText = "January 1, 2019";
            string timeText = "06:00 AM - 08:00 PM";

            (DateTime? start, DateTime? end) = service.ParseDateText(dateText, timeText);

            Assert.Equal(new DateTime(2019, 1, 1, 6, 0, 0), start);
            Assert.Equal(new DateTime(2019, 1, 1, 20, 0, 0), end);
        }

        [Fact]
        public void Parses_Link()
        {
            var service = new CalendarService(new DummyHttpService(EXAMPLE_FILE));
            string input = "http://www.plano.gov/Calendar.aspx?EID=22172";

            Uri output = service.ParseLink(input);

            Assert.Equal("http", output.Scheme);
            Assert.Equal("www.plano.gov", output.Host);
            Assert.Equal("/Calendar.aspx", output.AbsolutePath);
            Assert.Equal("?EID=22172", output.Query);
        }

        [Fact]
        public void Parses_Description()
        {
            var service = new CalendarService(new DummyHttpService(EXAMPLE_FILE));
            string input = "<strong>Event date:</strong> February 26, 2019 <br><strong>Event Time: </strong>06:30 PM - 11:59 PM<br><strong>Location:</strong> <br>Plano Housing Authority Administration Building<br>1740 Avenue G<br>Plano, TX 75074";

            string output = service.ParseDescription(input);

            Assert.Null(output); 
        }

        [Fact]
        public void Doesnt_Parse_Empty_Description()
        {
            var service = new CalendarService(new DummyHttpService(EXAMPLE_FILE));
            string input = "<strong>Event date:</strong> February 26, 2019 <br><strong>Event Time: </strong>09:30 AM - 10:00 AM<br><strong>Location:</strong> <br>2501 Coit Road<br>Plano, TX 75075<br><strong>Description:</strong><br>Songs, nursery rhymes and books provide a language-rich experience for the youngest child. Active parent/caregiver participation is a must! Ages 0-24 months";

            string output = service.ParseDescription(input);

            Assert.Equal("Songs, nursery rhymes and books provide a language-rich experience for the youngest child. Active parent/caregiver participation is a must! Ages 0-24 months", output);
        }
    }
}
