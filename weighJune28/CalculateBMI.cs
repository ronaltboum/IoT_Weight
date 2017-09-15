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
using Java.Util;
using Java.Net;

namespace weighJune28
{
    [Activity(Label = "Calculate your BMI")]
    public class CalculateBMI : Activity
    {
        private IMobileServiceTable<UsersTable> UsersTableRef;
        private IMobileServiceTable<weighTable> weighTableRef;
        float enteredHeight = 0;
        string ourUserId = ToDoActivity.CurrentActivity.Currentuserid;

        //TODO:  when Raspberry is incorporated:  check case where user first tries to 
        //calculate BMI,  but he has no weighs in the database,  then he goes back and 
        //weighs himself,  and then tries to calculate BMI again.

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your application here
            SetContentView(Resource.Layout.BMIview);
            //string ourUserId = ToDoActivity.CurrentActivity.Currentuserid;
            MobileServiceClient client = ToDoActivity.CurrentActivity.CurrentClient;
            weighTableRef = client.GetTable<weighTable>();
            UsersTableRef = client.GetTable<UsersTable>();

            var enterHeight = FindViewById<EditText>(Resource.Id.enterHeight);
            var heightText = FindViewById<TextView>(Resource.Id.heightText);
            heightText.Visibility = ViewStates.Gone;
            enterHeight.Visibility = ViewStates.Gone;
            enterHeight.TextChanged += (object sender, Android.Text.TextChangedEventArgs e) => {

                //heightText.Text = e.Text.ToString();
                string entered_height = e.Text.ToString();
                //TODO:  make sure conversion worked:   https://msdn.microsoft.com/en-us/library/0xh24xh4.aspx

                enteredHeight = float.Parse(entered_height);
                if (enteredHeight <= 0)
                {
                    //TODO
                }
                

            };
            var OKButton = FindViewById<Button>(Resource.Id.OKButton);
            OKButton.Visibility = ViewStates.Gone;
            //TODO:  try to use DONE event on keyboard instead of this button
            OKButton.Click += async(sender, e) =>
            {
                if(enteredHeight > 0)
                {
                    //enter user's info to UsersTable and then calculate BMI
                    var userUpdated = new UsersTable
                    {
                        UniqueUsername = ourUserId,
                        height = enteredHeight,
                    };
                    await UsersTableRef.InsertAsync(userUpdated);
                    //TODO:   call calculateBMI function
                    OKButton.Visibility = ViewStates.Gone;
                    heightText.Visibility = ViewStates.Gone;
                    enterHeight.Visibility = ViewStates.Gone;
                    CalculateUserBMI();
                }
            };

            try
            {
                //ourUserId = "debug test1";
                var userRecord = await UsersTableRef.Where(item => (item.UniqueUsername == ourUserId)).ToListAsync();
                //some inserts for debug:
                //var userUpdated = new UsersTable
                //{
                //    UniqueUsername = ourUserId,
                //    height = 1.75f,
                //};
                //await UsersTableRef.InsertAsync(userUpdated);

                if (userRecord.Count == 0)
                {
                    heightText.Visibility = ViewStates.Visible;
                    enterHeight.Visibility = ViewStates.Visible;
                    OKButton.Visibility = ViewStates.Visible;

                    //TODO:   ask user to enter height.  
                    //CreateAndShowDialog("Cannot calculate BMI", "Please enter your height");
                }

                //user already entered height:
                else
                {  
                    var user = userRecord[0];
                    float height = user.height;
                    //check if there are weighs of this user in the weighTable:
                    var weighRecords = await weighTableRef.Where(item => (item.username == ourUserId)).ToListAsync();
                    if (weighRecords.Count == 0)
                    {
                        CreateAndShowDialog("Cannot calculate BMI", "No previous weighs were found. Please weigh yourself and try again");
                        //TODO:   return to main screen after user presses back once
                    }
                    else
                    {
                        //calculate BMI from user's most recent weighing:
                        var mostRecendWeigh = weighRecords[weighRecords.Count - 1];
                        float weight = mostRecendWeigh.weigh;
                        //BMI formula from wikipedia:   BMI = weight in kg / (height in meters)^2
                        //https://en.wikipedia.org/wiki/Body_mass_index
                        //TODO:  make sure user didn't enter height of zero
                        double heigtSquared = Math.Pow(height, 2);
                        double BMI = weight / heigtSquared;
                        string BMIInStringFormat = Convert.ToString(BMI);
                        string debugMessage = "Your most recent weight = " + weight + " , and your BMI = " + BMIInStringFormat;
                        CreateAndShowDialog(debugMessage,  "Debugg message " );
                        //TODO:  alert user if his BMI is out of normal range.  see wiki.  maybe give
                        //wiki link for further information.

                    }
                }

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


        private async void CalculateUserBMI()
        {
            var weighRecords = await weighTableRef.Where(item => (item.username == ourUserId)).ToListAsync();
            if (weighRecords.Count == 0)
            {
                CreateAndShowDialog("Cannot calculate BMI", "No previous weighs were found. Please weigh yourself and try again");
                //TODO:   return to main screen after user presses back once
            }
            else
            {
                //calculate BMI from user's most recent weighing:
                var mostRecendWeigh = weighRecords[weighRecords.Count - 1];
                float weight = mostRecendWeigh.weigh;
                //BMI formula from wikipedia:   BMI = weight in kg / (height in meters)^2
                //https://en.wikipedia.org/wiki/Body_mass_index
                //TODO:  make sure user didn't enter height of zero
                double heigtSquared = Math.Pow(enteredHeight, 2);
                double BMI = weight / heigtSquared;
                string BMIInStringFormat = Convert.ToString(BMI);
                string debugMessage = "Your most recent weight = " + weight + " , and your BMI = " + BMIInStringFormat;
                CreateAndShowDialog(debugMessage, "Debugg message ");
                //TODO:  alert user if his BMI is out of normal range.  see wiki.  maybe give
                //wiki link for further information.

            }
        }
    }
}