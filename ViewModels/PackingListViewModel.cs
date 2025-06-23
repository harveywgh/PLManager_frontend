using System;
using System.Windows;
using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Win32;
using WPFModernVerticalMenu.Helpers;
using WPFModernVerticalMenu.Services;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using WPFModernVerticalMenu.Pages;
using System.Collections.Generic;
using System.Linq;

namespace WPFModernVerticalMenu.ViewModels
{
    public class PackingListViewModel : INotifyPropertyChanged
    {
        private bool _isFileUploaded;
        private string _statusMessage;
        private string _statusColor = "Black";
        private string _fileName;
        private readonly ApiClientService _apiClientService;

        public event EventHandler NavigateToSelectSupplier;
        public event EventHandler NavigateToLoadingPage;
        public event EventHandler CloseLoadingPage;

        public PackingListViewModel()
        {
            _apiClientService = new ApiClientService();
            SelectFileCommand = new RelayCommand(SelectFile);
            RemoveFileCommand = new RelayCommand(RemoveFile, () => IsFileUploaded);
            GoToNextPageCommand = new RelayCommand(OnGoToNextPage);
            UploadFileCommand = new RelayCommand(async () => await UploadPackingListAsync(), () => IsFileUploaded);
            ExtractCommand = new RelayCommand(async () =>
            {
                await ExtractPackingListAsync(
                    AppState.Instance.SelectedCountry,
                    AppState.Instance.SelectedForwarder,
                    AppState.Instance.SelectedImporter,
                    AppState.Instance.SelectedArchive
                );
            }, () => true);
        }

        // ✅ Vérifie si un fichier est importé
        public bool IsFileUploaded
        {
            get => _isFileUploaded;
            set
            {
                _isFileUploaded = value;
                OnPropertyChanged(nameof(IsFileUploaded));
                OnPropertyChanged(nameof(IsFileSelected)); // Active/désactive le bouton de suppression
            }
        }

        public bool IsFileSelected => !string.IsNullOrEmpty(FileName);

        // ✅ Nom du fichier importé
        public string FileName
        {
            get => _fileName;
            set
            {
                _fileName = value;
                OnPropertyChanged(nameof(FileName));
                OnPropertyChanged(nameof(IsFileSelected));
            }
        }

        // ✅ Statut du processus (Importation / Erreur / Envoi)
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(nameof(StatusMessage)); }
        }

        public string StatusColor
        {
            get => _statusColor;
            set
            {
                _statusColor = string.IsNullOrEmpty(value) ? "Black" : value;
                OnPropertyChanged(nameof(StatusColor));
            }
        }

        // ✅ Commandes pour les actions utilisateur
        public ICommand SelectFileCommand { get; }
        public ICommand RemoveFileCommand { get; }
        public ICommand GoToNextPageCommand { get; }
        public ICommand UploadFileCommand { get; }
        public ICommand ExtractCommand { get; }


        // ✅ Vérifie si un fichier est verrouillé
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

        // ✅ Sélection d'un fichier via la boîte de dialogue
        public void SelectFile()
        {
            OpenFileDialog dlg = new OpenFileDialog { Filter = "Excel Files (*.xls;*.xlsx)|*.xls;*.xlsx" };

            if (dlg.ShowDialog() == true)
            {
                if (IsFileLocked(dlg.FileName))
                {
                    UpdateStatus("❌ Le fichier est en cours d'utilisation par un autre programme.", "Red");
                    return;
                }

                AppState.Instance.SetSelectedFile(dlg.FileName);
                FileName = Path.GetFileName(dlg.FileName);
                IsFileUploaded = true;
                UpdateStatus("✅ Fichier importé avec succès!", "Green");
            }
        }


        // ✅ Suppression du fichier sélectionné
        public void RemoveFile()
        {
            AppState.Instance.SetSelectedFile(null);
            FileName = null;
            IsFileUploaded = false;
            UpdateStatus("Aucun fichier sélectionné.", "Red");
        }

        // ✅ Vérifie si un fichier a été importé avant d'aller à la prochaine étape
        public void OnGoToNextPage()
        {
            if (!IsFileUploaded)
            {
                UpdateStatus("Veuillez importer une Packing List avant de continuer.", "Red");
                return;
            }

            NavigateToSelectSupplier?.Invoke(this, EventArgs.Empty);
        }

        private (string, string, string, string) GetCSVSettings()
        {
            return (
                AppState.Instance.SelectedCountry ?? "Non spécifié",
                AppState.Instance.SelectedForwarder ?? "Non spécifié",
                AppState.Instance.SelectedImporter ?? "Non spécifié",
                AppState.Instance.SelectedArchive ?? "Non"
            );
        }

        // ✅ Méthode pour envoyer les paramètres puis lancer l’extraction.
        public async Task<bool> ExtractPackingListAsync(string country, string forwarder, string importer, string archive)
        {
            Console.WriteLine($"📌 DEBUG ENVOI API : Country={country}, Forwarder={forwarder}, Importer={importer}, Archive={archive}");

            if (string.IsNullOrEmpty(AppState.Instance.SelectedFile) || AppState.Instance.SelectedSupplier == null)
            {
                UpdateStatus("⚠️ Veuillez sélectionner un fichier et un fournisseur avant d'extraire.", "Red");
                return false;
            }

            bool success = await _apiClientService.SendCSVSettingsAsync(country, forwarder, importer, archive);
            if (!success)
            {
                UpdateStatus("❌ Échec de l’envoi des paramètres CSV.", "Red");
                return false;
            }

            UpdateStatus("✅ Paramètres enregistrés. Début de l’extraction...", "Green");

            try
            {
                string response = await _apiClientService.UploadPackingListAsync(
                    AppState.Instance.SelectedFile,
                    AppState.Instance.SelectedSupplier.Code
                );

                if (string.IsNullOrEmpty(response) || response.StartsWith("Erreur") || response.Contains("500"))
                {
                    UpdateStatus($"❌ Erreur extraction : {response}", "Red");
                    return false;
                }
                else
                {
                    // ✅ Extraction réussie — on essaye de parser et sauvegarder le chemin CSV
                    try
                    {
                        // Ne pas parser ici : on les récupère proprement avec GetExtractionFilesAsync
                        string extractionId = response;
                        AppState.Instance.SetExtractionId(extractionId);

                        // Téléchargement des fichiers associés
                        var fileList = await _apiClientService.GetExtractionFilesAsync(extractionId);
                        if (fileList != null && fileList.Count > 0)
                        {
                            AppState.Instance.SetExtractedFiles(fileList);
                            Console.WriteLine($"✅ Fichiers associés : {string.Join(", ", fileList)}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Impossible de parser la réponse API : {ex.Message}");
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"❌ Erreur inattendue : {ex.Message}", "Red");
                return false;
            }
        }

        // ✅ Envoi du fichier à l'API et affichage du statut
        public async Task UploadPackingListAsync()
        {
            if (string.IsNullOrEmpty(AppState.Instance.SelectedFile) || AppState.Instance.SelectedSupplier == null)
            {
                UpdateStatus("Veuillez sélectionner un fournisseur avant d'envoyer le fichier.", "Red");
                return;
            }

            try
            {
                NavigateToLoadingPage?.Invoke(this, EventArgs.Empty); // ✅ Ouvre la page de chargement

                string response = await _apiClientService.UploadPackingListAsync(
                    AppState.Instance.SelectedFile,
                    AppState.Instance.SelectedSupplier.Code
                );

                if (Application.Current.MainWindow is MainWindow mainWindow &&
                    mainWindow.fContainer.Content is LoadingPage loadingPage)
                {
                    if (string.IsNullOrEmpty(response) || response.StartsWith("Erreur") || response.Contains("500"))
                    {
                        // ❌ Erreur détectée dans la réponse
                        UpdateStatus("L'extraction a échoué.", "Red");
                        loadingPage.ShowErrorMessage($"L'extraction a échoué : {response}");
                    }
                    else
                    {
                        // ✅ Succès
                        UpdateStatus("✅ Fichier envoyé avec succès!", "Green");
                        loadingPage.ShowSuccessMessage();
                    }
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Erreur lors de l'envoi : {ex.Message}", "Red");

                if (Application.Current.MainWindow is MainWindow mainWindow &&
                    mainWindow.fContainer.Content is LoadingPage loadingPage)
                {
                    loadingPage.ShowErrorMessage("Erreur inattendue : " + ex.Message);
                }
            }
            finally
            {
                CloseLoadingPage?.Invoke(this, EventArgs.Empty);
            }
        }


        // ✅ Met à jour l'affichage du statut
        public void UpdateStatus(string message, string color)
        {
            StatusMessage = message;
            StatusColor = color;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
