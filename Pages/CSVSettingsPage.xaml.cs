using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WPFModernVerticalMenu;
using WPFModernVerticalMenu.Pages;
using WPFModernVerticalMenu.Services;
using WPFModernVerticalMenu.ViewModels;

namespace PLManager.Pages
{
    public partial class CSVSettingsPage : Page
    {
        private readonly PackingListViewModel _viewModel;

        // ✅ Définition des listes pour chaque ComboBox
        public List<string> CountryList { get; set; }
        public List<string> ForwarderList { get; set; }
        public List<string> ImporterList { get; set; }
        public List<string> ArchiveList { get; set; }
        public List<string> PackagingList { get; set; }      
        public List<string> PackagingTypeList { get; set; }    
        public List<string> CustomList1 { get; set; }      
        public List<string> CustomList2 { get; set; }        
        public string SelectedImporter { get; set; }


        public CSVSettingsPage()
        {
            InitializeComponent();
            _viewModel = new PackingListViewModel();
            DataContext = this; 

            // ✅ Initialisation des listes dynamiques
            CountryList = new List<string> { "ZA", "BR", "MA", "PE", "EG", "US", "IN", "ZW", "KE", "UY", "CO" };           ForwarderList = new List<string> { "COOL CONTROL", "VDH", "LBP", "SEALOGIS", "GATE 4 EU"};
            ImporterList = new List<string> { };
            ArchiveList = new List<string> { };

            // ✅ Initialisation des nouvelles listes
            PackagingList = new List<string> { };
            PackagingTypeList = new List<string> { };
            CustomList1 = new List<string> { };
            CustomList2 = new List<string> { };
        }

        private void cbCountry_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbCountry.SelectedItem != null)
            {
                string selectedCountry = cbCountry.SelectedItem.ToString();
                Console.WriteLine($"📌 Pays sélectionné : {selectedCountry}");

                AppState.Instance.SetCSVSettings(
                    selectedCountry,
                    AppState.Instance.SelectedForwarder,
                    AppState.Instance.SelectedImporter,
                    AppState.Instance.SelectedArchive
                );

                ValidateForm();
            }
        }

        private void cbForwarder_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbForwarder.SelectedItem != null)
            {
                string selectedForwarder = cbForwarder.SelectedItem.ToString();
                Console.WriteLine($"📌 Transitaire sélectionné : {selectedForwarder}");

                AppState.Instance.SetCSVSettings(
                    AppState.Instance.SelectedCountry,
                    selectedForwarder,
                    AppState.Instance.SelectedImporter,
                    AppState.Instance.SelectedArchive
                );

                ValidateForm();
            }
        }
        private void cbImporter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbImporter.SelectedItem != null)
            {
                string importer = cbImporter.SelectedItem.ToString();
                Console.WriteLine($"📌 Importateur sélectionné : {importer}");

                AppState.Instance.SetCSVSettings(
                    AppState.Instance.SelectedCountry,
                    AppState.Instance.SelectedForwarder,
                    importer,  
                    AppState.Instance.SelectedArchive
                );

                ValidateForm();
            }
        }

        private void cbArchive_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbArchive.SelectedItem != null)
            {
                string archive = cbArchive.SelectedItem.ToString();
                Console.WriteLine($"📌 Archivage sélectionné : {archive}");

                AppState.Instance.SetCSVSettings(
                    AppState.Instance.SelectedCountry,
                    AppState.Instance.SelectedForwarder,
                    AppState.Instance.SelectedImporter,
                    archive
                );

                ValidateForm();
            }
        }

        // ✅ Ajout des nouvelles méthodes pour gérer les nouveaux paramètres
        private void cbPackaging_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbPackaging.SelectedItem != null)
            {
                Console.WriteLine($"📌 Packaging sélectionné : {cbPackaging.SelectedItem}");
                ValidateForm();
            }
        }

        private void cbPackagingType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbPackagingType.SelectedItem != null)
            {
                Console.WriteLine($"📌 Type de Packaging sélectionné : {cbPackagingType.SelectedItem}");
                ValidateForm();
            }
        }

        private void cbCustom1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbCustom1.SelectedItem != null)
            {
                Console.WriteLine($"📌 Paramètre 1 sélectionné : {cbCustom1.SelectedItem}");
                ValidateForm();
            }
        }

        private void cbCustom2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbCustom2.SelectedItem != null)
            {
                Console.WriteLine($"📌 Paramètre 2 sélectionné : {cbCustom2.SelectedItem}");
                ValidateForm();
            }
        }

        private void ComboBox_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox != null && !comboBox.IsDropDownOpen)
            {
                comboBox.Focus();
                comboBox.IsDropDownOpen = true;
                e.Handled = true;
            }
        }

        private void ValidateForm()
        {
            // Champs obligatoires
            bool isValid = cbCountry.SelectedItem != null
                           && cbForwarder.SelectedItem != null;

            btnExtract.IsEnabled = isValid;
        }

        private void GoBack_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

        private async void Extract_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string country = cbCountry.SelectedItem?.ToString() ?? throw new Exception("Le pays d'origine est obligatoire !");
                string forwarder = cbForwarder.SelectedItem?.ToString() ?? throw new Exception("Le transitaire est obligatoire !");
                string importer = cbImporter.SelectedItem?.ToString() ?? "";  
                string archive = cbArchive.SelectedItem?.ToString() ?? "";  


                Console.WriteLine($"📌 DEBUG AVANT ENVOI : Country={country}, Forwarder={forwarder}, Importer={importer}, Archive={archive}");

                string packaging = cbPackaging.SelectedItem?.ToString() ?? "Non spécifié";
                string packagingType = cbPackagingType.SelectedItem?.ToString() ?? "Non spécifié";
                string custom1 = cbCustom1.SelectedItem?.ToString() ?? "Non spécifié";
                string custom2 = cbCustom2.SelectedItem?.ToString() ?? "Non spécifié";

                var settings = new
                {
                    country_of_origin = country,
                    forwarder = forwarder,
                    importer = importer,
                    archive = archive,
                    packaging = packaging,
                    packaging_type = packagingType,
                    custom1 = custom1,
                    custom2 = custom2
                };

                bool result = await _viewModel.ExtractPackingListAsync(country, forwarder, importer, archive);

                if (Application.Current.MainWindow is MainWindow mainWindow)
                {
                    mainWindow.NavigateToLoadingPage(this, EventArgs.Empty);
                    await Task.Delay(500);

                    if (mainWindow.fContainer.Content is LoadingPage loadingPage)
                    {
                        if (result)
                            loadingPage.ShowSuccessMessage();
                        else
                            loadingPage.ShowErrorMessage("❌ L'extraction a échoué. Vérifiez le fichier ou les colonnes manquantes.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur : {ex.Message}", "Validation des paramètres", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
