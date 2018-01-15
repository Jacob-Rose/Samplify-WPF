using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using WaveFormRendererLib;

namespace Samplify
{
    public class SampleReference
    { 
        public string FilePath { get; private set; }
        public FileInfo FileInfo { get { return new FileInfo(FilePath); } }
        public string FileName { get { return FileInfo.Name; } }
        public List<string> Tags { get; } = new List<string>();


        public PeakInfo[] WaveformPeaks { get; private set; }
        private int WaveformPointCount { get { return UserPreferences.waveformPointCount; } }
        public Brush SampleLineBrush { get { return UserPreferences.defaultSampleColor; } }




        public SampleReference(string file)
        {
            FilePath = file;
            WaveformPeaks = getSamplePeakArray(FilePath, WaveformPointCount);
        }
        public SampleReference(string file, PeakInfo[] waveformPeaks)
        {
            FilePath = file;
            this.WaveformPeaks = waveformPeaks;
        }
        public static SampleReference ToSampleReference(string file)
        {
            return new SampleReference(file);
        }
        public static SampleReference[] ToSampleReference(string[] fileNames)
        {
            SampleReference[] samples = new SampleReference[fileNames.Length];
            for(int i = 0; i < samples.Length; i++)
            {
                samples[i] = new SampleReference(fileNames[i]);
            }
            return samples;
        }

        public int getSampleBitRate()
        {
            using (var reader = new AudioFileReader(FilePath))
            {
                return reader.WaveFormat.BitsPerSample;
            }
        }
        public override string ToString()
        {
            return FileName;
        }

        public static PeakInfo[] getSamplePeakArray(string file, int desiredSamples)
        {
            PeakInfo[] samples = new PeakInfo[desiredSamples];
            MaxPeakProvider mpp = new MaxPeakProvider();
            using (var reader = new AudioFileReader(file))
            {
                int bytesPerSample = (reader.WaveFormat.BitsPerSample / 8);
                var sc = reader.Length / (bytesPerSample);
                var samplesPerPixel = (int)(sc / desiredSamples);
                mpp.Init(reader, samplesPerPixel - (samplesPerPixel%reader.WaveFormat.BlockAlign)); //this verifies the blockalign is set correctly
                samples = getSampleArray(mpp, desiredSamples);
            }
            return samples;
        }
        public static PeakInfo[] getSampleArray(IPeakProvider peakProvider, int sampleCount)
        {
            PeakInfo[] samples = new PeakInfo[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                samples[i] = peakProvider.GetNextPeak();
            }
            return samples;
        }

        public static SampleReference[] getAllValidSamplesInDirectories(string[] directories)
        {
            List<SampleReference> samples = new List<SampleReference>();
            foreach (string dir in directories)
            {
                samples.AddRange(getAllValidSamplesInDirectory(dir));
            }
            return samples.ToArray();
        }
        public static SampleReference[] getAllValidSamplesInDirectory(string directory)
        {
            int count = 0;

            List<SampleReference> samples = new List<SampleReference>();

            string[] files = Directory.GetFiles(directory, "*.wav", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                count++;
                SampleReference sample = getValidSample(file);
                if(sample != null)
                {
                    samples.Add(sample);
                }
                Console.WriteLine(count + "/" + files.Count() + " | " + file);
            }

            return samples.ToArray();
        }
        public static SampleReference getValidSample(string file)
        {
            FileInfo fileInfo = new FileInfo(file);
            string sampFile = string.Concat(file, ".samp");
            if (File.Exists(sampFile))
            {
                SampleInfo sampInfo = (SampleInfo)NewroseLib.loadObjectFromFile(sampFile);
                if (sampInfo.fileSize == fileInfo.Length) //cheap way to verify file hasnt been modified, but not best method
                {
                    SampleReference sample = new SampleReference(file, sampInfo.peakInfo);
                    return sample;
                }
            }
            else if (checkSampleValidity(file))
            {
                SampleReference sample = new SampleReference(file);
                SampleInfo sampInfo = new SampleInfo(fileInfo.Length, sample.WaveformPeaks, sample.Tags.ToArray());
                NewroseLib.saveObjectToFile(sampFile, sampInfo);
                return sample;
            }
            return null;
        }
        /// <summary>
        /// Determines if a file has no detectable errors that would cause a crash
        /// </summary>
        /// <param name="file">Path of file to check</param>
        /// <returns>if file is valid using NAudio</returns>
        public static bool checkSampleValidity(string file)
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
    }

    [Serializable]
    public struct SampleInfo
    {
        public long fileSize;
        public PeakInfo[] peakInfo;
        public string[] tags;
        public SampleInfo(long fileSize, PeakInfo[] peakInfo, string[] tags)
        {
            this.fileSize = fileSize;
            this.peakInfo = peakInfo;
            this.tags = tags;
        }
    }

    [Serializable]
    public struct Tag
    {
        public string Name { get; set; }
        public Color Color { get; set; }

        public Brush ColorBrush { get { return new SolidColorBrush(Color); } }
        public Tag(string name, Color color)
        {
            Name = name;
            Color = color;
        }
    }
}
