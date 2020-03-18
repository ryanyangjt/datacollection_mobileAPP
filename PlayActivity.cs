using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Hardware;
using Android.Media;
using Android.Support.V7.App;
using Java.IO;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using System.Threading.Tasks;
using Java.Net;
using Java.Util;

namespace Lie_Detector_Android_Version
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = false)]
    public class PlayActivity : Activity
    {
        private int counter = 0;
        // static 屬性可以讓其他Activity 呼叫此變數
        private static ProgressDialog progressDialog;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.play);

            //彈出視窗解說:
            AlertDialog.Builder alert_play = new AlertDialog.Builder(this);  // For initial tips dialog
            if (counter == 0)
            {
                alert_play.SetTitle("注意事項");
                alert_play.SetMessage("1. 請確認影片畫質是否正常且人臉完全包覆於合理範圍中，若不符合請\u201C返回操作\u201C重新測謊.\n"+
                    "2. 確認無誤後，即可\u201C上傳檔案\u201C進行測謊分析.\n"+
                    "3. 點選\u201C查看結果\u201C觀看測謊結果!!!");
                alert_play.SetIcon(Android.Resource.Drawable.IcDialogInfo);
                alert_play.SetPositiveButton(" ok", new EventHandler<DialogClickEventArgs>((sender, e) => { }));
                alert_play.Show();
                counter++;
            }

            // 跑馬燈
            TextView play_tv = FindViewById<TextView>(Resource.Id.playtextview);
            play_tv.Selected = true;

            FindViewById<Button>(Resource.Id.btn_back).Click += Back_Click;
            //FindViewById<Button>(Resource.Id.btn_upload).Click += Back_Upload;
            FindViewById<Button>(Resource.Id.btn_play).Click += Back_Play;
            FindViewById<Button>(Resource.Id.btn_result).Click += Back_Result;

        }

        // Back to MainActivity:
        private void Back_Click(object sender, EventArgs e)
        {
            this.Finish();
            this.StartActivity(typeof(MainActivity));
        }

        // Uploading video to WCIS-Server:
        /*
        private void Back_Upload(object sender, EventArgs e)
        {
            // Show uploading progress:
            progressDialog = new ProgressDialog(this);
            progressDialog.SetMessage("上傳中，請稍等...");
            progressDialog.Show();

            // Call uploading function
            new UploadFileAsync().Execute("");
        }
        */

        // Implement the upload class:
        /*
        private class UploadFileAsync : AsyncTask<String, int, String>
        {
            private HttpStatus serverResponseCode;
            protected override string RunInBackground(params string[] @params)
            {
                try
                {
                    String sourceFileUri = MainActivity.rec_video_uri;

                    Java.Net.HttpURLConnection conn = null;
                    DataOutputStream dos = null;
                    String lineEnd = "\r\n";
                    String twoHyphens = "--";
                    String boundary = "*****";
                    int bytesRead, bytesAvailable, bufferSize;
                    byte[] buffer;
                    int maxBufferSize = 1 * 1024 * 1024;
                    File sourceFile = new File(sourceFileUri);

                    if (sourceFile.IsFile)
                    {
                        try
                        {
                            String upLoadServerUri = "http://140.114.28.134/VideoUpload/uploads/upload.php";

                            // Ppen a URL connection to the Server
                            FileInputStream fileInputStream = new FileInputStream(
                                    sourceFile);
                            Java.Net.URL url = new Java.Net.URL(upLoadServerUri);

                            // Open a HTTP connection to the URL
                            conn = (Java.Net.HttpURLConnection)url.OpenConnection();
                            conn.DoInput = true; // Allow Inputs
                            conn.DoOutput = true; // Allow Outputs
                            conn.UseCaches = false; // Don't use a Cached Copy
                            conn.RequestMethod = "POST";
                            conn.SetRequestProperty("Connection", "Keep-Alive");
                            conn.SetRequestProperty("ENCTYPE", "multipart/form-data");
                            conn.SetRequestProperty("Content-Type",
                                    "multipart/form-data;boundary=" + boundary);
                            conn.SetRequestProperty("upload_name", sourceFileUri);

                            dos = new DataOutputStream(conn.OutputStream);

                            dos.WriteBytes(twoHyphens + boundary + lineEnd);
                            dos.WriteBytes("Content-Disposition: form-data; name=\"upload_name\";filename=\""
                                    + sourceFileUri + "\"" + lineEnd);

                            dos.WriteBytes(lineEnd);

                            // Create a buffer of maximum size
                            bytesAvailable = fileInputStream.Available();

                            bufferSize = Math.Min(bytesAvailable, maxBufferSize);
                            buffer = new byte[bufferSize];

                            // Read file and write
                            bytesRead = fileInputStream.Read(buffer, 0, bufferSize);

                            while (bytesRead > 0)
                            {

                                dos.Write(buffer, 0, bufferSize);
                                bytesAvailable = fileInputStream.Available();
                                bufferSize = Math
                                        .Min(bytesAvailable, maxBufferSize);
                                bytesRead = fileInputStream.Read(buffer, 0,
                                        bufferSize);

                            }

                            // Send multipart form data necesssary after file
                            dos.WriteBytes(lineEnd);
                            dos.WriteBytes(twoHyphens + boundary + twoHyphens
                                    + lineEnd);

                            // Responses from the server (code and message)
                            serverResponseCode = conn.ResponseCode;
                            String serverResponseMessage = conn
                                    .ResponseMessage;

                            // Close the streams 
                            fileInputStream.Close();
                            dos.Flush();
                            dos.Close();
                        }
                        catch (Exception e)
                        {
                            System.Console.WriteLine(e.ToString());
                        }
                    } // End if-else block
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine(ex.ToString());
                }
                progressDialog.Dismiss();

                return "Finished";
            }
        }
        */


        // Playing record video:
        private void Back_Play(object sender, EventArgs e)
        {
            var videoView = FindViewById<VideoView>(Resource.Id.videoView);
            //videoView.SetVideoPath(MainActivity.rec_video_uri);
            videoView.Start();
            videoView.Completion += videoView_Completion;
        }

        //Go to resultActivity:
        private void Back_Result(object sender, EventArgs e)
        {
            this.Finish();
            this.StartActivity(typeof(ResultActivity));
        }

        // For showing the informations:
        void videoView_Completion(object sender, EventArgs e)
        {
            Toast.MakeText(this, "播放完成", ToastLength.Short).Show();
        }
    }
}