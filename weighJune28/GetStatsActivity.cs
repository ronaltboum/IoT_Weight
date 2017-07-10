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

namespace weighJune28
{
    [Activity(Label = "GetStatsActivity")]
    public class GetStatsActivity : ListActivity
    {
        List<float> lastWeights = new List<float>();
        // Client reference.
        private MobileServiceClient client;
        // URL of the mobile app backend.
        const string applicationURL = @"https://weighjune28.azurewebsites.net";
        private IMobileServiceTable<weighTable> weighTableRef;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            CurrentPlatform.Init();
            // Create the client instance, using the mobile app backend URL.
            client = new MobileServiceClient(applicationURL);
            weighTableRef = client.GetTable<weighTable>();

            try
            {
                //var list1 = await weighTableRef.Where(item => item.username == "Reut").ToListAsync();
                DateTime date2 = new DateTime(2017, 6, 28, 16, 5, 0);
                DateTime today = DateTime.Now;
                //DateTime lastWeek = today.AddDays(-7);
                DateTime lastMonth = today.AddDays(-30);

                Console.WriteLine("last month from today is : {0}", lastMonth);
                //var list1 = await weighTableRef.Where(item => (item.username == "Reut") && (item.createdAt > date2) ).ToListAsync();
                var list1 = await weighTableRef.Where(item => (item.username == "Reut") && (item.createdAt > lastMonth)).ToListAsync();
                //adapter.Clear();

                foreach (weighTable weight in list1)
                {
                    string name = weight.username;
                    float currW = weight.weigh;
                    lastWeights.Add(currW);
                    Console.WriteLine("Username = : {0}", name);
                    Console.WriteLine("weight = : {0}", currW);
                    DateTime date1 = weight.createdAt;
                    Console.WriteLine(date1.ToString());
                }

            }
            catch (Exception e)
            {
                CreateAndShowDialog(e, "Error");
            }

            this.ListAdapter = new ArrayAdapter<float>(this, Android.Resource.Layout.SimpleListItem1, lastWeights);
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