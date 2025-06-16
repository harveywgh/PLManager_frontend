using System;
using System.Collections.Generic;
using System.IO;
using WPFModernVerticalMenu.Model;

namespace WPFModernVerticalMenu.Services
{
    public class AppState
    {
        private static AppState _instance;
        public static AppState Instance => _instance ?? (_instance = new AppState());

        public string ExtractionId { get; private set; }
        public string ExtractedCsvPath { get; private set; }
        public string SelectedFile { get; private set; }
        public SupplierModel SelectedSupplier { get; set; }

        // ✅ Ajout des paramètres CSV
        public string SelectedCountry { get; private set; }
        public string SelectedForwarder { get; private set; }
        public string SelectedImporter { get; private set; }
        public string SelectedArchive { get; private set; }
        public List<string> ExtractedFiles { get; private set; }

        public event Action OnStateChanged;

        // ✅ Vérifie si un fichier est verrouillé avant de l'ajouter
        public void SetSelectedFile(string file)
        {
            if (!string.IsNullOrEmpty(file) && IsFileLocked(file))
            {
                Console.WriteLine("❌ Le fichier est utilisé par un autre processus.");
                return;
            }

            Console.WriteLine($"?? Enregistrement du fichier sélectionné : {file}");
            SelectedFile = file;
            OnStateChanged?.Invoke();
        }

        public void SetSelectedSupplier(SupplierModel supplier)
        {
            SelectedSupplier = supplier;
            OnStateChanged?.Invoke();
        }

        public void SetExtractedFiles(List<string> files)
        {
            ExtractedFiles = files;
            OnStateChanged?.Invoke();
        }

        public void SetExtractedCsvPath(string path)
        {
            ExtractedCsvPath = path;
            Console.WriteLine($"✅ Chemin CSV sauvegardé : {ExtractedCsvPath}");
        }

        // ✅ Nouvelle méthode pour définir les paramètres CSV
        public void SetCSVSettings(string country, string forwarder, string importer, string archive)
        {
            SelectedCountry = country;
            SelectedForwarder = forwarder;
            SelectedImporter = importer;
            Console.WriteLine($"📌 DEBUG APPSTATE : Importer={SelectedImporter}");
            SelectedArchive = archive;
            OnStateChanged?.Invoke();
        }

        // ✅ Vérifie si le fichier est verrouillé
        private bool IsFileLocked(string filePath)
        {
            FileStream stream = null;
            try
            {
                stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
                return false;
            }
            catch (IOException)
            {
                return true;
            }
            finally
            {
                stream?.Dispose();
            }
        }

        public void SetExtractionId(string extractionId)
        {
            ExtractionId = extractionId;
            Console.WriteLine($"📌 Extraction ID sauvegardé dans AppState: {ExtractionId}");
        }
    }
}
