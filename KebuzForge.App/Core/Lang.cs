using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace KebuzForge.App.Core
{
    internal static class Lang
    {
        public static bool English { get; set; }

        private static readonly Dictionary<string, string> RuEn = new()
        {
            ["Файл"] = "File",
            ["Открыть..."] = "Open...",
            ["Выход"] = "Exit",
            ["Правка"] = "Edit",
            ["Отменить"] = "Undo",
            ["Повторить"] = "Redo",
            ["Сбросить настройки"] = "Reset settings",
            ["Пресеты"] = "Presets",
            ["Сохранить пресет..."] = "Save preset...",
            ["Экспорт"] = "Export",
            ["ICO (мульти-размер)..."] = "ICO (multi-size)...",
            ["Пакетная обработка..."] = "Batch processing...",
            ["Плагины"] = "Plugins",
            ["Открыть папку плагинов..."] = "Open plugins folder...",
            ["Плагины не найдены"] = "No plugins found",
            ["Вид"] = "View",
            ["Светлая"] = "Light",
            ["Тёмная"] = "Dark",
            ["Гибридная"] = "Hybrid",
            ["  Светлая"] = "  Light",
            ["  Тёмная"] = "  Dark",
            ["  Гибридная"] = "  Hybrid",
            ["Открыть"] = "Open",
            ["↶  Отменить"] = "↶  Undo",
            ["↷  Повторить"] = "↷  Redo",
            ["Пикселизация"] = "Pixelization",
            ["Ширина:"] = "Width:",
            ["Высота:"] = "Height:",
            ["Сохранять пропорции"] = "Keep aspect ratio",
            ["Алгоритм:"] = "Algorithm:",
            ["Среднее (блок)"] = "Average (block)",
            ["Центр блока"] = "Block center",
            ["Пикселизировать"] = "Pixelize",
            ["Палитра"] = "Palette",
            ["Цветов:"] = "Colors:",
            ["- Своя палитра -"] = "- Custom palette -",
            ["CGA (4 цвета)"] = "CGA (4 colors)",
            ["CGA (16 цветов)"] = "CGA (16 colors)",
            ["Применить"] = "Apply",
            ["Сохранить"] = "Save",
            ["Загрузить"] = "Load",
            ["Дизеринг"] = "Dithering",
            ["Нет"] = "None",
            ["Применить дизеринг"] = "Apply dithering",
            ["3D Эффекты"] = "3D Effects",
            ["Нет (плоский)"] = "None (flat)",
            ["Шар (Spherize)"] = "Sphere (Spherize)",
            ["Цилиндр"] = "Cylinder",
            ["Вертикальный"] = "Vertical",
            ["Горизонтальный"] = "Horizontal",
            ["Скруглённый квадрат"] = "Rounded square",
            ["Инверсия: □→○"] = "Inversion: □→○",
            ["Применить эффект"] = "Apply effect",
            ["Освещение"] = "Lighting",
            ["Общий свет"] = "Ambient",
            ["Рассеянный свет"] = "Diffuse",
            ["Горизонталь"] = "Azimuth",
            ["Высота"] = "Elevation",
            ["Центральный блик"] = "Specular highlight",
            ["Обводка"] = "Outline",
            ["Толщина:"] = "Thickness:",
            ["Снаружи"] = "Outside",
            ["Внутри"] = "Inside",
            ["Цвет обводки"] = "Outline color",
            ["Контуры"] = "Outline",
            ["Фон"] = "Background",
            ["Выбрать цвет фона"] = "Pick background color",
            ["Допуск:"] = "Tolerance:",
            ["Удалить фон"] = "Remove background",
            ["Масштаб"] = "Scale",
            ["Выпуклость"] = "Bulge",
            ["Смещение X"] = "Offset X",
            ["Смещение Y"] = "Offset Y",
            ["Скругление"] = "Rounding",
            ["Форма"] = "Shape",
            ["Интенсивность"] = "Intensity",
            ["ДО"] = "BEFORE",
            ["ПОСЛЕ"] = "AFTER",
            ["Редактор пикселей"] = "Pixel editor",
            ["Анимация"] = "Animation",
            ["Карандаш"] = "Pencil",
            ["Линия"] = "Line",
            ["Прямоугольник"] = "Rectangle",
            ["Заливка"] = "Fill",
            ["Пипетка"] = "Eyedropper",
            ["Ластик"] = "Eraser",
            ["Прозр. ластик"] = "Transp. eraser",
            ["В результат"] = "To result",
            ["Цвет карандаша (клик для выбора)"] = "Pencil color (click to choose)",
            ["Ластик стирает в прозрачный (иначе в фон)"] = "Eraser erases to transparent (otherwise to background)",
            ["Выбирать только цвета из текущей палитры"] = "Pick colors only from the current palette",
            ["Скопировать из редактора в результат"] = "Copy from editor to result",
            ["Открепить вкладку редактора"] = "Detach editor tab",
            ["Играть"] = "Play",
            ["Стоп"] = "Stop",
            ["Обработать кадры"] = "Process frames",
            ["Экспорт GIF"] = "Export GIF",
            ["Кадр: -"] = "Frame: -",
            ["Кадр"] = "Frame",
            ["Обработка"] = "Processing",
            ["кадров"] = "frames",
            ["Оригинал: -"] = "Original: -",
            ["Результат: -"] = "Result: -",
            ["Оригинал"] = "Original",
            ["Результат"] = "Result",
            ["📂\n\nПеретащите изображение сюда\nили нажмите, чтобы выбрать файл\n\nСовет: лучше всего подходят\nквадратные изображения (1:1)"] =
                "📂\n\nDrag an image here\nor click to choose a file\n\nTip: square images (1:1)\nwork best",
            ["Совет: для иконок лучше формат 1:1"] = "Tip: 1:1 aspect works best for icons",
            ["у вас"] = "yours is",
            ["Открыть изображение"] = "Open image",
            ["Изображения|*.png;*.jpg;*.jpeg;*.bmp;*.gif|Все файлы|*.*"] = "Images|*.png;*.jpg;*.jpeg;*.bmp;*.gif|All files|*.*",
            ["Палитры|*.pal;*.act;*.gpl|Все файлы|*.*"] = "Palettes|*.pal;*.act;*.gpl|All files|*.*",
            ["Ошибка"] = "Error",
            ["Ошибка загрузки"] = "Load error",
            ["Ошибка пикселизации"] = "Pixelization error",
            ["Ошибка квантования"] = "Quantization error",
            ["Ошибка эффекта"] = "Effect error",
            ["Ошибка экспорта"] = "Export error",
            ["Ошибка загрузки палитры"] = "Palette load error",
            ["Ошибка сохранения пресета"] = "Preset save error",
            ["Ошибка загрузки пресета"] = "Preset load error",
            ["Ошибка запуска плагина"] = "Plugin launch error",
            ["Сначала загрузите изображение."] = "Load an image first.",
            ["Сначала примените палитру."] = "Apply a palette first.",
            ["Сначала обработайте кадры."] = "Process the frames first.",
            ["Палитра пуста. Сначала примените палитру."] = "Palette is empty. Apply a palette first.",
            ["Палитра пуста"] = "Palette is empty",
            ["Нет результата для экспорта."] = "No result to export.",
            ["Нет изображения"] = "No image",
            ["Сохранить пресет"] = "Save preset",
            ["Название пресета:"] = "Preset name:",
            ["Пресет 1"] = "Preset 1",
            ["Отмена"] = "Cancel",
            ["ESC - отмена"] = "ESC - cancel",
            ["Цвет из палитры"] = "Color from palette",
            ["Сохранить палитру"] = "Save palette",
            ["Загрузить палитру"] = "Load palette",
            ["Сохранить PNG"] = "Save PNG",
            ["Сохранить Indexed PNG"] = "Save Indexed PNG",
            ["Сохранить ICO"] = "Save ICO",
            ["Экспорт анимированного GIF"] = "Export animated GIF",
            ["цветов"] = "colors",
            ["Пакетная обработка"] = "Batch processing",
            ["Входная папка"] = "Input folder",
            ["Выходная папка"] = "Output folder",
            ["Обзор..."] = "Browse...",
            ["Формат:"] = "Format:",
            ["ICO (мульти-размер)"] = "ICO (multi-size)",
            ["Начать"] = "Start",
            ["Закрыть"] = "Close",
            ["Входная папка не существует."] = "Input folder does not exist.",
            ["Изображений не найдено в выбранной папке."] = "No images found in the selected folder.",
            ["Укажите выходную папку."] = "Specify an output folder.",
            ["Готово!"] = "Done!",
            ["файлов обработано"] = "files processed",
            ["Предупреждение: палитра пуста - используется PNG вместо Indexed PNG."] =
                "Warning: palette is empty - PNG is used instead of Indexed PNG.",
            ["Русский"] = "Русский",
            ["English"] = "English",
            ["Язык / Language"] = "Язык / Language",
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
            ApplyControl(root);
        }

        private static void ApplyControl(Control c)
        {
            if (!string.IsNullOrEmpty(c.Text))
                c.Text = Translate(c.Text);

            if (c is ToolStrip strip)
                ApplyItems(strip.Items);

            if (c is ComboBox cmb)
                for (int i = 0; i < cmb.Items.Count; i++)
                    if (cmb.Items[i] is string s)
                        cmb.Items[i] = Translate(s);

            foreach (Control child in c.Controls)
                ApplyControl(child);
        }

        private static void ApplyItems(ToolStripItemCollection items)
        {
            foreach (ToolStripItem item in items)
            {
                if (!string.IsNullOrEmpty(item.Text))
                    item.Text = Translate(item.Text);
                if (!string.IsNullOrEmpty(item.ToolTipText))
                    item.ToolTipText = Translate(item.ToolTipText);
                if (item is ToolStripMenuItem mi)
                    ApplyItems(mi.DropDownItems);
            }
        }
    }
}
