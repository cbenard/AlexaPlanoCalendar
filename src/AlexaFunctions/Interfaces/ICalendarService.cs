using System;
using System.Threading.Tasks;
using AlexaFunctions.Models;

namespace AlexaFunctions.Interfaces
{
    public interface ICalendarService
    {
        Task<Calendar> GetCalendar(CalendarType calendarType);
    }
}
