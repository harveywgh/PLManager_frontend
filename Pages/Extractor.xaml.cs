using System.Windows;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WPFModernVerticalMenu.Services;
using WPFModernVerticalMenu.ViewModels;

namespace WPFModernVerticalMenu.Pages
{
    public partial class Extractor : Page
    {
        private PackingListViewModel _viewModel;

        public Extractor()
        {
            InitializeComponent();
            _viewModel = new PackingListViewModel();
            DataContext = _viewModel;

            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                _viewModel.NavigateToSelectSupplier += mainWindow.NavigateToSelectSupplier;
            }
        }

        private void Border_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                ((Border)sender).BorderBrush = new SolidColorBrush(Colors.Green);
            }
        }

        private void Border_DragLeave(object sender, DragEventArgs e)
        {
            ((Border)sender).BorderBrush = new SolidColorBrush(Colors.Gray);
        }

        private void Border_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    AppState.Instance.SetSelectedFile(files[0]);
                    _viewModel.FileName = Path.GetFileName(files[0]);
                    _viewModel.IsFileUploaded = true;
                    _viewModel.UpdateStatus("Fichier importé avec succès!", "Green");
                }
            }
            ((Border)sender).BorderBrush = new SolidColorBrush(Colors.Gray);
        }
    }
}
