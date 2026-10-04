using System.Windows.Forms;

namespace FaviconForge
{
    public static class PluginEntry
    {
        public static string PluginName => "Favicon Generator";

        public static string PluginDescription =>
            "Генератор полного favicon-пакета: favicon.ico, PNG, apple-touch-icon, " +
            "Android Chrome, PWA maskable, Windows Tiles, Safari pinned tab, " +
            "site.webmanifest, browserconfig.xml и HTML-код.";

        public static string PluginVersion => "1.0.0";

        public static void SetLanguage(bool english) => Lang.English = english;

        public static Form CreateMainForm() => new FaviconForm();
    }
}
