namespace KebuzForge.App.Models
{
    internal class AppSettings
    {

        public int  PixelWidth      { get; set; } = 64;
        public int  PixelHeight     { get; set; } = 64;
        public bool KeepAspect      { get; set; } = true;
        public int  PixelAlgorithm  { get; set; } = 0;

        public int ColorCount        { get; set; } = 16;
        public int RetroPaletteIndex { get; set; } = 0;

        public int DitherMode      { get; set; } = 0;
        public int DitherIntensity { get; set; } = 50;

        public int  EffectIndex     { get; set; } = 0;
        public int  CylinderDir     { get; set; } = 0;
        public int  SphereScale     { get; set; } = 100;
        public int  SphereBulge     { get; set; } = 100;
        public int  OffsetU         { get; set; } = 0;
        public int  OffsetV         { get; set; } = 0;
        public int  CornerSharpness { get; set; } = 0;
        public bool ShapeFromRect  { get; set; } = false;

        public int Ambient      { get; set; } = 25;
        public int Diffuse      { get; set; } = 85;
        public int LightAzimuth { get; set; } = -45;
        public int LightElev    { get; set; } = 45;
        public bool Specular    { get; set; } = true;

        public bool ShowEdges     { get; set; } = false;
        public int  EdgeThickness { get; set; } = 1;
        public int  EdgeMode      { get; set; } = 0;
        public int  EdgeColor     { get; set; } = unchecked((int)0xFF000000);

        public int BgTolerance { get; set; } = 15;

        public int ThemeIndex { get; set; } = 0;

        public int LanguageIndex { get; set; } = 0;
    }
}
