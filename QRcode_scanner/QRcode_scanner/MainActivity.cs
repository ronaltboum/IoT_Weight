using Android.App;
using Android.Widget;
using Android.OS;
using Intersoft.Crosslight.Services.Barcode;
using Intersoft.Crosslight;

namespace QRcode_scanner
{
    [Activity(Label = "QRcode_scanner", MainLauncher = true)]
    public class MainActivity : Activity
    {
        Button btn;
        TextView tv;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);


            btn = FindViewById<Button>(Resource.Id.btn_scan);
            tv = FindViewById<TextView>(Resource.Id.txt_data);

            btn.Click += Btn_Click;

           
        }

        private async void Btn_Click(object sender, System.EventArgs e)
        {
            IBarcodeReaderService service = ServiceProvider.GetService<IBarcodeReaderService>();
            service.SetOwner(this);

            string Result = await service.Scan();
            tv.Text = Result;
        }
    }
}

