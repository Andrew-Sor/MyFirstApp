using System;
using System.Collections.Generic;
using System.ComponentModel;
using Windows.ApplicationModel.Resources;
using Windows.ApplicationModel.Resources.Core;
using Windows.Globalization;
// removed diagnostic file-fallback dependencies

namespace MyNotes.Services
{
    public class Localizer : INotifyPropertyChanged
    {
        private static readonly Lazy<Localizer> _instance = new(() => new Localizer());
        public static Localizer Instance => _instance.Value;

        private ResourceLoader _loader => ResourceLoader.GetForViewIndependentUse();

        private Localizer()
        {
            // initialize resource contexts to the current language
            try
            {
                var tag = ApplicationLanguages.PrimaryLanguageOverride;
                if (string.IsNullOrEmpty(tag))
                {
                    var langs = ApplicationLanguages.Languages;
                    tag = (langs != null && langs.Count > 0) ? langs[0] : "en";
                }

                try
                {
                    var ctxView = ResourceContext.GetForCurrentView();
                    ctxView.Languages = new[] { tag };
                }
                catch { }

                try
                {
                    var ctxInd = ResourceContext.GetForViewIndependentUse();
                    ctxInd.Languages = new[] { tag };
                }
                catch { }
            }
            catch { }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void Raise(params string[] names)
        {
            if (PropertyChanged == null) return;
            foreach (var n in names)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
            }
        }

        // no diagnostic helpers
        private string GetString(string key)
        {
            try
            {
                string[] candidates = new[] {
                    key,
                    // try without common suffixes
                    key.Replace(".Text", ""),
                    key.Replace(".Content", ""),
                    key.Replace(".Header", "")
                };

                var rm = ResourceManager.Current;

                // Prefer current view context so UI language is respected
                try
                {
                    var ctxView = ResourceContext.GetForCurrentView();
                    foreach (var k in candidates)
                    {
                        if (k.Contains('.'))
                        {
                            var parts = k.Split(new[] { '.' }, 2);
                            var name = parts[0];
                            var sub = parts.Length > 1 ? parts[1] : "Text";
                            try
                            {
                                var full = "Resources/" + name + "/" + sub;
                                var val = rm.MainResourceMap.GetValue(full, ctxView);
                                var s = val?.ValueAsString;
                                if (!string.IsNullOrEmpty(s))
                                    return s;
                            }
                            catch { }
                        }
                    }
                }
                catch { }

                // Try view-independent context
                try
                {
                    var ctxInd = ResourceContext.GetForViewIndependentUse();
                    foreach (var k in candidates)
                    {
                        if (k.Contains('.'))
                        {
                            var parts = k.Split(new[] { '.' }, 2);
                            var name = parts[0];
                            var sub = parts.Length > 1 ? parts[1] : "Text";
                            try
                            {
                                var full = "Resources/" + name + "/" + sub;
                                var val = rm.MainResourceMap.GetValue(full, ctxInd);
                                var s = val?.ValueAsString;
                                if (!string.IsNullOrEmpty(s))
                                    return s;
                            }
                            catch { }
                        }
                    }
                }
                catch { }

                // Fallback to ResourceLoader
                try
                {
                    var loaderDefault = ResourceLoader.GetForViewIndependentUse();
                    foreach (var k in candidates)
                    {
                        try
                        {
                            var v = loaderDefault.GetString(k);
                            if (!string.IsNullOrEmpty(v))
                                return v;
                        }
                        catch { }
                    }
                }
                catch { }

                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        public void UpdateLanguage(string tag)
        {
            try
            {
                ApplicationLanguages.PrimaryLanguageOverride = tag;
                try
                {
                    var ctxView = ResourceContext.GetForCurrentView();
                    ctxView.Languages = new[] { tag };
                }
                catch { }

                try
                {
                    var ctxInd = ResourceContext.GetForViewIndependentUse();
                    ctxInd.Languages = new[] { tag };
                }
                catch { }
            }
            catch
            {
            }

            // Raise property changed for all exposed properties
            Raise(
                nameof(SettingsTitle), nameof(AppearanceLabel), nameof(ThemeHeaderTitle), nameof(ThemeHeaderSubtitle),
                nameof(ThemeLight), nameof(ThemeDark), nameof(ThemeDefault),
                nameof(TextHeaderTitle), nameof(TextHeaderSubtitle), nameof(FontComboBoxHeader), nameof(FontSizeComboBoxHeader),
                nameof(LanguageLabel), nameof(LanguageHeaderTitle), nameof(LanguageHeaderSubtitle),
                nameof(InfoTitle), nameof(InfoCopyright), nameof(VersionText),
                nameof(UpdateButton), nameof(RepoButton), nameof(IssueButton), nameof(SupportButton), nameof(DeveloperButton)
            );
        }

        public string SettingsTitle => GetString("Settings_TitleText.Text");
        public string AppearanceLabel => GetString("Settings_AppearanceLabel.Text");

        // Additional keys used by other pages
        public string AllNotes_NewNote_Label => GetString("AllNotes_NewNote.Label");
        public string AllNotes_Title => GetString("AllNotes_Title.Text");
        public string AllNotes_Settings_Label => GetString("AllNotes_Settings.Label");
        public string Menu_Edit_Text => GetString("Menu_Edit.Text");
        public string Menu_Delete_Text => GetString("Menu_Delete.Text");
        public string EditNote_Save_Label => GetString("EditNote_Save.Label");
        public string EditNote_Delete_Label => GetString("EditNote_Delete.Label");
        public string EditNote_Placeholder => GetString("EditNote_Placeholder.Text");
        public string DeleteDialog_Title => GetString("DeleteDialog_Title");
        public string DeleteDialog_PrimaryButton => GetString("DeleteDialog_PrimaryButton");
        public string DeleteDialog_CloseButton => GetString("DeleteDialog_CloseButton");
        public string SaveDialog_Title => GetString("SaveDialog_Title");
        public string SaveDialog_Content => GetString("SaveDialog_Content");
        public string SaveDialog_PrimaryButton => GetString("SaveDialog_PrimaryButton");
        public string SaveDialog_SecondaryButton => GetString("SaveDialog_SecondaryButton");
        public string SaveDialog_CloseButton => GetString("SaveDialog_CloseButton");

        public string ThemeHeaderTitle => GetString("Theme_HeaderTitle.Text");
        public string ThemeHeaderSubtitle => GetString("Theme_HeaderSubtitle.Text");

        public string ThemeLight => GetString("Theme_Light.Content");
        public string ThemeDark => GetString("Theme_Dark.Content");
        public string ThemeDefault => GetString("Theme_Default.Content");

        public string TextHeaderTitle => GetString("Text_HeaderTitle.Text");
        public string TextHeaderSubtitle => GetString("Text_HeaderSubtitle.Text");

        public string FontComboBoxHeader => GetString("FontComboBox.Header");
        public string FontSizeComboBoxHeader => GetString("FontSizeComboBox.Header");

        public string LanguageLabel => GetString("Language_Label.Text");
        public string LanguageHeaderTitle => GetString("Language_HeaderTitle.Text");
        public string LanguageHeaderSubtitle => GetString("Language_HeaderSubtitle.Text");

        public string InfoTitle => GetString("Info_Title.Text");
        public string InfoCopyright => GetString("Info_Copyright.Text");
        public string VersionText => GetString("VersionText.Text");

        public string UpdateButton => GetString("UpdateButton.Content");
        public string RepoButton => GetString("RepoButton.Content");
        public string IssueButton => GetString("IssueButton.Content");
        public string SupportButton => GetString("SupportButton.Content");
        public string DeveloperButton => GetString("DeveloperButton.Content");
    }
}
