using System.Threading.Tasks;

namespace AlexaFunctions.Interfaces
{
    public interface IHttpService
    {
        Task<string> GetContents(string url);
    }
}