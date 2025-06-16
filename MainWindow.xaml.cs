using System.IO;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
<<<<<<< Updated upstream
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
=======
using WPFModernVerticalMenu.Services;
using WPFModernVerticalMenu.ViewModels;
using PL_Manager.Pages;
using PLManager.Pages;
using PLManager.Windows;

namespace WPFModernVerticalMenu
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
<<<<<<< Updated upstream
        public MainWindow()
        {
            InitializeComponent();
=======
        private readonly ApiService _apiService;
        private string _apiStatusText = "API NOT OK";
        private SolidColorBrush _apiStatusColor = new SolidColorBrush(Colors.Red);


        public event PropertyChangedEventHandler PropertyChanged;

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        // ✅ Propriétés du statut de l'API
        public string ApiStatusText
        {
            get => _apiStatusText;
            set
            {
                if (_apiStatusText != value)
                {
                    _apiStatusText = value;
                    OnPropertyChanged(nameof(ApiStatusText));
                }
            }
        }

        public SolidColorBrush ApiStatusColor
        {
            get => _apiStatusColor;
            set
            {
                if (_apiStatusColor != value)
                {
                    _apiStatusColor = value;
                    OnPropertyChanged(nameof(ApiStatusColor));
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            // 🔗 Initialisation du service API
            _apiService = new ApiService();
            _apiService.ApiStatusChanged += UpdateApiStatus;

            // 🔗 Connexion aux événements du ViewModel
            if (DataContext is PackingListViewModel viewModel)
            {
                viewModel.NavigateToSelectSupplier += btnSelectSupplier_Click;
                viewModel.NavigateToLoadingPage += NavigateToLoadingPage;
                viewModel.CloseLoadingPage += CloseLoadingPage;
            }
        }


        // ✅ Met à jour l'affichage du statut API
        private void UpdateApiStatus(string statusText, SolidColorBrush statusColor)
        {
            Dispatcher.Invoke(() =>
            {
                ApiStatusText = statusText;
                ApiStatusColor = statusColor;
                OnPropertyChanged(nameof(ApiStatusText)); 
                OnPropertyChanged(nameof(ApiStatusColor)); 
            });
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void NavigateToSelectSupplier(object sender, EventArgs e)
        {
            fContainer.Navigate(new Uri("Pages/SelectSupplier.xaml", UriKind.Relative));
>>>>>>> Stashed changes
        }

        private void BG_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Tg_Btn.IsChecked = false;
        }

<<<<<<< Updated upstream
        // Start: MenuLeft PopupButton //
        private void btnHome_MouseEnter(object sender, MouseEventArgs e)
=======
        // ✅ Gestion des boutons du menu
        private void btnHome_MouseEnter(object sender, MouseEventArgs e) => ShowPopup(btnHome, "Tableau de bord");
        private void btnDashboard_MouseEnter(object sender, MouseEventArgs e) => ShowPopup(btnDashboard, "Extracteur PL");
        private void btnLocalEditor_MouseEnter(object sender, MouseEventArgs e) => ShowPopup(btnLocalEditor, "Éditeur Local");
        private void btnProductStock_MouseEnter(object sender, MouseEventArgs e) => ShowPopup(btnProductStock, "Réglages");
        private void btnOrderList_MouseEnter(object sender, MouseEventArgs e) => ShowPopup(btnOrderList, "Aide");
        private void btnApiStatus_MouseEnter(object sender, MouseEventArgs e)
>>>>>>> Stashed changes
        {
            if (Tg_Btn.IsChecked == false)
            {
                Popup.PlacementTarget = btnHome;
                Popup.Placement = PlacementMode.Right;
                Popup.IsOpen = true;
                Header.PopupText.Text = ApiStatusText;
            }
        }

        private void btnHome_MouseLeave(object sender, MouseEventArgs e)
        {
            Popup.Visibility = Visibility.Collapsed;
            Popup.IsOpen = false;
        }

        private void btnDashboard_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Tg_Btn.IsChecked == false)
            {
                Popup.PlacementTarget = btnDashboard;
                Popup.Placement = PlacementMode.Right;
                Popup.IsOpen = true;
                Header.PopupText.Text = "Extracteur PL";
            }
        }

        private void btnDashboard_MouseLeave(object sender, MouseEventArgs e)
        {
            Popup.Visibility = Visibility.Collapsed;
            Popup.IsOpen = false;
        }

<<<<<<< Updated upstream
        private void btnProducts_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Tg_Btn.IsChecked == false)
            {
                Popup.PlacementTarget = btnProducts;
                Popup.Placement = PlacementMode.Right;
                Popup.IsOpen = true;
                Header.PopupText.Text = "FAQ";
            }
        }
=======
        private void btnHome_MouseLeave(object sender, MouseEventArgs e) => HidePopup(sender, e);
        private void btnDashboard_MouseLeave(object sender, MouseEventArgs e) => HidePopup(sender, e);
        private void btnLocalEditor_MouseLeave(object sender, MouseEventArgs e) => HidePopup(sender, e);
        private void btnProducts_MouseLeave(object sender, MouseEventArgs e) => HidePopup(sender, e);
        private void btnProductStock_MouseLeave(object sender, MouseEventArgs e) => HidePopup(sender, e);
        private void btnOrderList_MouseLeave(object sender, MouseEventArgs e) => HidePopup(sender, e);
        private void btnSetting_MouseLeave(object sender, MouseEventArgs e) => HidePopup(sender, e);
>>>>>>> Stashed changes

        private void btnProducts_MouseLeave(object sender, MouseEventArgs e)
        {
            Popup.Visibility = Visibility.Collapsed;
            Popup.IsOpen = false;
        }

        private void btnProductStock_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Tg_Btn.IsChecked == false)
            {
                Popup.PlacementTarget = btnProductStock;
                Popup.Placement = PlacementMode.Right;
                Popup.IsOpen = true;
                Header.PopupText.Text = "Réglages";
            }
        }

        private void btnProductStock_MouseLeave(object sender, MouseEventArgs e)
        {
            Popup.Visibility = Visibility.Collapsed;
            Popup.IsOpen = false;
        }

        private void btnOrderList_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Tg_Btn.IsChecked == false)
            {
                Popup.PlacementTarget = btnOrderList;
                Popup.Placement = PlacementMode.Right;
                Popup.IsOpen = true;
                Header.PopupText.Text = "Aide";
            }
        }

        private void btnOrderList_MouseLeave(object sender, MouseEventArgs e)
        {
            Popup.Visibility = Visibility.Collapsed;
            Popup.IsOpen = false;
        }
        private void btnSetting_MouseLeave(object sender, MouseEventArgs e)
        {
            Popup.Visibility = Visibility.Collapsed;
            Popup.IsOpen = false;
        }
        // End: MenuLeft PopupButton //

        // Start: Button Close | Restore | Minimize 
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnRestore_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
                WindowState = WindowState.Maximized;
            else
                WindowState = WindowState.Normal;
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        // End: Button Close | Restore | Minimize

        private void btnHome_Click(object sender, RoutedEventArgs e)
        {
            fContainer.Navigate(new System.Uri("Pages/Home.xaml", UriKind.RelativeOrAbsolute));
        }

        private void btnDashboard_Click(object sender, RoutedEventArgs e)
        {
            fContainer.Navigate(new System.Uri("Pages/Extractor.xaml", UriKind.RelativeOrAbsolute));
        }

<<<<<<< Updated upstream
        private void MenuItem_Loaded(object sender, RoutedEventArgs e)
=======
        private void btnLocalEditor_Click(object sender, RoutedEventArgs e)
        {
            var win = new PLManager.Windows.EditorWindow();
            win.Show(); 
        }

        private void btnLocalEditor_Click(object sender, RoutedEventArgs e)
        {
            var win = new PLManager.Windows.EditorWindow();
            win.Show(); 
        }


        // ✅ Navigation dans l'application
        public void btnSelectSupplier_Click(object sender, EventArgs e)
>>>>>>> Stashed changes
        {

<<<<<<< Updated upstream
=======
        public void GoToCSVSettings_Click(object sender, EventArgs e)
        {
            fContainer.Navigate(new Uri("Pages/CSVSettingsPage.xaml", UriKind.Relative));
        }

        public void GoToCSVSettings_Click(object sender, EventArgs e)
        {
            fContainer.Navigate(new Uri("Pages/CSVSettingsPage.xaml", UriKind.Relative));
        }

        public void NavigateToLoadingPage(object sender, EventArgs e)
        {
            Console.WriteLine("🔹 Changement vers la Loading Page.");
            fContainer.Navigate(new Uri("Pages/LoadingPage.xaml", UriKind.Relative));
        }

        public void CloseLoadingPage(object sender, EventArgs e)
        {

            fContainer.Navigate(new Uri("Pages/Extractor.xaml", UriKind.Relative));
>>>>>>> Stashed changes
        }
        public void NavigateToCSVEditor()
        {
            fContainer.NavigationService.Navigate(new CSVEditorPage());
        }
        public void NavigateToCSVEditor()
        {
            fContainer.NavigationService.Navigate(new CSVEditorPage());
        }
    }
}
