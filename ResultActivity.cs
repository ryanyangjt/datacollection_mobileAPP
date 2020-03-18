using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Hardware;
using Android.Media;
using System.IO;

namespace Lie_Detector_Android_Version
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = false)]
    public class ResultActivity : Activity
    {
        private int feedback_tips = 0;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.feedback);
            
            //彈出視窗解說:
            AlertDialog.Builder alert_feedback = new AlertDialog.Builder(this);  // For initial tips dialog
            if (feedback_tips == 0)
            {
                alert_feedback.SetTitle("注意事項");
                alert_feedback.SetMessage("1. 點選**顯示結果**查看測謊結果.\n2. 若想再測謊一次，請按**再次測謊**.\n3. 懇請給予我們您寶貴的體驗建議，請按**填寫回饋單**給予我們進步的空間，感謝體驗!!!");
                alert_feedback.SetIcon(Android.Resource.Drawable.IcDialogInfo);
                alert_feedback.SetPositiveButton(" ok", new EventHandler<DialogClickEventArgs>((sender, e) => { }));
                alert_feedback.Show();
                feedback_tips++;
            }
            
            // 跑馬燈
            TextView result_tv = FindViewById<TextView>(Resource.Id.result_textview);
            result_tv.Selected = true;

            FindViewById<Button>(Resource.Id.btn_result).Click += Show_Result;
            FindViewById<Button>(Resource.Id.btn_feedback).Click += Go_FeedBack;
            FindViewById<Button>(Resource.Id.btn_onemore).Click += Play_Again;
            FindViewById<Button>(Resource.Id.btn_exit).Click += Exit_App;
        }

        private void Exit_App(object sender, EventArgs e)
        {
            this.FinishAffinity();
        }

        private void Play_Again(object sender, EventArgs e)
        {
            this.Finish();
            this.StartActivity(typeof(MainActivity));
        }

        private void Go_FeedBack(object sender, EventArgs e)
        {
            this.Finish();
            this.StartActivity(typeof(GoogleFormsActivity));
        }

        private void Show_Result(object sender, EventArgs e)
        {
            
        }
    }
}