using Android.App;
using Android.Widget;
using Android.OS;
using Microsoft.WindowsAzure.MobileServices;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
namespace AppCloudCommunication2
{
    [Activity(Label = "AppCloudCommunication2", MainLauncher = true)]
    public class MainActivity : Activity
    {
        Button btn;
        EditText et;
        MobileServiceClient MobileService;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            System.Diagnostics.Debug.WriteLine("================================", "iPrint");
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Main);

            // Initialization for Azure Mobile Apps
            CurrentPlatform.Init();

            MobileService = new MobileServiceClient("https://xamarintutorial.azurewebsites.net");
            

            // This MobileServiceClient has been configured to communicate with the Azure Mobile App and
            // Azure Gateway using the application url. You're all set to start working with your Mobile App!

            btn = FindViewById<Button>(Resource.Id.button1);
            et = FindViewById<EditText>(Resource.Id.editText1);
           
            btn.Click += Btn_Click;

            
            et.Text = "APP is Ready";
        }

        private async void Btn_Click(object sender, System.EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("================================", "iPrint");
            et.Text = "";
            IMobileServiceTable<TodoItem> todoTable = MobileService.GetTable<TodoItem>();
            List<TodoItem> items = await todoTable.Where(todoItem => todoItem.Complete == false).ToListAsync();

            if (items.Count == 0)
                et.Text = "No items.";
            else
                foreach(TodoItem item in items)
                {
                    et.Text += item.Text + ", ";
                }   
        }
    }
    class TodoItem
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        [JsonProperty(PropertyName = "complete")]
        public bool Complete { get; set; }
    }
}

