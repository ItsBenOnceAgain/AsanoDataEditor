using DataEditorUE4.Models;
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
        public DataRowViewer(Dictionary<string, UEDataTableCell> childObjects)
        {
            InitializeComponent();
            ChildDataObjects = childObjects;
        }

        private void Viewer_Loaded(object sender, RoutedEventArgs e)
        {
            UEObjectDataPanel.Children.Clear();
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(20, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(10, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(70, GridUnitType.Star) });

            for (int i = 0; i < ChildDataObjects.Count; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition());
                var keyValuePair = ChildDataObjects.ToList()[i];
                var keyLabel = new Label();
                keyLabel.Content = keyValuePair.Key;
                Grid.SetColumn(keyLabel, 0);
                Grid.SetRow(keyLabel, i);
                grid.Children.Add(keyLabel);

                var cell = keyValuePair.Value;
                var element = GenerateElementFromCell(cell);
                Grid.SetColumn(element, cell.Column.ColumnType == UE4PropertyType.ArrayProperty || cell.Column.ColumnType == UE4PropertyType.StructProperty ? 1 : 2);
                Grid.SetRow(element, i);
                grid.Children.Add(element);
            }
            UEObjectDataPanel.Children.Add(grid);
        }

        private void ExpandStructButton_Clicked(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            Grid parent = (Grid)(button).Parent;
            int rowNum = Grid.GetRow(button);
            string key = (string)((Label)(parent.Children.Cast<UIElement>().First(element => Grid.GetRow(element) == rowNum && Grid.GetColumn(element) == 0))).Content;
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
            string key = (string)((Label)(parent.Children.Cast<UIElement>().First(element => Grid.GetRow(element) == rowNum && Grid.GetColumn(element) == 0))).Content;
            var dataObject = ChildDataObjects[key];
            var childViewer = CreateDataRowViewerFromArrayCell(dataObject);
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
                    break;
                default:
                    element = new TextBox();
                    Binding textBinding = new Binding("Value");
                    textBinding.Source = cell;
                    ((TextBox)element).SetBinding(TextBox.TextProperty, textBinding);
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
            return new DataRowViewer(childDictionary);
        }

        private DataRowViewer CreateDataRowViewerFromArrayCell(UEDataTableCell cell)
        {
            var childCellList = (List<UEDataTableCell>)cell.Value;
            var childDictionary = new Dictionary<string, UEDataTableCell>();
            for (int i = 0; i < childCellList.Count; i++)
            {
                childDictionary.Add(i.ToString(), childCellList[i]);
            }
            return new DataRowViewer(childDictionary);
        }

        private Button CreateExpandStructButton()
        {
            var expandButton = new Button();
            expandButton.Content = "expand";
            expandButton.Margin = new Thickness(5);
            expandButton.HorizontalAlignment = HorizontalAlignment.Left;
            expandButton.VerticalAlignment = VerticalAlignment.Center;
            expandButton.Click += ExpandStructButton_Clicked;
            return expandButton;
        }

        private Button CreateCollapseStructButton()
        {
            var collapseButton = new Button();
            collapseButton.Content = "collapse";
            collapseButton.Margin = new Thickness(5);
            collapseButton.HorizontalAlignment = HorizontalAlignment.Left;
            collapseButton.VerticalAlignment = VerticalAlignment.Top;
            collapseButton.Click += CollapseStructButton_Clicked;
            return collapseButton;
        }

        private Button CreateExpandArrayButton()
        {
            var expandButton = new Button();
            expandButton.Content = "expand";
            expandButton.Margin = new Thickness(5);
            expandButton.HorizontalAlignment = HorizontalAlignment.Left;
            expandButton.VerticalAlignment = VerticalAlignment.Center;
            expandButton.Click += ExpandArrayButton_Clicked;
            return expandButton;
        }

        private Button CreateCollapseArrayButton()
        {
            var collapseButton = new Button();
            collapseButton.Content = "collapse";
            collapseButton.Margin = new Thickness(5);
            collapseButton.HorizontalAlignment = HorizontalAlignment.Left;
            collapseButton.VerticalAlignment = VerticalAlignment.Top;
            collapseButton.Click += CollapseArrayButton_Clicked;
            return collapseButton;
        }
    }
}
