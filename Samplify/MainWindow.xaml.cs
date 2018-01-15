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
        

        public MainWindow()
        {
            SamplifyEngine.loadUserData(); //loads saved directories
            InitializeComponent();

            sampleListView.ItemsSource = SamplifyEngine.currentSamples;

            SamplifyEngine.updateAllSamples();
            sampleListView.Items.Refresh();
            //update listview
            
            
            resetTreeView();
            
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = (sender as TextBox).Text;
            SamplifyEngine.UpdateCurrentSamples(text);
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
            SamplifyEngine.saveDirectoriesToRegistry();
            SamplifyEngine.saveTagsToRegistry();
        }
        private void editDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            DirectoryEditorWindow window = new DirectoryEditorWindow(SamplifyEngine.directories.ToArray());
            
            if (window.ShowDialog() == true)
            {
                SamplifyEngine.directories = window.directories;
                SamplifyEngine.updateAllSamples();
                resetTreeView();
            }
        }

        TreeViewItem dummyNode = new TreeViewItem();
        public void resetTreeView()
        {
            directoryTreeViewer.Items.Clear();
            foreach (string s in SamplifyEngine.directories)
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

        private void addTagButton_Click(object sender, RoutedEventArgs e)
        {
            TagCreator c = new TagCreator();
            if(c.ShowDialog() == true)
            {
                SamplifyEngine.createNewTag(c.tagTitle.Text, (Color)c.colorPicker.SelectedColor);
            }

        }

        

        private void refreshLibrariesButton_Click(object sender, RoutedEventArgs e)
        {
            SamplifyEngine.regenerateLibrary();
        }
    }
}
