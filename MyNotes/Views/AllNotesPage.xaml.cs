using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyNotes.Models;
using System;
using System.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyNotes.Views;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class AllNotesPage : Page
{
    public string NoteText { get; private set; } = string.Empty;

    private AllNotes notesModel = new();
    public AllNotesPage()
    {
        InitializeComponent();
        this.DataContext = MyNotes.Services.Localizer.Instance;
    }

    private void NewNoteButton_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(EditNote));
    }

    private void NotesView_ItemClick(object sender, ItemClickEventArgs args)
    {
        Frame.Navigate(typeof(EditNote), args.ClickedItem);
    }

    private void Edit_Click(object sender, RoutedEventArgs e)
    {
        var menuItem = (MenuFlyoutItem)sender;
        var menunote = menuItem.DataContext as Models.Note;

        if (menunote == null)
        {
            var parentMenu = menuItem.Parent as MenuFlyout;
            var target = parentMenu?.Target as FrameworkElement;
            menunote = target?.DataContext as Models.Note;
        }

        Frame.Navigate(typeof(EditNote), menunote);
    }

    private async void Delete_Click(object sender, RoutedEventArgs e)
    {
        var menuItem = (MenuFlyoutItem)sender;

        // CommandParameter in XAML is bound to Filename (string). Try to resolve Note by filename first.
        Note? menunote = null;
        string? filename = menuItem.CommandParameter as string;

        if (!string.IsNullOrEmpty(filename))
        {
            menunote = notesModel.Notes.FirstOrDefault(n => n.Filename == filename);
        }

        // Fallback: try DataContext or parent menu target's DataContext
        menunote ??= menuItem.DataContext as Note;

        if (menunote == null)
        {
            var parentMenu = menuItem.Parent as MenuFlyout;
            var target = parentMenu?.Target as FrameworkElement;
            menunote = target?.DataContext as Note;
        }

        if (menunote == null)
        {
            // Nothing to delete
            return;
        }

        ContentDialog dialog = new();

        // ”казываем XamlRoot из основного окна и тему (аналогично EditNote)
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

        dialog.Style = Microsoft.UI.Xaml.Application.Current.Resources["DefaultContentDialogStyle"] as Style;
        dialog.Title = MyNotes.Services.Localizer.Instance.DeleteDialog_Title;
        dialog.PrimaryButtonText = MyNotes.Services.Localizer.Instance.DeleteDialog_PrimaryButton;
        dialog.CloseButtonText = MyNotes.Services.Localizer.Instance.DeleteDialog_CloseButton;
        dialog.DefaultButton = ContentDialogButton.Primary;
        dialog.Content = new DialogDelete();

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            // Note.DeleteAsync removes the underlying file by Filename property
            await menunote.DeleteAsync();
            // Remove from the observable collection so the UI updates
            notesModel.Notes.Remove(menunote);
        }
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(SettingsPage));
    }

}
