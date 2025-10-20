using Microsoft.UI.Xaml.Controls;

using proyectoRefaccionaria.ViewModels;

namespace proyectoRefaccionaria.Views;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel
    {
        get;
    }

    public MainPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        InitializeComponent();
    }
}
