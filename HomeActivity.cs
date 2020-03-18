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
using Android.Support.V4.Content;
using Android;
using Android.Support.V4.App;
using Android.Content.PM;
using Android.Support.V7.App;

namespace Lie_Detector_Android_Version
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class HomeActivity : AppCompatActivity
    {
        public int REQUEST_LOCATION { get; set; }
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.home);

            // 防止發生預期錯誤用:
            StrictMode.VmPolicy.Builder builder = new StrictMode.VmPolicy.Builder();
            StrictMode.SetVmPolicy(builder.Build());

            // Check Permission
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.WriteExternalStorage) == (int)Permission.Granted)
            {
                // We have permission, go ahead.
            }
            else
            {
                   ActivityCompat.RequestPermissions(this,
                   new String[] { Manifest.Permission.WriteExternalStorage, Manifest.Permission.Camera, Manifest.Permission.RecordAudio, Manifest.Permission.Internet, Manifest.Permission.ReadExternalStorage },
                   1);
            }

            FindViewById<Button>(Resource.Id.btn_next).Click += Home_Click;
        }

        private void Home_Click(object sender, EventArgs e)
        {
            this.StartActivity(typeof(MainActivity));
        }   

    }
}