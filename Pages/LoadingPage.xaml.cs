using PLManager.Windows;
using System;
using System.IO;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
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
                // ✅ Utilise le chemin distant tel qu'il est dans outputs/
                string remoteApiPath = extractedFiles[0];

                Console.WriteLine($"📁 Chemin du fichier à éditer (distant) : {remoteApiPath}");

                // ✅ Ouvre l’éditeur avec ce chemin — ce sera lui qui fera l’appel API et chargera le contenu
                var editorWindow = new CSVEditorWindow(); 
                await editorWindow.LoadCsvFiles(AppState.Instance.ExtractedFiles);
                editorWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Erreur lors de l'ouverture de l'éditeur : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                Console.WriteLine("❌ Exception dans btnVisualiser_Click : " + ex);
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
