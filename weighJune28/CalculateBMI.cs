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

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your application here
            SetContentView(Resource.Layout.BMIview);
            string ourUserId = ToDoActivity.CurrentActivity.Currentuserid;
            MobileServiceClient client = ToDoActivity.CurrentActivity.CurrentClient;
            weighTableRef = client.GetTable<weighTable>();
            UsersTableRef = client.GetTable<UsersTable>();
            try
            {
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
                    

                    //TODO:   ask user to enter height.  
                    CreateAndShowDialog("Cannot calculate BMI", "Please enter your height");
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
                        //TODO:   calculate BMI from user's most recent weighing:
                        //var item = integerList[integerList.Count - 1];
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


                    //if (height == 0)
                    //{

                    //    //update record with user's input:
                    //    user.height = 1.75f;

                    //    //var userUpdated = new UsersTable
                    //    //{
                    //    //    UniqueUsername = ourUserId,
                    //    //    height = 1.75f,
                    //    //    Id = user.Id
                    //    //};
                    //    await UsersTableRef.UpdateAsync(user);

                    //}

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
    }
}