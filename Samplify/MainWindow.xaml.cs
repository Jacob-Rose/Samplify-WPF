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
using System.Text;
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
using WaveFormRendererLib;

namespace Samplify
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SampleReference[] allSamples;
        List<SampleReference> currentSamples = new List<SampleReference>();
        List<string> directories = new List<string>() {  };
        public MainWindow()
        {
            InitializeComponent();
            setupDirectories();
            setupUserPreferences();
            updateAllSamples();
            UpdateCurrentSamples("");
            sampleListView.ItemsSource = currentSamples;
            resetTreeView();
            
        }

        public void setupUserPreferences()
        {
            UserPreferences.defaultSampleColor = Brushes.Crimson; //color of lines drawn
        }

        public void setupDirectories()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Newrose\Samplify");

            string[] directoryArray = (string[])NewroseLib.StringToObject((string)key.GetValue("Directories"));
            if(key != null)
            {
                var dirsToAdd = NewroseLib.StringToObject((string)key.GetValue("Directories"));
                if ((dirsToAdd as string[]).Length > 0)
                {
                    directories.AddRange(dirsToAdd as string[]);
                }
            }
            if(directories.Count == 0)
            {
                string dir = userSelectDirectoryDialog();
                if(dir != null)
                {
                    directories.Add(dir);
                }
            }

        }

        public static string userSelectDirectoryDialog()
        {
            CommonOpenFileDialog dF = new CommonOpenFileDialog();
            dF.Title = "Directory Finder";
            dF.IsFolderPicker = true;

            dF.Multiselect = false;
            dF.EnsureFileExists = true;
            dF.EnsurePathExists = true;
            dF.EnsureValidNames = true;
            dF.AllowNonFileSystemItems = false;
            dF.AddToMostRecentlyUsedList = false;

            if (dF.ShowDialog() == CommonFileDialogResult.Ok) //simple window dialog with a textbox to copy the path into
            {
                string userDirectory = dF.FileName;
                if (Directory.Exists(userDirectory))
                {
                    return userDirectory;
                }

            }
            return null;
        }
        public void saveDirectories()
        {
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Newrose\Samplify");
            key.SetValue("Directories", NewroseLib.ObjectToString(directories.ToArray()));
        }

        public SampleReference[] updateAllSamples(string[] directories)
        {
            List<SampleReference> samples = new List<SampleReference>();
            foreach(string dir in directories)
            {
                samples.AddRange(updateAllSamples(dir));
            }
            return samples.ToArray();
        }
        public SampleReference[] updateAllSamples(string directory)
        {
            LoadingWindow loadingWindow = new LoadingWindow();
            loadingWindow.Show();
            int count = 0;

            List<SampleReference> samples = new List<SampleReference>();
            
            string[] files = Directory.GetFiles(directory, "*.wav", SearchOption.AllDirectories);
            loadingWindow.progressBar.Maximum = files.Length;
            foreach (string file in files)
            {
                FileInfo fileInfo = new FileInfo(file);
                string sampFile = string.Concat(file, ".samp");
                count++;
                IFormatter formatter = new BinaryFormatter();
                if(File.Exists(sampFile))
                {
                    StreamReader reader = new StreamReader(sampFile);
                    SampleInfo sampInfo = (SampleInfo)formatter.Deserialize(reader.BaseStream);
                    if (sampInfo.fileSize == fileInfo.Length) //cheap way to verify file hasnt been modified, but not best method
                    {
                        SampleReference sample = new SampleReference(file, sampInfo.peakInfo);
                        samples.Add(sample);
                    }
                }
                else
                {
                    if (checkValidity(file))
                    {
                        SampleReference sample = new SampleReference(file);
                        //loadingWindow.loadingInfoTextBlock.Text = file;
                        //loadingWindow.progressBar.Value = count;
                        StreamWriter writer = new StreamWriter(sampFile);
                        SampleInfo sampInfo = new SampleInfo(fileInfo.Length, sample.waveformPoints);
                        formatter.Serialize(writer.BaseStream, sampInfo);

                        samples.Add(sample);
                        
                    }
                }
                Console.WriteLine(count + "/" + files.Count() + " | " + file);

            }
            loadingWindow.Close();

            return samples.ToArray();
        }
        public static bool checkValidity(string file)
        {
            bool validFile = true;
            try { using (AudioFileReader reader = new AudioFileReader(file)) { } } //simple test to see if file will work properly, checks for corrupt or misidentified
                catch (FormatException)
                {
                    Console.WriteLine("invalid file extension found");
                    validFile = false;
                }
            return validFile;
        }
        public void updateAllSamples()
        {
            allSamples = updateAllSamples(directories.ToArray());
            UpdateCurrentSamples("");
        }
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = (sender as TextBox).Text;
            UpdateCurrentSamples(text);
        }
        private void UpdateCurrentSamples(string query)
        {
            currentSamples.Clear();
            foreach (SampleReference rf in allSamples)
            {
                if (rf.fileName.ToUpper().Contains(query.ToUpper())) //uses toUpper to ignore case
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
                    string fileLoc = (sampleListView.SelectedItem as SampleReference).filePath;
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
    }
}
