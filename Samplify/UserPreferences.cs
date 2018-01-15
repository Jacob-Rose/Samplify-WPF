using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Samplify
{
    public class UserPreferences
    {
        public static Brush defaultSampleColor = Brushes.Crimson;
        public static Brush baseUIColor = Brushes.White;
        public static Brush baseTextColor = Brushes.Black;
        public static Color defaultTagColor = Colors.Yellow;

        public static int waveformPointCount = 64;
    }
    public static class SamplifyEngine
    {
        public static SampleReference[] allSamples; //all valid samples in 
        public static List<SampleReference> currentSamples = new List<SampleReference>(); //samples that are currently shown in the listview
        //CHANGE TO DATAPROVIDER FOR OPTIMIZATION when using currentsamples in listview
        public static List<string> directories = new List<string>() { };
        public static List<Tag> allTags = new List<Tag>();

        public static void loadUserData()
        {
            loadDirectoriesFromRegistry();
            loadTagsFromRegistry();
        }

        public static void loadTagsFromRegistry()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Newrose\Samplify");
            if (key != null)
            {
                object dirs = NewroseLib.loadObjectFromRegistry(key, "AllTags");
                allTags.AddRange(dirs as Tag[]);
            }
        }

        /// <summary>
        /// Loads in directory information from registry
        /// </summary>
        public static void loadDirectoriesFromRegistry()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Newrose\Samplify");
            if (key != null)
            {
                object dirs = NewroseLib.loadObjectFromRegistry(key, "Directories");
                directories.AddRange(dirs as string[]);
            }
            if (directories.Count == 0)
            {
                string dir = NewroseLib.userSelectDirectoryDialog();
                if (dir != null)
                {
                    directories.Add(dir);
                }
            }

        }

        /// <summary>
        /// Saves the directory array to registry
        /// </summary>
        public static void saveDirectoriesToRegistry()
        {
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Newrose\Samplify");
            NewroseLib.SaveObjectToRegistry(key, "Directories", directories.ToArray());
        }

        public static void saveTagsToRegistry()
        {
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Newrose\Samplify");
            NewroseLib.SaveObjectToRegistry(key, "AllTags", allTags.ToArray());
        }

       
        
        /// <summary>
        /// Creates all sample objects from the directories in the directories array async
        /// </summary>
        public static void updateAllSamples()
        {
            //await Task.Run(() => allSamples = SampleReference.getAllValidSamplesInDirectories(directories.ToArray()));
            //await Task.Run(() => UpdateCurrentSamples(""));
            allSamples = SampleReference.getAllValidSamplesInDirectories(directories.ToArray());
            UpdateCurrentSamples("");
        }

        public static void UpdateCurrentSamples(string query)
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
        }

        public static void regenerateLibrary()
        {
            deleteSampFilesInDirectories();
            updateAllSamples();
        }
        public static void deleteSampFilesInDirectories()
        {
            NewroseLib.deleteSampFilesInDirectories(directories.ToArray());
        }

        public static void createNewTag(string name)
        {
            createNewTag(name, UserPreferences.defaultTagColor);
        }

        public static void createNewTag(string name, Color color)
        {
            Tag t = new Tag(name, color);
            allTags.Add(t);
        }
    }
}
