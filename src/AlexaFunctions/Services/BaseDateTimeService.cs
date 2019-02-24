using System;
using System.Linq;
using AlexaFunctions.Interfaces;

namespace AlexaFunctions.Services
{
    public abstract class BaseDateTimeService : IDateTimeService
    {
        private const string CENTRAL_TIMEZONE_WINDOWS = "Central Standard Time";
        private const string CENTRAL_TIMEZONE_UNIX = "America/Chicago";
        // Workaround from https://stackoverflow.com/a/51315221/448 for cross platform
        private readonly TimeZoneInfo _centralTimeZone =
            TimeZoneInfo.GetSystemTimeZones().Any(x => x.Id == CENTRAL_TIMEZONE_WINDOWS) ?
            TimeZoneInfo.FindSystemTimeZoneById(CENTRAL_TIMEZONE_WINDOWS) :
            TimeZoneInfo.FindSystemTimeZoneById(CENTRAL_TIMEZONE_UNIX);

        public abstract DateTime Now { get; }
        public DateTime NowCentral => TimeZoneInfo.ConvertTime(Now, _centralTimeZone);
    }
}
