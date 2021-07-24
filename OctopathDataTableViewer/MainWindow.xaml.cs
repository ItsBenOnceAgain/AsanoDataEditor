using DataEditorUE4.Models;
using DataEditorUE4.Utilities;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace OctopathDataTableViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public UEDataTable CurrentTable { get; set; }
        public List<string> CurrentKeys { get; set; }
        public UEDataTableObject CurrentSelectedObject { get; set; }
        public bool TableHasLoaded { get; set; }
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TableHasLoaded = false;
            AddRowButton.IsEnabled = false;
            RowKeyTextBox.IsEnabled = false;
            KeySearchTextBox.IsEnabled = false;
            LoadRowsButton.IsEnabled = false;
        }

        private void MenuItemOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog uassetOFD = new OpenFileDialog();
            uassetOFD.DefaultExt = "uasset";
            uassetOFD.Filter = "Unreal Asset Header Files (*.uasset) | *.uasset";
            uassetOFD.Title = "Select DB uasset file";
            bool? uassetOFDResult = uassetOFD.ShowDialog();

            OpenFileDialog uexpOFD = new OpenFileDialog();
            uexpOFD.DefaultExt = "uexp";
            uexpOFD.Filter = "Unreal Asset Expanded Files (*.uexp) | *.uexp";
            uexpOFD.Title = "Select DB uexp file";
            bool? uexpOFDResult = uexpOFD.ShowDialog();

            if (uassetOFDResult == true && uexpOFDResult == true)
            {
                CurrentTable = DataTableParser.CreateDataTable(uassetOFD.FileName, uexpOFD.FileName);
                CurrentKeys = CurrentTable.Rows.Keys.ToList();
                var formattedDictionary = CurrentTable.Rows.ToDictionary(x => x.Key, x => new UEDataTableCell(new UEDataTableColumn("Data", UE4PropertyType.StructProperty), x.Value));
                MainCanvas.Content = new DataRowViewer(formattedDictionary, null, true);
                TableHasLoaded = true;
                AddRowButton.IsEnabled = true;
                RowKeyTextBox.IsEnabled = true;
                KeySearchTextBox.IsEnabled = true;
                LoadRowsButton.IsEnabled = true;
            }
            else
            {
                MessageBox.Show(this, "Cannot open DB file.");
            }
        }

        private void MenuItemSaveAs_Click(object sender, RoutedEventArgs e)
        {
            string fullUassetPath = CurrentTable.SourceUassetPath;
            string fullUexpPath = CurrentTable.SourceUexpPath;
            string[] fullUassetPathPieces = fullUassetPath.Split(@"\");
            string[] fullUexpPathPieces = fullUexpPath.Split(@"\");
            string uassetDirectory = string.Join(@"\", fullUassetPathPieces.Take(fullUassetPathPieces.Length - 1));
            string uexpDirectory = string.Join(@"\", fullUexpPathPieces.Take(fullUexpPathPieces.Length - 1));
            string uassetFileName = fullUassetPathPieces.Last();
            string uexpFileName = fullUexpPathPieces.Last();

            SaveFileDialog uassetSFD = new SaveFileDialog();
            uassetSFD.DefaultExt = "uasset";
            uassetSFD.FileName = uassetFileName;
            uassetSFD.InitialDirectory = uassetDirectory;
            uassetSFD.Filter = "Unreal Asset Header Files (*.uasset) | *.uasset";
            uassetSFD.Title = "Select uasset save location";
            bool? uassetSFDResult = uassetSFD.ShowDialog();

            SaveFileDialog uexpSFD = new SaveFileDialog();
            uexpSFD.DefaultExt = "uexp";
            uexpSFD.FileName = uexpFileName;
            uexpSFD.InitialDirectory = uexpDirectory;
            uexpSFD.Filter = "Unreal Asset Expanded Files (*.uexp) | *.uexp";
            uexpSFD.Title = "Select uexp save location";
            bool? uexpSFDResult = uexpSFD.ShowDialog();

            if (uassetSFDResult == true && uexpSFDResult == true)
            {
                DataTableFileWriter.WriteTableToFile(CurrentTable, uassetSFD.FileName, uexpSFD.FileName);
            }
            else
            {
                MessageBox.Show(this, "The operation has been cancelled.");
            }
        }

        private void LoadRowsButton_Click(object sender, RoutedEventArgs e)
        {
            if (TableHasLoaded)
            {
                var mainViewer = (DataRowViewer)MainCanvas.Content;
                mainViewer.LoadRows(50);
            }
        }

        private void KeySearchTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (TableHasLoaded)
            {
                var text = ((TextBox)e.Source).Text;
                var mainViewer = (DataRowViewer)MainCanvas.Content;
                mainViewer.FilterContentByKey(text);
            }
        }

        private void AddRowButton_Click(object sender, RoutedEventArgs e)
        {
            if(TableHasLoaded)
            {
                string keyToAdd = RowKeyTextBox.Text;
                if (CurrentKeys.Contains(keyToAdd))
                {
                    MessageBox.Show("This key already exists, please select a unique key!", "Duplicate key detected", MessageBoxButton.OK);
                }
                else if (RowKeyTextBox.Text == "")
                {
                    MessageBox.Show("The key cannot be an empty string!", "Empty key detected", MessageBoxButton.OK);
                }
                else
                {
                    var result = MessageBox.Show($"This will add a new row with key {keyToAdd}, is that OK?", "Add row confirmation", MessageBoxButton.OKCancel);
                    if(result == MessageBoxResult.OK)
                    {
                        var newObject = CurrentTable.Rows.ToList().First().Value.Copy();
                        CurrentKeys.Add(keyToAdd);
                        CurrentTable.Rows.Add(keyToAdd, newObject);
                        var formattedDictionary = CurrentTable.Rows.ToDictionary(x => x.Key, x => new UEDataTableCell(new UEDataTableColumn("Data", UE4PropertyType.StructProperty), x.Value));
                        MainCanvas.Content = new DataRowViewer(formattedDictionary, null, true);
                    }
                }
            }
        }
    }
}
