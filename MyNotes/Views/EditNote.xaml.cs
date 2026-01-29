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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyNotes.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
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
            if (noteModel is not null)
            {
                await noteModel.DeleteAsync();
            }

            if (Frame.CanGoBack == true)
            {
                Frame.GoBack();
            }
        }

        private async void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (NoteEditor.Text != NoteText)
            {
                ContentDialog dialog = new ContentDialog();

                // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
                dialog.XamlRoot = this.XamlRoot;
                dialog.Style = Microsoft.UI.Xaml.Application.Current.Resources["DefaultContentDialogStyle"] as Style;
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
    }
}
