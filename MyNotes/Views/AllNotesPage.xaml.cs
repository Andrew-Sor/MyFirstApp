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
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyNotes.Views;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class AllNotesPage : Page
{
    private Note? noteModel;
    public string NoteText { get; private set; } = string.Empty;

    private AllNotes notesModel = new AllNotes();
    public AllNotesPage()
    {
        InitializeComponent();
    }

    private void NewNoteButton_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(EditNote));
    }

    private void ItemsView_ItemInvoked(ItemsView sender, ItemsViewItemInvokedEventArgs args)
    {
        Frame.Navigate(typeof(EditNote), args.InvokedItem);

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
        ContentDialog dialog = new ContentDialog();

        // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
        dialog.XamlRoot = this.XamlRoot;
        dialog.Style = Microsoft.UI.Xaml.Application.Current.Resources["DefaultContentDialogStyle"] as Style;
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

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(SettingsPage));
    }
}
