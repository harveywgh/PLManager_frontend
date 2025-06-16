using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace WPFModernVerticalMenu.Services
{
    public class FileUploadService
    {
        private readonly HttpClient _httpClient;

        public FileUploadService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<string> UploadFileAsync(string filePath, string apiUrl)
        {
            Console.WriteLine($"📂 Tentative d'envoi du fichier : {filePath}");
            Console.WriteLine($"🌐 URL de destination : {apiUrl}");

            if (string.IsNullOrEmpty(filePath) || string.IsNullOrEmpty(apiUrl))
            {
                Console.WriteLine("❌ Erreur : Le fichier ou l'URL de l'API est manquant !");
                throw new ArgumentException("Le fichier et l'URL de l'API sont requis.");
            }

            try
            {
                using (var formData = new MultipartFormDataContent())
                using (var fileStream = File.OpenRead(filePath))
                {
                    formData.Add(new StreamContent(fileStream), "file", Path.GetFileName(filePath));

                    var response = await _httpClient.PostAsync(apiUrl, formData);

                    response.EnsureSuccessStatusCode();
                    string result = await response.Content.ReadAsStringAsync();

                    return result;
                }
            }
            catch (HttpRequestException httpEx)
            {
                return $"Erreur de connexion : {httpEx.Message}";
            }
            catch (Exception ex)
            {
                return $"Erreur lors de l'envoi du fichier : {ex.Message}";
            }
        }
    }
}
