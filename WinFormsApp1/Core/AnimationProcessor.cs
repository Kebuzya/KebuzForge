using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace WinFormsApp1.Core
{

    internal class PipelineSettings
    {
        public int    PixelWidth      { get; set; }
        public int    PixelHeight     { get; set; }
        public string PixelAlgorithm  { get; set; } = "Average";
        public Color[] Palette        { get; set; } = [];
        public string DitherMode      { get; set; } = "None";
        public float  DitherIntensity { get; set; }

        public int  EffectIndex        { get; set; }
        public bool CylinderHorizontal { get; set; }

        public float Scale          { get; set; } = 1f;
        public float Bulge          { get; set; } = 1f;
        public float CornerSharpness { get; set; }
        public bool  ShapeFromRect   { get; set; }
        public float OffsetU        { get; set; }
        public float OffsetV        { get; set; }
        public float Ambient   { get; set; } = 0.25f;
        public float Diffuse   { get; set; } = 0.85f;
        public float Azimuth   { get; set; } = -45f;
        public float Elevation { get; set; } = 45f;
        public bool  Specular  { get; set; } = true;
    }

    internal static class AnimationProcessor
    {

        public static bool IsAnimated(string path)
        {
            try
            {
                using var img = Image.FromFile(path);
                var dim = new FrameDimension(img.FrameDimensionsList[0]);
                return img.GetFrameCount(dim) > 1;
            }
            catch
            {
                return false;
            }
        }

        public static (List<Bitmap> frames, List<int> delays) ExtractFrames(string path)
        {
            using var img = Image.FromFile(path);
            var dim        = new FrameDimension(img.FrameDimensionsList[0]);
            int frameCount = img.GetFrameCount(dim);

            int[] rawDelays = new int[frameCount];
            try
            {
                var prop = img.GetPropertyItem(0x5100);
                if (prop?.Value is not null)
                {
                    for (int i = 0; i < frameCount && i * 4 + 3 < prop.Value.Length; i++)
                        rawDelays[i] = BitConverter.ToInt32(prop.Value, i * 4);
                }
            }
            catch {   }

            var frames = new List<Bitmap>(frameCount);
            var delays = new List<int>(frameCount);

            for (int i = 0; i < frameCount; i++)
            {
                img.SelectActiveFrame(dim, i);
                frames.Add(new Bitmap(img));

                int delayMs = Math.Max(20, rawDelays[i] * 10);
                delays.Add(delayMs);
            }

            return (frames, delays);
        }

        public static Bitmap ProcessFrame(Bitmap source, PipelineSettings s)
        {

            Bitmap current = (s.PixelWidth > 0 && s.PixelHeight > 0)
                ? ImageProcessor.Pixelize(source, s.PixelWidth, s.PixelHeight, s.PixelAlgorithm)
                : new Bitmap(source);

            if (s.Palette is { Length: > 0 })
            {
                Bitmap paletted;
                if (s.DitherMode != "None")
                {
                    paletted = DitheringEngine.Apply(current, s.Palette, s.DitherMode, s.DitherIntensity);
                }
                else
                {
                    paletted = PaletteManager.ApplyPalette(current, s.Palette);
                }
                current.Dispose();
                current = paletted;
            }

            if (s.EffectIndex > 0)
            {
                Bitmap effected;
                if (s.EffectIndex == 1)
                {
                    effected = EffectsProcessor.Spherize(current,
                        s.Scale, s.Bulge, s.OffsetU, s.OffsetV,
                        s.Ambient, s.Diffuse, s.Azimuth, s.Elevation, s.Specular);
                }
                else if (s.EffectIndex == 3)
                {
                    effected = EffectsProcessor.RoundedRect(current,
                        s.Scale, s.Bulge, s.CornerSharpness, s.ShapeFromRect,
                        s.OffsetU, s.OffsetV, s.Ambient, s.Diffuse, s.Azimuth, s.Elevation, s.Specular);
                }
                else
                {
                    effected = EffectsProcessor.Cylinderize(current, s.CylinderHorizontal,
                        s.Scale, s.Bulge, s.OffsetU, s.OffsetV,
                        s.Ambient, s.Diffuse, s.Azimuth, s.Elevation);
                }
                current.Dispose();
                current = effected;
            }

            return current;
        }

        public static void ExportGif(IReadOnlyList<Bitmap> frames, IReadOnlyList<int> delays, string path)
        {
            if (frames.Count == 0)
                throw new ArgumentException("No frames to export.");

            int defaultDelay = delays.Count > 0 ? delays[0] : 100;

            using var gif = AnimatedGif.AnimatedGif.Create(path, defaultDelay);
            for (int i = 0; i < frames.Count; i++)
            {
                int delay = i < delays.Count ? delays[i] : defaultDelay;
                gif.AddFrame(frames[i], delay: delay, quality: AnimatedGif.GifQuality.Bit8);
            }
        }
    }
}
