using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI; // Colors
using WinRT.Interop; // WindowNative
using Microsoft.UI; // (для совместимости типов)
using Windows.UI.ViewManagement; // UISettings, UIColorType
using static MyNotes.App;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyNotes
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Hide the default system title bar.
            ExtendsContentIntoTitleBar = true;
            // Replace system title bar with the WinUI TitleBar.
            SetTitleBar(AppTitleBar);

            // Применяем сохранённую тему к заголовку при создании окна (если есть)
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
        }

        /// <summary>
        /// Применяет тему к элементу заголовка окна и к системной панели заголовка (AppWindow.TitleBar).
        /// Для ElementTheme.Default определяется системная (осевая) тема и используются соответствующие цвета.
        /// </summary>
        public void ApplyThemeToTitleBar(ElementTheme elementTheme)
        {
            // Применяем тему к XAML-элементу заголовка (если он есть в дереве)
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

            // Также настраиваем цвета системной панели заголовка через AppWindow.TitleBar
            try
            {
                // Получаем AppWindow для этого окна
                var hwnd = WindowNative.GetWindowHandle(this);
                var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
                var appWindow = AppWindow.GetFromWindowId(windowId);
                if (appWindow?.TitleBar != null)
                {
                    var titleBar = appWindow.TitleBar;

                    // Если тема Default — определяем системную тему (светлая/тёмная)
                    ElementTheme effectiveTheme = elementTheme;
                    if (elementTheme == ElementTheme.Default)
                    {
                        try
                        {
                            var uiSettings = new UISettings();
                            var bg = uiSettings.GetColorValue(UIColorType.Background); // system background color
                            // вычисляем яркость (luminance)
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
                // В некоторых окружениях AppWindow/GetWindowId может быть недоступен — безопасно игнорируем
            }
        }

        private void AppTitleBar_BackRequested(TitleBar sender, object args)
        {
            if (rootFrame.CanGoBack == true)
            {
                rootFrame.GoBack();
            }
        }
    }
}
