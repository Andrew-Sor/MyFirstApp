using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyNotes.Views;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class DialogDelete : Page
{
    public DialogDelete()
    {
        InitializeComponent();
        this.DataContext = MyNotes.Services.Localizer.Instance;
    }
}
