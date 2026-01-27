using Microsoft.AspNetCore.Http;

namespace PizzaApp.Services
{
    public interface IFileService
    {
        // Zwraca URL do wgranego pliku
        Task<string> UploadFileAsync(IFormFile file, string folderName);

        // Usuwa plik na podstawie URL
        Task DeleteFileAsync(string fileUrl);
    }
}