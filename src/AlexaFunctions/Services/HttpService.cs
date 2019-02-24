using System;
using System.Net.Http;
using System.Threading.Tasks;
using AlexaFunctions.Interfaces;

namespace AlexaFunctions.Services
{
    internal class HttpService : IHttpService
    {
        public async Task<string> GetContents(string url)
        {
            var client = new HttpClient();
            string contents = await client.GetStringAsync(url).ConfigureAwait(false);

            return contents;
        }
    }
}
