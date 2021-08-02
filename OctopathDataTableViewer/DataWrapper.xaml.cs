using DataEditorUE4.Models;
using DataEditorUE4.Utilities;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OctopathDataTableViewer
{
    /// <summary>
    /// Interaction logic for DataWrapper.xaml
    /// </summary>
    public partial class DataWrapper : UserControl
    {
        public UEDataTable CurrentTable { get; set; }
        public List<string> CurrentKeys { get; set; }
        public DataWrapper(UEDataTable table)
        {
            InitializeComponent();

            CurrentTable = table;
            CurrentKeys = CurrentTable.Rows.Keys.ToList();
            var formattedDictionary = CurrentTable.Rows.ToDictionary(x => x.Key, x => new UEDataTableCell(new UEDataTableColumn("Data", UE4PropertyType.StructProperty), x.Value));
            MainCanvas.Content = new DataRowViewer(formattedDictionary, null, true, true);
        }

        private void LoadRowsButton_Click(object sender, RoutedEventArgs e)
        {
            var mainViewer = (DataRowViewer)MainCanvas.Content;
            mainViewer.LoadRows(50);
        }

        private void KeySearchTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            var text = ((TextBox)e.Source).Text;
            var mainViewer = (DataRowViewer)MainCanvas.Content;
            mainViewer.FilterContentByKey(text);
        }

        private void AddRowButton_Click(object sender, RoutedEventArgs e)
        {
            string keyToAdd = RowKeyTextBox.Text;
            if (CurrentKeys.Contains(keyToAdd))
            {
                MessageBox.Show("This key already exists, please select a unique key!", "Duplicate key detected", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else if (RowKeyTextBox.Text == "")
            {
                MessageBox.Show("The key cannot be an empty string!", "Empty key detected", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                var result = MessageBox.Show($"This will add a new row with key {keyToAdd}, is that OK?", "Add row confirmation", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (result == MessageBoxResult.OK)
                {
                    var newObject = CurrentTable.Rows.ToList().First().Value.Copy();
                    CurrentKeys.Add(keyToAdd);
                    CurrentTable.Rows.Add(keyToAdd, newObject);
                    var formattedDictionary = CurrentTable.Rows.ToDictionary(x => x.Key, x => new UEDataTableCell(new UEDataTableColumn("Data", UE4PropertyType.StructProperty), x.Value));
                    MainCanvas.Content = new DataRowViewer(formattedDictionary, null, true, true);
                }
            }
        }

        private void CopyRowButton_Click(object sender, RoutedEventArgs e)
        {
            string keyToAdd = RowKeyTextBox.Text;
            string keyToCopy = CopyRowKeyTextBox.Text;
            if (CurrentKeys.Contains(keyToAdd))
            {
                MessageBox.Show("This key already exists, please select a unique key!", "Duplicate key detected", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else if (RowKeyTextBox.Text == "")
            {
                MessageBox.Show("The key cannot be an empty string!", "Empty key detected", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else if (!CurrentKeys.Contains(keyToCopy))
            {
                MessageBox.Show("The key to copy cannot be found!", "Can't find key", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                var result = MessageBox.Show($"This will add a new row with key {keyToAdd}, with data copied from {keyToCopy}. Is that OK?", "Add row confirmation", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (result == MessageBoxResult.OK)
                {
                    var newObject = CurrentTable.Rows[keyToCopy].Copy();
                    CurrentKeys.Add(keyToAdd);
                    CurrentTable.Rows.Add(keyToAdd, newObject);
                    var formattedDictionary = CurrentTable.Rows.ToDictionary(x => x.Key, x => new UEDataTableCell(new UEDataTableColumn("Data", UE4PropertyType.StructProperty), x.Value));
                    MainCanvas.Content = new DataRowViewer(formattedDictionary, null, true, true);
                }
            }
        }
    }
}
