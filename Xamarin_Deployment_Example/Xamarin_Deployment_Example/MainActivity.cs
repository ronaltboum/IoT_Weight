using Android.App;
using Android.Widget;
using Android.OS;

namespace Xamarin_Deployment_Example
{
    [Activity(Label = "Xamarin_Deployment_Example", MainLauncher = true)]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
        }
    }
}

