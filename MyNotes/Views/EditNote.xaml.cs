using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyNotes.Models;
using System;

namespace MyNotes.Views
{
    public sealed partial class EditNote : Page
    {
        private Note? noteModel;

        public string NoteText { get; private set; } = string.Empty;

        public EditNote()
        {
            InitializeComponent();
        this.DataContext = MyNotes.Services.Localizer.Instance;
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is Note note)
            {
                noteModel = note;
                NoteText = noteModel.Text;
            }
            else
            {
                noteModel = new Note();
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (NoteEditor.Text != string.Empty)
            {
                ContentDialog dialog = new();

                // ”казываем XamlRoot из основного окна (гарантирует наследование темы)
                if (App.Instance?.MainWindow?.Content is FrameworkElement fe)
                {
                    dialog.XamlRoot = fe.XamlRoot;
                    // ѕередаЄм текущую тему приложени€ диалогу
                    dialog.RequestedTheme = fe.RequestedTheme;
                }
                else
                {
                    // fallback на текущий XamlRoot страницы
                    dialog.XamlRoot = this.XamlRoot;
                    dialog.RequestedTheme = this.RequestedTheme;
                }

                dialog.Title = MyNotes.Services.Localizer.Instance.DeleteDialog_Title;
                dialog.PrimaryButtonText = MyNotes.Services.Localizer.Instance.DeleteDialog_PrimaryButton;
                dialog.CloseButtonText = MyNotes.Services.Localizer.Instance.DeleteDialog_CloseButton;
                dialog.DefaultButton = ContentDialogButton.Primary;
                dialog.Content = new DialogDelete();

                var result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    await noteModel.DeleteAsync();
                    Frame.GoBack();
                }
            }
            else
            {
                await noteModel.DeleteAsync();
                Frame.GoBack();
            }
        }

        private async void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (NoteEditor.Text != NoteText)
            {
                ContentDialog dialog = new();

                // ”казываем XamlRoot из основного окна и тему
                if (App.Instance?.MainWindow?.Content is FrameworkElement fe)
                {
                    dialog.XamlRoot = fe.XamlRoot;
                    dialog.RequestedTheme = fe.RequestedTheme;
                }
                else
                {
                    dialog.XamlRoot = this.XamlRoot;
                    dialog.RequestedTheme = this.RequestedTheme;
                }

                dialog.Title = MyNotes.Services.Localizer.Instance.SaveDialog_Title;
                dialog.PrimaryButtonText = MyNotes.Services.Localizer.Instance.SaveDialog_PrimaryButton;
                dialog.SecondaryButtonText = MyNotes.Services.Localizer.Instance.SaveDialog_SecondaryButton;
                dialog.CloseButtonText = MyNotes.Services.Localizer.Instance.SaveDialog_CloseButton;
                dialog.DefaultButton = ContentDialogButton.Primary;
                dialog.Content = new DialogSave();

                var result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Secondary)
                {
                    Frame.GoBack();
                }

                if (result == ContentDialogResult.Primary)
                {
                    await noteModel.SaveAsync();
                    Frame.GoBack();
                }
            }
            else
            {
                Frame.GoBack();
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            NoteText = NoteEditor.Text;
            await noteModel.SaveAsync();
        }
    }
}
