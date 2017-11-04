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

using Microsoft.WindowsAzure.MobileServices;

namespace IoTWeight
{
    [Activity(Label = "InsertWeightsForDebugg")]
    public class InsertWeightsForDebugg : Activity
    {

        List<float> lastWeights = new List<float>();
        List<WeighDatePair> weighDateList = new List<WeighDatePair>();
        private IMobileServiceTable<weighTable> weighTableRef;
        private IMobileServiceTable<UsersTable> UsersTableRef;
        private IMobileServiceTable<RaspberryTable> raspberryTableRef;
        string ourUserId = ToDoActivity.CurrentActivity.Currentuserid;


        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.InsertWeights);

            MobileServiceClient client = ToDoActivity.CurrentActivity.CurrentClient;
            weighTableRef = client.GetTable<weighTable>();
            UsersTableRef = client.GetTable<UsersTable>();
            raspberryTableRef = client.GetTable<RaspberryTable>();


            try
            {
                string QR_Code = "testing3";
                string ipAdd = "10.0.0.6";
                var record1 = new RaspberryTable
                {
                    QRCode = QR_Code,
                    IPAddress = ipAdd,
                };
                //await raspberryTableRef.InsertAsync(record1);



                int i;
                for (i = 88; i <= 99; i++)
                {
                    //if(i > 60)
                    //{
                    //    Console.WriteLine("Don't insert too much guys. There's a space limit");
                    //    break;
                    //}
                        
                    var newweightablerecord = new weighTable
                    {
                        username = ourUserId,
                        weigh = i
                    };
                    await weighTableRef.InsertAsync(newweightablerecord);
                }

                CreateAndShowDialog("", "Inserted successfully");
            }
            catch (Exception e)
            {
                CreateAndShowDialog(e, "Error");
            }

        }

        private void CreateAndShowDialog(Exception exception, String title)
        {
            CreateAndShowDialog(exception.Message, title);
        }

        private void CreateAndShowDialog(string message, string title)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(this);

            builder.SetMessage(message);
            builder.SetTitle(title);
            builder.Create().Show();
        }
    }
}