// <copyright file="WindowTheme.cs" company="AAllard">License: http://www.gnu.org/licenses/gpl.html GPL version 3.</copyright>

namespace FileConverter.Views
{
    using System;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Interop;

    /// <summary>
    /// Applies the dark theme to the parts of a window that the resource dictionaries can't reach on their own.
    /// </summary>
    public static class WindowTheme
    {
        // Asks Windows to draw the title bar in dark mode. The attribute was 19 on Windows 10 before version 20H1.
        private const int DwmUseImmersiveDarkMode = 20;
        private const int DwmUseImmersiveDarkModeBefore20H1 = 19;

        public static void Apply(Window window)
        {
            if (window == null)
            {
                throw new ArgumentNullException(nameof(window));
            }

            // Implicit styles are matched on the exact type of the element, and every window of the application derives
            // from Window, so the Window style of CustomStyles.xaml never applied to any of them: they kept the white
            // background of the system theme. Reference it explicitly.
            window.SetResourceReference(FrameworkElement.StyleProperty, typeof(Window));

            // The title bar is drawn by Windows, not by WPF. It has to be switched to dark mode once the window handle
            // exists but before the window is displayed, otherwise it only turns dark on the next repaint of the frame.
            window.SourceInitialized += WindowTheme.OnSourceInitialized;
        }

        private static void OnSourceInitialized(object sender, EventArgs eventArgs)
        {
            IntPtr handle = new WindowInteropHelper((Window)sender).Handle;
            int enabled = 1;
            if (WindowTheme.DwmSetWindowAttribute(handle, WindowTheme.DwmUseImmersiveDarkMode, ref enabled, sizeof(int)) != 0)
            {
                WindowTheme.DwmSetWindowAttribute(handle, WindowTheme.DwmUseImmersiveDarkModeBefore20H1, ref enabled, sizeof(int));
            }
        }

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);
    }
}
