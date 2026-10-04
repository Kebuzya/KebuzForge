using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace KebuzForge.App.Core
{
    public sealed class PluginInfo
    {
        public required string Name { get; init; }
        public required string Description { get; init; }
        public required string Version { get; init; }
        public required string FilePath { get; init; }
        public required Func<Form> CreateForm { get; init; }
        public Action<bool>? SetLanguage { get; init; }
    }

    internal static class PluginManager
    {
        private static readonly Dictionary<string, Assembly> _loaded = new(StringComparer.OrdinalIgnoreCase);

        public static string PluginDirectory => AppPaths.PluginsDirectory;

        public static void OpenPluginFolder()
        {
            Directory.CreateDirectory(PluginDirectory);
            Process.Start(new ProcessStartInfo
            {
                FileName = PluginDirectory,
                UseShellExecute = true
            });
        }

        public static List<PluginInfo> Discover()
        {
            var result = new List<PluginInfo>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var dir in new[] { PluginDirectory, AppPaths.PortablePluginsDirectory })
            {
                if (!Directory.Exists(dir))
                    continue;
                foreach (var dll in Directory.GetFiles(dir, "*.dll"))
                {
                    if (!seen.Add(Path.GetFileName(dll)))
                        continue;
                    var info = TryLoad(dll);
                    if (info is not null)
                        result.Add(info);
                }
            }
            return result.OrderBy(p => p.Name).ToList();
        }

        private static PluginInfo? TryLoad(string dllPath)
        {
            try
            {
                if (!_loaded.TryGetValue(dllPath, out var asm))
                {
                    asm = Assembly.LoadFrom(dllPath);
                    _loaded[dllPath] = asm;
                }

                var entry = asm.GetExportedTypes()
                    .FirstOrDefault(t => t.Name == "PluginEntry" && t.IsAbstract && t.IsSealed);
                if (entry is null)
                    return null;

                var create = entry.GetMethod("CreateMainForm",
                    BindingFlags.Public | BindingFlags.Static, Type.EmptyTypes);
                if (create is null || !typeof(Form).IsAssignableFrom(create.ReturnType))
                    return null;

                var setLang = entry.GetMethod("SetLanguage",
                    BindingFlags.Public | BindingFlags.Static, new[] { typeof(bool) });

                return new PluginInfo
                {
                    Name = ReadStaticString(entry, "PluginName")
                           ?? Path.GetFileNameWithoutExtension(dllPath),
                    Description = ReadStaticString(entry, "PluginDescription") ?? "",
                    Version = ReadStaticString(entry, "PluginVersion") ?? "1.0",
                    FilePath = dllPath,
                    CreateForm = () => (Form)create.Invoke(null, null)!,
                    SetLanguage = setLang is null
                        ? null
                        : english => setLang.Invoke(null, new object[] { english })
                };
            }
            catch
            {
                return null;
            }
        }

        private static string? ReadStaticString(Type type, string propertyName)
        {
            var prop = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Static);
            return prop?.GetValue(null) as string;
        }
    }
}
