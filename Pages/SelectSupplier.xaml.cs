using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
<<<<<<< Updated upstream
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
=======
using System;
using WPFModernVerticalMenu.Services;
>>>>>>> Stashed changes

namespace WPFModernVerticalMenu.Pages
{
    /// <summary>
    /// Lógica de interacción para Home.xaml
    /// </summary>
    public partial class SelectSupplier : Page
    {
        public SelectSupplier()
        {
            InitializeComponent();
        }
<<<<<<< Updated upstream
=======

        // Méthode pour la recherche
        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            if (DataContext is SelectSupplierViewModel viewModel)
            {
                viewModel.FilterSuppliers();
            }
        }

        // Permet le défilement avec la molette de la souris
        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;
            if (scrollViewer != null)
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta / 3);
                e.Handled = true; 
            }
        }


        private void ListBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is ListBox listBox)
            {
                ScrollViewer scrollViewer = FindScrollViewer(listBox);
                if (scrollViewer != null)
                {
                    scrollViewer.ScrollToVerticalOffset(0);
                }
            }
        }

        // Méthode pour récupérer la ScrollViewer d'un ListBox
        private ScrollViewer FindScrollViewer(DependencyObject parent)
        {
            if (parent is ScrollViewer)
                return (ScrollViewer)parent;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                var result = FindScrollViewer(child);
                if (result != null)
                    return result;
            }
            return null;
        }

        private void OnSupplierSelected(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is SelectSupplierViewModel viewModel)
            {
                if (viewModel.SelectedSupplier != null)
                {
                    Console.WriteLine($"📦 Fournisseur sélectionné via clic : {viewModel.SelectedSupplier.Name}");
                    AppState.Instance.SetSelectedSupplier(viewModel.SelectedSupplier);

                    // Vérification après enregistrement
                    Console.WriteLine($"✅ DEBUG: Fournisseur après sélection dans AppState: {AppState.Instance.SelectedSupplier?.Name ?? "NULL"}");
                }
            }
        }

        private void GoBack_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (PlaceholderTextBlock != null)
            {
                PlaceholderTextBlock.Visibility = string.IsNullOrWhiteSpace(SearchBox.Text) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void GoToCSVSettings_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.GoToCSVSettings_Click(sender, e);
            }
        }




>>>>>>> Stashed changes
    }
}
