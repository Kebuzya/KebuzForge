using System;
using System.IO;

namespace KebuzForge.App.Core
{
    internal static class AppPaths
    {
        public static string Root =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Kebuz", "KebuzForge");

        public static string SettingsFile => Path.Combine(Root, "settings.json");

        public static string PresetsDirectory => Path.Combine(Root, "presets");

        public static string PluginsDirectory => Path.Combine(Root, "Plugins");

        public static string PortablePluginsDirectory =>
            Path.Combine(AppContext.BaseDirectory, "Plugins");

        public static void MigrateLegacy()
        {
            try
            {
                string legacy = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "KebuzForge");
                if (!Directory.Exists(legacy) || Directory.Exists(Root))
                    return;
                Directory.CreateDirectory(Path.GetDirectoryName(Root)!);
                Directory.Move(legacy, Root);
            }
            catch
            {
            }
        }
    }
}
