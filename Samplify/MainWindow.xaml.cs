using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Samplify
{
    /// <summary>
    /// MainWindow
    /// </summary>
    public partial class MainWindow : Window
    {
        SampleReference[] allSamples; //all valid samples in 
        List<SampleReference> currentSamples = new List<SampleReference>(); //samples that are currently shown in the listview
        List<string> directories = new List<string>() {  };
        public MainWindow()
        {
            setupUserPreferences(); //setup colors of UI
            InitializeComponent();
            setupDirectories(); 
            
            
            sampleListView.ItemsSource = currentSamples; 
            resetTreeView();
            
        }

        /// <summary>
        /// Setup colors of the UI
        /// </summary>
        public void setupUserPreferences()
        {
            //UserPreferences.defaultSampleColor = Brushes.AliceBlue; //color of lines drawn
        }

        /// <summary>
        /// Loads in directory information from registry
        /// </summary>
        public void setupDirectories()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Newrose\Samplify");
            if(key != null)
            {
                object dirs = NewroseLib.loadObjectFromRegistry(key, "Directories");
                directories.AddRange(dirs as string[]);
            }
            if(directories.Count == 0)
            {
                string dir = NewroseLib.userSelectDirectoryDialog();
                if(dir != null)
                {
                    directories.Add(dir);
                }
            }

        }

        /// <summary>
        /// Saves the directory array to registry
        /// </summary>
        public void saveDirectories()
        {
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Newrose\Samplify");
            NewroseLib.SaveObjectToRegistry(key, "Directories", directories.ToArray());
        }

        /// <summary>
        /// Creates all sample objects from the directories in the directories array
        /// </summary>
        public async void updateAllSamples()
        {
            await Task.Run(() => allSamples = SampleReference.getAllValidSamplesInDirectories(directories.ToArray()));
            UpdateCurrentSamples("");
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = (sender as TextBox).Text;
            UpdateCurrentSamples(text);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        private void UpdateCurrentSamples(string query)
        {
            currentSamples.Clear();
            foreach (SampleReference rf in allSamples)
            {
                if (rf.FileName.ToUpper().Contains(query.ToUpper())) //uses toUpper to ignore case
                {
                    //verify if file is in current dir
                    currentSamples.Add(rf);
                }
            }
            sampleListView.Items.Refresh();
        }

        Point _startPoint; //used to determine if dragging
        private void ListViewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(null);
        }
        private void ListViewPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point position = e.GetPosition(null);

                if (Math.Abs(position.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(position.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    string dataFormat = DataFormats.FileDrop;
                    string fileLoc = (sampleListView.SelectedItem as SampleReference).FilePath;
                    DataObject data = new DataObject(dataFormat, fileLoc);
                    data.SetFileDropList(new StringCollection() { fileLoc });
                    
                    Rectangle r = new Rectangle();
                    r.Fill = Brushes.Beige;
                    r.Width = 10;
                    r.Height = 10;
                    DragDrop.DoDragDrop(r, data, DragDropEffects.Copy);
                }
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            saveDirectories();
        }
        private void editDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            DirectoryEditorWindow window = new DirectoryEditorWindow(directories.ToArray());
            
            if (window.ShowDialog() == true)
            {
                directories = window.directories;
                updateAllSamples();
                resetTreeView();
            }
        }
        TreeViewItem dummyNode = new TreeViewItem();
        public void resetTreeView()
        {
            directoryTreeViewer.Items.Clear();
            foreach (string s in directories)
            {
                TreeViewItem item = new TreeViewItem();
                item.Header = new DirectoryInfo(s).Name;
                item.Tag = s;
                item.FontWeight = FontWeights.Normal;
                item.Items.Add(dummyNode);
                item.Expanded += new RoutedEventHandler(folder_Expanded);
                directoryTreeViewer.Items.Add(item);
            }

        }
        private void folder_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)sender;
            if (item.Items.Count == 1 && item.Items[0] == dummyNode)
            {
                item.Items.Clear();
                try
                {
                    foreach (string s in Directory.GetDirectories(item.Tag as string))
                    {
                        TreeViewItem subitem = new TreeViewItem();
                        subitem.Header = s.Substring(s.LastIndexOf("\\") + 1);
                        subitem.Tag = s;
                        subitem.FontWeight = FontWeights.Normal;
                        subitem.Items.Add(dummyNode);
                        subitem.Expanded += new RoutedEventHandler(folder_Expanded);
                        item.Items.Add(subitem);
                    }
                }
                catch (Exception) { }
            }
        }

        private void sampleListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //playSample(sampleListView.SelectedItem as SampleReference);
        }

        private void directoryTreeViewer_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            //update currentSamples to only include if in path
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            updateAllSamples();
        }
    }
}
