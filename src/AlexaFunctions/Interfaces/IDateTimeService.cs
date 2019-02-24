using System;
namespace AlexaFunctions.Interfaces
{
    public interface IDateTimeService
    {
        DateTime Now { get; }
        DateTime NowCentral { get; }
    }
}
