using Android.App;
using Android.Widget;
using Android.OS;
using BarChart;
using System;

namespace DataVisualization
{
    [Activity(Label = "DataVisualization", MainLauncher = true)]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);


            var data = new[] { 1f, 2f, 4f, 8f, 16f, 32f };
            var chart = new BarChartView(this)
            {
                ItemsSource = Array.ConvertAll(data, v => new BarModel { Value = v })
            };

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
        }
    }
}

