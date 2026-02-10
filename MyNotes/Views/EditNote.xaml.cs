using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using MyNotes.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using static System.Net.Mime.MediaTypeNames;

namespace MyNotes.Views
{
    public sealed partial class EditNote : Page
    {
        private Note? noteModel;

        public string NoteText { get; private set; } = string.Empty;

        public EditNote()
        {
            InitializeComponent();
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
                ContentDialog dialog = new ContentDialog();

                // Указываем XamlRoot из основного окна (гарантирует наследование темы)
                if (App.Instance?.MainWindow?.Content is FrameworkElement fe)
                {
                    dialog.XamlRoot = fe.XamlRoot;
                    // Передаём текущую тему приложения диалогу
                    dialog.RequestedTheme = fe.RequestedTheme;
                }
                else
                {
                    // fallback на текущий XamlRoot страницы
                    dialog.XamlRoot = this.XamlRoot;
                    dialog.RequestedTheme = this.RequestedTheme;
                }

                dialog.Title = "Удалить?";
                dialog.PrimaryButtonText = "Удалить";
                dialog.CloseButtonText = "Отмена";
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
                Frame.GoBack();
            }
        }

        private async void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (NoteEditor.Text != NoteText)
            {
                ContentDialog dialog = new ContentDialog();

                // Указываем XamlRoot из основного окна и тему
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

                dialog.Title = "Сохранить?";
                dialog.PrimaryButtonText = "Сохранить";
                dialog.SecondaryButtonText = "Не сохранять";
                dialog.CloseButtonText = "Отмена";
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
