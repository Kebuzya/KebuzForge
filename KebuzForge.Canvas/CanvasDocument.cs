using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace KebuzForge.Canvas
{
    public sealed class CanvasDocument
    {
        private readonly List<CanvasObject> _objects = new();
        private int _nextId = 1;

        public CanvasDocument(int width, int height)
        {
            if (width <= 0 || height <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            Width = width;
            Height = height;
        }

        public int Width { get; }
        public int Height { get; }
        public Color BackgroundColor { get; set; } = Color.Transparent;

        public IReadOnlyList<CanvasObject> Objects => _objects;

        public event EventHandler? Changed;

        public void NotifyChanged() => Changed?.Invoke(this, EventArgs.Empty);

        public T Add<T>(T obj) where T : CanvasObject
        {
            obj.Id = _nextId++;
            _objects.Add(obj);
            return obj;
        }

        internal int NextId
        {
            get => _nextId;
            set => _nextId = value;
        }

        internal void Restore(CanvasObject obj, int id)
        {
            obj.Id = id;
            _objects.Add(obj);
            _nextId = Math.Max(_nextId, id + 1);
        }

        public CanvasObject? Find(int id) => _objects.FirstOrDefault(o => o.Id == id);

        public int IndexOf(CanvasObject obj) => _objects.IndexOf(obj);

        public void Remove(CanvasObject obj)
        {
            if (!_objects.Remove(obj)) return;
            var dependents = _objects.OfType<IntersectionObject>()
                .Where(i => (!i.FirstIsGroup && i.FirstId == obj.Id) || (!i.SecondIsGroup && i.SecondId == obj.Id)).ToList();
            if (obj.GroupId != 0 && !GroupMembers(obj.GroupId).Any(o => o is not IntersectionObject))
                dependents.AddRange(_objects.OfType<IntersectionObject>()
                    .Where(i => (i.FirstIsGroup && i.FirstId == obj.GroupId) || (i.SecondIsGroup && i.SecondId == obj.GroupId)));
            foreach (var d in dependents.Distinct().ToList()) Remove(d);
        }

        public IReadOnlyList<CanvasObject> GroupMembers(int groupId) =>
            groupId == 0 ? [] : _objects.Where(o => o.GroupId == groupId).ToList();

        public IReadOnlyList<CanvasObject> WithGroups(IEnumerable<CanvasObject> objects)
        {
            var set = objects.ToHashSet();
            var groups = set.Where(o => o.GroupId != 0).Select(o => o.GroupId).ToHashSet();
            return _objects.Where(o => set.Contains(o) || (o.GroupId != 0 && groups.Contains(o.GroupId))).ToList();
        }

        private int NewGroupId() => _objects.Count == 0 ? 1 : _objects.Max(o => o.GroupId) + 1;

        private static List<(int Id, bool IsGroup)> Units(IEnumerable<CanvasObject> objects)
        {
            var units = new List<(int Id, bool IsGroup)>();
            foreach (var o in objects)
            {
                if (o is IntersectionObject) continue;
                var unit = o.GroupId != 0 ? (o.GroupId, true) : (o.Id, false);
                if (!units.Contains(unit)) units.Add(unit);
            }
            return units;
        }

        public bool CanGroup(IEnumerable<CanvasObject> objects)
        {
            var units = Units(objects);
            return units.Count >= 2;
        }

        public bool CanUngroup(IEnumerable<CanvasObject> objects) => objects.Any(o => o.GroupId != 0);

        public int Group(IEnumerable<CanvasObject> objects)
        {
            var members = WithGroups(objects.Where(_objects.Contains));
            if (!CanGroup(members)) return 0;
            int id = NewGroupId();
            var oldGroups = members.Where(o => o.GroupId != 0).Select(o => o.GroupId).ToHashSet();
            foreach (var o in members) o.GroupId = id;
            foreach (var ix in _objects.OfType<IntersectionObject>())
            {
                if (ix.FirstIsGroup && oldGroups.Contains(ix.FirstId)) ix.FirstId = id;
                if (ix.SecondIsGroup && oldGroups.Contains(ix.SecondId)) ix.SecondId = id;
            }
            return id;
        }

        public void Ungroup(IEnumerable<CanvasObject> objects)
        {
            var groups = objects.Where(o => o.GroupId != 0).Select(o => o.GroupId).ToHashSet();
            if (groups.Count == 0) return;
            var dependents = _objects.OfType<IntersectionObject>()
                .Where(i => (i.FirstIsGroup && groups.Contains(i.FirstId)) || (i.SecondIsGroup && groups.Contains(i.SecondId))).ToList();
            foreach (var o in _objects.Where(o => groups.Contains(o.GroupId))) o.GroupId = 0;
            foreach (var d in dependents) Remove(d);
        }

        public bool CanIntersect(IEnumerable<CanvasObject> objects) => Units(objects).Count == 2;

        public IntersectionObject? AddIntersection(IEnumerable<CanvasObject> objects, Color fill)
        {
            var units = Units(objects.Where(_objects.Contains));
            if (units.Count != 2) return null;
            return Add(new IntersectionObject
            {
                FirstId = units[0].Id,
                FirstIsGroup = units[0].IsGroup,
                SecondId = units[1].Id,
                SecondIsGroup = units[1].IsGroup,
                Fill = fill
            });
        }

        public bool MoveUp(CanvasObject obj) => Move(obj, +1);

        public bool MoveDown(CanvasObject obj) => Move(obj, -1);

        private bool Move(CanvasObject obj, int step)
        {
            int i = _objects.IndexOf(obj);
            int j = i + step;
            if (i < 0 || j < 0 || j >= _objects.Count) return false;
            (_objects[i], _objects[j]) = (_objects[j], _objects[i]);
            return true;
        }

        public IntersectionObject? AddIntersection(CanvasObject first, CanvasObject second, Color fill)
        {
            if (first == second || first is IntersectionObject || second is IntersectionObject) return null;
            if (!_objects.Contains(first) || !_objects.Contains(second)) return null;
            return Add(new IntersectionObject { FirstId = first.Id, SecondId = second.Id, Fill = fill });
        }

        public IReadOnlyList<CanvasObject> Duplicate(IEnumerable<CanvasObject> source, float dx, float dy)
        {
            var originals = _objects.Where(o => source.Contains(o) && o is not IntersectionObject).ToList();
            var map = new Dictionary<int, int>();
            var groupMap = new Dictionary<int, int>();
            int nextGroup = NewGroupId();
            var copies = new List<CanvasObject>();
            foreach (var o in originals)
            {
                var copy = o.Clone();
                copy.Offset(dx, dy);
                if (o.GroupId != 0)
                {
                    if (!groupMap.TryGetValue(o.GroupId, out int g)) groupMap[o.GroupId] = g = nextGroup++;
                    copy.GroupId = g;
                }
                Add(copy);
                map[o.Id] = copy.Id;
                copies.Add(copy);
            }
            foreach (var ix in _objects.OfType<IntersectionObject>().ToList())
            {
                var firstMap = ix.FirstIsGroup ? groupMap : map;
                var secondMap = ix.SecondIsGroup ? groupMap : map;
                if (!firstMap.TryGetValue(ix.FirstId, out int first) || !secondMap.TryGetValue(ix.SecondId, out int second)) continue;
                var copy = (IntersectionObject)ix.Clone();
                copy.FirstId = first;
                copy.SecondId = second;
                if (ix.GroupId != 0) copy.GroupId = groupMap.TryGetValue(ix.GroupId, out int g) ? g : 0;
                Add(copy);
                copies.Add(copy);
            }
            return copies;
        }

        public CanvasDocument Clone()
        {
            var copy = new CanvasDocument(Width, Height)
            {
                BackgroundColor = BackgroundColor,
                _nextId = _nextId
            };
            foreach (var o in _objects) copy._objects.Add(o.Clone());
            return copy;
        }
    }
}
