using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Windows.Media;
using System.Windows.Threading;

namespace WPFModernVerticalMenu.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly DispatcherTimer _timer;

        public string BaseApiUrl { get; } = "http://192.168.1.2:8890/api/";

        public event Action<string, SolidColorBrush> ApiStatusChanged;

        public ApiService()
        {
            _httpClient = new HttpClient();
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };
            _timer.Tick += async (s, e) => await CheckApiStatus();
            _timer.Start();
            _ = CheckApiStatus();
        }

        public async Task<string> UploadFileAsync(string filePath, string supplierCode)
        {
            if (string.IsNullOrEmpty(filePath) || string.IsNullOrEmpty(supplierCode))
                throw new ArgumentException("Le chemin du fichier et le fournisseur sont requis.");

            string apiUrl = $"{BaseApiUrl}archives-file/{supplierCode}/";

            try
            {
                var formData = new MultipartFormDataContent();
                formData.Add(new StreamContent(File.OpenRead(filePath)), "file", System.IO.Path.GetFileName(filePath));

                HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, formData);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Erreur réseau : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                ApiStatusChanged?.Invoke("❌ Connexion perdue avec l'API.", new SolidColorBrush(Colors.Red));
                return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                ApiStatusChanged?.Invoke($"❌ Erreur API: {ex.Message}", new SolidColorBrush(Colors.Red));
                return null;
            }
        }

        private const string HealthCheckEndpoint = "health-check";

        private async Task CheckApiStatus()
        {
            string fullUrl = $"{BaseApiUrl}{HealthCheckEndpoint}";

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(fullUrl);

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
