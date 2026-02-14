using Microsoft.UI; // (дл€ совместимости типов)
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Runtime.InteropServices;
using Windows.Storage;
using Windows.UI; // Colors
using Windows.UI.ViewManagement; // UISettings, UIColorType
using WinRT.Interop; // WindowNative

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyNotes
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        // Win32 interop for subclassing and MINMAXINFO
        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        }

        private delegate IntPtr SubclassProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam, IntPtr uIdSubclass, IntPtr dwRefData);
        private SubclassProcDelegate? _subclassProcInstance;

        [DllImport("comctl32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool SetWindowSubclass(IntPtr hWnd, SubclassProcDelegate pfnSubclass, IntPtr uIdSubclass, IntPtr dwRefData);

        [DllImport("comctl32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr DefSubclassProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern uint GetDpiForWindow(IntPtr hWnd);

        public MainWindow()
        {
            InitializeComponent();

            // Hide the default system title bar.
            ExtendsContentIntoTitleBar = true;
            // Replace system title bar with the WinUI TitleBar.
            SetTitleBar(AppTitleBar);

            // ѕримен€ем сохранЄнную тему к заголовку при создании окна (если есть)
            var localSettings = ApplicationData.Current.LocalSettings;
            if (localSettings.Values.TryGetValue("AppTheme", out object savedObj) && savedObj is string savedTheme)
            {
                ElementTheme elementTheme = savedTheme switch
                {
                    "Light" => ElementTheme.Light,
                    "Dark" => ElementTheme.Dark,
                    _ => ElementTheme.Default
                };

                ApplyThemeToTitleBar(elementTheme);
            }

            // «адаЄм минимальный размер окна (ширина x высота в DIPs) Ч через обработку WM_GETMINMAXINFO
            const int minWidth = 505;
            const int minHeight = 330;

            try
            {
                var hwnd = WindowNative.GetWindowHandle(this);

                // ѕодклассируем окно, чтобы перехватить WM_GETMINMAXINFO и задать минимум
                _subclassProcInstance = new SubclassProcDelegate(SubclassProcLocal);
                SetWindowSubclass(hwnd, _subclassProcInstance, IntPtr.Zero, IntPtr.Zero);

                IntPtr SubclassProcLocal(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam, IntPtr uIdSubclass, IntPtr dwRefData)
                {
                    const uint WM_GETMINMAXINFO = 0x24;
                    if (msg == WM_GETMINMAXINFO)
                    {
                        try
                        {
                            uint dpi = 96;
                            try
                            {
                                dpi = GetDpiForWindow(hWnd);
                                if (dpi == 0) dpi = 96;
                            }
                            catch
                            {
                                dpi = 96;
                            }

                            double scale = dpi / 96.0;
                            int pxMinW = (int)Math.Round(minWidth * scale);
                            int pxMinH = (int)Math.Round(minHeight * scale);

                            // Marshal MINMAXINFO, set ptMinTrackSize
                            var mmi = Marshal.PtrToStructure<MINMAXINFO>(lParam);
                            mmi.ptMinTrackSize.x = pxMinW;
                            mmi.ptMinTrackSize.y = pxMinH;
                            Marshal.StructureToPtr(mmi, lParam, true);
                        }
                        catch
                        {
                            // ignore
                        }
                    }

                    return DefSubclassProc(hWnd, msg, wParam, lParam);
                }
            }
            catch
            {
                // ѕодклассирование может быть недоступно в некоторых окружени€х Ч игнорируем
            }
        }

        /// <summary>
        /// ѕримен€ет тему к элементу заголовка окна и к системной панели заголовка (AppWindow.TitleBar).
        /// ƒл€ ElementTheme.Default определ€етс€ системна€ (осева€) тема и используютс€ соответствующие цвета.
        /// </summary>
        public void ApplyThemeToTitleBar(ElementTheme elementTheme)
        {
            // ѕримен€ем тему к XAML-элементу заголовка (если он есть в дереве)
            try
            {
                if (AppTitleBar != null)
                {
                    AppTitleBar.RequestedTheme = elementTheme;
                }
            }
            catch
            {
                // игнорируем ошибки установки RequestedTheme
            }

            // “акже настраиваем цвета системной панели заголовка через AppWindow.TitleBar
            try
            {
                // ѕолучаем AppWindow дл€ этого окна
                var hwnd = WindowNative.GetWindowHandle(this);
                var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
                var appWindow = AppWindow.GetFromWindowId(windowId);
                if (appWindow?.TitleBar != null)
                {
                    var titleBar = appWindow.TitleBar;

                    // ≈сли тема Default Ч определ€ем системную тему (светла€/тЄмна€)
                    ElementTheme effectiveTheme = elementTheme;
                    if (elementTheme == ElementTheme.Default)
                    {
                        try
                        {
                            var uiSettings = new UISettings();
                            var bg = uiSettings.GetColorValue(UIColorType.Background); // system background color
                            // вычисл€ем €ркость (luminance)
                            double lum = (0.2126 * bg.R + 0.7152 * bg.G + 0.0722 * bg.B) / 255.0;
                            effectiveTheme = lum < 0.5 ? ElementTheme.Dark : ElementTheme.Light;
                        }
                        catch
                        {
                            // fallback на светлую тему, если определить системную не удалось
                            effectiveTheme = ElementTheme.Light;
                        }
                    }

                    if (effectiveTheme == ElementTheme.Dark)
                    {
                        titleBar.BackgroundColor = Colors.Black;
                        titleBar.ForegroundColor = Colors.White;
                        titleBar.ButtonBackgroundColor = Colors.Transparent;
                        titleBar.ButtonForegroundColor = Colors.White;
                        titleBar.ButtonHoverBackgroundColor = Color.FromArgb(0xFF, 0x33, 0x33, 0x33);
                        titleBar.ButtonHoverForegroundColor = Colors.White;
                        titleBar.ButtonPressedBackgroundColor = Color.FromArgb(0xFF, 0x22, 0x22, 0x22);
                        titleBar.ButtonPressedForegroundColor = Colors.White;
                        titleBar.InactiveBackgroundColor = Color.FromArgb(0xFF, 0x12, 0x12, 0x12);
                        titleBar.InactiveForegroundColor = Colors.Gray;
                    }
                    else // Light
                    {
                        titleBar.BackgroundColor = Colors.White;
                        titleBar.ForegroundColor = Colors.Black;
                        titleBar.ButtonBackgroundColor = Colors.Transparent;
                        titleBar.ButtonForegroundColor = Colors.Black;
                        titleBar.ButtonHoverBackgroundColor = Color.FromArgb(0xFF, 0xEE, 0xEE, 0xEE);
                        titleBar.ButtonHoverForegroundColor = Colors.Black;
                        titleBar.ButtonPressedBackgroundColor = Color.FromArgb(0xFF, 0xDD, 0xDD, 0xDD);
                        titleBar.ButtonPressedForegroundColor = Colors.Black;
                        titleBar.InactiveBackgroundColor = Color.FromArgb(0xFF, 0xF8, 0xF8, 0xF8);
                        titleBar.InactiveForegroundColor = Colors.Gray;
                    }
                }
            }
            catch
            {
                // ¬ некоторых окружени€х AppWindow/GetWindowId может быть недоступен Ч безопасно игнорируем
            }
        }
    }
}
