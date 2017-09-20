using Android.App;
using Android.Widget;
using Android.OS;

using Microsoft.WindowsAzure.MobileServices.Eventing;
using System.Text;
using System.Threading.Tasks;
namespace BusApp
{
    [Activity(Label = "BusApp", MainLauncher = true)]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Initialization for Azure Mobile Apps
            Microsoft.WindowsAzure.MobileServices.CurrentPlatform.Init();
            // This MobileServiceClient has been configured to communicate with the Azure Mobile App and
            // Azure Gateway using the application url. You're all set to start working with your Mobile App!
            Microsoft.WindowsAzure.MobileServices.MobileServiceClient TAUtrycommuncateClient = new Microsoft.WindowsAzure.MobileServices.MobileServiceClient(
            "https://tautrycommuncate.azurewebsites.net");

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

        }
    }
}

