using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace KebuzForge.Canvas
{
    public static class CanvasJson
    {
        public static JsonObject Write(CanvasDocument doc, Func<Bitmap, string> imageName)
        {
            var objects = new JsonArray();
            foreach (var o in doc.Objects)
                objects.Add(WriteObject(o, imageName));
            return new JsonObject
            {
                ["width"] = doc.Width,
                ["height"] = doc.Height,
                ["background"] = ColorText(doc.BackgroundColor),
                ["nextId"] = doc.NextId,
                ["objects"] = objects
            };
        }

        public static CanvasDocument Read(JsonObject node, Func<string, Bitmap> loadImage)
        {
            var doc = new CanvasDocument(Int(node, "width"), Int(node, "height"))
            {
                BackgroundColor = ParseColor(Str(node, "background")) ?? Color.Transparent
            };
            if (node["objects"] is JsonArray objects)
                foreach (var item in objects.OfType<JsonObject>())
                    doc.Restore(ReadObject(item, loadImage), Int(item, "id"));
            if (node["nextId"] is JsonValue next)
                doc.NextId = Math.Max(doc.NextId, next.GetValue<int>());
            return doc;
        }

        private static JsonObject WriteObject(CanvasObject o, Func<Bitmap, string> imageName)
        {
            var b = o.Bounds;
            var j = new JsonObject
            {
                ["type"] = TypeName(o),
                ["id"] = o.Id,
                ["bounds"] = new JsonArray(b.X, b.Y, b.Width, b.Height),
                ["rotation"] = o.Rotation,
                ["fill"] = ColorText(o.Fill),
                ["stroke"] = ColorText(o.Stroke),
                ["strokeWidth"] = o.StrokeWidth,
                ["opacity"] = o.Opacity
            };
            if (o.Shadow is { } s)
                j["shadow"] = new JsonObject { ["x"] = s.OffsetX, ["y"] = s.OffsetY, ["color"] = ColorText(s.Color) };
            if (o.GroupId != 0)
                j["group"] = o.GroupId;
            switch (o)
            {
                case RectObject r:
                    j["cornerRadius"] = r.CornerRadius;
                    break;
                case PolygonObject p:
                    j["points"] = new JsonArray(p.Points.Select(pt => (JsonNode)new JsonArray(pt.X, pt.Y)).ToArray());
                    break;
                case LineObject l:
                    j["start"] = new JsonArray(l.Start.X, l.Start.Y);
                    j["end"] = new JsonArray(l.End.X, l.End.Y);
                    j["arrow"] = l.ArrowEnd;
                    break;
                case ArcObject a:
                    j["startAngle"] = a.StartAngle;
                    j["sweepAngle"] = a.SweepAngle;
                    j["arrow"] = a.ArrowEnd;
                    break;
                case TextObject t:
                    j["text"] = t.Text;
                    j["font"] = t.FontName;
                    j["bold"] = t.Bold;
                    break;
                case ImageObject i:
                    j["image"] = imageName(i.Image);
                    break;
                case IntersectionObject x:
                    j["first"] = x.FirstId;
                    j["second"] = x.SecondId;
                    if (x.FirstIsGroup) j["firstGroup"] = true;
                    if (x.SecondIsGroup) j["secondGroup"] = true;
                    break;
            }
            return j;
        }

        private static CanvasObject ReadObject(JsonObject j, Func<string, Bitmap> loadImage)
        {
            string type = Str(j, "type") ?? "";
            CanvasObject o = type switch
            {
                "rect" => new RectObject { CornerRadius = Float(j, "cornerRadius") },
                "ellipse" => new EllipseObject(),
                "polygon" => new PolygonObject { Points = ReadPoints(j["points"]) },
                "line" => new LineObject { Start = ReadPoint(j["start"]), End = ReadPoint(j["end"]), ArrowEnd = Bool(j, "arrow") },
                "arc" => new ArcObject { StartAngle = Float(j, "startAngle"), SweepAngle = Float(j, "sweepAngle"), ArrowEnd = Bool(j, "arrow") },
                "text" => new TextObject { Text = Str(j, "text") ?? "", FontName = Str(j, "font") ?? "Arial", Bold = Bool(j, "bold") },
                "image" => new ImageObject(loadImage(Str(j, "image") ?? throw new FormatException("image without file"))),
                "intersection" => new IntersectionObject
                {
                    FirstId = Int(j, "first"),
                    SecondId = Int(j, "second"),
                    FirstIsGroup = Bool(j, "firstGroup"),
                    SecondIsGroup = Bool(j, "secondGroup")
                },
                _ => throw new FormatException($"unknown object type '{type}'")
            };
            if (o is not LineObject && j["bounds"] is JsonArray b && b.Count == 4)
                o.Bounds = new RectangleF(b[0]!.GetValue<float>(), b[1]!.GetValue<float>(), b[2]!.GetValue<float>(), b[3]!.GetValue<float>());
            o.Rotation = Float(j, "rotation");
            o.Fill = ParseColor(Str(j, "fill"));
            o.Stroke = ParseColor(Str(j, "stroke"));
            o.StrokeWidth = Float(j, "strokeWidth", 4f);
            o.Opacity = Float(j, "opacity", 1f);
            o.GroupId = j["group"] is JsonValue g ? g.GetValue<int>() : 0;
            if (j["shadow"] is JsonObject s)
                o.Shadow = new ShadowSpec(Float(s, "x"), Float(s, "y"), ParseColor(Str(s, "color")) ?? Color.Black);
            return o;
        }

        private static string TypeName(CanvasObject o) => o switch
        {
            RectObject => "rect",
            EllipseObject => "ellipse",
            PolygonObject => "polygon",
            LineObject => "line",
            ArcObject => "arc",
            TextObject => "text",
            ImageObject => "image",
            IntersectionObject => "intersection",
            _ => throw new NotSupportedException(o.GetType().Name)
        };

        public static string? ColorText(Color? c) => c is { } v ? $"#{v.A:X2}{v.R:X2}{v.G:X2}{v.B:X2}" : null;

        public static Color? ParseColor(string? text)
        {
            if (text is null || text.Length != 9 || text[0] != '#') return null;
            return uint.TryParse(text.AsSpan(1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint argb)
                ? Color.FromArgb(unchecked((int)argb))
                : null;
        }

        private static PointF ReadPoint(JsonNode? n) =>
            n is JsonArray a && a.Count == 2 ? new PointF(a[0]!.GetValue<float>(), a[1]!.GetValue<float>()) : PointF.Empty;

        private static PointF[] ReadPoints(JsonNode? n) =>
            n is JsonArray a ? a.Select(ReadPoint).ToArray() : [new(0f, 0f), new(1f, 0.5f), new(0f, 1f)];

        private static string? Str(JsonObject j, string key) =>
            j[key] is JsonValue v && v.TryGetValue(out string? s) ? s : null;

        private static int Int(JsonObject j, string key) =>
            j[key] is JsonValue v ? v.GetValue<int>() : throw new FormatException($"missing '{key}'");

        private static float Float(JsonObject j, string key, float fallback = 0f) =>
            j[key] is JsonValue v ? v.GetValue<float>() : fallback;

        private static bool Bool(JsonObject j, string key) =>
            j[key] is JsonValue v && v.GetValue<bool>();
    }
}
