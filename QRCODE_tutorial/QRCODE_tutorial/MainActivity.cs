using Android.App;
using Android.Widget;
using Android.OS;
using Intersoft.Crosslight;
using Intersoft.Crosslight.Android;
using Intersoft.Crosslight.Services.Barcode;
using Intersoft.Crosslight.Services.Barcode.Android;

namespace QRCODE_tutorial
{
    [Activity(Label = "QRCODE_tutorial", MainLauncher = true)]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);


            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

             btn = FindViewById<Button>(Resource.Id.btn_scan);
             tv = FindViewById<TextView>(Resource.Id.txt_data);

            btn.Click += Btn_Click;
        }
        Button btn;
        TextView tv;
        private void Btn_Click(object sender, System.EventArgs e)
        {
            tv.Text = "boo!";

           // IBarcodeReaderService service = ServiceProvider.GetService<IBarcodeReaderService>();
           // service.SetOwner(this);

           // this.Result = await service.Scan();
        }
    }
}

