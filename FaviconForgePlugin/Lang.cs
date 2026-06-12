using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace FaviconForge
{
    internal static class Lang
    {
        public static bool English { get; set; }

        private static readonly Dictionary<string, string> RuEn = new()
        {
            ["FaviconForge — генератор favicon-пакета"] = "FaviconForge — favicon package generator",
            ["Открыть изображение..."] = "Open image...",
            ["Перетащите изображение или откройте файл"] = "Drag an image or open a file",
            ["Общие"] = "General",
            ["Путь до иконок на сайте:"] = "Icon path on the site:",
            ["Цвет темы (theme-color):"] = "Theme color (theme-color):",
            ["Пиксельный режим (без сглаживания)"] = "Pixel mode (no smoothing)",
            ["Фоновая заливка"] = "Background fill",
            ["Отступ"] = "Margin",
            ["Отступ: 0%"] = "Margin: 0%",
            ["Отступ: 10%"] = "Margin: 10%",
            ["Отступ: 20%"] = "Margin: 20%",
            ["Скругление"] = "Rounding",
            ["Скругление: 0%"] = "Rounding: 0%",
            ["Имя под иконкой (Apple):"] = "Name under the icon (Apple):",
            ["Название (name):"] = "Name:",
            ["Короткое имя (short name):"] = "Short name:",
            ["Круглая иконка"] = "Round icon",
            ["Цвет плитки:"] = "Tile color:",
            ["Цвет pinned tab:"] = "Pinned tab color:",
            ["Порог силуэта"] = "Silhouette threshold",
            ["Порог силуэта: 128"] = "Silhouette threshold: 128",
            ["Браузер — светлая тема"] = "Browser — light theme",
            ["Браузер — тёмная тема"] = "Browser — dark theme",
            ["Поиск — светлая тема"] = "Search — light theme",
            ["Поиск — тёмная тема"] = "Search — dark theme",
            ["Рабочий стол Apple"] = "Apple home screen",
            ["Рабочий стол Android"] = "Android home screen",
            ["Android — переключатель"] = "Android — app switcher",
            ["Windows — плитка"] = "Windows — tile",
            ["Сгенерировать ZIP..."] = "Generate ZIP...",
            ["Сгенерировать в папку..."] = "Generate to folder...",
            ["Копировать HTML"] = "Copy HTML",
            ["HTML-код для вставки в <head>:"] = "HTML code for <head>:",
            ["Открыть исходное изображение"] = "Open source image",
            ["Изображения|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.ico|Все файлы|*.*"] = "Images|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.ico|All files|*.*",
            ["Сохранить favicon-пакет"] = "Save favicon package",
            ["ZIP архив|*.zip"] = "ZIP archive|*.zip",
            ["Папка для favicon-пакета"] = "Folder for the favicon package",
            ["Ошибка загрузки"] = "Load error",
            ["Ошибка генерации"] = "Generation error",
            ["Сначала загрузите исходное изображение."] = "Load a source image first.",
            ["Готово"] = "Done",
            ["файлов"] = "files",
            ["файлов в"] = "files in",
        };

        private static readonly Dictionary<string, string> EnRu = BuildReverse();

        private static Dictionary<string, string> BuildReverse()
        {
            var map = new Dictionary<string, string>();
            foreach (var (ru, en) in RuEn)
                map.TryAdd(en, ru);
            return map;
        }

        public static string T(string ru) =>
            English && RuEn.TryGetValue(ru, out var en) ? en : ru;

        private static string Translate(string text)
        {
            if (English)
                return RuEn.TryGetValue(text, out var en) ? en : text;
            return EnRu.TryGetValue(text, out var ru) ? ru : text;
        }

        public static void Apply(Control root)
        {
            if (!string.IsNullOrEmpty(root.Text))
                root.Text = Translate(root.Text);
            foreach (Control child in root.Controls)
                Apply(child);
        }
    }
}
