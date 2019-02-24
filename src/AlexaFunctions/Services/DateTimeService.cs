using System;
using AlexaFunctions.Interfaces;

namespace AlexaFunctions.Services
{
    public class DateTimeService : BaseDateTimeService, IDateTimeService
    {
        public override DateTime Now => DateTime.Now;
    }
}
