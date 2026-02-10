using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using MyNotes;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyNotes.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        public ImmutableArray<string> ElementThemeOptions { get; } = ImmutableArray.Create(Enum.GetNames<ElementTheme>());
        const string ThemeSettingKey = "AppTheme"; // "Default", "Light", "Dark"
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        public SettingsPage()
        {
            InitializeComponent();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Установить выбранный RadioButton из настроек (по умолчанию "Default")
            var saved = localSettings.Values[ThemeSettingKey] as string ?? "Default";

            foreach (var item in ThemeRadioButtons.Items)
            {
                if (item is RadioButton rb && (rb.Tag?.ToString() ?? "Default") == saved)
                {
                    rb.IsChecked = true;
                    break;
                }
            }

            ApplyTheme(saved);
        }

        private void ThemeRadioButtons_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ThemeRadioButtons.SelectedItem is RadioButton rb)
            {
                var tag = rb.Tag?.ToString() ?? "Default";
                localSettings.Values[ThemeSettingKey] = tag;
                ApplyTheme(tag);
            }
        }

        private void ApplyTheme(string theme)
        {
            ElementTheme elementTheme = theme switch
            {
                "Light" => ElementTheme.Light,
                "Dark" => ElementTheme.Dark,
                _ => ElementTheme.Default
            };

            // Ранее использовалось Application.Current.Windows -- в WinUI3 этого нет.
            // Используем публичный статический экземпляр App для доступа к основному окну.
            if (App.Instance != null)
            {
                App.Instance.ApplyTheme(elementTheme);
            }
        }
    }
}
