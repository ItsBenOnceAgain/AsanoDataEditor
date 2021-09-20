using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace AsanoDataEditor
{
    public class CloseableTab : TabItem
    {
        public CloseableTab()
        {
            var closableTabHeader = new CloseableHeader();

            closableTabHeader.button_close.MouseEnter +=
                new MouseEventHandler(button_close_MouseEnter);
            closableTabHeader.button_close.MouseLeave +=
               new MouseEventHandler(button_close_MouseLeave);
            closableTabHeader.button_close.Click +=
               new RoutedEventHandler(button_close_Click);

            this.Header = closableTabHeader;
        }

        public string Title
        {
            get
            {
                return (string)((CloseableHeader)this.Header).label_TabTitle.Content;
            }

            set
            {
                ((CloseableHeader)this.Header).label_TabTitle.Content = value;
            }
        }

        protected override void OnSelected(RoutedEventArgs e)
        {
            base.OnSelected(e);
            ((CloseableHeader)this.Header).button_close.Visibility = Visibility.Visible;
        }

        protected override void OnUnselected(RoutedEventArgs e)
        {
            base.OnUnselected(e);
            ((CloseableHeader)this.Header).button_close.Visibility = Visibility.Hidden;
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            ((CloseableHeader)this.Header).button_close.Visibility = Visibility.Visible;
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            if (!this.IsSelected)
            {
                ((CloseableHeader)this.Header).button_close.Visibility = Visibility.Hidden;
            }
        }

        void button_close_MouseEnter(object sender, MouseEventArgs e)
        {
            ((CloseableHeader)this.Header).button_close.Foreground = Brushes.Red;
        }

        void button_close_MouseLeave(object sender, MouseEventArgs e)
        {
            ((CloseableHeader)this.Header).button_close.Foreground = Brushes.Black;
        }

        void button_close_Click(object sender, RoutedEventArgs e)
        {
            var messageResult = MessageBox.Show($"Are you sure you want to close {Title}? Unsaved changes will be lost.", "Warning!", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
            if(messageResult == MessageBoxResult.OK)
            {
                ((TabControl)this.Parent).Items.Remove(this);
            }
        }
    }
}
