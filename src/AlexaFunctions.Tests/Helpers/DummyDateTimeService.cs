using System;
using AlexaFunctions.Interfaces;
using AlexaFunctions.Services;

namespace AlexaFunctions.Tests.Helpers
{
    public class DummyDateTimeService : BaseDateTimeService, IDateTimeService
    {
        private readonly DateTime DateTime;

        public DummyDateTimeService(DateTime dateTime)
        {
            DateTime = dateTime;
        }

        public override DateTime Now => DateTime;
    }
}
