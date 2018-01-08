using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WaveFormRendererLib;

namespace Samplify.WaveFormRendererLib
{
    class PeakGetter
    {
        public Point[] getMaxPointArray(string file, int desiredSamples)
        {
            return new Point[0];
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
                //var gapSize = settings.PixelsPerPeak + settings.SpacerPixels;
                mpp.Init(reader, samplesPerPixel);
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
