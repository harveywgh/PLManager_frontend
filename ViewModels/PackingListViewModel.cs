<<<<<<< Updated upstream
﻿using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using WPFModernVerticalMenu.Helpers;
=======
﻿using System;
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
>>>>>>> Stashed changes

namespace WPFModernVerticalMenu.ViewModels
{
    public class PackingListViewModel : INotifyPropertyChanged
    {
        private string _selectedFile;
        private bool _isFileUploaded;

        public string SelectedFile
        {
<<<<<<< Updated upstream
            get => _selectedFile;
            set
            {
                _selectedFile = value;
                OnPropertyChanged(nameof(SelectedFile));
            }
=======
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
>>>>>>> Stashed changes
        }

        public bool IsFileUploaded
        {
            get => _isFileUploaded;
            set
            {
                _isFileUploaded = value;
                OnPropertyChanged(nameof(IsFileUploaded));
            }
        }

        public ICommand SelectFileCommand { get; }
        public ICommand GoToNextPageCommand { get; }
<<<<<<< Updated upstream
=======
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
>>>>>>> Stashed changes

        public PackingListViewModel()
        {
            SelectFileCommand = new RelayCommand<object>(param => SelectFile());
            GoToNextPageCommand = new RelayCommand<object>(param => GoToNextPage());
        }

        private void SelectFile()
        {
            OpenFileDialog dlg = new OpenFileDialog
            {
                Filter = "Excel Files (*.xls;*.xlsx)|*.xls;*.xlsx"
            };

            if (dlg.ShowDialog() == true)
            {
<<<<<<< Updated upstream
                SelectedFile = dlg.FileName;
                IsFileUploaded = true;
                MessageBox.Show("Fichier importé avec succès!", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void GoToNextPage()
        {
            Application.Current.MainWindow.Content = new WPFModernVerticalMenu.Pages.SelectSupplier();
=======
                if (IsFileLocked(dlg.FileName))
                {
                    UpdateStatus("❌ Le fichier est en cours d'utilisation par un autre programme.", "Red");
                    return;
                }

                Console.WriteLine($"📂 Enregistrement du fichier sélectionné : {dlg.FileName}");

                AppState.Instance.SetSelectedFile(dlg.FileName);  // 🔥 Assure-toi que c'est bien sauvegardé
                Console.WriteLine($"✅ DEBUG: Fichier enregistré dans AppState: {AppState.Instance.SelectedFile}");
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
                        var parsed = JsonConvert.DeserializeObject<Dictionary<string, object>>(response);

                        if (parsed.ContainsKey("generated_files") && parsed["generated_files"] is Newtonsoft.Json.Linq.JArray filesArray)
                        {
                            var fileList = filesArray.ToObject<List<string>>();
                            if (fileList?.Count > 0)
                            {
                                AppState.Instance.SetExtractedFiles(fileList);

                                string relativePath = fileList[0].Replace("/", "\\");
                                string absolutePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);
                                AppState.Instance.SetExtractedCsvPath(absolutePath);
                                Console.WriteLine($"✅ Chemin CSV enregistré dans AppState : {absolutePath}");
                            }
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
                        UpdateStatus("❌ L'extraction a échoué.", "Red");
                        loadingPage.ShowErrorMessage($"❌ L'extraction a échoué : {response}");
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
                UpdateStatus($"❌ Erreur lors de l'envoi : {ex.Message}", "Red");

                if (Application.Current.MainWindow is MainWindow mainWindow &&
                    mainWindow.fContainer.Content is LoadingPage loadingPage)
                {
                    loadingPage.ShowErrorMessage("❌ Erreur inattendue : " + ex.Message);
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
>>>>>>> Stashed changes
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
