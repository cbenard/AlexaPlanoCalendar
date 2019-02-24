using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using AlexaFunctions.Interfaces;
using AlexaFunctions.Models;

namespace AlexaFunctions.Services
{
    internal class CalendarService : ICalendarService
    {
        private const string CALENDAR_NAME_PREFIX = "Plano, TX - Calendar - ";
        private readonly Regex _timeRegex = new Regex(@"(?<Start>\d{2}:\d{2} (?:AM|PM)) \- (?<End>\d{2}:\d{2} (?:AM|PM))");
        private readonly Regex _descriptionRegex = new Regex(@"Description:\s*(?<Description>\S.*)");

        public IHttpService HttpService { get; }

        public CalendarService(IHttpService httpService)
        {
            HttpService = httpService;
        }

        // Poor man's dependency injection
        public CalendarService() : this(new HttpService())
        {
        }

        public async Task<Calendar> GetCalendar(CalendarType calendarType)
        {
            // Find the URL by calendar type
            string url = GetUrl(calendarType);

            // Grab the contents
            string contents = await HttpService.GetContents(url).ConfigureAwait(false);

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

        internal Calendar ParseContents(string contents)
        {
            XNamespace ns = "http://www.plano.gov/Calendar.aspx";
            XDocument doc = XDocument.Parse(contents);

            // Parse the calendar title and strip the prefix, if present
            string title = ParseCalendarName(doc?.Root?.Element("channel")?.Element("title")?.Value?.Trim());

            var items = doc?.Root?.Elements("channel")?.Elements("item");
            var list = new List<CalendarEntry>();

            if (items != null)
            {
                foreach (var item in items)
                {
                    string itemTitle = ParseEventName(item.Element("title")?.Value?.Trim());
                    string dateText = item.Element(ns + "EventDates")?.Value?.Trim();
                    string timeText = item.Element(ns + "EventTimes")?.Value?.Trim();
                    (DateTime? start, DateTime? end) = ParseDateText(dateText, timeText);
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
                Entries = list.OrderBy(x => x.Start).ToArray()
            };

            return calendar;
        }

        internal string ParseCalendarName(string title)
        {
            if (String.IsNullOrWhiteSpace(title))
            {
                return null;
            }

            if (title?.StartsWith(CALENDAR_NAME_PREFIX) == true
                && title?.Length > CALENDAR_NAME_PREFIX.Length)
            {
                title = title.Substring(CALENDAR_NAME_PREFIX.Length).Trim();
            }

            if (String.IsNullOrWhiteSpace(title))
            {
                return null;
            }

            return title;
        }

        internal string ParseEventName(string title)
        {
            if (String.IsNullOrWhiteSpace(title)
                || title.Contains("Cancellation", StringComparison.OrdinalIgnoreCase)
                || title.Contains("Cancelation", StringComparison.OrdinalIgnoreCase)
                || title.Contains("Cancelled", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return title.Trim();
        }

        internal (DateTime?, DateTime?) ParseDateText(string dateText, string timeText)
        {
            if (String.IsNullOrWhiteSpace(dateText))
            {
                return (null, null);
            }

            if (String.IsNullOrWhiteSpace(timeText))
            {
                return (null, null);
            }

            // No support for ranges currently
            if (dateText.Contains("-"))
            {
                return (null, null);
            }

            // Parsing string like: "09:30 AM - 10:00 AM"
            Match match = _timeRegex.Match(timeText);
            if (!match.Success)
            {
                return (null, null);
            }

            DateTime? start = null, end = null;

            // Parsing string like: " February 26, 2019 "
            if (DateTime.TryParse($"{dateText} {match.Groups["Start"].Value}",
                out DateTime parsedStartDate))
            {
                start = parsedStartDate;
            }

            if (DateTime.TryParse($"{dateText} {match.Groups["End"].Value}",
               out DateTime parsedEndDate))
            {
                end = parsedEndDate;
            }

            return (start, end);
        }

        internal string ParseDescription(string description)
        {
            // Parsing string like: "<strong>Event date:</strong> February 26, 2019 <br><strong>Event Time: </strong>09:30 AM - 10:00 AM<br><strong>Location:</strong> <br>2501 Coit Road<br>Plano, TX 75075<br><strong>Description:</strong><br>Songs, nursery rhymes and books provide a language-rich experience for the youngest child. Active parent/caregiver participation is a must! Ages 0-24 months"
            string stripped = Regex.Replace(description, "<.*?>", " ");
            Match match = _descriptionRegex.Match(stripped);
            if (match.Success)
            {
                return match.Groups["Description"].Value.Trim();
            }

            return null;
        }

        internal Uri ParseLink(string url)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
            {
                return uri;
            }

            return null;
        }
    }
}
