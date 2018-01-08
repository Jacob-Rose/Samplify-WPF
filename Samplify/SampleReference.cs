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
        public string filePath { get; private set; }
        public FileInfo fileInfo { get { return new FileInfo(filePath); } }
        public string fileName { get { return fileInfo.Name; } }

        public Brush sampleLineBrush { get { return UserPreferences.defaultSampleColor; } } 

        public PeakInfo[] waveformPoints { get; private set; }
        public int waveformPointCount { get { return UserPreferences.waveformPointCount; } }
        public SampleReference(string file)
        {
            filePath = file;
            waveformPoints = getSamplePeakArray(filePath, waveformPointCount);
        }
        public SampleReference(string file, PeakInfo[] waveformPoints)
        {
            filePath = file;
            this.waveformPoints = waveformPoints;
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
            using (var reader = new AudioFileReader(filePath))
            {
                return reader.WaveFormat.BitsPerSample;
            }
        }
        public override string ToString()
        {
            return fileName;
        }

        public PeakInfo[] getSamplePeakArray(string file, int desiredSamples)
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
        public PeakInfo[] getSampleArray(IPeakProvider peakProvider, int sampleCount)
        {
            PeakInfo[] samples = new PeakInfo[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                samples[i] = peakProvider.GetNextPeak();
            }
            return samples;
        }
    }
}
