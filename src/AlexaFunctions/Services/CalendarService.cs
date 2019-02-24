using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using AlexaFunctions.Interfaces;
using AlexaFunctions.Models;

namespace AlexaFunctions.Services
{
    public class CalendarService : ICalendarService
    {
        private const string CALENDAR_NAME_PREFIX = "Plano, TX - Calendar - ";
        public async Task<Calendar> GetCalendar(CalendarType calendarType)
        {
            // Find the URL by calendar type
            string url = GetUrl(calendarType);

            // Grab the contents
            var client = new HttpClient();
            string contents = await client.GetStringAsync(url).ConfigureAwait(false);

            // Parse the contents into an object for querying
            Calendar calendar = ParseContents(contents);

            return calendar;
        }

        private string GetUrl(CalendarType calendarType)
        {
            switch (calendarType)
            {
                case CalendarType.Meetings:
                    return "http://plano.gov/RSSFeed.aspx?ModID=58&CID=City-Council-Commissions-Meetings-56";
                default:
                    throw new ArgumentException($"Unsupported calendar type: {calendarType}", nameof(calendarType));
            }
        }

        private Calendar ParseContents(string contents)
        {
            XNamespace ns = "http://www.plano.gov/Calendar.aspx";
            XDocument doc = XDocument.Parse(contents);

            // Parse the calendar title and strip the prefix, if present
            string title = doc?.Element("channel")?.Element("title")?.Value?.Trim();
            if (title?.StartsWith(CALENDAR_NAME_PREFIX) == true
                && title?.Length > CALENDAR_NAME_PREFIX.Length)
            {
                title = title.Substring(CALENDAR_NAME_PREFIX.Length);
            }

            var items = doc?.Elements("channel")?.Elements("item");
            var list = new List<CalendarEntry>();

            if (items != null)
            {
                foreach (var item in items)
                {
                    string itemTitle = item.Element("title")?.Value?.Trim();
                    string dateText = item.Element(ns + "EventDates")?.Value?.Trim();
                    string timeText = item.Element(ns + "EventTimes")?.Value?.Trim();
                    DateTime? start = ParseStartDateTime(dateText, timeText);
                    DateTime? end = ParseEndDateTime(dateText, timeText);
                    string location = item.Element(ns + "Location")?.Value?.Trim();
                    string description = ParseDescription(item.Element("description")?.Value?.Trim());
                    Uri link = ParseLink(item.Element("link")?.Value?.Trim());

                    if (!String.IsNullOrWhiteSpace(itemTitle) && start.HasValue)
                    {
                        list.Add(new CalendarEntry
                        {
                            Name = itemTitle,
                            Start = start.Value,
                            End = end,
                            Location = location,
                            Description = description,
                            Link = link
                        });
                    }
                }
            }

            var calendar = new Calendar
            {
                Name = title,
                Entries = list
            };

            return calendar;
        }

        private DateTime? ParseStartDateTime(string dateText, string timeText)
        {
            throw new NotImplementedException();
        }

        private DateTime? ParseEndDateTime(string dateText, string timeText)
        {
            throw new NotImplementedException();
        }

        private string ParseDescription(string v)
        {
            throw new NotImplementedException();
        }

        private Uri ParseLink(string v)
        {
            throw new NotImplementedException();
        }
    }
}
