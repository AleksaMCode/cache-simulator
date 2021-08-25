using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using ControlzEx.Standard;
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
        private List<string> traceFileFullPaths = null;
        private string ramFileFullPath = null;

        public static List<Task> CoreTaskList = new List<Task>();

        private CPU cpu = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void StartSimulation(object sender, RoutedEventArgs e)
        {
            if (traceFileFullPaths == null)
            {
                MessageBox.Show("Please insert the trace file first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            else if (ramFileFullPath == null)
            {
                MessageBox.Show("Please insert the RAM file first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            else if (cacheSize.Text == "" || cacheLineSize.Text == "")
            {
                MessageBox.Show("Please fill out all of the cache parameters with values.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            EnableWindowComponents(false);
            cacheStatsTextBox.Text = "";

            try
            {
                var size = Int32.Parse(cacheSize.Text);
                var lineSize = Int32.Parse(cacheLineSize.Text);

                var associativity = GetCacheAssociativity(size, lineSize);
                var numberOfCores = GetNumberOfCores();

                cpu = new CPU((ramFileFullPath, size, associativity, lineSize,
                    GetWritePolicy(cacheWriteHitPolicyComboBox.Text), GetWritePolicy(cacheWriteMissPolicyComboBox.Text), GetReplacementPolicy(cacheReplacementPolicyComboBox.Text)), numberOfCores);

                // Set trace files for every core.
                for (var i = 0; i < numberOfCores; ++i)
                {
                    cpu.SetCoreTraceFile(i, traceFileFullPaths[i]);

                    var indx = i;

                    var task = Task.Run(() =>
                    {
                        const int bufferSize = 4_096;
                        using var fileStream = File.OpenRead(traceFileFullPaths[indx]);
                        using var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, bufferSize);

                        string line;
                        var traceIndex = 0;

                        while ((line = streamReader.ReadLine()) != null)
                        {
                            var cacheLogInfo = cpu.ExecuteTraceLine(line, ++traceIndex, indx);
                            if (cacheLogInfo != null)
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    cacheStatsTextBox.AppendText(cacheLogInfo.Replace("\n\n", "\n"));
                                    cacheStatsTextBox.ScrollToEnd();
                                });
                            }
                        }
                    });

                    CoreTaskList.Add(task);
                }

                cacheLogProgressRing.Visibility = Visibility.Visible;
                cacheLogProgressRing.IsActive = true;

                try
                {
                    await Task.WhenAll(CoreTaskList.ToArray());
                }
                catch (Exception/*OperationCanceledException*/)
                {
                    cacheLogProgressRing.Visibility = Visibility.Hidden;
                    cacheLogProgressRing.IsActive = false;
                }

                cacheLogProgressRing.Visibility = Visibility.Hidden;
                var sb = new StringBuilder();

                for (var i = 0; i < numberOfCores; ++i)
                {
                    sb.AppendLine(cpu.GetCacheStatistics(i));
                }

                // Output cache statistics for each core in to a file.
                var filename = $"cache_statistics-{DateTime.Now:yyyyMMddHHmmss}.txt";
                File.WriteAllText(filename, sb.ToString());
                Process.Start(filename);

                //MessageBox.Show(sb.ToString(), "Cache Statistics", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            EnableWindowComponents(true);
        }

        private int GetCacheAssociativity(int cacheSize, int lineSize)
        {
            return cacheAssociativityComboBox.Text switch
            {
                "Directly mapped" => 1,
                "Fully associative" => cacheSize / lineSize,
                /*"N-way set associative" */
                _ => Int32.Parse(cacheAssociativity.Text)
            };
        }

        private int GetNumberOfCores()
        {
            return numberOfCoresComboBox.Text switch
            {
                "Single-core" => 1,
                //"Dual-core" => 2,
                _ => 2
            };
        }

        private void EnableWindowComponents(bool value)
        {
            simulationContolsGrid.IsEnabled = cacheParametersGrid.IsEnabled = memoryGeneratorsGrid.IsEnabled = value;
        }

        private WritePolicy GetWritePolicy(string policy)
        {
            return policy switch
            {
                "Write-through" => WritePolicy.WriteThrough,
                "Write allocate" => WritePolicy.WriteAllocate,
                "No-write allocate" => WritePolicy.WriteAround,
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
                "Time aware least recently used (TLRU)" => ReplacementPolicy.TimeAwareLeastRecentlyUsed,
                "Most recently used (MRU)" => ReplacementPolicy.MostRecentlyUsed,
                "Random replacement (RR)" => ReplacementPolicy.RandomReplacement,
                "Least-frequently used (LFU)" => ReplacementPolicy.LeastFrequentlyUsed,
                "LFU with dynamic aging (LFUDA)" => ReplacementPolicy.LeastFrequentlyUsedWithDynamicAging,
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
            if (FilePicker())
            {
                traceFileNameTextBox.Text = traceFileFullPaths.Aggregate((i, j) => i + "; " + j);
            }
        }

        private void RamFilePicker(object sender, RoutedEventArgs e)
        {
            if (FilePicker(false))
            {
                ramFileNameTextBox.Text = ramFileFullPath.Substring(ramFileFullPath.LastIndexOf('\\') + 1);
            }
        }

        private bool FilePicker(bool isItTraceFile = true)
        {
            var fileChooseDialog = new OpenFileDialog()
            {
                Multiselect = isItTraceFile
            };

            var dialog = fileChooseDialog.ShowDialog();

            if (dialog.HasValue && dialog.Value)
            {
                if (isItTraceFile)
                {
                    if (fileChooseDialog.FileNames.Length != GetNumberOfCores())
                    {
                        MessageBox.Show($"Every core has to have unique trace file.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return false;
                    }

                    traceFileFullPaths = new List<string>(fileChooseDialog.FileNames.Length);

                    foreach (var traceFile in fileChooseDialog.FileNames)
                    {
                        traceFileFullPaths.Add(traceFile);
                    }
                }
                else
                {
                    ramFileFullPath = fileChooseDialog.FileName;
                }
            }
            else
            {
                MessageBox.Show($"There was an error while getting your {(isItTraceFile ? "trace file(s)." : "ram file.")}", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private async void GenerateTraceFile(object sender, RoutedEventArgs e)
        {
            var cacheBlockize = Int32.Parse(cacheLineSize.Text);

            if (cacheLineSize.Text == "")
            {
                MessageBox.Show("Please enter the cache line size first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else if (!Cache.CheckNumberForPowerOfTwo(cacheBlockize))
            {
                MessageBox.Show("Cache line is not a power of 2.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                var msgReply = MessageBox.Show($"Trace file will use {(int)ramSizeNumericUpDown.Value.Value} MB RAM size for generating address range and cache line size {cacheLineSize.Text} B for generating random data.", "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (msgReply == MessageBoxResult.OK)
                {
                    try
                    {
                        var trace = new TraceGenerator.TraceGenerator(traceFileSizeComboBox.Text);

                        traceFileProgressRing.IsActive = true;
                        traceFileProgressRing.Visibility = Visibility.Visible;
                        EnableWindowComponents(false);

                        var ramSize = (int)ramSizeNumericUpDown.Value.Value;

                        var task = Task.Run(() => trace.GenerateTraceFile(ramSize, cacheBlockize));
                        await task;

                        traceFileProgressRing.IsActive = false;
                        traceFileProgressRing.Visibility = Visibility.Collapsed;
                        EnableWindowComponents(true);

                        MessageBox.Show($"Trace file {trace.FileName} has been successfully created.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
        }

        private async void GenerateRamFile(object sender, RoutedEventArgs e)
        {
            try
            {
                var ram = new RamGenerator.RamGenerator((int)ramSizeNumericUpDown.Value.Value);

                ramFileProgressRing.IsActive = true;
                ramFileProgressRing.Visibility = Visibility.Visible;
                EnableWindowComponents(false);

                var task = Task.Run(() => ram.GenerateRam());
                await task;

                ramFileProgressRing.IsActive = false;
                ramFileProgressRing.Visibility = Visibility.Collapsed;
                EnableWindowComponents(true);

                MessageBox.Show($"RAM file {ram.FileName} has been successfully created.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
