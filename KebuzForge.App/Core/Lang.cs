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
            ["Подобрать палитру"] = "Build palette",
            ["Разложить по палитре"] = "Map to palette",
            ["Сохранить"] = "Save",
            ["Загрузить"] = "Load",
            ["Авто-каскад"] = "Auto-cascade",
            ["Пикселизация не сбрасывает палитру, дизеринг и эффекты - они пересчитываются автоматически"] = "Pixelization keeps palette, dithering and effects - they are recomputed automatically",
            ["Палитра загружена - нажмите «Разложить по палитре»"] = "Palette loaded - press \"Map to palette\"",
            ["Палитра пуста. Подберите или загрузите палитру."] = "Palette is empty. Build or load a palette.",
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
            ["Допуск:"] = "Tolerance:",
            ["Удалить фон"] = "Remove background",
            ["Готово (Esc)"] = "Done (Esc)",
            ["Щёлкайте по «ПОСЛЕ»: область станет прозрачной"] = "Click areas on AFTER to make them transparent",
            ["Нажмите и щёлкайте по областям на «ПОСЛЕ», чтобы сделать их прозрачными. Похожие соседние цвета берутся по «Допуску». Каждый щелчок отменяется Ctrl+Z."] = "Press, then click areas on AFTER to make them transparent. Similar neighboring colors are taken by Tolerance. Each click can be undone with Ctrl+Z.",
            ["Масштаб"] = "Scale",
            ["Выпуклость"] = "Bulge",
            ["Смещение X"] = "Offset X",
            ["Смещение Y"] = "Offset Y",
            ["Скругление"] = "Rounding",
            ["Форма"] = "Shape",
            ["Интенсивность"] = "Intensity",
            ["Двойной клик - ввести значение"] = "Double-click to type a value",
            ["ДО"] = "BEFORE",
            ["ПОСЛЕ"] = "AFTER",
            ["Редактор пикселей"] = "Pixel editor",
            ["Анимация"] = "Animation",
            ["Карандаш"] = "Pencil",
            ["Линия"] = "Line",
            ["Прямоугольник"] = "Rectangle",
            ["Заливка"] = "Fill",
            ["Пипетка"] = "Eyedropper",
            ["Добавить цвет"] = "Add color",
            ["Пипетка: щёлкните в любом месте экрана (Esc - отмена)"] = "Eyedropper: click anywhere on the screen (Esc cancels)",
            ["Привязка к центру и краям холста и других объектов (Alt при перетаскивании - временно наоборот)"] = "Snap to the center and edges of the canvas and other objects (Alt while dragging inverts it)",
            ["Удалить цвет"] = "Remove color",
            ["Добавить недостающий"] = "Add missing color",
            ["Все цвета картинки уже есть в палитре"] = "All image colors are already in the palette",
            ["Тени и блики..."] = "Shades and highlights...",
            ["Сравнение по восприятию"] = "Perceptual matching",
            ["Вкл: пиксели раскладываются по цветам так, как их видит глаз, меньше случайных точек на контурах. Выкл: простое сравнение RGB, плавный градиент на маленькой палитре не рвётся."] = "On: pixels are matched to colors the way the eye sees them, fewer stray dots on outlines. Off: plain RGB matching, a smooth gradient on a small palette does not break apart.",
            ["Тени и блики"] = "Shades and highlights",
            ["Ступеней тени"] = "Shadow steps",
            ["Ступеней блика"] = "Highlight steps",
            ["Сила ступени"] = "Step strength",
            ["Сдвиг оттенка"] = "Hue shift",
            ["Ластик"] = "Eraser",
            ["Окружность"] = "Ellipse",
            ["Окружность (Shift - ровный круг, Ctrl - от центра)"] = "Ellipse (Shift - perfect circle, Ctrl - from center)",
            ["Прямоугольник (Shift - квадрат, Ctrl - от центра)"] = "Rectangle (Shift - square, Ctrl - from center)",
            ["Прозр. ластик"] = "Transp. eraser",
            ["ПКМ-ластик"] = "RMB eraser",
            ["Правая кнопка стирает (иначе рисует цветом ПКМ)"] = "Right button erases (otherwise draws with RMB color)",
            ["Цвет карандаша (клик для выбора)"] = "Pencil color (click to choose)",
            ["Цвет ПКМ (клик для выбора)"] = "RMB color (click to choose)",
            ["Ластик стирает в прозрачный (иначе в фон)"] = "Eraser erases to transparent (otherwise to background)",
            ["Выбирать только цвета из текущей палитры"] = "Pick colors only from the current palette",
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
            ["Новая версия:"] = "New version:",
            ["Открыть страницу релиза"] = "Open the release page",
            ["Скрыть до следующей версии"] = "Hide until the next version",
            ["Холст"] = "Canvas",
            ["Новый холст..."] = "New canvas...",
            ["Эллипс (Shift - круг, Ctrl - от центра)"] = "Ellipse (Shift - circle, Ctrl - from center)",
            ["Треугольник (Shift - ровный, Ctrl - от центра)"] = "Triangle (Shift - even, Ctrl - from center)",
            ["Линия (Shift - шаг 45°)"] = "Line (Shift - 45° steps)",
            ["Тень"] = "Shadow",
            ["Жирный"] = "Bold",
            ["Непрозрачность, %"] = "Opacity, %",
            ["Сдвиг X"] = "Shadow X",
            ["Текст"] = "Text",
            ["Выше"] = "Up",
            ["Ниже"] = "Down",
            ["Пересечение"] = "Intersection",
            ["Группа"] = "Group",
            ["Без группы"] = "Ungroup",
            ["Удалить"] = "Delete",
            ["Выбор: щелчок выделяет, Shift - добавить к выделению, правая кнопка - выбрать объект под другим, тянуть - двигать, Ctrl + тянуть - копировать, Ctrl+D - дублировать, кружок сверху - повернуть; маркеры: Shift - пропорции, Ctrl - от центра"] = "Select: click selects, Shift adds to the selection, right click picks an object underneath, drag moves, Ctrl + drag copies, Ctrl+D duplicates, the circle on top rotates; handles: Shift keeps proportions, Ctrl scales from the center",
            ["Дуга: жёлтые ромбы на концах меняют её длину"] = "Arc: the yellow diamonds at the ends change its length",
            ["Текст: введите его во вкладке «Холст», затем щёлкните или растяните на холсте"] = "Text: type it on the Canvas tab, then click or drag on the canvas",
            ["Оформление"] = "Appearance",
            ["Фигура"] = "Shape options",
            ["Объект"] = "Object",
            ["Включить"] = "Enable",
            ["Стрелка на конце"] = "Arrow at the end",
            ["Поворот, °"] = "Rotation, °",
            ["или перетащите изображение сюда"] = "or drag an image here",
            ["Эллипс"] = "Oval",
            ["Треугольник"] = "Triangle",
            ["Дуга"] = "Arc",
            ["Картинка"] = "Picture",
            ["Объекты под курсором"] = "Objects under the cursor",
            ["Слой:"] = "Layer:",
            ["После пикселизации"] = "After pixelization",
            ["Рисовать по картинке сразу после пикселизации: палитра, дизеринг и эффекты ложатся поверх правок"] = "Draw on the picture right after pixelization: palette, dithering and effects are applied over the edits",
            ["Итог"] = "Final",
            ["Рисовать по готовому результату: что нарисовано, то и будет"] = "Draw on the finished result: what you draw is what you get",
            ["На слой пикселизации"] = "To the pixelization layer",
            ["Перенести правки итога на слой после пикселизации: они переживут смену палитры и эффектов"] = "Move the final edits to the layer after pixelization: they will survive palette and effect changes",
            ["Сохранить проект"] = "Save project",
            ["Сохранить проект как..."] = "Save project as...",
            ["Проект KebuzForge|*.kforge"] = "KebuzForge project|*.kforge",
            ["Проекты и изображения|*.kforge;*.png;*.jpg;*.jpeg;*.bmp;*.gif|Проекты KebuzForge|*.kforge|Изображения|*.png;*.jpg;*.jpeg;*.bmp;*.gif|Все файлы|*.*"] = "Projects and images|*.kforge;*.png;*.jpg;*.jpeg;*.bmp;*.gif|KebuzForge projects|*.kforge|Images|*.png;*.jpg;*.jpeg;*.bmp;*.gif|All files|*.*",
            ["Нечего сохранять: откройте картинку или создайте холст."] = "Nothing to save: open an image or create a canvas.",
            ["Не удалось сохранить проект"] = "Could not save the project",
            ["Не удалось открыть проект"] = "Could not open the project",
            ["Сохранить изменения в проекте?"] = "Save changes to the project?",
            ["Проект создан более новой версией программы."] = "The project was made by a newer version of the program.",
            ["Программа была закрыта без сохранения. Найдено автосохранение от"] = "The program was closed without saving. Found an autosave from",
            ["Восстановить?"] = "Restore it?",
            ["Вписать холст в окно (Ctrl+0). Приближение: Ctrl+колесо. Перемещение: средняя кнопка мыши или пробел + мышь"] = "Fit the canvas to the window (Ctrl+0). Zoom: Ctrl+wheel. Pan: middle mouse button or Space + mouse",
            ["Добавить картинку..."] = "Add image...",
            ["Картинка уменьшена для скорости:"] = "Image reduced for speed:",
            ["Ширина"] = "Width",
            ["Прозрачный"] = "Transparent",
            ["Цвет"] = "Color",
            ["Создать"] = "Create",
            ["Новый холст"] = "New canvas",
            ["Новый пиксель-арт"] = "New pixel art",
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
