using Android.App;
using Android.Widget;
using Android.OS;
using BarChart;
using System;
using Android.Views;

namespace DataVisualization2
{
    [Activity(Label = "DataVisualization2", MainLauncher = true)]
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

            chart.BarClick += (sender, args) => {
                BarModel barm = args.Bar;
                Console.WriteLine("Pressed {0}", barm);
            };

            chart.BarColor = Android.Graphics.Color.Yellow;
            chart.BarCaptionInnerColor = Android.Graphics.Color.White;
            chart.BarCaptionOuterColor = Android.Graphics.Color.Black;
            chart.MinimumValue = null;
            chart.MaximumValue = null;

            var bar = new BarModel
            {
                Value = 100500,
                Color = Android.Graphics.Color.Blue,
                Legend = "Unit Sales",
                ValueCaptionHidden = false,
                ValueCaption = "100k"
            };

            SetContentView(Resource.Layout.Main);
#pragma warning disable CS0618 // Type or member is obsolete
            AddContentView(chart, new ViewGroup.LayoutParams(width: ViewGroup.LayoutParams.FillParent, height: ViewGroup.LayoutParams.FillParent));
#pragma warning restore CS0618 // Type or member is obsolete
        }
    }
}

