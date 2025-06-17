using PLManager.Windows;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using System.Windows;
using System;
using System.Windows.Controls;
using System.IO;
using WPFModernVerticalMenu.Services;

namespace WPFModernVerticalMenu.Pages
{
    public partial class LoadingPage : Page, INotifyPropertyChanged
    {
        private bool _isLoading = true;
        private bool _isSuccess = false;
        private bool _isError = false;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        public bool IsSuccess
        {
            get => _isSuccess;
            set
            {
                _isSuccess = value;
                OnPropertyChanged(nameof(IsSuccess));
            }
        }

        public bool IsError
        {
            get => _isError;
            set
            {
                _isError = value;
                OnPropertyChanged(nameof(IsError));
            }
        }

        public LoadingPage()
        {
            InitializeComponent();
            DataContext = this;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // ✅ Afficher l'animation de succès après traitement
        public async void ShowSuccessMessage()
        {
            await Task.Delay(1000); // Pause avant affichage

            Dispatcher.Invoke(() =>
            {
                IsLoading = false;
                IsSuccess = true;

                LoadingGrid.Visibility = Visibility.Collapsed;
                SuccessGrid.Visibility = Visibility.Visible;

                Storyboard animation = (Storyboard)FindResource("CheckAnimation");
                animation.Begin();
            });
        }

        public async void ShowErrorMessage(string message = null)
        {
            await Task.Delay(500); // petite pause pour l'effet

            Dispatcher.Invoke(() =>
            {
                IsLoading = false;
                IsError = true;

                LoadingGrid.Visibility = Visibility.Collapsed;
                ErrorGrid.Visibility = Visibility.Visible;

                // Optionnel : log ou afficher le message quelque part
                Console.WriteLine("❌ ERREUR : " + message);
            });
        }


        private async void btnVisualiser_Click(object sender, RoutedEventArgs e)
        {
            var extractedFiles = AppState.Instance.ExtractedFiles;

            if (extractedFiles == null || extractedFiles.Count == 0)
            {
                MessageBox.Show("Aucun fichier CSV extrait trouvé.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string remotePath = extractedFiles[0];

                if (string.IsNullOrWhiteSpace(remotePath) || remotePath.Contains(":\\"))
                {
                    MessageBox.Show("❌ Chemin API invalide : chemin local détecté au lieu d’un chemin distant (outputs/...).", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string tempCsvPath = await new ApiClientService().DownloadFileToTempAsync(remotePath);

                if (!File.Exists(tempCsvPath))
                {
                    MessageBox.Show("Le fichier CSV n'a pas pu être téléchargé.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // ✅ Ouvre l’éditeur en mode API avec le chemin original
                var editorWindow = new CSVEditorWindow(remotePath);
                editorWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement du fichier : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        // ✅ Retourner au tableau de bord
        private void btnDashboard_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.btnDashboard_Click(sender, e);
            }
        }

    }
}