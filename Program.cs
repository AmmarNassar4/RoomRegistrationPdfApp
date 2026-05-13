using PdfSharp.Fonts;

namespace RoomRegistrationPdfApp;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        GlobalFontSettings.UseWindowsFontsUnderWindows = true;

        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}
