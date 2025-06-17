using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace EventEaseBookingSystem.Services
{
    public interface IAzureBlobStorageService
    {
        Task<string> UploadFileAsync(IFormFile file);
    }
}
