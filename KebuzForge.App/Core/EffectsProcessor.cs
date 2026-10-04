using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace KebuzForge.App.Core
{
    internal static class EffectsProcessor
    {

        public static Bitmap Spherize(Bitmap src,
            float scale    = 1f,
            float bulge    = 1f,
            float offsetU  = 0f,
            float offsetV  = 0f,
            float ambient  = 0.25f,
            float diffuse  = 0.85f,
            float azimuth  = -45f,
            float elevation = 45f,
            bool specular  = true)
        {
            int w = src.Width, h = src.Height;
            float cx = (w - 1) / 2f, cy = (h - 1) / 2f;
            float r  = Math.Min(cx, cy);

            byte[] raw = ImageProcessor.LockCopy(src, out int stride);
            var result  = new Bitmap(w, h, PixelFormat.Format32bppArgb);
            var dstData = result.LockBits(new Rectangle(0, 0, w, h),
                ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            var dst = new byte[h * dstData.Stride];
            int ds  = dstData.Stride;

            LightDir(azimuth, elevation, out float LX, out float LY, out float LZ);

            for (int py = 0; py < h; py++)
            {
                for (int px = 0; px < w; px++)
                {
                    float nx = (px - cx) / r;
                    float ny = (py - cy) / r;
                    float d2 = nx * nx + ny * ny;

                    int di = py * ds + px * 4;

                    if (d2 > 1f) { ClearPixel(dst, di); continue; }

                    float nz = MathF.Sqrt(1f - d2);

                    float d = MathF.Sqrt(d2);
                    float warp = d > 0.0001f
                        ? MathF.Asin(Math.Clamp(d, 0f, 1f)) / (MathF.PI / 2f) / d
                        : 2f / MathF.PI;
                    float su = 0.5f + nx * warp * 0.5f;
                    float sv = 0.5f + ny * warp * 0.5f;

                    float fu = (nx + 1f) * 0.5f;
                    float fv = (ny + 1f) * 0.5f;

                    float u0 = fu + (su - fu) * bulge;
                    float v0 = fv + (sv - fv) * bulge;

                    float u = 0.5f + (u0 - 0.5f + offsetU) / scale;
                    float v = 0.5f + (v0 - 0.5f + offsetV) / scale;

                    if (u < 0f || u > 1f || v < 0f || v > 1f)
                    {
                        ClearPixel(dst, di);
                        continue;
                    }

                    int sx = Math.Clamp((int)(u * (w - 1)), 0, w - 1);
                    int sy = Math.Clamp((int)(v * (h - 1)), 0, h - 1);
                    int si = sy * stride + sx * 4;

                    float NdotL = Math.Max(0f, LX * nx + LY * ny + LZ * nz);

                    float rz   = 2f * NdotL * nz - LZ;
                    float spec = specular
                        ? MathF.Pow(Math.Max(0f, rz), 24f) * 0.55f * bulge
                        : 0f;

                    float lit = Math.Clamp(
                        (1f - bulge) + bulge * (ambient + diffuse * NdotL),
                        0f, 1f);
                    float inv = 1f - spec;

                    dst[di]     = (byte)Math.Clamp(raw[si]     * lit * inv + 255f * spec, 0, 255);
                    dst[di + 1] = (byte)Math.Clamp(raw[si + 1] * lit * inv + 255f * spec, 0, 255);
                    dst[di + 2] = (byte)Math.Clamp(raw[si + 2] * lit * inv + 255f * spec, 0, 255);
                    dst[di + 3] = raw[si + 3];
                }
            }

            Marshal.Copy(dst, 0, dstData.Scan0, dst.Length);
            result.UnlockBits(dstData);
            return result;
        }

        public static Bitmap RoundedRect(Bitmap src,
            float scale      = 1f,
            float bulge      = 1f,
            float sharpness  = 50f,
            bool  fromRect   = false,
            float offsetU    = 0f,
            float offsetV    = 0f,
            float ambient    = 0.25f,
            float diffuse    = 0.85f,
            float azimuth    = -45f,
            float elevation  = 45f,
            bool specular    = true)
        {
            int w = src.Width, h = src.Height;
            float cx = (w - 1) / 2f, cy = (h - 1) / 2f;

            float rx = MathF.Max(cx, 0.5f);
            float ry = MathF.Max(cy, 0.5f);

            float p         = fromRect ? 2f : 2f + sharpness * 0.3f;
            float rc        = fromRect ? sharpness / 100f : 0f;
            float innerHalf = fromRect ? 1f - rc : 0f;

            byte[] raw = ImageProcessor.LockCopy(src, out int stride);
            var result  = new Bitmap(w, h, PixelFormat.Format32bppArgb);
            var dstData = result.LockBits(new Rectangle(0, 0, w, h),
                ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            var dst = new byte[h * dstData.Stride];
            int ds  = dstData.Stride;

            LightDir(azimuth, elevation, out float LX, out float LY, out float LZ);

            for (int py = 0; py < h; py++)
            {
                for (int px = 0; px < w; px++)
                {

                    float nx = (px - cx) / rx;
                    float ny = (py - cy) / ry;

                    int di = py * ds + px * 4;

                    float nz, gnx, gny;

                    if (!fromRect)
                    {

                        float Dp = MathF.Pow(MathF.Pow(MathF.Abs(nx), p) + MathF.Pow(MathF.Abs(ny), p), 1f / p);
                        if (Dp > 1f) { ClearPixel(dst, di); continue; }

                        nz = MathF.Sqrt(1f - Dp * Dp);

                        if (Dp < 0.0001f)
                        {
                            gnx = 0f; gny = 0f;
                        }
                        else
                        {
                            float Dp2mp = MathF.Pow(Dp, 2f - p);

                            gnx = Dp2mp * MathF.Pow(MathF.Abs(nx), p - 2f) * nx;
                            gny = Dp2mp * MathF.Pow(MathF.Abs(ny), p - 2f) * ny;
                        }
                    }
                    else
                    {

                        float qx   = MathF.Max(0f, MathF.Abs(nx) - innerHalf);
                        float qy   = MathF.Max(0f, MathF.Abs(ny) - innerHalf);
                        float qLen = MathF.Sqrt(qx * qx + qy * qy);
                        float sdf  = qLen - rc;

                        if (sdf > 0f) { ClearPixel(dst, di); continue; }

                        if (qLen > 0.0001f)
                        {
                            gnx = qx / qLen * (nx >= 0f ? 1f : -1f);
                            gny = qy / qLen * (ny >= 0f ? 1f : -1f);
                        }
                        else
                        {
                            gnx = 0f; gny = 0f;
                        }

                        float Dp_eff = (rc > 0.001f)
                            ? Math.Clamp(1f + sdf / rc, 0f, 1f)
                            : 0f;
                        nz = MathF.Sqrt(1f - Dp_eff * Dp_eff);

                        gnx *= Dp_eff;
                        gny *= Dp_eff;
                    }

                    float lon = MathF.Atan2(nx, MathF.Max(nz, 0.0001f));
                    float lat = MathF.Atan2(ny, MathF.Max(nz, 0.0001f));
                    float su  = lon / (MathF.PI / 2f) * 0.5f + 0.5f;
                    float sv  = lat / (MathF.PI / 2f) * 0.5f + 0.5f;

                    float fu = (nx + 1f) * 0.5f;
                    float fv = (ny + 1f) * 0.5f;

                    float u0 = fu + (su - fu) * bulge;
                    float v0 = fv + (sv - fv) * bulge;

                    float u = 0.5f + (u0 - 0.5f + offsetU) / scale;
                    float v = 0.5f + (v0 - 0.5f + offsetV) / scale;

                    if (u < 0f || u > 1f || v < 0f || v > 1f)
                    {
                        ClearPixel(dst, di);
                        continue;
                    }

                    int sx = Math.Clamp((int)(u * (w - 1)), 0, w - 1);
                    int sy = Math.Clamp((int)(v * (h - 1)), 0, h - 1);
                    int si = sy * stride + sx * 4;

                    float NX = gnx * bulge, NY = gny * bulge, NZ = nz;
                    float nLen = MathF.Sqrt(NX * NX + NY * NY + NZ * NZ);
                    if (nLen > 0.0001f) { NX /= nLen; NY /= nLen; NZ /= nLen; }
                    else                { NX = 0f; NY = 0f; NZ = 1f; }

                    float NdotL = Math.Max(0f, LX * NX + LY * NY + LZ * NZ);
                    float rz    = 2f * NdotL * NZ - LZ;
                    float spec  = specular
                        ? MathF.Pow(Math.Max(0f, rz), 24f) * 0.55f * bulge
                        : 0f;
                    float lit   = Math.Clamp(
                        (1f - bulge) + bulge * (ambient + diffuse * NdotL),
                        0f, 1f);
                    float inv = 1f - spec;

                    dst[di]     = (byte)Math.Clamp(raw[si]     * lit * inv + 255f * spec, 0, 255);
                    dst[di + 1] = (byte)Math.Clamp(raw[si + 1] * lit * inv + 255f * spec, 0, 255);
                    dst[di + 2] = (byte)Math.Clamp(raw[si + 2] * lit * inv + 255f * spec, 0, 255);
                    dst[di + 3] = raw[si + 3];
                }
            }

            Marshal.Copy(dst, 0, dstData.Scan0, dst.Length);
            result.UnlockBits(dstData);
            return result;
        }

        public static Bitmap Cylinderize(Bitmap src, bool horizontal,
            float scale    = 1f,
            float bulge    = 1f,
            float offsetU  = 0f,
            float offsetV  = 0f,
            float ambient  = 0.3f,
            float diffuse  = 0.85f,
            float azimuth  = -45f,
            float elevation = 45f)
        {
            int w = src.Width, h = src.Height;
            float cx = (w - 1) / 2f, cy = (h - 1) / 2f;

            byte[] raw = ImageProcessor.LockCopy(src, out int stride);
            var result  = new Bitmap(w, h, PixelFormat.Format32bppArgb);
            var dstData = result.LockBits(new Rectangle(0, 0, w, h),
                ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            var dst = new byte[h * dstData.Stride];
            int ds  = dstData.Stride;

            LightDir(azimuth, elevation, out float LX, out float LY, out float LZ);

            for (int py = 0; py < h; py++)
            {
                for (int px = 0; px < w; px++)
                {
                    int di = py * ds + px * 4;
                    int sx, sy;
                    float nx = 0f, ny = 0f, nz;

                    if (horizontal)
                    {
                        nx = (px - cx) / cx;
                        if (MathF.Abs(nx) >= 1f) { ClearPixel(dst, di); continue; }
                        nz = MathF.Sqrt(1f - nx * nx);

                        float u_cyl  = MathF.Atan2(nx, nz) / (MathF.PI / 2f) * 0.5f + 0.5f;
                        float u_flat = (nx + 1f) * 0.5f;
                        float u0     = u_flat + (u_cyl - u_flat) * bulge;
                        float u      = 0.5f + (u0 - 0.5f + offsetU) / scale;
                        if (u < 0f || u > 1f) { ClearPixel(dst, di); continue; }
                        sx = Math.Clamp((int)(u * (w - 1)), 0, w - 1);
                        sy = py;
                    }
                    else
                    {
                        ny = (py - cy) / cy;
                        if (MathF.Abs(ny) >= 1f) { ClearPixel(dst, di); continue; }
                        nz = MathF.Sqrt(1f - ny * ny);

                        float v_cyl  = MathF.Atan2(ny, nz) / (MathF.PI / 2f) * 0.5f + 0.5f;
                        float v_flat = (ny + 1f) * 0.5f;
                        float v0     = v_flat + (v_cyl - v_flat) * bulge;
                        float v      = 0.5f + (v0 - 0.5f + offsetV) / scale;
                        if (v < 0f || v > 1f) { ClearPixel(dst, di); continue; }
                        sx = px;
                        sy = Math.Clamp((int)(v * (h - 1)), 0, h - 1);
                    }

                    nz = MathF.Sqrt(Math.Max(0f, 1f - nx * nx - ny * ny));
                    int si      = sy * stride + sx * 4;
                    float NdotL = Math.Max(0f, LX * nx + LY * ny + LZ * nz);
                    float lit   = Math.Clamp(
                        (1f - bulge) + bulge * (ambient + diffuse * NdotL),
                        0f, 1f);

                    dst[di]     = (byte)Math.Clamp(raw[si]     * lit, 0, 255);
                    dst[di + 1] = (byte)Math.Clamp(raw[si + 1] * lit, 0, 255);
                    dst[di + 2] = (byte)Math.Clamp(raw[si + 2] * lit, 0, 255);
                    dst[di + 3] = raw[si + 3];
                }
            }

            Marshal.Copy(dst, 0, dstData.Scan0, dst.Length);
            result.UnlockBits(dstData);
            return result;
        }

        public static Bitmap FlatTransform(Bitmap src,
            float scale   = 1f,
            float offsetU = 0f,
            float offsetV = 0f)
        {
            int w = src.Width, h = src.Height;
            byte[] raw = ImageProcessor.LockCopy(src, out int stride);
            var result  = new Bitmap(w, h, PixelFormat.Format32bppArgb);
            var dstData = result.LockBits(new Rectangle(0, 0, w, h),
                ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            var dst = new byte[h * dstData.Stride];
            int ds  = dstData.Stride;

            for (int py = 0; py < h; py++)
            {
                for (int px = 0; px < w; px++)
                {
                    int di = py * ds + px * 4;
                    float u = 0.5f + ((float)px / (w - 1) - 0.5f + offsetU) / scale;
                    float v = 0.5f + ((float)py / (h - 1) - 0.5f + offsetV) / scale;

                    if (u < 0f || u > 1f || v < 0f || v > 1f)
                    {
                        ClearPixel(dst, di);
                        continue;
                    }

                    int sx = Math.Clamp((int)(u * (w - 1)), 0, w - 1);
                    int sy = Math.Clamp((int)(v * (h - 1)), 0, h - 1);
                    int si = sy * stride + sx * 4;
                    dst[di]     = raw[si];
                    dst[di + 1] = raw[si + 1];
                    dst[di + 2] = raw[si + 2];
                    dst[di + 3] = raw[si + 3];
                }
            }

            Marshal.Copy(dst, 0, dstData.Scan0, dst.Length);
            result.UnlockBits(dstData);
            return result;
        }

        public static bool TrySphereUV(int px, int py, int w, int h,
            float scale, float bulge, float offsetU, float offsetV,
            out int sx, out int sy)
        {
            float cx = (w - 1) / 2f, cy = (h - 1) / 2f;
            float r  = Math.Min(cx, cy);
            float nx = (px - cx) / r;
            float ny = (py - cy) / r;
            float d2 = nx * nx + ny * ny;

            if (d2 > 1f) { sx = sy = 0; return false; }

            float d = MathF.Sqrt(d2);
            float warp = d > 0.0001f
                ? MathF.Asin(Math.Clamp(d, 0f, 1f)) / (MathF.PI / 2f) / d
                : 2f / MathF.PI;
            float su = 0.5f + nx * warp * 0.5f;
            float sv = 0.5f + ny * warp * 0.5f;
            float fu  = (nx + 1f) * 0.5f;
            float fv  = (ny + 1f) * 0.5f;
            float u0  = fu + (su - fu) * bulge;
            float v0  = fv + (sv - fv) * bulge;
            float u   = 0.5f + (u0 - 0.5f + offsetU) / scale;
            float v   = 0.5f + (v0 - 0.5f + offsetV) / scale;

            if (u < 0f || u > 1f || v < 0f || v > 1f) { sx = sy = 0; return false; }
            sx = Math.Clamp((int)(u * (w - 1)), 0, w - 1);
            sy = Math.Clamp((int)(v * (h - 1)), 0, h - 1);
            return true;
        }

        public static bool TryRoundedRectUV(int px, int py, int w, int h,
            float scale, float bulge, float sharpness, bool fromRect, float offsetU, float offsetV,
            out int sx, out int sy)
        {
            float cx = (w - 1) / 2f, cy = (h - 1) / 2f;
            float rx = MathF.Max(cx, 0.5f);
            float ry = MathF.Max(cy, 0.5f);

            float nx = (px - cx) / rx;
            float ny = (py - cy) / ry;

            float nz;
            if (!fromRect)
            {
                float p  = 2f + sharpness * 0.3f;
                float Dp = MathF.Pow(MathF.Pow(MathF.Abs(nx), p) + MathF.Pow(MathF.Abs(ny), p), 1f / p);
                if (Dp > 1f) { sx = sy = 0; return false; }
                nz = MathF.Sqrt(1f - Dp * Dp);
            }
            else
            {
                float rc        = sharpness / 100f;
                float innerHalf = 1f - rc;
                float qx  = MathF.Max(0f, MathF.Abs(nx) - innerHalf);
                float qy  = MathF.Max(0f, MathF.Abs(ny) - innerHalf);
                float sdf = MathF.Sqrt(qx * qx + qy * qy) - rc;
                if (sdf > 0f) { sx = sy = 0; return false; }
                float Dp_eff = (rc > 0.001f) ? Math.Clamp(1f + sdf / rc, 0f, 1f) : 0f;
                nz = MathF.Sqrt(1f - Dp_eff * Dp_eff);
            }

            float lon = MathF.Atan2(nx, MathF.Max(nz, 0.0001f));
            float lat = MathF.Atan2(ny, MathF.Max(nz, 0.0001f));
            float su  = lon / (MathF.PI / 2f) * 0.5f + 0.5f;
            float sv  = lat / (MathF.PI / 2f) * 0.5f + 0.5f;
            float fu  = (nx + 1f) * 0.5f;
            float fv  = (ny + 1f) * 0.5f;
            float u0  = fu + (su - fu) * bulge;
            float v0  = fv + (sv - fv) * bulge;
            float u   = 0.5f + (u0 - 0.5f + offsetU) / scale;
            float v   = 0.5f + (v0 - 0.5f + offsetV) / scale;

            if (u < 0f || u > 1f || v < 0f || v > 1f) { sx = sy = 0; return false; }
            sx = Math.Clamp((int)(u * (w - 1)), 0, w - 1);
            sy = Math.Clamp((int)(v * (h - 1)), 0, h - 1);
            return true;
        }

        public static bool TryCylinderUV(int px, int py, int w, int h,
            bool horizontal, float scale, float bulge, float offsetU, float offsetV,
            out int sx, out int sy)
        {
            float cx = (w - 1) / 2f, cy = (h - 1) / 2f;

            if (horizontal)
            {
                float nx = (px - cx) / cx;
                if (MathF.Abs(nx) >= 1f) { sx = sy = 0; return false; }
                float nz    = MathF.Sqrt(1f - nx * nx);
                float u_cyl = MathF.Atan2(nx, nz) / (MathF.PI / 2f) * 0.5f + 0.5f;
                float u_flat = (nx + 1f) * 0.5f;
                float u0    = u_flat + (u_cyl - u_flat) * bulge;
                float u     = 0.5f + (u0 - 0.5f + offsetU) / scale;
                if (u < 0f || u > 1f) { sx = sy = 0; return false; }
                sx = Math.Clamp((int)(u * (w - 1)), 0, w - 1);
                sy = py;
            }
            else
            {
                float ny = (py - cy) / cy;
                if (MathF.Abs(ny) >= 1f) { sx = sy = 0; return false; }
                float nz    = MathF.Sqrt(1f - ny * ny);
                float v_cyl = MathF.Atan2(ny, nz) / (MathF.PI / 2f) * 0.5f + 0.5f;
                float v_flat = (ny + 1f) * 0.5f;
                float v0    = v_flat + (v_cyl - v_flat) * bulge;
                float v     = 0.5f + (v0 - 0.5f + offsetV) / scale;
                if (v < 0f || v > 1f) { sx = sy = 0; return false; }
                sx = px;
                sy = Math.Clamp((int)(v * (h - 1)), 0, h - 1);
            }
            return true;
        }

        private static void ClearPixel(byte[] dst, int di) =>
            dst[di] = dst[di + 1] = dst[di + 2] = dst[di + 3] = 0;

        private static void LightDir(float azimuth, float elevation,
            out float lx, out float ly, out float lz)
        {
            float az = azimuth   * MathF.PI / 180f;
            float el = elevation * MathF.PI / 180f;
            lx = MathF.Sin(az) * MathF.Cos(el);
            ly = -MathF.Sin(el);
            lz = MathF.Cos(az) * MathF.Cos(el);
            float len = MathF.Sqrt(lx * lx + ly * ly + lz * lz);
            if (len > 0.0001f) { lx /= len; ly /= len; lz /= len; }
        }
    }
}
