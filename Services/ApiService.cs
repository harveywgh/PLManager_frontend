using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace WPFModernVerticalMenu.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl = "http://192.168.1.2:8890/api/health-check"; 
        private readonly DispatcherTimer _timer;

        public event Action<string, SolidColorBrush> ApiStatusChanged;
>>>>>>> Stashed changes

        public ApiService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<string> UploadFileAsync(string filePath, string supplierCode)
        {
            if (string.IsNullOrEmpty(filePath) || string.IsNullOrEmpty(supplierCode))
            {
                throw new ArgumentException("Le chemin du fichier et le fournisseur sont requis.");
            }

            using (var client = new HttpClient()) { }
            var formData = new MultipartFormDataContent();
            formData.Add(new StreamContent(File.OpenRead(filePath)), "file", Path.GetFileName(filePath));

            var apiUrl = $"http://127.0.0.1:8000/api/archives-file/{supplierCode}/";

            try
            {
<<<<<<< Updated upstream
                HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, formData);
                response.EnsureSuccessStatusCode();

                string result = await response.Content.ReadAsStringAsync();
                return result;
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Erreur réseau : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
=======
                HttpResponseMessage response = await _httpClient.GetAsync(_apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    ApiStatusChanged?.Invoke("✅ API OK", new SolidColorBrush(Colors.Green));
                }
                else
                {
                    ApiStatusChanged?.Invoke($"❌ API Erreur ({response.StatusCode})", new SolidColorBrush(Colors.Red));
                }
            }
            catch (HttpRequestException)
            {
                ApiStatusChanged?.Invoke("❌ Connexion perdue avec l'API.", new SolidColorBrush(Colors.Red));
            }
            catch (Exception ex)
            {
                ApiStatusChanged?.Invoke($"❌ Erreur API: {ex.Message}", new SolidColorBrush(Colors.Red));
            }
        }

    }
}
