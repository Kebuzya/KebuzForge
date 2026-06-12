# KebuzForge

Инструмент для создания пиксель-арта и иконок из обычных изображений (Windows, WinForms, .NET 8).

## Возможности

- **Пикселизация** - усреднение или выборка по центру, привязка пропорций
- **Палитры** - Median Cut квантование, ретро-палитры, редактирование цветов, импорт/экспорт (JASC PAL, Photoshop ACT, GIMP GPL)
- **Дизеринг** - Floyd–Steinberg, Bayer 4×4 / 8×8 с настраиваемой интенсивностью
- **3D-эффекты** - сфера, цилиндр, скруглённый прямоугольник с освещением (ambient/diffuse, направление света)
- **Пиксельный редактор** - карандаш, линия, прямоугольник, заливка, пипетка, ластик, зум до 16×
- **Анимация** - обработка GIF покадрово всем конвейером, экспорт анимированного GIF
- **Экспорт** - PNG, Indexed PNG (8 bpp), многоразмерный ICO, пакетная обработка
- **Удаление фона**, контуры, отсоединяемые панели, тёмная/светлая/гибридная темы, пресеты
- **Плагины** - расширение функционала через DLL

## Скачать

Готовые сборки - в разделе [Releases](../../releases):

| Файл | Описание |
|---|---|
| `KebuzForge-Setup-…-win-x64.exe` | Установщик: ярлыки, деинсталлятор, сам докачивает .NET 8 Runtime |
| `KebuzForge-win-x64-portable.exe` | Один файл, ничего устанавливать не нужно (~68 МБ) |
| `KebuzForge-win-x64-lite.exe` | Один файл ~0,4 МБ, требует [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) |
| `KebuzForge-win-arm64-…` / `win-x86-…` | То же для Windows ARM64 и 32-битных систем |
| `FaviconForgePlugin-….zip` | Плагин «Favicon Generator» |

## Плагины

Плагины - это DLL, которые кладутся в `%LocalAppData%\Kebuz\KebuzForge\Plugins`
(меню **Плагины → Открыть папку плагинов...**) или в папку `Plugins` рядом с exe (portable-вариант).

### Favicon Generator

Генератор полного favicon-пакета для сайта (аналог realfavicongenerator.net):
favicon.ico + PNG (16/32/48), apple-touch-icon, android-chrome 192/512, PWA maskable,
Windows Tiles + browserconfig.xml, safari-pinned-tab.svg, site.webmanifest и готовый HTML-код.
Настройки фона, отступов, скругления и цветов для каждой платформы, живые превью, экспорт в ZIP или папку.

### Свой плагин

Плагин - это сборка net8.0-windows с публичным статическим классом `PluginEntry`:

```csharp
public static class PluginEntry
{
    public static string PluginName => "Имя плагина";
    public static string PluginDescription => "Описание";
    public static string PluginVersion => "1.0.0";
    public static Form CreateMainForm() => new MyPluginForm();
}
```

## Сборка из исходников

```
dotnet build KebuzForge.sln -c Release
```

Публикация одним файлом:

```
dotnet publish WinFormsApp1 -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
dotnet publish WinFormsApp1 -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=false
```

Установщик (нужен [Inno Setup 6](https://jrsoftware.org/isinfo.php)):

```
iscc installer\setup.iss
```

## Лицензия

[MIT](LICENSE)
