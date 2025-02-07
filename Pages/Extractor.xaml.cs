using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WPFModernVerticalMenu.ViewModels;

namespace WPFModernVerticalMenu.Pages
{
    public partial class Extractor : Page
    {
        public Extractor()
        {
            InitializeComponent();
            DataContext = new PackingListViewModel();
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
                    if (DataContext is PackingListViewModel viewModel)
                    {
                        viewModel.SelectedFile = files[0];
                        viewModel.IsFileUploaded = true;
                    }

                    MessageBox.Show($"Fichier glissé : {files[0]}", "Fichier Importé", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            ((Border)sender).BorderBrush = new SolidColorBrush(Colors.Gray);
        }
    }
}
