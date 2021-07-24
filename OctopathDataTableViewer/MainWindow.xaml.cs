using DataEditorUE4.Models;
using DataEditorUE4.Utilities;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

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
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void MenuItemOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog uassetOFD = new OpenFileDialog();
            uassetOFD.DefaultExt = "uasset";
            uassetOFD.Title = "Select DB uasset file";
            bool? uassetOFDResult = uassetOFD.ShowDialog();

            OpenFileDialog uexpOFD = new OpenFileDialog();
            uexpOFD.DefaultExt = "uexp";
            uexpOFD.Title = "Select DB uexp file";
            bool? uexpOFDResult = uexpOFD.ShowDialog();

            if (uassetOFDResult == true && uexpOFDResult == true)
            {
                CurrentTable = DataTableParser.CreateDataTable(uassetOFD.FileName, uexpOFD.FileName);
                CurrentKeys = CurrentTable.Rows.Keys.ToList();
                var formattedDictionary = CurrentTable.Rows.ToDictionary(x => x.Key, x => new UEDataTableCell(new UEDataTableColumn("Data", UE4PropertyType.StructProperty), x.Value));
                MainCanvas.Content = new DataRowViewer(formattedDictionary);
            }
            else
            {
                MessageBox.Show(this, "Cannot open DB file.");
            }
        }

        private void MenuItemSaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog uassetSFD = new SaveFileDialog();
            uassetSFD.DefaultExt = "uasset";
            uassetSFD.Title = "Select uasset save location";
            bool? uassetSFDResult = uassetSFD.ShowDialog();

            SaveFileDialog uexpSFD = new SaveFileDialog();
            uexpSFD.DefaultExt = "uexp";
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
    }
}
