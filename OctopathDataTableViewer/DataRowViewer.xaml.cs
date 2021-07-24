using DataEditorUE4.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace OctopathDataTableViewer
{
    /// <summary>
    /// Interaction logic for ObjectViewer.xaml
    /// </summary>
    public partial class DataRowViewer : UserControl
    {
        public Dictionary<string, UEDataTableCell> ChildDataObjects { get; set; }
        public Dictionary<string, UEDataTableCell> CurrentlyDisplayableObjects { get; set; }
        public int CurrentRowsDisplayed { get; set; }
        public bool IsMasterViewer { get; set; }
        public UEDataTableCell ParentCell { get; set; }
        public DataRowViewer(Dictionary<string, UEDataTableCell> childObjects, UEDataTableCell parent, bool isMaster = false)
        {
            InitializeComponent();
            ChildDataObjects = childObjects;
            CurrentlyDisplayableObjects = childObjects;
            IsMasterViewer = isMaster;
            ParentCell = parent;
        }

        private void Viewer_Loaded(object sender, RoutedEventArgs e)
        {
            UEObjectDataPanel.Children.Clear();
            UEObjectDataPanel.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto) });
            UEObjectDataPanel.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto) });
            UEObjectDataPanel.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto) });
            LoadRows(IsMasterViewer ? 50 : ChildDataObjects.Count);
        }

        private void ExpandStructButton_Clicked(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            Grid parent = (Grid)(button).Parent;
            int rowNum = Grid.GetRow(button);
            string key = (string)((TextBlock)(parent.Children.Cast<UIElement>().First(element => Grid.GetRow(element) == rowNum && Grid.GetColumn(element) == 0))).Text;
            var dataObject = ChildDataObjects[key];
            var childViewer = CreateDataRowViewerFromStructCell(dataObject);
            Grid.SetRow(childViewer, rowNum);
            Grid.SetColumn(childViewer, 2);
            parent.Children.Remove(button);

            var collapseButton = CreateCollapseStructButton();
            Grid.SetRow(collapseButton, rowNum);
            Grid.SetColumn(collapseButton, 1);
            parent.Children.Add(collapseButton);
            parent.Children.Add(childViewer);
        }

        private void CollapseStructButton_Clicked(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            Grid parent = (Grid)(button).Parent;
            int rowNum = Grid.GetRow(button);
            var childViewer = (DataRowViewer)parent.Children.Cast<UIElement>().First(element => Grid.GetRow(element) == rowNum && Grid.GetColumn(element) == 2);
            var expandButton = CreateExpandStructButton();
            Grid.SetRow(expandButton, rowNum);
            Grid.SetColumn(expandButton, 1);
            parent.Children.Remove(button);
            parent.Children.Remove(childViewer);
            parent.Children.Add(expandButton);
        }

        private void ExpandArrayButton_Clicked(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            Grid parent = (Grid)(button).Parent;
            int rowNum = Grid.GetRow(button);
            string key = (string)((TextBlock)(parent.Children.Cast<UIElement>().First(element => Grid.GetRow(element) == rowNum && Grid.GetColumn(element) == 0))).Text;
            var dataObject = ChildDataObjects[key];
            var childViewer = CreateDataRowViewerFromArrayCell(dataObject);
            childViewer.AddArrayElementButtonGrid.Visibility = Visibility.Visible;
            Grid.SetRow(childViewer, rowNum);
            Grid.SetColumn(childViewer, 2);
            parent.Children.Remove(button);

            var collapseButton = CreateCollapseArrayButton();
            Grid.SetRow(collapseButton, rowNum);
            Grid.SetColumn(collapseButton, 1);
            parent.Children.Add(collapseButton);
            parent.Children.Add(childViewer);
        }

        private void CollapseArrayButton_Clicked(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            Grid parent = (Grid)(button).Parent;
            int rowNum = Grid.GetRow(button);
            var childViewer = (DataRowViewer)parent.Children.Cast<UIElement>().First(element => Grid.GetRow(element) == rowNum && Grid.GetColumn(element) == 2);
            var expandButton = CreateExpandArrayButton();
            Grid.SetRow(expandButton, rowNum);
            Grid.SetColumn(expandButton, 1);
            parent.Children.Remove(button);
            parent.Children.Remove(childViewer);
            parent.Children.Add(expandButton);
        }

        public void LoadRows(int rowsToLoad, bool reload = false)
        {
            if (reload)
            {
                CurrentRowsDisplayed = 0;
                UEObjectDataPanel.Children.Clear();
                UEObjectDataPanel.RowDefinitions.Clear();
            }
            int startingRows = CurrentRowsDisplayed;
            for (int i = startingRows; i < CurrentlyDisplayableObjects.Count && i < startingRows + rowsToLoad; i++)
            {
                UEObjectDataPanel.RowDefinitions.Add(new RowDefinition());
                var keyValuePair = CurrentlyDisplayableObjects.ToList()[i];
                var keyTextBlock = new TextBlock();
                keyTextBlock.Text = keyValuePair.Key;
                keyTextBlock.MinHeight = 20;
                keyTextBlock.Margin = new Thickness(5);
                Grid.SetColumn(keyTextBlock, 0);
                Grid.SetRow(keyTextBlock, i);
                UEObjectDataPanel.Children.Add(keyTextBlock);

                var cell = keyValuePair.Value;
                var element = GenerateElementFromCell(cell);
                Grid.SetColumn(element, cell.Column.ColumnType == UE4PropertyType.ArrayProperty || cell.Column.ColumnType == UE4PropertyType.StructProperty ? 1 : 2);
                Grid.SetRow(element, i);
                UEObjectDataPanel.Children.Add(element);
                CurrentRowsDisplayed = i + 1;
            }
        }

        public void FilterContentByKey(string key)
        {
            CurrentlyDisplayableObjects = ChildDataObjects.Where(x => x.Key.ToLower().Contains(key.ToLower())).ToDictionary(x => x.Key, x => x.Value);
            CurrentRowsDisplayed = 0;
            UEObjectDataPanel.Children.Clear();
            LoadRows(50);
        }

        private UIElement GenerateElementFromCell(UEDataTableCell cell)
        {
            UIElement element;
            switch (cell.Column.ColumnType)
            {
                case UE4PropertyType.StructProperty:
                    element = CreateExpandStructButton();
                    break;
                case UE4PropertyType.ArrayProperty:
                    element = CreateExpandArrayButton();
                    break;
                case UE4PropertyType.BoolProperty:
                    element = new CheckBox();
                    Binding checkBinding = new Binding("Value");
                    checkBinding.Source = cell;
                    ((CheckBox)element).SetBinding(CheckBox.IsCheckedProperty, checkBinding);
                    ((CheckBox)element).VerticalAlignment = VerticalAlignment.Center;
                    ((CheckBox)element).Margin = new Thickness(2);
                    break;
                default:
                    element = new TextBox();
                    Binding textBinding = new Binding("Value");
                    textBinding.Source = cell;
                    ((TextBox)element).SetBinding(TextBox.TextProperty, textBinding);
                    ((TextBox)element).VerticalAlignment = VerticalAlignment.Center;
                    ((TextBox)element).Margin = new Thickness(2);
                    ((TextBox)element).VerticalContentAlignment = VerticalAlignment.Center;
                    break;
            }
            return element;
        }

        private DataRowViewer CreateDataRowViewerFromStructCell(UEDataTableCell cell)
        {
            var childObject = (UEDataTableObject)cell.Value;
            var childDictionary = new Dictionary<string, UEDataTableCell>();
            foreach (var child in childObject.Cells)
            {
                childDictionary.Add(child.Column.ColumnName, child);
            }
            return new DataRowViewer(childDictionary, cell);
        }

        private DataRowViewer CreateDataRowViewerFromArrayCell(UEDataTableCell cell)
        {
            var childCellList = (List<UEDataTableCell>)cell.Value;
            var childDictionary = new Dictionary<string, UEDataTableCell>();
            for (int i = 0; i < childCellList.Count; i++)
            {
                childDictionary.Add(i.ToString(), childCellList[i]);
            }
            return new DataRowViewer(childDictionary, cell);
        }

        private Button CreateExpandStructButton()
        {
            var expandButton = new Button();
            expandButton.Content = "+";
            expandButton.Height = 20;
            expandButton.Width = 20;
            expandButton.Margin = new Thickness(5);
            expandButton.HorizontalAlignment = HorizontalAlignment.Left;
            expandButton.VerticalAlignment = VerticalAlignment.Top;
            expandButton.VerticalContentAlignment = VerticalAlignment.Center;
            expandButton.HorizontalAlignment = HorizontalAlignment.Center;
            expandButton.Click += ExpandStructButton_Clicked;
            return expandButton;
        }

        private Button CreateCollapseStructButton()
        {
            var collapseButton = new Button();
            collapseButton.Content = "-";
            collapseButton.Height = 20;
            collapseButton.Width = 20;
            collapseButton.Margin = new Thickness(5);
            collapseButton.HorizontalAlignment = HorizontalAlignment.Left;
            collapseButton.VerticalAlignment = VerticalAlignment.Top;
            collapseButton.VerticalContentAlignment = VerticalAlignment.Center;
            collapseButton.HorizontalAlignment = HorizontalAlignment.Center;
            collapseButton.Click += CollapseStructButton_Clicked;
            return collapseButton;
        }

        private Button CreateExpandArrayButton()
        {
            var expandButton = new Button();
            expandButton.Content = "+";
            expandButton.Height = 20;
            expandButton.Width = 20;
            expandButton.Margin = new Thickness(5);
            expandButton.HorizontalAlignment = HorizontalAlignment.Left;
            expandButton.VerticalAlignment = VerticalAlignment.Top;
            expandButton.VerticalContentAlignment = VerticalAlignment.Center;
            expandButton.HorizontalAlignment = HorizontalAlignment.Center;
            expandButton.Click += ExpandArrayButton_Clicked;
            return expandButton;
        }

        private Button CreateCollapseArrayButton()
        {
            var collapseButton = new Button();
            collapseButton.Content = "-";
            collapseButton.Height = 20;
            collapseButton.Width = 20;
            collapseButton.Margin = new Thickness(5);
            collapseButton.HorizontalAlignment = HorizontalAlignment.Left;
            collapseButton.VerticalAlignment = VerticalAlignment.Top;
            collapseButton.VerticalContentAlignment = VerticalAlignment.Center;
            collapseButton.HorizontalAlignment = HorizontalAlignment.Center;
            collapseButton.Click += CollapseArrayButton_Clicked;
            return collapseButton;
        }

        private void AddArrayElementButton_Click(object sender, RoutedEventArgs e)
        {
            string keyToAdd = (int.Parse(ChildDataObjects.Last().Key) + 1).ToString();
            var result = MessageBox.Show($"This will add a new row to this array, is that OK?", "Add row confirmation", MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.OK)
            {
                var newObject = ChildDataObjects.ToList().First().Value.Copy();
                ChildDataObjects.Add(keyToAdd, newObject);

                var parentCellsList = (List<UEDataTableCell>)ParentCell.Value;
                parentCellsList.Add(newObject);
                LoadRows(ChildDataObjects.Count, true);
            }
        }
    }
}
