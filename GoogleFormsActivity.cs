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
using Android.Webkit;
namespace Lie_Detector_Android_Version
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = false)]
    public class GoogleFormsActivity : Activity
    {
        WebView web_google_forms;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.google_forms);

            web_google_forms = FindViewById<WebView>(Resource.Id.web_googleforms);
            web_google_forms.Settings.JavaScriptEnabled = true;
            web_google_forms.SetWebViewClient(new HelloWebViewClient());
            web_google_forms.LoadUrl("https://goo.gl/forms/OJpJwfSqDTMZiIiM2");
        }

        public class HelloWebViewClient : WebViewClient
        {
            public override bool ShouldOverrideUrlLoading(WebView view, IWebResourceRequest request)
            {
                view.LoadUrl(request.Url.ToString());
                return false;
            }
        }
        public override bool OnKeyDown(Android.Views.Keycode keyCode, Android.Views.KeyEvent e)
        {
            if (keyCode == Keycode.Back && web_google_forms.CanGoBack())
            {
                web_google_forms.GoBack();
                return true;
            }
            this.Finish();
            this.StartActivity(typeof(ResultActivity));
            return base.OnKeyDown(keyCode, e);
        }
    }
}