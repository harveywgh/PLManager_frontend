using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using WPFModernVerticalMenu.Helpers;

namespace WPFModernVerticalMenu.ViewModels
{
    public class PackingListViewModel : INotifyPropertyChanged
    {
        private string _selectedFile;
        private bool _isFileUploaded;

        public string SelectedFile
        {
            get => _selectedFile;
            set
            {
                _selectedFile = value;
                OnPropertyChanged(nameof(SelectedFile));
            }
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
                SelectedFile = dlg.FileName;
                IsFileUploaded = true;
                MessageBox.Show("Fichier importé avec succès!", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void GoToNextPage()
        {
            Application.Current.MainWindow.Content = new WPFModernVerticalMenu.Pages.SelectSupplier();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
