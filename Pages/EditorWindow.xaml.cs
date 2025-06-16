using Microsoft.Win32;
using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using PLManager.Model;
using CsvHelper;
using CsvHelper.Configuration;
using SourceGrid;
using System.Windows.Input;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms.VisualStyles;
using System.Threading.Tasks;

namespace PLManager.Windows
{
    public partial class EditorWindow : Window
    {
        private DataTable dataTable;
        private Grid sourceGrid;
        private string currentFilePath;
        private Stack<List<CellEditModel>> undoStack = new Stack<List<CellEditModel>>();
        private Stack<List<CellEditModel>> redoStack = new Stack<List<CellEditModel>>();


        public EditorWindow()
        {
            InitializeComponent();
            InitializeSourceGrid();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void InitializeSourceGrid()
        {
            sourceGrid = new Grid
            {
                Dock = System.Windows.Forms.DockStyle.Fill,
                BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
            };

            GridHost.Child = sourceGrid;
            KeyDown += CSVEditorWindow_KeyDown;
        }

        private void ImportCsv_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                Title = "Sélectionner un fichier CSV"
            };

            if (dialog.ShowDialog() == true)
            {
                currentFilePath = dialog.FileName;
                using (var reader = new StreamReader(dialog.FileName, Encoding.UTF8))
                {
                    var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                    {
                        Delimiter = ";",
                        Encoding = Encoding.UTF8,
                        MissingFieldFound = null,
                        HeaderValidated = null
                    };

                    using (var csv = new CsvReader(reader, config))
                    using (var dr = new CsvDataReader(csv))
                    {
                        dataTable = new DataTable();
                        dataTable.Load(dr);

                        // ✅ Désactive ReadOnly sur toutes les colonnes
                        foreach (DataColumn col in dataTable.Columns)
                        {
                            col.ReadOnly = false;
                        }
                    }

                    DisplayDataInGrid();
                }
                Title = $"Éditeur CSV - {System.IO.Path.GetFileName(currentFilePath)}";
            }
        }


        private void DisplayDataInGrid()
        {
            sourceGrid.Redim(dataTable.Rows.Count + 1, dataTable.Columns.Count);

            for (int col = 0; col < dataTable.Columns.Count; col++)
                sourceGrid[0, col] = new SourceGrid.Cells.ColumnHeader(dataTable.Columns[col].ColumnName);

            for (int row = 0; row < dataTable.Rows.Count; row++)
            {
                for (int col = 0; col < dataTable.Columns.Count; col++)
                {
                    string cellValue = dataTable.Rows[row][col]?.ToString();
                    var editableCell = new SourceGrid.Cells.Cell(cellValue);
                    var textEditor = new SourceGrid.Cells.Editors.TextBox(typeof(string));
                    editableCell.Editor = textEditor;
                    sourceGrid[row + 1, col] = editableCell;

                    int rowCopy = row;
                    int colCopy = col;

                    textEditor.Control.Validated += (s, e) =>
                    {
                        var control = s as System.Windows.Forms.Control;
                        var newVal = control.Text;
                        var oldVal = dataTable.Rows[rowCopy][colCopy]?.ToString();

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

                            dataTable.Rows[rowCopy][colCopy] = newVal;
                        }
                    };
                }
            }
            sourceGrid.AutoSizeCells();
        }

        private void SaveAsCsv_Click(object sender, RoutedEventArgs e)
        {
            if (dataTable == null)
            {
                MessageBox.Show("Aucune donnée à enregistrer.");
                return;
            }

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Fichier CSV (*.csv)|*.csv",
                Title = "Enregistrer sous..."
            };

            if (dialog.ShowDialog() == true)
            {
                using (var writer = new StreamWriter(dialog.FileName, false, Encoding.UTF8))
                using (var csv = new CsvHelper.CsvWriter(writer, new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ";",
                    NewLine = "\n"
                }))
                {
                    foreach (DataColumn column in dataTable.Columns)
                        csv.WriteField(column.ColumnName);
                    csv.NextRecord();

                    foreach (DataRow row in dataTable.Rows)
                    {
                        foreach (var cell in row.ItemArray)
                            csv.WriteField(cell?.ToString());
                        csv.NextRecord();
                    }
                }

                MessageBox.Show("✅ Fichier enregistré sous avec succès !");
            }
        }

        private void RegisterEdit(List<CellEditModel> edits)
        {
            if (edits != null && edits.Count > 0)
            {
                undoStack.Push(edits);
                redoStack.Clear(); 
            }
        }

        private void RedoLastEdit()
        {
            if (redoStack.Count == 0)
                return;

            var lastRedo = redoStack.Pop();
            var inverse = new List<CellEditModel>();

            foreach (var edit in lastRedo)
            {
                int gridRow = edit.Row + 1; 
                int gridCol = edit.Column;

                if (gridRow < sourceGrid.RowsCount && gridCol < sourceGrid.ColumnsCount)
                {
                    if (sourceGrid[gridRow, gridCol] is SourceGrid.Cells.Cell cell)
                    {
                        var currentVal = cell.Value?.ToString();

                        // Appliquer la modif
                        cell.Value = edit.NewValue;

                        // Mettre à jour le DataTable aussi
                        if (edit.Row < dataTable.Rows.Count && edit.Column < dataTable.Columns.Count)
                        {
                            dataTable.Rows[edit.Row][edit.Column] = edit.NewValue;
                        }

                        // On prépare l'inverse pour le UNDO suivant
                        inverse.Add(new CellEditModel
                        {
                            Row = edit.Row,
                            Column = edit.Column,
                            OldValue = currentVal,
                            NewValue = edit.NewValue
                        });
                    }
                }
            }

            undoStack.Push(inverse);
            sourceGrid.Invalidate(); 
        }




        private void UndoLastEdit()
        {
            if (undoStack.Count == 0)
                return;

            var lastEdit = undoStack.Pop();
            var inverse = new List<CellEditModel>();

            foreach (var edit in lastEdit)
            {
                if (edit.Row + 1 < sourceGrid.RowsCount && edit.Column < sourceGrid.ColumnsCount)
                {
                    if (sourceGrid[edit.Row + 1, edit.Column] is SourceGrid.Cells.Cell cell)
                    {
                        var currentVal = cell.Value?.ToString();
                        inverse.Add(new CellEditModel
                        {
                            Row = edit.Row,
                            Column = edit.Column,
                            OldValue = currentVal,
                            NewValue = edit.OldValue
                        });

                        cell.Value = edit.OldValue;
                        dataTable.Rows[edit.Row][edit.Column] = edit.OldValue;
                    }
                }
            }

            redoStack.Push(inverse);
            sourceGrid.Invalidate();
        }



        private void SaveCsv_Click(object sender, RoutedEventArgs e)
        {
            if (dataTable == null)
            {
                MessageBox.Show("Aucune donnée à enregistrer.");
                return;
            }

            var dialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                Title = "Enregistrer le fichier CSV"
            };

            if (dialog.ShowDialog() == true)
            {
                using (var writer = new StreamWriter(dialog.FileName, false, Encoding.UTF8))
                {
                    var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                    {
                        Delimiter = ";",
                        NewLine = "\n"
                    };

                    using (var csv = new CsvWriter(writer, config))
                    {
                        foreach (DataColumn column in dataTable.Columns)
                            csv.WriteField(column.ColumnName);
                        csv.NextRecord();

                        foreach (DataRow row in dataTable.Rows)
                        {
                            foreach (var cell in row.ItemArray)
                                csv.WriteField(cell?.ToString());
                            csv.NextRecord();
                        }
                    }
                }

                MessageBox.Show("✅ CSV enregistré avec succès !");
            }
        }

        private void SyncGridToDataTable()
        {
            for (int row = 0; row < dataTable.Rows.Count; row++)
            {
                for (int col = 0; col < dataTable.Columns.Count; col++)
                {
                    if (sourceGrid[row + 1, col] is SourceGrid.Cells.Cell cell)
                    {
                        dataTable.Rows[row][col] = cell.Value?.ToString();
                    }
                }
            }
        }

        private async void SaveToCurrentFile_Click(object sender, RoutedEventArgs e)
        {
            SaveToCurrentFile();
        }

        private async Task ShowToastAsync(string message, int durationMs = 2000)
        {
            ToastNotification.Text = message;
            ToastContainer.Visibility = Visibility.Visible;

            await Task.Delay(durationMs);

            ToastContainer.Visibility = Visibility.Collapsed;
        }

        private async void SaveToCurrentFile()
        {
            if (string.IsNullOrEmpty(currentFilePath))
            {
                MessageBox.Show("❌ Aucun fichier d'origine pour enregistrer.");
                return;
            }

            SyncGridToDataTable();

            try
            {
                using (var writer = new StreamWriter(currentFilePath, false, Encoding.UTF8))
                using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ";",
                    NewLine = "\n"
                }))
                {
                    foreach (DataColumn col in dataTable.Columns)
                        csv.WriteField(col.ColumnName);
                    csv.NextRecord();

                    foreach (DataRow row in dataTable.Rows)
                    {
                        foreach (var cell in row.ItemArray)
                            csv.WriteField(cell?.ToString());
                        csv.NextRecord();
                    }
                }

                await ShowToastAsync("Fichier enregistré !");
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Erreur lors de l'enregistrement : " + ex.Message);
            }
        }


        private void CSVEditorWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == System.Windows.Input.Key.C)
            {
                var regions = sourceGrid.Selection.GetSelectionRegion();
                if (regions.Count > 0)
                {
                    var sb = new StringBuilder();
                    foreach (var region in regions)
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
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.V)
            {
                var pasted = Clipboard.GetText();
                if (!string.IsNullOrWhiteSpace(pasted))
                {
                    var lines = pasted.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                    // ✅ Cas 1 : une seule cellule copiée, collage dans plusieurs cellules sélectionnées
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
                                        string oldValue = cell.Value?.ToString();
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
                        // ✅ Cas 2 : collage multi-cellules (grille standard)
                        var start = sourceGrid.Selection.ActivePosition;
                        var edits = new List<CellEditModel>();

                        for (int i = 0; i < lines.Length; i++)
                        {
                            var values = lines[i].Split('\t');
                            for (int j = 0; j < values.Length; j++)
                            {
                                int r = start.Row + i;
                                int c = start.Column + j;

                                if (r < sourceGrid.RowsCount && c < sourceGrid.ColumnsCount &&
                                    sourceGrid[r, c] is SourceGrid.Cells.Cell cell)
                                {
                                    string oldVal = cell.Value?.ToString();
                                    string newVal = values[j];

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


            else if (e.Key == System.Windows.Input.Key.Delete)
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

                if (edits.Count > 0)
                    RegisterEdit(edits);

                e.Handled = true;
            }

            // Enregistrer (Ctrl + S)
            else if (e.Key == Key.S && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                SaveToCurrentFile();
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
            // REDO (Ctrl + Y)
            else if (e.Key == Key.Y && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                RedoLastEdit();
                e.Handled = true;
            }


            // UNDO (Ctrl + Z)
            else if (e.Key == Key.Z && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                UndoLastEdit();
                e.Handled = true;
            }


        }
    }
}
