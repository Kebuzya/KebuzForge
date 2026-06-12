using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using WinFormsApp1.Models;

namespace WinFormsApp1.Core
{
    internal sealed class HistorySnapshot
    {
        public Bitmap? Image { get; init; }
        public required AppSettings Settings { get; init; }
        public Bitmap? Source { get; init; }
        public byte[]? PaletteIndices { get; init; }
        public Color[] Palette { get; init; } = [];
    }

    internal sealed class HistoryManager : IDisposable
    {
        private readonly LinkedList<HistorySnapshot> _undo = new();
        private readonly LinkedList<HistorySnapshot> _redo = new();
        private const int MaxDepth = 100;

        public bool CanUndo => _undo.Count > 0;
        public bool CanRedo => _redo.Count > 0;

        public void Push(Bitmap? image, AppSettings settings, Bitmap? source, byte[]? indices, Color[] palette)
        {
            _undo.AddLast(Make(image, settings, source, indices, palette));
            ClearList(_redo);
            while (_undo.Count > MaxDepth)
            {
                DisposeSnapshot(_undo.First!.Value);
                _undo.RemoveFirst();
            }
        }

        public HistorySnapshot? Undo(Bitmap? image, AppSettings settings, Bitmap? source, byte[]? indices, Color[] palette)
        {
            if (!CanUndo) return null;
            _redo.AddLast(Make(image, settings, source, indices, palette));
            var prev = _undo.Last!.Value;
            _undo.RemoveLast();
            return prev;
        }

        public HistorySnapshot? Redo(Bitmap? image, AppSettings settings, Bitmap? source, byte[]? indices, Color[] palette)
        {
            if (!CanRedo) return null;
            _undo.AddLast(Make(image, settings, source, indices, palette));
            var next = _redo.Last!.Value;
            _redo.RemoveLast();
            return next;
        }

        private static HistorySnapshot Make(Bitmap? image, AppSettings settings, Bitmap? source, byte[]? indices, Color[] palette) =>
            new()
            {
                Image = image is null ? null : new Bitmap(image),
                Settings = settings,
                Source = source is null ? null : new Bitmap(source),
                PaletteIndices = indices?.ToArray(),
                Palette = (Color[])palette.Clone()
            };

        public void Clear()
        {
            ClearList(_undo);
            ClearList(_redo);
        }

        private static void ClearList(LinkedList<HistorySnapshot> list)
        {
            foreach (var s in list)
                DisposeSnapshot(s);
            list.Clear();
        }

        private static void DisposeSnapshot(HistorySnapshot s)
        {
            s.Image?.Dispose();
            s.Source?.Dispose();
        }

        public void Dispose() => Clear();
    }
}
