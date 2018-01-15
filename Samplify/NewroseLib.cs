using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Samplify
{
    public static class NewroseLib
    {
        /// <summary>
        /// Converts object into a single string using BinaryFormatter
        /// </summary>
        /// <example>
        /// Converting an array to a string to save it to the registry
        /// </example>
        /// <param name="obj">the object to convert to a string</param>
        /// <returns>converted string</returns>
        public static void SaveObjectToRegistry(RegistryKey key, string name, object obj)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                new BinaryFormatter().Serialize(ms, obj);
                key.SetValue(name, ms.ToArray(), RegistryValueKind.Binary);
            }
        }
        /// <summary>
        /// Converts a string from BinaryFormatter back into an object
        /// </summary>
        /// <param name="base64String">string to convert back to an object</param>
        /// <returns>converted object</returns>
        public static object loadObjectFromRegistry(RegistryKey key, string name)
        {
            byte[] bytes = (byte[])key.GetValue(name);
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream b = new MemoryStream(bytes))
            {
                return bf.Deserialize(b);
            }
        }

        /// <summary>
        /// Creates a OpenFileDialog that selects a single directory
        /// </summary>
        /// <returns>Path of user selected directory</returns>
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

        public static void saveObjectToFile(string filePath,object obj)
        {
            StreamWriter writer = new StreamWriter(filePath);
            new BinaryFormatter().Serialize(writer.BaseStream, obj);
            writer.Close();
        }
        public static object loadObjectFromFile(string filePath)
        {
            StreamReader reader = new StreamReader(filePath);
            object obj = new BinaryFormatter().Deserialize(reader.BaseStream);
            reader.Close(); //not sure if necessary in a method of this size but just being safe
            return obj;
        }

        public static void deleteSampFilesInDirectories(string[] directories)
        {
            foreach(string dir in directories)
            {
                deleteSampFilesInDirectory(dir);
            }
        }

        public static void deleteSampFilesInDirectory(string directory)
        {
            string[] files = Directory.GetFiles(directory, "*.samp", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                Console.WriteLine("Deleted " + file);
                File.Delete(file);
            }
        }
    }
}
