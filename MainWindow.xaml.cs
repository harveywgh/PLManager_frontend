using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using PL_Manager.Pages;
using PLManager.Pages;
using PLManager.Windows;
using WPFModernVerticalMenu.Services;
using WPFModernVerticalMenu.ViewModels;

namespace WPFModernVerticalMenu
{
    public partial class MainWindow : Window
    {
        private readonly ApiService _apiService;
        private string _apiStatusText = "API NOT OK";
        private SolidColorBrush _apiStatusColor = new SolidColorBrush(Colors.Red);

        public event PropertyChangedEventHandler PropertyChanged;

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

            _apiService = new ApiService();
            _apiService.ApiStatusChanged += UpdateApiStatus;

            if (DataContext is PackingListViewModel viewModel)
            {
                viewModel.NavigateToSelectSupplier += btnSelectSupplier_Click;
                viewModel.NavigateToLoadingPage += NavigateToLoadingPage;
                viewModel.CloseLoadingPage += CloseLoadingPage;
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

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
        }

        private void BG_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Tg_Btn.IsChecked = false;
        }

        // Show/Hide Popup Helpers
        private void ShowPopup(Button button, string text)
        {
            if (Tg_Btn.IsChecked == false)
            {
                Popup.PlacementTarget = button;
                Popup.Placement = PlacementMode.Right;
                Popup.IsOpen = true;
                Header.PopupText.Text = text;
            }
        }

        private void HidePopup(object sender, MouseEventArgs e)
        {
            Popup.Visibility = Visibility.Collapsed;
            Popup.IsOpen = false;
        }

        // MouseEnter Events
        private void btnHome_MouseEnter(object sender, MouseEventArgs e) => ShowPopup(btnHome, "Tableau de bord");
        private void btnDashboard_MouseEnter(object sender, MouseEventArgs e) => ShowPopup(btnDashboard, "Extracteur PL");
        private void btnLocalEditor_MouseEnter(object sender, MouseEventArgs e) => ShowPopup(btnLocalEditor, "Éditeur Local");
        private void btnProductStock_MouseEnter(object sender, MouseEventArgs e) => ShowPopup(btnProductStock, "Réglages");
        private void btnOrderList_MouseEnter(object sender, MouseEventArgs e) => ShowPopup(btnOrderList, "Aide");
        private void btnApiStatus_MouseEnter(object sender, MouseEventArgs e) => ShowPopup(btnApiStatus, ApiStatusText);


        // MouseLeave Events
        private void btnHome_MouseLeave(object sender, MouseEventArgs e) => HidePopup(sender, e);
        private void btnDashboard_MouseLeave(object sender, MouseEventArgs e) => HidePopup(sender, e);
        private void btnLocalEditor_MouseLeave(object sender, MouseEventArgs e) => HidePopup(sender, e);
        private void btnProducts_MouseLeave(object sender, MouseEventArgs e) => HidePopup(sender, e);
        private void btnProductStock_MouseLeave(object sender, MouseEventArgs e) => HidePopup(sender, e);
        private void btnOrderList_MouseLeave(object sender, MouseEventArgs e) => HidePopup(sender, e);
        private void btnSetting_MouseLeave(object sender, MouseEventArgs e) => HidePopup(sender, e);

        // Button Click Events
        private void btnClose_Click(object sender, RoutedEventArgs e) => Close();

        private void btnRestore_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void btnHome_Click(object sender, RoutedEventArgs e)
        {
            fContainer.Navigate(new Uri("Pages/Home.xaml", UriKind.RelativeOrAbsolute));
        }

        public void btnDashboard_Click(object sender, RoutedEventArgs e)
        {
            fContainer.Navigate(new Uri("Pages/Extractor.xaml", UriKind.RelativeOrAbsolute));
        }

        private void btnLocalEditor_Click(object sender, RoutedEventArgs e)
        {
            var win = new EditorWindow();
            win.Show();
        }

        private void MenuItem_Loaded(object sender, RoutedEventArgs e)
        {
            // Ajoute ici le code à exécuter lors du chargement d'un élément de menu
        }

        public void btnSelectSupplier_Click(object sender, EventArgs e)
        {
            fContainer.Navigate(new Uri("Pages/SelectSupplier.xaml", UriKind.Relative));
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
        }

        public void NavigateToCSVEditor()
        {
            fContainer.NavigationService.Navigate(new CSVEditorPage());
        }
    }
}
