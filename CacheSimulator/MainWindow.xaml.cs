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

namespace CacheSimulator
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
    }
}
