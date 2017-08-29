using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using Intersoft.Crosslight.Services.Barcode;
using Intersoft.Crosslight;

namespace QRcode_scanner
{
    class BarcodeScanner
    {
        private string result;
        private IBarcodeReaderService bservice;
        public BarcodeScanner()
        {
            bservice = ServiceProvider.GetService<IBarcodeReaderService>();
            bservice.SetOwner(this);
        }

        public string Result { get => result; }
        public IBarcodeReaderService Service { get => bservice;}

        public async void scan()
        {
            this.result = await bservice.Scan();
        }
        
    }
}