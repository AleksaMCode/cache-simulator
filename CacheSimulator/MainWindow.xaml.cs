using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace CacheSimulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string traceFileFullPath = null;
        private string ramFileFullPath = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void TextBoxNumberValidator(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void CacheAssociativityChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((((sender as ComboBox).SelectedItem as ComboBoxItem).Content as string) == "N-way set associative")
            {
                cacheAssociativity.Visibility = Visibility.Visible;
            }
            else if (cacheAssociativity != null)
            {
                cacheAssociativity.Visibility = Visibility.Collapsed;
            }
        }

        private void TraceFilePicker(object sender, RoutedEventArgs e)
        {
            FilePicker();
        }

        private void RamFilePicker(object sender, RoutedEventArgs e)
        {
            FilePicker(false);
        }

        private void FilePicker(bool isItTraceFile = true)
        {
            var fileChooseDialog = new OpenFileDialog();
            var dialog = fileChooseDialog.ShowDialog();

            if (dialog.HasValue && dialog.Value)
            {
                if (isItTraceFile)
                {
                    traceFileFullPath = fileChooseDialog.FileName;
                }
                else
                {
                    ramFileFullPath = fileChooseDialog.FileName;
                }
            }
            else
            {
                MessageBox.Show($"There was an error while getting your {(isItTraceFile ? "trace" : "ram")} file", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
