using Android.App;
using Android.Widget;
using Android.OS;

namespace AndroidAppTutorial
{
    [Activity(Label = "AndroidAppTutorial", MainLauncher = true)]
    public class MainActivity : Activity
    {
        int counter = 0;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Initialization for Azure Mobile Apps
            Microsoft.WindowsAzure.MobileServices.CurrentPlatform.Init();
            // This MobileServiceClient has been configured to communicate with the Azure Mobile App and
            // Azure Gateway using the application url. You're all set to start working with your Mobile App!
            Microsoft.WindowsAzure.MobileServices.MobileServiceClient XamarinTutorialClient = new Microsoft.WindowsAzure.MobileServices.MobileServiceClient(
            "https://xamarintutorial.azurewebsites.net");

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            Button btn = FindViewById<Button>(Resource.Id.btn_clickme);
            var lable = FindViewById<TextView>(Resource.Id.txt_count);
            Button btn_azure = FindViewById<Button>(Resource.Id.btn_could);
            btn.Click += delegate
            {
                counter++;
                lable.Text = "You clicked " + counter + " times.";
            };
            btn_azure.Click += async delegate
            {
                lable.Text = await AzureIoTHub.ReceiveCloudToDeviceMessageAsync();
            };
        }
    }
}

