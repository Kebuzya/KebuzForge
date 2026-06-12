namespace WinFormsApp1.Models
{
    internal class AppState
    {

        public int    TargetWidth      { get; set; } = 64;
        public int    TargetHeight     { get; set; } = 64;
        public string PixelAlgorithm  { get; set; } = "Average";

        public int    ColorCount       { get; set; } = 16;
        public string RetroPalette    { get; set; } = "Custom";

        public string DitherMode      { get; set; } = "None";
        public int    DitherIntensity  { get; set; } = 50;

        public string Effect           { get; set; } = "None";
        public string CylinderDir     { get; set; } = "Horizontal";

        public int    Ambient          { get; set; } = 30;
        public int    Diffuse          { get; set; } = 70;

        public bool   ShowEdges        { get; set; } = false;
        public int    EdgeThickness    { get; set; } = 1;
    }
}
