using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Immutable;
using Windows.Storage;
using Windows.ApplicationModel.Resources;

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
        const string LanguageSettingKey = "AppLanguage"; // BCP-47 tag like "en-US", "ru-RU"
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        private bool _isInitializing = false;

        public SettingsPage()
        {
            InitializeComponent();
            this.DataContext = MyNotes.Services.Localizer.Instance;
        }

        // Expose Localizer for x:Bind in XAML
        public MyNotes.Services.Localizer Localizer => MyNotes.Services.Localizer.Instance;

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

            // localization via Localizer bindings

            // Заполнить список языков и установить выбранный язык
            _isInitializing = true;
            try
            {
                var savedLang = localSettings.Values[LanguageSettingKey] as string;
                LanguageComboBox.Items.Clear();
                foreach (var lang in Windows.Globalization.ApplicationLanguages.ManifestLanguages)
                {
                    var language = new Windows.Globalization.Language(lang);
                    var display = $"{language.DisplayName} ({lang})";
                    var item = new ComboBoxItem { Content = display, Tag = lang };
                    LanguageComboBox.Items.Add(item);

                    if (!string.IsNullOrEmpty(savedLang) && string.Equals(savedLang, lang, StringComparison.OrdinalIgnoreCase))
                    {
                        LanguageComboBox.SelectedItem = item;
                    }
                }

                // Если сохранённого языка нет, попробуем выбрать текущий PrimaryLanguageOverride
                if (LanguageComboBox.SelectedItem == null)
                {
                    var current = Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride;
                    if (!string.IsNullOrEmpty(current))
                    {
                        foreach (ComboBoxItem it in LanguageComboBox.Items)
                        {
                            if (string.Equals((it.Tag as string) ?? string.Empty, current, StringComparison.OrdinalIgnoreCase))
                            {
                                LanguageComboBox.SelectedItem = it;
                                break;
                            }
                        }
                    }
                }
            }
            finally
            {
                _isInitializing = false;
            }

            // localization via Localizer

            // Ensure Localizer properties are populated for current language
            try
            {
                var savedLang = localSettings.Values[LanguageSettingKey] as string;
                var langToUse = !string.IsNullOrEmpty(savedLang) ? savedLang : (Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride ?? (Windows.Globalization.ApplicationLanguages.Languages.Count > 0 ? Windows.Globalization.ApplicationLanguages.Languages[0] : "en-US"));
                MyNotes.Services.Localizer.Instance.UpdateLanguage(langToUse);
            }
            catch { }
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

        private async void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing)
                return;

            if (LanguageComboBox.SelectedItem is ComboBoxItem cbi)
            {
                var tag = cbi.Tag?.ToString();
                if (string.IsNullOrEmpty(tag))
                    return;

                // Не сохраняем и не меняем PrimaryLanguageOverride до подтверждения перезапуска.

                // Сохранить выбранный язык и установить PrimaryLanguageOverride
                localSettings.Values[LanguageSettingKey] = tag;
                Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = tag;

                // Попробовать обновить контекст ресурсов для текущего вида и для view-independent use
                try
                {
                    var ctxView = Windows.ApplicationModel.Resources.Core.ResourceContext.GetForCurrentView();
                    ctxView.Languages = new[] { tag };
                }
                catch { }

                try
                {
                    var ctxInd = Windows.ApplicationModel.Resources.Core.ResourceContext.GetForViewIndependentUse();
                    ctxInd.Languages = new[] { tag };
                }
                catch { }

                // Обновить локализатор и оповестить UI
                MyNotes.Services.Localizer.Instance.UpdateLanguage(tag);

                // Обновить заголовок окна из ресурсов
                try
                {
                    var loader = ResourceLoader.GetForCurrentView();
                    var appTitle = loader.GetString("Info_Title");
                    var mainWinFE = App.Instance?.MainWindow?.Content as FrameworkElement;
                    if (mainWinFE != null)
                    {
                        // Обновляем TitleBar.Title, если он существует
                        if (mainWinFE.FindName("AppTitleBar") is Microsoft.UI.Xaml.Controls.TitleBar titleBar)
                        {
                            titleBar.Title = appTitle;
                        }

                        // Также обновляем заголовок окна
                        try
                        {
                            App.Instance.MainWindow.Title = appTitle;
                        }
                        catch { }
                    }
                }
                catch { }

                // Пересоздать всю страницу настроек: перейти на новый экземпляр SettingsPage
                try
                {
                    var mainWin = App.Instance?.MainWindow?.Content as FrameworkElement;
                    if (mainWin != null)
                    {
                        var frame = mainWin.FindName("rootFrame") as Frame;
                        if (frame != null)
                        {
                            // Навигируем на новый экземпляр SettingsPage
                            frame.Navigate(typeof(SettingsPage), System.Guid.NewGuid().ToString());

                            // Удаляем предыдущую запись из backstack, чтобы не дублировать
                            if (frame.BackStackDepth > 0)
                            {
                                try { frame.BackStack.RemoveAt(frame.BackStackDepth - 1); } catch { }
                            }
                        }
                    }
                }
                catch { }
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
            App.Instance?.ApplyTheme(elementTheme);
        }
    }
}
