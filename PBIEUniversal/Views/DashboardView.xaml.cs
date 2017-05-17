using PBIEMobileSDK.ViewModels;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace PBIEUniversal.Views
{
    public sealed partial class DashboardView : Page
    {
        public DashboardView()
        {
            this.InitializeComponent();
            this.DataContext = new DashboardViewModel();
        }
    }
}
