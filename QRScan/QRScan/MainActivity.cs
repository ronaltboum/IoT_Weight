using Android.App;
using Android.Widget;
using Android.OS;
using Android.Graphics;
using ZXing;
using Java.IO;

namespace QRScan
{
    [Activity(Label = "QRScan", MainLauncher = true)]
    public class MainActivity : Activity
    {

        ImageView _imageView;
        TextView tv;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);


            Button button = FindViewById<Button>(Resource.Id.btn_takepic);
            _imageView = FindViewById<ImageView>(Resource.Id.img_showpic);
            tv = FindViewById<TextView>(Resource.Id.tv_result);

            Bitmap bitmap;

            //File f = new File("codes/working1.jpg");
            BitmapFactory.Options options = new BitmapFactory.Options();
            options.InPreferredConfig = Bitmap.Config.Argb8888;
            try
            {  
                bitmap = BitmapFactory.DecodeFile("working1.jpg");
                _imageView.SetImageBitmap(bitmap);
            }
            catch (FileNotFoundException e)
            {
                System.Diagnostics.Debug.WriteLine(":-(");
            }

            
        }

        private void scanBarcode(Bitmap bitmap, TextView tv)
        {
            // create a barcode reader instance
            var barcodeReader = new BarcodeReader();

            barcodeReader.AutoRotate = true;
            barcodeReader.Options.TryHarder = true;

            // decode the barcode from the in memory bitmap
            ZXing.Result barcodeResult = barcodeReader.Decode(bitmap);

            if (barcodeResult != null)
                tv.Text = barcodeResult.Text;
            else
                tv.Text = "No QR found";
        }
    }
}

