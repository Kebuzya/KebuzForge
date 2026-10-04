using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace KebuzForge.App.Core
{
    internal sealed record UpdateInfo(Version Version, string Url);

    internal static class UpdateChecker
    {
        private const string LatestReleaseApi = "https://api.github.com/repos/Kebuzya/KebuzForge/releases/latest";
        private const string ReleasesPage = "https://github.com/Kebuzya/KebuzForge/releases";

        private sealed class UpdateState
        {
            public string? DismissedVersion { get; set; }
        }

        public static Version CurrentVersion =>
            Normalize(Assembly.GetEntryAssembly()?.GetName().Version ?? new Version(0, 0, 0));

        public static async Task<UpdateInfo?> FindNewerAsync()
        {
            try
            {
                using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
                http.DefaultRequestHeaders.UserAgent.ParseAdd($"KebuzForge/{CurrentVersion}");
                http.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");

                string json = await http.GetStringAsync(LatestReleaseApi).ConfigureAwait(false);
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (!TryParseTag(root.GetProperty("tag_name").GetString(), out var latest)) return null;
                if (latest <= CurrentVersion) return null;
                if (TryParseTag(ReadState().DismissedVersion, out var dismissed) && dismissed == latest) return null;

                string? url = root.TryGetProperty("html_url", out var u) ? u.GetString() : null;
                if (url is null || !url.StartsWith(ReleasesPage + "/", StringComparison.Ordinal))
                    url = ReleasesPage;
                return new UpdateInfo(latest, url);
            }
            catch
            {
                return null;
            }
        }

        public static void Dismiss(Version version)
        {
            try
            {
                Directory.CreateDirectory(AppPaths.Root);
                File.WriteAllText(AppPaths.UpdateStateFile,
                    JsonSerializer.Serialize(new UpdateState { DismissedVersion = version.ToString() },
                        new JsonSerializerOptions { WriteIndented = true }));
            }
            catch { }
        }

        public static void OpenReleasePage(UpdateInfo update)
        {
            try { Process.Start(new ProcessStartInfo(update.Url) { UseShellExecute = true }); }
            catch { }
        }

        private static UpdateState ReadState()
        {
            try
            {
                if (File.Exists(AppPaths.UpdateStateFile))
                    return JsonSerializer.Deserialize<UpdateState>(File.ReadAllText(AppPaths.UpdateStateFile)) ?? new();
            }
            catch { }
            return new();
        }

        internal static bool TryParseTag(string? tag, out Version version)
        {
            version = new Version(0, 0, 0);
            if (string.IsNullOrWhiteSpace(tag)) return false;
            if (!Version.TryParse(tag.Trim().TrimStart('v', 'V'), out var parsed)) return false;
            version = Normalize(parsed);
            return true;
        }

        private static Version Normalize(Version v) => new(v.Major, v.Minor, Math.Max(v.Build, 0));
    }
}
