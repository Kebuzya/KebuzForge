namespace KebuzForge.App
{
    internal static class Program
    {

        [STAThread]
        static void Main()
        {
            Core.AppPaths.MigrateLegacy();
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }
}
