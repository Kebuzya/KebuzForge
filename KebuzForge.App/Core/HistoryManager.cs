using System;
using System.Collections.Generic;
using System.Drawing;
using KebuzForge.App.Models;
using KebuzForge.Canvas;

namespace KebuzForge.App.Core
{
    internal sealed class HistorySnapshot
    {
        public Bitmap? Image { get; init; }
        public Bitmap? PipelineResult { get; init; }
        public required AppSettings Settings { get; init; }
        public Bitmap? Source { get; init; }
        public byte[]? PaletteIndices { get; init; }
        public Color[] Palette { get; init; } = [];
        public CanvasDocument? Canvas { get; init; }
        public bool BackgroundRemoved { get; init; }
        public PixelOverlay? PreOverlay { get; init; }
        public PixelOverlay? FinalOverlay { get; init; }
    }

    internal sealed class HistoryManager : IDisposable
    {
        private readonly LinkedList<HistorySnapshot> _undo = new();
        private readonly LinkedList<HistorySnapshot> _redo = new();
        private const int MaxDepth = 100;

        public bool CanUndo => _undo.Count > 0;
        public bool CanRedo => _redo.Count > 0;

        public void Push(HistorySnapshot current)
        {
            _undo.AddLast(current);
            ClearList(_redo);
            while (_undo.Count > MaxDepth)
            {
                DisposeSnapshot(_undo.First!.Value);
                _undo.RemoveFirst();
            }
        }

        public HistorySnapshot? Undo(HistorySnapshot current)
        {
            if (!CanUndo)
            {
                DisposeSnapshot(current);
                return null;
            }
            _redo.AddLast(current);
            var prev = _undo.Last!.Value;
            _undo.RemoveLast();
            return prev;
        }

        public HistorySnapshot? Redo(HistorySnapshot current)
        {
            if (!CanRedo)
            {
                DisposeSnapshot(current);
                return null;
            }
            _undo.AddLast(current);
            var next = _redo.Last!.Value;
            _redo.RemoveLast();
            return next;
        }

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
            s.PipelineResult?.Dispose();
            s.Source?.Dispose();
        }

        public void Dispose() => Clear();
    }
}
