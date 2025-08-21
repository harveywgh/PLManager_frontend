using System;
using System.Windows;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using WPFModernVerticalMenu.Helpers;
using WPFModernVerticalMenu.Model;
using WPFModernVerticalMenu.Services;
using WPFModernVerticalMenu.Pages;

namespace WPFModernVerticalMenu.ViewModels
{
    public class SelectSupplierViewModel : INotifyPropertyChanged
    {
        private string _searchText;
        private SupplierModel _selectedSupplier;
        private ObservableCollection<SupplierModel> _filteredSuppliers;
        private string _statusMessage;
        private string _statusColor;
        private readonly FileUploadService _fileUploadService;

        public ObservableCollection<SupplierModel> SupplierList { get; set; }
        public ObservableCollection<SupplierModel> FilteredSuppliers
        {
            get => _filteredSuppliers;
            set { _filteredSuppliers = value; OnPropertyChanged(nameof(FilteredSuppliers)); }
        }

        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(nameof(SearchText)); FilterSuppliers(); }
        }

        public SupplierModel SelectedSupplier
        {
            get => _selectedSupplier;
            set
            {
                _selectedSupplier = value;
                OnPropertyChanged(nameof(SelectedSupplier));
                OnPropertyChanged(nameof(IsSupplierSelected));
                ((RelayCommand)ConfirmSupplierCommand).RaiseCanExecuteChanged();
                ((RelayCommand)ConfirmAndNavigateCommand).RaiseCanExecuteChanged();
            }
        }

        public bool IsSupplierSelected => SelectedSupplier != null;
        public ICommand ConfirmSupplierCommand { get; }
        public ICommand ConfirmAndNavigateCommand { get; } 

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(nameof(StatusMessage)); }
        }

        public string StatusColor
        {
            get => _statusColor;
            set { _statusColor = value; OnPropertyChanged(nameof(StatusColor)); }
        }

        public SelectSupplierViewModel()
        {
            _fileUploadService = new FileUploadService();
            SupplierList = new ObservableCollection<SupplierModel>
            {
                new SupplierModel { Index = 1, Name = "Komati", Code = "Komati" },
                new SupplierModel { Index = 2, Name = "Mahela", Code = "Mahela" },
                new SupplierModel { Index = 3, Name = "Grosa", Code = "Grosa" },
                new SupplierModel { Index = 4, Name = "ZestFruit", Code = "ZestFruit" },
                new SupplierModel { Index = 5, Name = "Sunny", Code = "Sunny" },
                new SupplierModel { Index = 6, Name = "Safpro", Code = "Safpro" },
                new SupplierModel { Index = 7, Name = "ALG", Code = "ALG" },
                new SupplierModel { Index = 8, Name = "Agualima", Code = "Agualima" },
                new SupplierModel { Index = 9, Name = "Exportadora Fruticola Athos", Code = "Exportadora Fruticola Athos" },
                new SupplierModel { Index = 10, Name = "Asica", Code = "Asica" },
                new SupplierModel { Index = 11, Name = "Laran", Code = "Laran" },
                new SupplierModel { Index = 12, Name = "Jaguacy", Code = "Jaguacy" },
                new SupplierModel { Index = 13, Name = "Angon", Code = "Angon" },
                new SupplierModel { Index = 14, Name = "Camposol", Code = "Camposol" },
                new SupplierModel { Index = 15, Name = "CPF", Code = "CPF" },
                new SupplierModel { Index = 16, Name = "Mosqueta", Code = "Mosqueta" },
                new SupplierModel { Index = 17, Name = "Pirona", Code = "Pirona" },
                new SupplierModel { Index = 18, Name = "Hefei", Code = "Hefei" },
                new SupplierModel { Index = 19, Name = "Sasini", Code = "Sasini" },
                new SupplierModel { Index = 20, Name = "Kakuzi", Code = "Kakuzi" },
                new SupplierModel { Index = 21, Name = "Jorie", Code = "Jorie" },
                new SupplierModel { Index = 22, Name = "Mavuno", Code = "Mavuno" },
                new SupplierModel { Index = 23, Name = "Viru", Code = "Viru" },
                new SupplierModel { Index = 24, Name = "Exportadora Fruticola Athos V2", Code = "Exportadora Fruticola Athos V2" },
                new SupplierModel { Index = 25, Name = "HNP", Code = "HNP" },
                new SupplierModel { Index = 26, Name = "Swellen", Code = "Swellen" },
                new SupplierModel { Index = 27, Name = "Shalimar", Code = "Shalimar" },
                new SupplierModel { Index = 28, Name = "Ingophase", Code = "Ingophase" },
                new SupplierModel { Index = 29, Name = "Avaleza", Code = "Avaleza" },
                new SupplierModel { Index = 30, Name = "Mountain Avocado", Code = "Mountain Avocado" },
            };

            FilteredSuppliers = new ObservableCollection<SupplierModel>(SupplierList);
            ConfirmSupplierCommand = new RelayCommand(() => ConfirmSupplier(), () => IsSupplierSelected);
            ConfirmAndNavigateCommand = new RelayCommand(ExecuteConfirmAndNavigate, () => IsSupplierSelected);
        }

        public void FilterSuppliers()
        {
            FilteredSuppliers = string.IsNullOrWhiteSpace(SearchText)
                ? new ObservableCollection<SupplierModel>(SupplierList)
                : new ObservableCollection<SupplierModel>(SupplierList.Where(s => s.Name.ToLower().Contains(SearchText.ToLower())));
        }

        private void ExecuteConfirmAndNavigate()
        {
            ConfirmSupplier();
            if (AppState.Instance.SelectedSupplier == null || string.IsNullOrEmpty(AppState.Instance.SelectedFile))
            {
                return;
            }

            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.GoToCSVSettings_Click(this, EventArgs.Empty);
            }
        }

        public void ConfirmSupplier()
        {
            if (SelectedSupplier == null)
            {
                return;
            }
            AppState.Instance.SetSelectedSupplier(SelectedSupplier);
            Console.WriteLine($"✅ DEBUG: Fournisseur après enregistrement -> {AppState.Instance.SelectedSupplier?.Name ?? "NULL"}");
        }


        private void UpdateStatus(string message, string color)
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
