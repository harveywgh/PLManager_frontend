using Microsoft.Win32;
using System;
using System.Data;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CsvHelper;
using System.Globalization;
using CsvHelper.Configuration;

namespace PLManager.Pages
{
    public partial class Veditor : Page
    {
        private DataTable dataTable;

        public Veditor()
        {
            InitializeComponent();
        }

        private void ImportCsv_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Fichiers CSV (*.csv)|*.csv",
                Title = "Importer un fichier CSV"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    using (var reader = new StreamReader(openFileDialog.FileName, Encoding.UTF8))
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

                            foreach (DataColumn col in dataTable.Columns)
                                col.ReadOnly = false;

                            ExcelGrid.ItemsSource = dataTable.DefaultView;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("❌ Erreur lors de l'import : " + ex.Message);
                }
            }
        }

        private void SaveCsv_Click(object sender, RoutedEventArgs e)
        {
            if (dataTable == null)
            {
                MessageBox.Show("Aucune donnée à enregistrer.");
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Fichier CSV (*.csv)|*.csv",
                Title = "Enregistrer sous",
                FileName = "modifié.csv"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    using (var writer = new StreamWriter(saveFileDialog.FileName, false, Encoding.UTF8))
                    {
                        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                        {
                            Delimiter = ";",
                            NewLine = "\n"
                        };

                        using (var csv = new CsvWriter(writer, config))
                        {
                            // En-têtes
                            foreach (DataColumn col in dataTable.Columns)
                                csv.WriteField(col.ColumnName);
                            csv.NextRecord();

                            // Lignes
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
                catch (Exception ex)
                {
                    MessageBox.Show("❌ Erreur d'enregistrement : " + ex.Message);
                }
            }
        }

        private void ExcelGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.C && Keyboard.Modifiers == ModifierKeys.Control)
            {
                ApplicationCommands.Copy.Execute(null, ExcelGrid);
                e.Handled = true;
            }

            if (e.Key == Key.V && Keyboard.Modifiers == ModifierKeys.Control)
            {
                ApplicationCommands.Paste.Execute(null, ExcelGrid);
                e.Handled = true;
            }

            if (e.Key == Key.Delete || e.Key == Key.Back)
            {
                foreach (var cell in ExcelGrid.SelectedCells)
                {
                    if (cell.Item is DataRowView row && cell.Column != null)
                    {
                        string columnName = cell.Column.Header.ToString();
                        row[columnName] = string.Empty;
                    }
                }
                e.Handled = true;
            }
        }
    }
}
