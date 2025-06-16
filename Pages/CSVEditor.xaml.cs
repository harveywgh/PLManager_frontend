using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using System.Windows.Threading;
using WPFModernVerticalMenu.Services;
using System.Windows.Input;
using System.Windows.Media;

namespace PL_Manager.Pages
{
    public partial class CSVEditorPage : Page
    {
        private string csvFilePath;
        private DataTable csvDataTable;
        private static readonly HttpClient client = new HttpClient();
        private readonly ApiClientService _apiClientService;
        private List<string> allExtractedFiles;
        private int currentFileIndex = 0;
        private DispatcherTimer scrollTimer;
        private double scrollStep = 15;
        private int scrollDirection = 0;



        public CSVEditorPage()
        {
            InitializeComponent();

            CsvDataGrid.PreviewMouseWheel += (s, e) =>
            {
                if (!e.Handled)
                {
                    MainScrollViewer.ScrollToVerticalOffset(MainScrollViewer.VerticalOffset - e.Delta);
                    e.Handled = true;
                }
            };

            _apiClientService = new ApiClientService();
            LoadCsvData();

            MainScrollViewer.PreviewMouseWheel += MainScrollViewer_PreviewMouseWheel;

            scrollTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(20)
            };
            scrollTimer.Tick += (s, e) =>
            {
                if (scrollDirection == -1)
                    MainScrollViewer.ScrollToHorizontalOffset(MainScrollViewer.HorizontalOffset - scrollStep);
                else if (scrollDirection == 1)
                    MainScrollViewer.ScrollToHorizontalOffset(MainScrollViewer.HorizontalOffset + scrollStep);
            };
        }


        private void MainScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                MainScrollViewer.ScrollToHorizontalOffset(MainScrollViewer.HorizontalOffset - e.Delta);
                e.Handled = true;
            }
        }



        private async void LoadCsvData()
        {
            allExtractedFiles = AppState.Instance.ExtractedFiles;
            if (allExtractedFiles == null || allExtractedFiles.Count == 0)
            {
                MessageBox.Show("Aucun fichier CSV à afficher.");
                return;
            }

            var topLeftCell = new Border
            {
                Height = 30,
                Background = Brushes.LightGray,
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(0, 0, 1, 1)
            };
            ExcelRowNumbers.Children.Add(topLeftCell);


            // Charger le premier fichier par défaut
            await LoadCsvFileByIndex(0);
            GenerateFileSelectorButtons();
            ExcelHeaderGrid.Children.Clear();

            CsvDataGrid.ItemsSource = csvDataTable.DefaultView;
            // 🧮 Numéros de lignes (1, 2, 3...)
            CsvDataGrid.UpdateLayout(); // force layout pour que les lignes soient prêtes

            ExcelRowNumbers.Children.Clear();
            for (int i = 0; i < csvDataTable.Rows.Count; i++)
            {
                var row = (DataGridRow)CsvDataGrid.ItemContainerGenerator.ContainerFromIndex(i);
                double rowHeight = row?.ActualHeight > 0 ? row.ActualHeight : CsvDataGrid.RowHeight;

                var border = new Border
                {
                    Height = rowHeight,
                    Background = Brushes.LightGray,
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Thickness(0, 0, 1, 1),
                    Child = new TextBlock
                    {
                        Text = (i + 1).ToString(),
                        FontWeight = FontWeights.Bold,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Padding = new Thickness(2)
                    }
                };
                ExcelRowNumbers.Children.Add(border);
            }




            // 🔁 Génération manuelle des en-têtes A, B, C...
            ExcelHeaderGrid.Children.Clear();
            ExcelHeaderGrid.Columns = csvDataTable.Columns.Count;

            for (int i = 0; i < csvDataTable.Columns.Count; i++)
            {
                TextBlock header = new TextBlock
                {
                    Text = GetExcelColumnName(i),
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Padding = new Thickness(5),
                    Height = 30
                };
                ExcelHeaderGrid.Children.Add(header);
            }

        }

        private string GetExcelColumnName(int columnIndex)
{
    string columnName = "";
    while (columnIndex >= 0)
    {
        columnName = (char)('A' + (columnIndex % 26)) + columnName;
        columnIndex = (columnIndex / 26) - 1;
    }
    return columnName;
}


        private async Task LoadCsvFileByIndex(int index)
        {
            try
            {
                if (index < 0 || index >= allExtractedFiles.Count) return;

                // ✅ Télécharger depuis le back
                string tempPath = await _apiClientService.DownloadFileToTempAsync(allExtractedFiles[index]);

                csvFilePath = tempPath;
                currentFileIndex = index;

                using (var reader = new StreamReader(csvFilePath))
                using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ";" }))
                using (var dr = new CsvDataReader(csv))
                {
                    csvDataTable = new DataTable();
                    csvDataTable.Load(dr);
                }

                foreach (DataColumn column in csvDataTable.Columns)
                    column.ReadOnly = false;

                CsvDataGrid.ItemsSource = csvDataTable.DefaultView;
                MainScrollViewer.ScrollChanged -= MainScrollViewer_ScrollChanged;
                MainScrollViewer.ScrollChanged += MainScrollViewer_ScrollChanged;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERREUR: {ex.Message}");
            }
        }

        private void MainScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (MainScrollViewer.ScrollableWidth > 0)
            {
                HorizontalSlider.Maximum = MainScrollViewer.ScrollableWidth;
                HorizontalSlider.Value = MainScrollViewer.HorizontalOffset;
            }
        }


        private void GenerateFileSelectorButtons()
        {
            FileButtonPanel.Children.Clear();

            for (int i = 0; i < allExtractedFiles.Count; i++)
            {
                int fileIndex = i; // Pour la closure

                Button button = new Button
                {
                    Content = $"Conteneur {fileIndex + 1}",
                    Tag = fileIndex,
                    Style = (Style)FindResource("PaginationButtonStyle")
                };

                button.Click += async (sender, args) =>
                {
                    if (((Button)sender)?.Tag is int index)
                    {
                        await LoadCsvFileByIndex(index);
                        HighlightActiveButton(index);
                    }
                };

                FileButtonPanel.Children.Add(button);
            }

            HighlightActiveButton(0);
        }

        private void HighlightActiveButton(int activeIndex)
        {
            foreach (Button btn in FileButtonPanel.Children)
            {
                if ((int)btn.Tag == activeIndex)
                {
                    btn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#406442"));
                    btn.Foreground = Brushes.White;
                    btn.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#406442"));
                }
                else
                {
                    btn.Background = Brushes.White;
                    btn.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2D2D2D"));
                    btn.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2D2D2D"));
                }
            }
        }


        private void HorizontalSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (MainScrollViewer != null && MainScrollViewer.ScrollableWidth > 0)
            {
                if (Math.Abs(MainScrollViewer.HorizontalOffset - e.NewValue) > 1)
                {
                    MainScrollViewer.ScrollToHorizontalOffset(e.NewValue);
                }
            }
        }



        private void CsvDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            try
            {
                // 🔹 Récupère la cellule modifiée
                DataRowView rowView = e.Row.Item as DataRowView;
                if (rowView != null)
                {
                    string columnName = e.Column.Header.ToString();
                    TextBox editedTextBox = e.EditingElement as TextBox;

                    if (editedTextBox != null)
                    {
                        string newValue = editedTextBox.Text;
                        Console.WriteLine($"📌 DEBUG: Cellule modifiée - Colonne: {columnName}, Nouvelle valeur: {newValue}");

                        // 🔹 Met à jour la valeur dans le DataTable
                        rowView[columnName] = newValue;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERREUR: Impossible d'appliquer la modification : {ex.Message}");
            }
        }

        private void CsvDataGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            Console.WriteLine("📌 DEBUG: Début d'édition de cellule.");
        }

        private void CsvDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                string pasted = Clipboard.GetText();

                foreach (var cellInfo in CsvDataGrid.SelectedCells)
                {
                    if (cellInfo.Item is DataRowView row && cellInfo.Column != null)
                    {
                        string columnName = cellInfo.Column.Header.ToString();
                        row[columnName] = pasted;
                    }
                }

                Console.WriteLine($"📥 Collage multiple : '{pasted}' dans {CsvDataGrid.SelectedCells.Count} cellules");
                e.Handled = true;
            }

            // Copier (CTRL + C)
            if (e.Key == Key.C && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                if (CsvDataGrid.CurrentCell != null)
                {
                    var cellInfo = CsvDataGrid.CurrentCell;
                    if (cellInfo.Item is DataRowView row && cellInfo.Column != null)
                    {
                        string columnName = cellInfo.Column.Header.ToString();
                        var value = row[columnName]?.ToString() ?? "";
                        Clipboard.SetText(value);
                        Console.WriteLine($"📋 Copié : {value}");
                        e.Handled = true;
                    }
                }
            }

            // Coller (CTRL + V)
            if (e.Key == Key.V && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                if (CsvDataGrid.CurrentCell != null)
                {
                    var cellInfo = CsvDataGrid.CurrentCell;
                    if (cellInfo.Item is DataRowView row && cellInfo.Column != null)
                    {
                        string columnName = cellInfo.Column.Header.ToString();
                        string pasted = Clipboard.GetText();
                        row[columnName] = pasted;
                        Console.WriteLine($"📥 Collé : {pasted} dans {columnName}");
                        e.Handled = true;
                    }
                }
            }

            if (e.Key == Key.Back || e.Key == Key.Delete)
            {
                // Vérifie si la cellule est en mode édition
                if (CsvDataGrid.CurrentColumn != null && CsvDataGrid.CurrentCell.Item is DataRowView rowView)
                {
                    var columnName = CsvDataGrid.CurrentColumn.Header.ToString();
                    var editingElement = CsvDataGrid.CurrentColumn.GetCellContent(rowView) as TextBox;

                    // 🔹 Si la cellule est en mode édition, suppression caractère par caractère
                    if (editingElement != null)
                    {
                        int cursorPosition = editingElement.SelectionStart;

                        if (!string.IsNullOrEmpty(editingElement.Text) && cursorPosition > 0)
                        {
                            editingElement.Text = editingElement.Text.Remove(cursorPosition - 1, 1);
                            editingElement.SelectionStart = cursorPosition - 1;
                        }
                        e.Handled = true;
                    }
                    else
                    {
                        // 🔹 Si la cellule n'est pas en mode édition, on efface tout
                        rowView[columnName] = "";
                        e.Handled = true;
                    }
                }
            }
        }

        private void ScrollLeftBtn_Click(object sender, RoutedEventArgs e)
        {
            MainScrollViewer.ScrollToHorizontalOffset(MainScrollViewer.HorizontalOffset - 100);
        }

        private void ScrollRightBtn_Click(object sender, RoutedEventArgs e)
        {
            MainScrollViewer.ScrollToHorizontalOffset(MainScrollViewer.HorizontalOffset + 100);
        }

        private void ScrollLeftBtn_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            scrollDirection = -1;
            scrollTimer.Start();
        }

        private void ScrollRightBtn_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            scrollDirection = 1;
            scrollTimer.Start();
        }

        private void ScrollBtn_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            scrollTimer.Stop();
            scrollDirection = 0;
        }


        private async void SaveCsv_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Voulez-vous vraiment sauvegarder les modifications ?",
                                "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
            {
                return;
            }

            try
            {
                string tempCsvPath = Path.Combine(Path.GetTempPath(), "modified.csv");

                using (var writer = new StreamWriter(tempCsvPath, false, new UTF8Encoding(true)))
                using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ";",
                    NewLine = "\n"
                }))
                {
                    foreach (DataColumn column in csvDataTable.Columns)
                        csv.WriteField(column.ColumnName);
                    csv.NextRecord();

                    int ligneCounter = 0;

                    foreach (DataRow row in csvDataTable.Rows)
                    {
                        bool isEmpty = true;
                        foreach (var cell in row.ItemArray)
                        {
                            if (cell != null && !string.IsNullOrWhiteSpace(cell.ToString()))
                            {
                                isEmpty = false;
                                break;
                            }
                        }

                        if (isEmpty)
                        {
                            Console.WriteLine($"🚫 Ligne ignorée (vide) à l’index {ligneCounter}");
                            ligneCounter++;
                            continue;
                        }

                        Console.Write($"✅ Écriture ligne {ligneCounter} : ");
                        foreach (var cell in row.ItemArray)
                        {
                            string cleanValue = cell?.ToString()?.Trim() ?? "NULL";
                            Console.Write($"[{cleanValue}] ");
                            csv.WriteField(cleanValue);
                        }
                        Console.WriteLine();
                        csv.NextRecord();
                        ligneCounter++;
                    }
                }

                // 🔁 On récupère le chemin complet tel qu'il a été renvoyé par le backend
                string correctCsvPath = allExtractedFiles[currentFileIndex];


                Console.WriteLine($"📡 DEBUG: Chemin original du fichier CSV : {csvFilePath}");
                Console.WriteLine($"📡 DEBUG: Chemin du fichier envoyé : {correctCsvPath}");
                Console.WriteLine($"📡 DEBUG: Chemin temporaire utilisé : {tempCsvPath}");

                using (var content = new MultipartFormDataContent())
                {
                    content.Add(new StreamContent(File.OpenRead(tempCsvPath)), "file", Path.GetFileName(tempCsvPath));
                    content.Add(new StringContent(correctCsvPath), "csv_path");

                    Console.WriteLine($"📡 Envoi du fichier modifié vers {_apiClientService.BaseUrl}update-csv/");

                    var response = await client.PutAsync($"{_apiClientService.BaseUrl}update-csv/", content);
                    string responseMessage = await response.Content.ReadAsStringAsync();

                    Console.WriteLine($"📡 Réponse API : {response.StatusCode} - {responseMessage}");

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Fichier mis à jour avec succès !");
                    }
                    else
                    {
                        MessageBox.Show($"Erreur lors de la mise à jour : {responseMessage}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'enregistrement du CSV : {ex.Message}");
            }
        }


        protected override void OnPreviewKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.S && (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) == System.Windows.Input.ModifierKeys.Control)
            {
                SaveCsv_Click(this, null);
            }
        }



    }
}