using Microsoft.Win32;
using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using CsvHelper;
using CsvHelper.Configuration;
using SourceGrid;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using PLManager.Model;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Forms.VisualStyles;
using WPFModernVerticalMenu.Services;

namespace PLManager.Windows
{
    public partial class CSVEditorWindow : Window
    {
        private DataTable csvData;
        private Grid sourceGrid;
        private string CurrentFilePath;
        private bool IsFromApi = false;
        private string OriginalApiPath = null;
        private List<string> allExtractedFiles = new List<string>();
        private int currentFileIndex = 0;
        private readonly ApiClientService _apiClientService = new ApiClientService();
        private Stack<List<CellEditModel>> undoStack = new Stack<List<CellEditModel>>();



        public CSVEditorWindow()
        {
            InitializeComponent();
            InitializeSourceGrid();
            this.KeyDown += CSVEditorWindow_KeyDown;
        }

        public CSVEditorWindow(string csvPath) : this()
        {
            OriginalApiPath = csvPath;
            _ = LoadRemoteCsvAsync(csvPath);
            if (AppState.Instance.ExtractedFiles != null && AppState.Instance.ExtractedFiles.Count > 0)
            {
                _ = LoadCsvFiles(AppState.Instance.ExtractedFiles);
            }
        }

        private void InitializeSourceGrid()
        {
            sourceGrid = new Grid
            {
                Dock = System.Windows.Forms.DockStyle.Fill,
                BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
            };

            GridHost.Child = sourceGrid; 
        }

        private void ImportCsv_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                Title = "Sélectionner un fichier CSV"
            };

            if (dialog.ShowDialog() == true)
            {
                using (var reader = new StreamReader(dialog.FileName))
                using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ";" }))
                using (var dr = new CsvDataReader(csv))
                {
                    csvData = new DataTable();
                    csvData.Load(dr);
                    foreach (DataColumn col in csvData.Columns)
                    {
                        col.ReadOnly = false;
                    }
                }

                DisplayDataInGrid();
            }
        }


        private void LoadCsvFile(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ";" }))
            using (var dr = new CsvDataReader(csv))
            {
                csvData = new DataTable();
                csvData.Load(dr);

                foreach (DataColumn col in csvData.Columns)
                    col.ReadOnly = false;
            }

            DisplayDataInGrid();
        }


        public async Task LoadCsvFiles(List<string> extractedFiles)
        {
            if (extractedFiles == null || extractedFiles.Count == 0)
            {
                MessageBox.Show("Aucun fichier à afficher.");
                return;
            }

            allExtractedFiles = extractedFiles;
            currentFileIndex = 0;

            await LoadAndDisplayFile(allExtractedFiles[0]);
            GenerateFileButtons();
        }


        private void GenerateFileButtons()
        {
            FileButtonsPanel.Children.Clear();

            for (int i = 0; i < allExtractedFiles.Count; i++)
            {
                int index = i;
                var button = new System.Windows.Controls.Button
                {
                    Content = $"Conteneur {i + 1}",
                    Margin = new Thickness(5),
                    Padding = new Thickness(10),
                    Tag = index
                };

                button.Click += async (s, e) =>
                {
                    currentFileIndex = index;
                    await LoadAndDisplayFile(allExtractedFiles[index]);
                    HighlightActiveButton(index);
                };

                FileButtonsPanel.Children.Add(button);
            }

            HighlightActiveButton(0);
        }


        private void HighlightActiveButton(int activeIndex)
        {
            foreach (System.Windows.Controls.Button btn in FileButtonsPanel.Children)
            {
                if ((int)btn.Tag == activeIndex)
                {
                    btn.Background = Brushes.Green;
                    btn.Foreground = Brushes.White;
                }
                else
                {
                    btn.ClearValue(System.Windows.Controls.Control.BackgroundProperty);
                    btn.ClearValue(System.Windows.Controls.Control.ForegroundProperty);
                }
            }
        }

        private async Task LoadAndDisplayFile(string filePath)
        {
            try
            {
                using (Stream stream = await _apiClientService.DownloadFileAsStreamAsync(filePath))
                {
                    OriginalApiPath = filePath;
                    LoadCsvFile(stream);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur de chargement du fichier : " + ex.Message);
            }
        }


        private async Task ShowToastAsync(string message, int durationMs = 2000)
        {
            ToastNotification.Text = message;
            ToastContainer.Opacity = 1;
            ToastContainer.Visibility = Visibility.Visible;

            await Task.Delay(durationMs);

            ToastContainer.Visibility = Visibility.Collapsed;
        }


        private void DisplayDataInGrid()
        {
            sourceGrid.Redim(csvData.Rows.Count + 1, csvData.Columns.Count);

            // Headers
            for (int col = 0; col < csvData.Columns.Count; col++)
                sourceGrid[0, col] = new SourceGrid.Cells.ColumnHeader(csvData.Columns[col].ColumnName);

            // Data
            for (int row = 0; row < csvData.Rows.Count; row++)
            {
                for (int col = 0; col < csvData.Columns.Count; col++)
                {
                    string cellValue = csvData.Rows[row][col]?.ToString();
                    var editableCell = new SourceGrid.Cells.Cell(cellValue);
                    var textEditor = new SourceGrid.Cells.Editors.TextBox(typeof(string));
                    editableCell.Editor = textEditor;
                    sourceGrid[row + 1, col] = editableCell;

                    // 🔁 On capture les indices actuels
                    int rowCopy = row;
                    int colCopy = col;

                    // ⚠️ Ajout de l'événement sur le contrôle d'édition
                    textEditor.Control.Validated += (s, e) =>
                    {
                        var control = s as System.Windows.Forms.Control;
                        var newVal = control.Text;
                        var oldVal = csvData.Rows[rowCopy][colCopy]?.ToString();

                        if (oldVal != newVal)
                        {
                            RegisterEdit(new List<CellEditModel>
                            {
                                new CellEditModel
                                {
                                    Row = rowCopy,
                                    Column = colCopy,
                                    OldValue = oldVal,
                                    NewValue = newVal
                                }
                            });
                            csvData.Rows[rowCopy][colCopy] = newVal;
                        }
                    };
                }
            }

            sourceGrid.AutoSizeCells();
        }


        private void RegisterEdit(List<CellEditModel> edits)
        {
            if (edits != null && edits.Count > 0)
                undoStack.Push(edits);
        }

        private void UndoLastEdit()
        {
            if (undoStack.Count == 0)
                return;

            var lastEdit = undoStack.Pop();

            foreach (var edit in lastEdit)
            {
                if (edit.Row + 1 < sourceGrid.RowsCount && edit.Column < sourceGrid.ColumnsCount)
                {
                    if (sourceGrid[edit.Row + 1, edit.Column] is SourceGrid.Cells.Cell cell)
                    {
                        cell.Value = edit.OldValue;
                    }
                }
            }

            sourceGrid.Invalidate();
        }



        private void SaveCsv_Click(object sender, RoutedEventArgs e)
        {
            if (csvData == null)
            {
                System.Windows.MessageBox.Show("Aucune donnée à enregistrer.");
                return;
            }

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                Title = "Enregistrer le fichier CSV"
            };

            if (dialog.ShowDialog() == true)
            {
                using (var writer = new StreamWriter(dialog.FileName, false, new UTF8Encoding(true)))
                using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ";",
                    NewLine = "\n"
                }))
                {
                    foreach (DataColumn column in csvData.Columns)
                        csv.WriteField(column.ColumnName);
                    csv.NextRecord();

                    foreach (DataRow row in csvData.Rows)
                    {
                        foreach (var cell in row.ItemArray)
                            csv.WriteField(cell?.ToString()?.Trim());
                        csv.NextRecord();
                    }
                }

                System.Windows.MessageBox.Show("Fichier enregistré avec succès !");
            }
        }

        private async void SaveToCurrentFile_Click(object sender, RoutedEventArgs e)
        {
            await SaveToCurrentFileAsync();
        }


        private void SyncGridToDataTable()
        {
            for (int row = 0; row < csvData.Rows.Count; row++)
            {
                for (int col = 0; col < csvData.Columns.Count; col++)
                {
                    if (sourceGrid[row + 1, col] is SourceGrid.Cells.Cell cell)
                    {
                        csvData.Rows[row][col] = cell.Value?.ToString();
                    }
                }
            }
        }

        private void SaveAsCsv_Click(object sender, RoutedEventArgs e)
        {
            if (csvData == null)
            {
                MessageBox.Show("Aucune donnée à enregistrer.");
                return;
            }

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                Title = "Enregistrer sous"
            };

            if (dialog.ShowDialog() == true)
            {
                using (var writer = new StreamWriter(dialog.FileName, false, new UTF8Encoding(true)))
                using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ";",
                    NewLine = "\n"
                }))
                {
                    foreach (DataColumn column in csvData.Columns)
                        csv.WriteField(column.ColumnName);
                    csv.NextRecord();

                    foreach (DataRow row in csvData.Rows)
                    {
                        foreach (var cell in row.ItemArray)
                            csv.WriteField(cell?.ToString()?.Trim());
                        csv.NextRecord();
                    }
                }

                MessageBox.Show("✅ Fichier enregistré sous un nouveau nom !");
            }
        }


        private async Task SaveToCurrentFileAsync()
        {

            if (csvData == null)
            {
                MessageBox.Show("Aucune donnée à sauvegarder.");
                return;
            }

            SyncGridToDataTable();

            try
            {
                string tempPath = Path.Combine(Path.GetTempPath(), "edited_" + Guid.NewGuid() + ".csv");

                using (var writer = new StreamWriter(tempPath, false, new UTF8Encoding(true)))
                using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ";",
                    NewLine = "\n"
                }))
                {
                    foreach (DataColumn col in csvData.Columns)
                    {
                        Console.Write(col.ColumnName + "; ");
                        csv.WriteField(col.ColumnName);
                    }
                    csv.NextRecord();

                    for (int row = 0; row < csvData.Rows.Count; row++)
                    {
                        string logLine = "";
                        foreach (var cell in csvData.Rows[row].ItemArray)
                        {
                            string val = cell?.ToString()?.Trim();
                            logLine += $"{val}; ";
                            csv.WriteField(val);
                        }
                        csv.NextRecord();
                    }
                }

                if (string.IsNullOrEmpty(OriginalApiPath))
                {
                    MessageBox.Show("❌ Chemin API manquant pour upload.");
                    Console.WriteLine("❌ [WPF] OriginalApiPath est vide !");
                    return;
                }

                string remotePath = OriginalApiPath.Replace("\\", "/");

                // ✅ Si le chemin commence déjà par "outputs/", on garde tel quel
                if (!remotePath.StartsWith("outputs/"))
                {
                    // ✅ Sinon, on ajoute outputs/ + nom du fournisseur si possible
                    string supplier = AppState.Instance.SelectedSupplier?.Code;

                    if (!string.IsNullOrEmpty(supplier) && !remotePath.StartsWith(supplier + "/"))
                    {
                        remotePath = $"outputs/{supplier}/{Path.GetFileName(remotePath)}";
                    }
                    else
                    {
                        remotePath = $"outputs/{remotePath}";
                    }
                }

                using (var content = new MultipartFormDataContent())
                {
                    content.Add(new StreamContent(File.OpenRead(tempPath)), "file", Path.GetFileName(tempPath));
                    content.Add(new StringContent(remotePath), "csv_path");

                    var response = await _apiClientService.HttpClient.PutAsync($"{_apiClientService.BaseUrl}update-csv/", content);

                    string result = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        await ShowToastAsync("Fichier mis à jour avec succès.");
                    }
                    else
                    {
                        await ShowToastAsync("Erreur de sauvegarde : " + result);
                    }
                }

                File.Delete(tempPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Exception lors de la sauvegarde : " + ex.Message);
            }
        }

        private async Task LoadRemoteCsvAsync(string csvPath)
        {
            try
            {

                using (Stream stream = await _apiClientService.DownloadFileAsStreamAsync(csvPath))
                {
                    LoadCsvFile(stream);
                    Title = $"Éditeur CSV - {System.IO.Path.GetFileName(csvPath)}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Erreur de téléchargement : " + ex.Message);
                Console.WriteLine("❌ Exception : " + ex);
            }
        }

        private void CSVEditorWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Copier (Ctrl + C)
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && e.Key == Key.C)
            {
                var selectedCells = sourceGrid.Selection.GetSelectionRegion();
                if (selectedCells.Count > 0)
                {
                    var sb = new StringBuilder();
                    foreach (var region in selectedCells)
                    {
                        for (int row = region.Start.Row; row <= region.End.Row; row++)
                        {
                            for (int col = region.Start.Column; col <= region.End.Column; col++)
                            {
                                var cell = sourceGrid[row, col];
                                sb.Append(cell?.DisplayText ?? "");
                                if (col < region.End.Column) sb.Append('\t');
                            }
                            sb.AppendLine();
                        }
                    }
                    Clipboard.SetText(sb.ToString());
                    e.Handled = true;
                }
            }

            // Coller (Ctrl + V)
            else if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && e.Key == Key.V)
            {
                var pastedText = Clipboard.GetText();
                if (!string.IsNullOrWhiteSpace(pastedText))
                {
                    var lines = pastedText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                    // ✅ Cas 1 : une seule cellule copiée, collage dans plusieurs cellules
                    if (lines.Length == 1 && !lines[0].Contains('\t'))
                    {
                        string value = lines[0];
                        var edits = new List<CellEditModel>();

                        foreach (var region in sourceGrid.Selection.GetSelectionRegion())
                        {
                            for (int row = region.Start.Row; row <= region.End.Row; row++)
                            {
                                for (int col = region.Start.Column; col <= region.End.Column; col++)
                                {
                                    if (sourceGrid[row, col] is SourceGrid.Cells.Cell cell)
                                    {
                                        var oldValue = cell.Value?.ToString();
                                        if (oldValue != value)
                                        {
                                            edits.Add(new CellEditModel
                                            {
                                                Row = row - 1,
                                                Column = col,
                                                OldValue = oldValue,
                                                NewValue = value
                                            });

                                            cell.Value = value;
                                        }
                                    }
                                }
                            }
                        }
                        RegisterEdit(edits);
                    }
                    else
                    {
                        // ✅ Cas 2 : collage multiple (grille)
                        int baseRow = sourceGrid.Selection.ActivePosition.Row;
                        int baseCol = sourceGrid.Selection.ActivePosition.Column;
                        var edits = new List<CellEditModel>();

                        for (int i = 0; i < lines.Length; i++)
                        {
                            var cells = lines[i].Split('\t');

                            for (int j = 0; j < cells.Length; j++)
                            {
                                int r = baseRow + i;
                                int c = baseCol + j;

                                if (r < sourceGrid.RowsCount && c < sourceGrid.ColumnsCount && sourceGrid[r, c] is SourceGrid.Cells.Cell cell)
                                {
                                    var oldVal = cell.Value?.ToString();
                                    var newVal = cells[j];

                                    if (oldVal != newVal)
                                    {
                                        edits.Add(new CellEditModel
                                        {
                                            Row = r - 1,
                                            Column = c,
                                            OldValue = oldVal,
                                            NewValue = newVal
                                        });

                                        cell.Value = newVal;
                                    }
                                }
                            }
                        }
                        RegisterEdit(edits);
                    }

                    e.Handled = true;
                }
            }


            // Supprimer (DEL)
            else if (e.Key == Key.Delete)
            {
                var edits = new List<CellEditModel>();

                foreach (var region in sourceGrid.Selection.GetSelectionRegion())
                {
                    for (int row = region.Start.Row; row <= region.End.Row; row++)
                    {
                        for (int col = region.Start.Column; col <= region.End.Column; col++)
                        {
                            if (sourceGrid[row, col] is SourceGrid.Cells.Cell cell)
                            {
                                string oldVal = cell.Value?.ToString();
                                if (!string.IsNullOrEmpty(oldVal))
                                {
                                    edits.Add(new CellEditModel
                                    {
                                        Row = row - 1,
                                        Column = col,
                                        OldValue = oldVal,
                                        NewValue = ""
                                    });

                                    cell.Value = "";
                                }
                            }
                        }
                    }
                }
                RegisterEdit(edits);
                e.Handled = true;
            }

            // Enregistrer (Ctrl + S)
            else if (e.Key == Key.S && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                _ = SaveToCurrentFileAsync();
                e.Handled = true;
            }


            // Sélectionner tout (Ctrl + A)
            else if (e.Key == Key.A && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                // ✅ Si une cellule est en édition (TextBox visible), sélectionne tout le texte
                if (sourceGrid.Controls.Count > 0 &&
                    sourceGrid.Controls[0] is System.Windows.Forms.TextBox editor &&
                    editor.Visible)
                {
                    editor.SelectAll();
                    e.Handled = true;
                }
                else
                {
                    // ✅ Sinon, sélectionne tout le tableau
                    sourceGrid.Selection.SelectRange(
                        new SourceGrid.Range(1, 0, sourceGrid.RowsCount - 1, sourceGrid.ColumnsCount - 1), true);
                    e.Handled = true;
                }
            }


            // Annuler (Ctrl + Z)
            else if (e.Key == Key.Z && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                UndoLastEdit();
                e.Handled = true;
            }


        }
    
    
    }
}
