using Android.App;
using Android.Widget;
using Android.OS;
using Intersoft.Crosslight.Services.Barcode;
using Intersoft.Crosslight;
using System.Threading.Tasks;

namespace QRcode_scanner
{
    [Activity(Label = "QRcode_scanner", MainLauncher = true)]
    public class MainActivity : Activity
    {
        Button btn;
        TextView tv;
        BarcodeScanner scanner;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);


            btn = FindViewById<Button>(Resource.Id.btn_scan);
            tv = FindViewById<TextView>(Resource.Id.txt_data);

            btn.Click += Btn_Click;

            
        }
        
        public void Btn_Click(object sender, System.EventArgs e)
        {
            scanner = new BarcodeScanner();
            string res;
            scanner.scan();
            res = scanner.Result;
            tv.Text = res;
        }
    }
}

