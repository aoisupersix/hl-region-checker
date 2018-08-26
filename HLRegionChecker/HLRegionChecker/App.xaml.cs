using Prism;
using Prism.Ioc;
using HLRegionChecker.ViewModels;
using HLRegionChecker.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Prism.Unity;
using HLRegionChecker.Models;
using Prism.Navigation;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace HLRegionChecker
{
    public partial class App : PrismApplication
    {
        /* 
         * The Xamarin Forms XAML Previewer in Visual Studio uses System.Activator.CreateInstance.
         * This imposes a limitation in which the App class must have a default constructor. 
         * App(IPlatformInitializer initializer = null) cannot be handled by the Activator.
         */
        public App() : this(null) { }

        public App(IPlatformInitializer initializer) : base(initializer) { }

        protected override async void OnInitialized()
        {
            InitializeComponent();

            if (UserDataModel.Instance.MemberId == UserDataModel.DefaultMemberId)
                await NavigationService.NavigateAsync("MainMasterPage/NavigationPage/StatusListPage/IdentifierSelectPage");
            else
                await NavigationService.NavigateAsync("MainMasterPage/NavigationPage/StatusListPage");
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<NavigationPage>();
            containerRegistry.RegisterForNavigation<StatusListPage>();
            containerRegistry.RegisterForNavigation<StatusDetailPage>();
            containerRegistry.RegisterForNavigation<MainMasterPage>();
            containerRegistry.RegisterForNavigation<IdentifierSelectPage>();
            containerRegistry.RegisterForNavigation<AppInfoPage>();
        }
    }
}
