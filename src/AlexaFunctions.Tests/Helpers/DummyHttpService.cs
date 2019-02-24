using System;
using System.IO;
using System.Threading.Tasks;
using AlexaFunctions.Interfaces;

namespace AlexaFunctions.Tests.Helpers
{
    public class DummyHttpService : IHttpService
    {
        private readonly string _filename;

        public DummyHttpService(string filename)
        {
            _filename = filename;
        }

        public Task<string> GetContents(string url)
        {
            return File.ReadAllTextAsync(Path.Combine("./Samples/", _filename));
        }
    }
}
