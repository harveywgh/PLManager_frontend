using Newtonsoft.Json;
using WPFModernVerticalMenu.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;


public class ApiClientService
{
    private readonly HttpClient _httpClient;
    public HttpClient HttpClient => _httpClient;
    private readonly string _baseUrl;
    public string BaseUrl => _baseUrl;


    public ApiClientService()
    {
        _httpClient = new HttpClient();
        var apiService = new ApiService();
        _baseUrl = apiService.BaseApiUrl;
    }

    public async Task<bool> SendCSVSettingsAsync(string country, string forwarder, string importer, string archive)
    {
        try
        {
            var settings = new
            {
                country_of_origin = country,
                forwarder = forwarder,
                importer = importer,
                archive = archive
            };

            string json = JsonConvert.SerializeObject(settings);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync($"{_baseUrl}csv-settings", content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erreur lors de l'envoi des paramètres CSV : {ex.Message}");
            return false;
        }
    }

    public async Task<string> UploadPackingListAsync(string filePath, string supplierName)
    {
        if (string.IsNullOrEmpty(filePath) || string.IsNullOrEmpty(supplierName))
        {
            throw new ArgumentException("Le fichier et le fournisseur sont requis.");
        }

        string apiUrl = $"{_baseUrl}archives-file/{supplierName}/";

        try
        {
            using (var formData = new MultipartFormDataContent())
            {
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                using (var streamContent = new StreamContent(fileStream))
                {
                    formData.Add(streamContent, "file", Path.GetFileName(filePath));

                    using (HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, formData))
                    {
                        string result = await response.Content.ReadAsStringAsync();

                        // ✅ Cas d'erreur explicite
                        if (!response.IsSuccessStatusCode)
                        {
                            string apiError = await response.Content.ReadAsStringAsync();
                            string errorMessage = "Erreur inconnue.";

                            try
                            {
                                var errorObj = Newtonsoft.Json.Linq.JObject.Parse(apiError);
                                errorMessage = errorObj["detail"]?.ToString() ?? apiError;
                            }
                            catch
                            {
                                errorMessage = apiError;
                            }

                            Console.WriteLine("❌ Erreur API : " + errorMessage);
                            throw new Exception(errorMessage); 
                        }

                        var jsonResponse = JsonConvert.DeserializeObject<Dictionary<string, object>>(result);

                        if (jsonResponse.ContainsKey("extraction_id"))
                        {
                            string extractionId = jsonResponse["extraction_id"].ToString();
                            Console.WriteLine($"✅ Extraction ID reçu : {extractionId}");
                            AppState.Instance.SetExtractionId(extractionId);

                            if (jsonResponse.ContainsKey("generated_files") &&
                                jsonResponse["generated_files"] is Newtonsoft.Json.Linq.JArray filesArray)
                            {
                                var fileList = filesArray.ToObject<List<string>>();
                                AppState.Instance.SetExtractedFiles(fileList);
                                Console.WriteLine($"✅ Fichiers associés : {string.Join(", ", fileList)}");
                            }

                            return extractionId;
                        }
                        else
                        {
                            Console.WriteLine("❌ ERREUR: extraction_id manquant dans la réponse API !");
                            return "Erreur : extraction_id introuvable dans la réponse";
                        }
                    }
                }
            }
        }
        catch (HttpRequestException ex)
        {
            return $"Erreur HTTP : {ex.Message}";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erreur d'envoi : {ex.Message}");
            return $"Erreur inattendue : {ex.Message}";
        }
    }


    public async Task<List<string>> GetExtractionFilesAsync(string extractionId)
    {
        try
        {

            // 🔹 1. Récupérer la liste des fichiers disponibles
            string apiUrl = $"{_baseUrl}get-extraction-files/{extractionId}/";
            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"❌ Erreur API: {response.StatusCode}");
                return null;
            }

            string jsonResponse = await response.Content.ReadAsStringAsync();
            var responseData = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonResponse);

            if (!responseData.ContainsKey("files") || !(responseData["files"] is Newtonsoft.Json.Linq.JArray filesArray))
            {
                Console.WriteLine("❌ Aucune liste de fichiers retournée.");
                return null;
            }

            List<string> remoteFileList = filesArray.ToObject<List<string>>();

            if (remoteFileList.Count == 0)
            {
                Console.WriteLine("❌ Aucun fichier disponible.");
                return null;
            }

            // ✅ Sauvegarder les chemins distants côté API (ex: outputs/ZestFruit/...)
            AppState.Instance.SetExtractedFiles(remoteFileList);

            // Optionnel : Télécharger les fichiers en local si tu veux les visualiser maintenant
            foreach (string remoteFilePath in remoteFileList)
            {
                string downloadUrl = $"{_baseUrl}download-csv/?file_path={Uri.EscapeDataString(remoteFilePath)}";
                HttpResponseMessage fileResponse = await _httpClient.GetAsync(downloadUrl);

                if (!fileResponse.IsSuccessStatusCode)
                {
                    Console.WriteLine($"❌ Erreur lors du téléchargement : {remoteFilePath} -> {fileResponse.StatusCode}");
                    continue;
                }

                string localPath = Path.Combine(Path.GetTempPath(), Path.GetFileName(remoteFilePath));
                using (var fs = new FileStream(localPath, FileMode.Create, FileAccess.Write))
                {
                    await fileResponse.Content.CopyToAsync(fs);
                }
            }

            return remoteFileList;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ ERREUR: {ex.Message}");
            return null;
        }
    }

    public async Task<Stream> DownloadFileAsStreamAsync(string remoteFilePath)
    {
        if (string.IsNullOrWhiteSpace(remoteFilePath) || remoteFilePath.Contains(":\\"))
            throw new Exception("❌ Chemin API invalide : chemin local détecté.");

        string downloadUrl = $"{_baseUrl}download-csv/?file_path={Uri.EscapeDataString(remoteFilePath)}";
        HttpResponseMessage response = await _httpClient.GetAsync(downloadUrl);

        if (!response.IsSuccessStatusCode)
            throw new Exception("Échec du téléchargement depuis l'API.");

        return await response.Content.ReadAsStreamAsync(); 
    }

}
