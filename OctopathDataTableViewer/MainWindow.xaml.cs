using DataEditorUE4.Models;
using DataEditorUE4.Utilities;
using FontAwesome.WPF;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OctopathDataTableViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void MenuItemOpen_Click(object sender, RoutedEventArgs e)
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
                StartLoading();
                try
                {
                    var currentTable = await OpenFile(uassetOFD.FileName, uexpOFD.FileName);
                    AddTab(currentTable);
                }
                catch(Exception error)
                {
                    MessageBox.Show(error.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                StopLoading();
            }
            else
            {
                MessageBox.Show(this, "Cannot open DB file.");
            }
        }

        private async void MenuItemSaveAs_Click(object sender, RoutedEventArgs e)
        {
            var currentTable = ((DataWrapper)((TabItem)TableTabs.SelectedItem).Content).CurrentTable;
            string fullUassetPath = currentTable.SourceUassetPath;
            string fullUexpPath = currentTable.SourceUexpPath;
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
                StartLoading();
                try
                {
                    await SaveFile(currentTable, uassetSFD.FileName, uexpSFD.FileName);
                }
                catch(Exception error)
                {
                    MessageBox.Show(error.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                StopLoading();
            }
            else
            {
                MessageBox.Show(this, "The operation has been cancelled.");
            }
        }

        private void AddTab(UEDataTable table)
        {
            foreach(var tab in TableTabs.Items)
            {
                ((TabItem)tab).IsSelected = false;
            }
            var newTab = new CloseableTab();
            newTab.Title = table.TableName;
            newTab.Content = new DataWrapper(table);
            newTab.IsSelected = true;
            TableTabs.Items.Add(newTab);
        }

        private void StartLoading()
        {
            MainMenu.IsEnabled = false;
            TableTabs.IsEnabled = false;
            LoadingSpinner.Visibility = Visibility.Visible;
        }

        private void StopLoading()
        {
            LoadingSpinner.Visibility = Visibility.Hidden;
            MainMenu.IsEnabled = true;
            TableTabs.IsEnabled = true;
        }
        
        private async Task<UEDataTable> OpenFile(string uasset, string uexp)
        {
            return await Task.Run(() =>
            {
                return DataTableParser.CreateDataTable(uasset, uexp);
            });
        }

        private async Task SaveFile(UEDataTable currentTable, string uasset, string uexp)
        {
            await Task.Run(() =>
            {
                DataTableFileWriter.WriteTableToFile(currentTable, uasset, uexp);
            });
        }

        private void CloseButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var tabToClose = ((StackPanel)((ImageAwesome)sender).Parent).Parent;
        }
    }
}
