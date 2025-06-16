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
    public readonly string _baseUrl = "http://192.168.1.2:8890/api/";
    public string BaseUrl => _baseUrl;


    public ApiClientService()
    {
        _httpClient = new HttpClient();
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

            Console.WriteLine($"📌 DEBUG JSON : {JsonConvert.SerializeObject(settings)}");

            string json = JsonConvert.SerializeObject(settings);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync($"{_baseUrl}csv-settings", content);

            Console.WriteLine($"📌 DEBUG REPONSE API : {response.StatusCode}");
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
            Console.WriteLine("❌ Erreur: Le fichier et le fournisseur sont requis.");
            throw new ArgumentException("Le fichier et le fournisseur sont requis.");
        }

        string apiUrl = $"{_baseUrl}archives-file/{supplierName}/";
        Console.WriteLine($"📡 Envoi du fichier {filePath} vers {apiUrl}");

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
                        Console.WriteLine($"📤 Réponse reçue du serveur : {response.StatusCode}");

                        // ✅ Cas d'erreur explicite
                        if (!response.IsSuccessStatusCode)
                        {
                            Console.WriteLine("❌ Erreur API : " + result);
                            return $"Erreur : {result}"; 
                        }

                        // ✅ Réponse OK - on continue
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


    public async Task<string> GetExtractionFilesAsync(string extractionId)
    {
        try
        {
            Console.WriteLine($"📡 DEBUG: Demande des fichiers CSV avec extraction ID: {extractionId}");

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

            List<string> fileList = filesArray.ToObject<List<string>>();
            if (fileList.Count == 0)
            {
                Console.WriteLine("❌ Aucun fichier disponible.");
                return null;
            }

            // 🔹 2. Télécharger le premier fichier via l'API
            string remoteFilePath = fileList[0];
            string downloadUrl = $"{_baseUrl}download-csv/?file_path={Uri.EscapeDataString(remoteFilePath)}";

            HttpResponseMessage fileResponse = await _httpClient.GetAsync(downloadUrl);

            if (!fileResponse.IsSuccessStatusCode)
            {
                Console.WriteLine($"❌ Erreur lors du téléchargement du fichier CSV : {fileResponse.StatusCode}");
                return null;
            }

            // 🔹 3. Sauvegarde temporaire en local
            string localCsvPath = Path.Combine(Path.GetTempPath(), Path.GetFileName(remoteFilePath));
            using (var fileStream = new FileStream(localCsvPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await fileResponse.Content.CopyToAsync(fileStream);
            }

            Console.WriteLine($"✅ Fichier CSV téléchargé en local : {localCsvPath}");
            return localCsvPath;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ ERREUR: {ex.Message}");
            return null;
        }
    }

    public async Task<string> DownloadFileToTempAsync(string remoteFilePath)
    {
        string downloadUrl = $"{_baseUrl}download-csv/?file_path={Uri.EscapeDataString(remoteFilePath)}";

        HttpResponseMessage response = await _httpClient.GetAsync(downloadUrl);
        if (!response.IsSuccessStatusCode)
            throw new Exception("Échec du téléchargement du fichier depuis l'API.");

        string localTempPath = Path.Combine(Path.GetTempPath(), Path.GetFileName(remoteFilePath));
        using (var stream = new FileStream(localTempPath, FileMode.Create, FileAccess.Write))
        {
            await response.Content.CopyToAsync(stream);
        }

        return localTempPath;
    }


}
