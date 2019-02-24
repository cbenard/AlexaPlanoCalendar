using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlexaFunctions.Interfaces;
using AlexaFunctions.Services;
using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AlexaFunctions
{
    public class Function
    {
        public ICalendarService CalendarService { get; }

        public Function(ICalendarService calendarService)
        {
            CalendarService = calendarService;
        }

        // Poor man's dependency injection
        public Function() : this(new CalendarService())
        {
        }

        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public string FunctionHandler(string input, ILambdaContext context)
        {
            return input?.ToUpper();
        }
    }
}
