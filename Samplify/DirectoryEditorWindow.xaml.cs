using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Samplify
{
    /// <summary>
    /// Interaction logic for DirectoryEditorWindow.xaml
    /// </summary>
    public partial class DirectoryEditorWindow : Window
    {
        public List<string> directories = new List<string>();
        public DirectoryEditorWindow(string[] directories)
        {
            InitializeComponent();
            this.directories.AddRange(directories);
            directoryListView.ItemsSource = this.directories;

        }

        private void addDirButton_Click(object sender, RoutedEventArgs e)
        {
            string dir = MainWindow.userSelectDirectoryDialog();
            Activate(); //brings window to foreground after dialog
            if (dir != null)
            {
                Console.WriteLine("directory added");
                directories.Add(dir);
            }
            refreshListView();
        }

        private void rmDirButton_Click(object sender, RoutedEventArgs e)
        {
            directories.Remove(directoryListView.SelectedItem as string);
            refreshListView();
        }

        private void refreshListView()
        {
            directoryListView.Items.Refresh();
        }

        private void saveDirButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
