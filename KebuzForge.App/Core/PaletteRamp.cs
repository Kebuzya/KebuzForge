using System;
using System.Collections.Generic;
using System.Drawing;

namespace KebuzForge.App.Core
{
    internal static class PaletteRamp
    {
        private const float ShadowHue = 264f;
        private const float LightHue = 105f;
        private const float MaxHueStep = 30f;
        private const float DistinctDistance = 0.025f;

        public static Color[] Build(Color[] palette, int darkSteps, int lightSteps, float strength, float hueShift)
        {
            var result = new List<Color>();
            var labs = new List<(float L, float A, float B)>();
            foreach (var c in palette)
                labs.Add(ToOklab(c));

            void AddShade(Color shade)
            {
                var lab = ToOklab(shade);
                foreach (var o in labs)
                {
                    float dl = lab.L - o.L, da = lab.A - o.A, db = lab.B - o.B;
                    if (dl * dl + da * da + db * db < DistinctDistance * DistinctDistance) return;
                }
                labs.Add(lab);
                result.Add(shade);
            }

            foreach (var c in palette)
            {
                for (int k = darkSteps; k >= 1; k--)
                    AddShade(Shade(c, -k, strength, hueShift));
                result.Add(c);
                for (int k = 1; k <= lightSteps; k++)
                    AddShade(Shade(c, k, strength, hueShift));
            }
            return result.Count > 256 ? result.GetRange(0, 256).ToArray() : result.ToArray();
        }

        public static Color Shade(Color c, int step, float strength, float hueShift)
        {
            var (l, a, b) = ToOklab(c);
            float chroma = MathF.Sqrt(a * a + b * b);
            float hue = MathF.Atan2(b, a) * 180f / MathF.PI;
            int n = Math.Abs(step);
            float t = Math.Clamp(n * strength, 0f, 1f);
            float target = step < 0 ? ShadowHue : LightHue;

            float newL = step < 0 ? l * (1f - t) : l + (1f - l) * t;
            float newC = step < 0 ? chroma : chroma * (1f - 0.5f * t);

            if (chroma < 0.02f)
            {
                hue = target;
                newC = Math.Max(newC, 0.03f * hueShift * n);
            }
            else
            {
                float diff = ((target - hue) % 360f + 540f) % 360f - 180f;
                float turn = Math.Min(Math.Abs(diff), hueShift * MaxHueStep * n);
                hue += Math.Sign(diff) * turn;
            }

            return FromOklch(newL, newC, hue);
        }

        private static (float L, float A, float B) ToOklab(Color c)
        {
            float r = ToLinear(c.R), g = ToLinear(c.G), b = ToLinear(c.B);
            float l = MathF.Cbrt(0.4122214708f * r + 0.5363325363f * g + 0.0514459929f * b);
            float m = MathF.Cbrt(0.2119034982f * r + 0.6806995451f * g + 0.1073969566f * b);
            float s = MathF.Cbrt(0.0883024619f * r + 0.2817188376f * g + 0.6299787005f * b);
            return (
                0.2104542553f * l + 0.7936177850f * m - 0.0040720468f * s,
                1.9779984951f * l - 2.4285922050f * m + 0.4505937099f * s,
                0.0259040371f * l + 0.7827717662f * m - 0.8086757660f * s);
        }

        private static Color FromOklch(float l, float chroma, float hue)
        {
            l = Math.Clamp(l, 0f, 1f);
            float rad = hue * MathF.PI / 180f;
            float lo = 0f, hi = chroma;
            if (InGamut(l, hi, rad, out var rgb)) return rgb;
            InGamut(l, 0f, rad, out rgb);
            for (int i = 0; i < 16; i++)
            {
                float mid = (lo + hi) / 2f;
                if (InGamut(l, mid, rad, out var probe)) { lo = mid; rgb = probe; }
                else hi = mid;
            }
            return rgb;
        }

        private static bool InGamut(float l, float chroma, float rad, out Color color)
        {
            float a = chroma * MathF.Cos(rad), b = chroma * MathF.Sin(rad);
            float lp = l + 0.3963377774f * a + 0.2158037573f * b;
            float mp = l - 0.1055613458f * a - 0.0638541728f * b;
            float sp = l - 0.0894841775f * a - 1.2914855480f * b;
            float lc = lp * lp * lp, mc = mp * mp * mp, sc = sp * sp * sp;
            float r = 4.0767416621f * lc - 3.3077115913f * mc + 0.2309699292f * sc;
            float g = -1.2684380046f * lc + 2.6097574011f * mc - 0.3413193965f * sc;
            float bl = -0.0041960863f * lc - 0.7034186147f * mc + 1.7076147010f * sc;
            const float eps = 0.0005f;
            bool ok = r >= -eps && r <= 1f + eps && g >= -eps && g <= 1f + eps && bl >= -eps && bl <= 1f + eps;
            color = Color.FromArgb(ToByte(r), ToByte(g), ToByte(bl));
            return ok;
        }

        private static float ToLinear(byte v)
        {
            float c = v / 255f;
            return c <= 0.04045f ? c / 12.92f : MathF.Pow((c + 0.055f) / 1.055f, 2.4f);
        }

        private static int ToByte(float v)
        {
            v = Math.Clamp(v, 0f, 1f);
            float s = v <= 0.0031308f ? 12.92f * v : 1.055f * MathF.Pow(v, 1f / 2.4f) - 0.055f;
            return Math.Clamp((int)MathF.Round(s * 255f), 0, 255);
        }
    }
}
