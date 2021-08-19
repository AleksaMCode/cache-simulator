using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using CacheSimulation;
using Microsoft.Win32;
using RamGenerator;
using TraceGenerator;

namespace CacheSimulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string traceFileFullPath = null;
        private string ramFileFullPath = null;

        private CPU cpu = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void StartSimulation(object sender, RoutedEventArgs e)
        {
            try
            {
                var size = Int32.Parse(cacheSize.Text);
                var lineSize = Int32.Parse(cacheLineSize.Text);

                var associativity = cacheAssociativityComboBox.Text switch
                {
                    "Directly mapped" => 1,
                    "Fully associative" => size / lineSize,
                    /*"N-way set associative" */
                    _ => Int32.Parse(cacheAssociativity.Text)
                };

                cpu = new CPU((ramFileFullPath, traceFileFullPath, size, associativity, lineSize,
                    GetWritePolicy(cacheWritePolicyComboBox.Text), GetReplacementPolicy(cacheReplacementPolicyComboBox.Text)));

                cpu.StartSimulation();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private WritePolicy GetWritePolicy(string policy)
        {
            return policy switch
            {
                "Write-through" => WritePolicy.WriteBack,
                "Write allocate" => WritePolicy.WriteBack,
                "No-write allocate" => WritePolicy.WriteBack,
                /*"Write-back"*/
                _ => WritePolicy.WriteBack,
            };
        }

        private ReplacementPolicy GetReplacementPolicy(string policy)
        {
            return policy switch
            {
                "First in first out (FIFO)" => ReplacementPolicy.FirstInFirstOut,
                "Last in first out (LIFO)" => ReplacementPolicy.LastInFirstOut,
                "Bélády's algorithm" => ReplacementPolicy.Belady,
                "Time aware least recently used (TLRU)" => ReplacementPolicy.Belady,
                "Most recently used (MRU)" => ReplacementPolicy.Belady,
                "Random replacement (RR)" => ReplacementPolicy.Belady,
                "Least-frequently used (LFU)" => ReplacementPolicy.Belady,
                "LFU with dynamic aging (LFUDA)" => ReplacementPolicy.Belady,
                /*"Least recently used (LRU)"*/
                _ => ReplacementPolicy.LeastRecentlyUsed
            };
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

        private void GenerateTraceFile(object sender, RoutedEventArgs e)
        {
            var msgReply = MessageBox.Show($"Trace file will use {(int)ramSizeNumericUpDown.Value.Value} MB RAM size for generating address range and cache line size {cacheLineSize.Text} B for generating random data.", "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
            if (msgReply == MessageBoxResult.OK)
            {
                try
                {
                    var trace = new TraceGenerator.TraceGenerator(traceFileSizeComboBox.Text);
                    trace.GenerateTraceFile((int)ramSizeNumericUpDown.Value.Value, Int32.Parse(cacheLineSize.Text));
                    MessageBox.Show($"Trace file {trace.FileName} has been successfully created.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void GenerateRamFile(object sender, RoutedEventArgs e)
        {
            var msgReply = MessageBox.Show($"Trace file will use {(int)ramSizeNumericUpDown.Value.Value} MB RAM size for generating address range and cache line size {cacheLineSize.Text} B for generating random data.", "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
            if (msgReply == MessageBoxResult.OK)
            {
                try
                {
                    var ram = new RamGenerator.RamGenerator((int)ramSizeNumericUpDown.Value.Value);
                    ram.GenerateRam();
                    MessageBox.Show($"RAM file {ram.FileName} has been successfully created.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }
    }
}
