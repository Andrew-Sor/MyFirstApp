using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;

namespace MyNotes
{
    public partial class App : Application
    {
        private Window? _window;

        // Статический экземпляр приложения для глобального доступа
        public static App? Instance { get; private set; }

        public App()
        {
            InitializeComponent();
            Instance = this;
        }

        /// <summary>
        /// Доступ к основному окну приложения (если создано).
        /// </summary>
        public Window? MainWindow => _window;

        /// <summary>
        /// Применяет тему к контенту окна и, при необходимости, к заголовку.
        /// </summary>
        public void ApplyTheme(ElementTheme elementTheme)
        {
            if (_window == null)
                return;

            if (_window.Content is FrameworkElement fe)
            {
                // Применяем тему к корневому элементу содержимого окна
                fe.RequestedTheme = elementTheme;
            }

            // Если окно — наш MainWindow, вызываем метод для явного применения темы к заголовку
            if (_window is MainWindow mw)
            {
                mw.ApplyThemeToTitleBar(elementTheme);
            }
            else
            {
                // Резервный вариант: попытаться найти заголовок по имени в дереве контента
                if (_window.Content is FrameworkElement root)
                {
                    if (root.FindName("AppTitleBar") is FrameworkElement titleBar)
                    {
                        titleBar.RequestedTheme = elementTheme;
                    }
                }
            }
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            _window = new MainWindow();

            // Применяем сохранённую тему (если есть) сразу после создания окна
            var localSettings = ApplicationData.Current.LocalSettings;
            if (localSettings.Values.TryGetValue("AppTheme", out object savedObj) && savedObj is string savedTheme)
            {
                ElementTheme elementTheme = savedTheme switch
                {
                    "Light" => ElementTheme.Light,
                    "Dark" => ElementTheme.Dark,
                    _ => ElementTheme.Default
                };

                // Применяем тему к окну и заголовку
                ApplyTheme(elementTheme);
            }

            _window.Activate();
        }
    }
}
