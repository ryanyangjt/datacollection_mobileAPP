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
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = false)]
    public class MainActivity : AppCompatActivity, ISurfaceHolderCallback
    {
        // 在 class MainActivity 的全域變數定義
        ISurfaceHolder _Holder;
        public Camera Camera { get; set; }
        public int REQUEST_LOCATION { get; set; }
        MediaRecorder _MediaRecorder;
        SurfaceHolderCallBack _SurfaceHolderCallBack;
        bool isRecing;
        TextView timerViewer;
        public int show_time = 0;   // 計算顯示使用順序視窗出現的次數

        private ISurfaceHolder holder;
        private SurfaceView sv_rec;
        private SurfaceView surfaceView;
        public static String fileUUID = UUID.RandomUUID().ToString();
        public static String rec_video_uri_1 = System.IO.Path.Combine(
                    Android.OS.Environment.GetExternalStoragePublicDirectory(
                    Android.OS.Environment.DirectoryMovies).Path,
                    "Video_1_" + fileUUID + ".mp4");
        public static String rec_video_uri_2 = System.IO.Path.Combine(
                    Android.OS.Environment.GetExternalStoragePublicDirectory(
                    Android.OS.Environment.DirectoryMovies).Path,
                    "Video_2_" + fileUUID + ".mp4");
        public static String rec_video_uri_3 = System.IO.Path.Combine(
                    Android.OS.Environment.GetExternalStoragePublicDirectory(
                    Android.OS.Environment.DirectoryMovies).Path,
                    "Video_3_" + fileUUID + ".mp4");

        // For uploading three video files
        public static string[] rec_video_uri = { rec_video_uri_1, rec_video_uri_2, rec_video_uri_3 };
        // Show ProgressDialog for uploading files
        private static ProgressDialog progressDialog;


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            // 防止發生預期錯誤用:
            StrictMode.VmPolicy.Builder builder = new StrictMode.VmPolicy.Builder();
            StrictMode.SetVmPolicy(builder.Build());

            // 顯示最上方的app名稱
            //Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            //SetSupportActionBar(toolbar);

            FindViewById<Button>(Resource.Id.btn_rec).Click += Rec_Click;
            FindViewById<Button>(Resource.Id.btn_view).Click += View_Click;

            // 跑馬燈
            TextView tv = FindViewById<TextView>(Resource.Id.mytv);
            tv.Selected = true;

            // 設定使用說明的按鈕
            AlertDialog.Builder alert_tips = new AlertDialog.Builder(this);   // For Button_Tips
            AlertDialog.Builder alert_initial = new AlertDialog.Builder(this);  // For initial tips dialog

            if (show_time == 0)
            {
                alert_initial.SetTitle("使用順序");
                alert_initial.SetMessage("1. 測謊規則 -> 開始測謊 -> 查看影像.\n2. 請詳細閱讀\u201C測謊規則\u201C.\n3. 閱讀完畢後，即可\u201C開始測謊\u201C.\n4. \u201C查看影像\u201C可預覽影像，方便確認影像品質!");
                alert_initial.SetIcon(Android.Resource.Drawable.IcDialogInfo);
                alert_initial.SetPositiveButton(" ok", new EventHandler<DialogClickEventArgs>((sender, e) => { }));
                alert_initial.Show();
                show_time++;
            }

            FindViewById<Button>(Resource.Id.btn_tips).Click += (sender, e) =>
            {
                alert_tips.SetTitle("測謊說明");
                alert_tips.SetMessage("1. 確保受測者臉部完全包覆於黃色提示框內.\n"
                    + "2. 請確保問題可讓受測者於6秒內回答完畢，敘述問題完畢後，即可按下**開始測謊**錄製影像，6秒後將自動停止錄影.\n"
                    + "3. 錄製完畢後，按下**查看影像**預覽影像品質.\n"
                    + "4. 請確認影片品質並檢查Wi-Fi功能是否開啟，無誤即可按下**上傳檔案**，等待數秒後即獲得測謊結果.\n"
                    + "5. 最後，由衷的邀請您按下**填寫回饋單**，給予我們寶貴的建議，謝謝體驗!!!");
                alert_tips.SetIcon(Android.Resource.Drawable.IcDialogInfo);

                alert_tips.Show();
            };
            alert_tips.SetPositiveButton(" ok", new EventHandler<DialogClickEventArgs>((sender, e) => { }));

            // 相機與錄製功能設定
            _MediaRecorder = new MediaRecorder();
            Camera = Camera.Open();
            surfaceView = FindViewById<SurfaceView>(Resource.Id.surfaceView);

            _Holder = surfaceView.Holder;

            _SurfaceHolderCallBack = new SurfaceHolderCallBack(this, Camera, _MediaRecorder);
            _Holder.AddCallback(_SurfaceHolderCallBack);
            _Holder.SetType(SurfaceType.PushBuffers);

            // For rectangle on surfaceview
            sv_rec = FindViewById<SurfaceView>(Resource.Id.surface_rec);
            holder = sv_rec.Holder;
            sv_rec.Holder.SetFormat(Android.Graphics.Format.Transparent);
            holder.SetFormat(Android.Graphics.Format.Transparent);
            sv_rec.SetZOrderMediaOverlay(true);
            sv_rec.SetZOrderOnTop(true);
            holder.AddCallback(this);


        } // End of OnCreate() 

        //--------------------------------------------------------------
        // 繪製提示方框的 SurfaceHolderCallBack 3個function實作:
        public void SurfaceChanged(ISurfaceHolder holder, [GeneratedEnum] Android.Graphics.Format format, int width, int height) { }
        public void SurfaceCreated(ISurfaceHolder holder)
        {
            this.holder = holder;
            //畫筆定義
            Android.Graphics.Paint mpaint = new Android.Graphics.Paint();
            mpaint.Color = Android.Graphics.Color.Yellow;
            mpaint.SetStyle(Android.Graphics.Paint.Style.Stroke);
            mpaint.StrokeWidth = 4f;
            //畫布
            Android.Graphics.Canvas canvas = holder.LockCanvas();
            //清除畫布用
            canvas.DrawColor(Android.Graphics.Color.Transparent, Android.Graphics.PorterDuff.Mode.Clear);
            // 畫布長寬
            var w = sv_rec.Width;
            var h = sv_rec.Height;
            //控制正方形位置
            Android.Graphics.Rect r = new Android.Graphics.Rect((int)w / 16, (int)h / 8, (int)w / 16 + 7 * w / 8, (int)h / 8 + h / 2);

            //canvas.DrawRect(r, mpaint);   //正方形

            //左下框
            canvas.DrawRect(r.Left - 2, r.Bottom, r.Left + 50, r.Bottom + 2, mpaint);
            canvas.DrawRect(r.Left - 2, r.Bottom - 50, r.Left, r.Bottom, mpaint);
            //左上框
            canvas.DrawRect(r.Left - 2, r.Top - 2, r.Left + 50, r.Top, mpaint);
            canvas.DrawRect(r.Left - 2, r.Top, r.Left, r.Top + 50, mpaint);
            //右上框
            canvas.DrawRect(r.Right - 50, r.Top - 2, r.Right + 2, r.Top, mpaint);
            canvas.DrawRect(r.Right, r.Top, r.Right + 2, r.Top + 50, mpaint);
            //右下框
            canvas.DrawRect(r.Right - 50, r.Bottom, r.Right + 2, r.Bottom + 2, mpaint);
            canvas.DrawRect(r.Right, r.Bottom - 50, r.Right + 2, r.Bottom, mpaint);

            holder.UnlockCanvasAndPost(canvas);
        }

        public void SurfaceDestroyed(ISurfaceHolder holder) { }
        // 繪製提示方框的 SurfaceHolderCallBack 3個function實作結束
        //--------------------------------------------------------------

        // 錄製影像函式定義
        /*
        void Rec_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            timerViewer = FindViewById<TextView>(Resource.Id.counter);
            if (!isRecing)
            {
                btn.Text = "測謊中";
                btn.Clickable = false;
                Camera.StopPreview();
                Camera.Unlock();
                _MediaRecorder.SetCamera(Camera);
                _MediaRecorder.SetMaxDuration(6000);
                _MediaRecorder.SetVideoSource(VideoSource.Default);
                _MediaRecorder.SetAudioSource(AudioSource.Mic);
                _MediaRecorder.SetProfile(
                    CamcorderProfile.Get(CamcorderQuality.High));
                
                _MediaRecorder.SetOutputFile(rec_video_uri);
                                
                _MediaRecorder.SetPreviewDisplay(_Holder.Surface);
                _MediaRecorder.SetOrientationHint
                    (_SurfaceHolderCallBack.RotataDegrees);
                _MediaRecorder.Prepare();
                _MediaRecorder.Start();
                RunUpdateLoop(btn, e);

            }
            else
            {
                btn.Clickable = true;
                btn.Text = "開始測謊";
                _MediaRecorder.Stop();
                Camera.Reconnect();
                Camera.StartPreview();
                timerViewer.Text = "Recording Time: 0.0 s";
            }
            isRecing = !isRecing;
        }
        */

        // For Testing three video recording:
        void Rec_Click(object sender, EventArgs e)
        {

            Button btn = sender as Button;
            timerViewer = FindViewById<TextView>(Resource.Id.counter);
            AlertDialog.Builder alert_rec = new AlertDialog.Builder(this);  
            alert_rec.SetTitle("Recording Video_1");
            alert_rec.SetMessage("第一題請要求受測者誠實回答問題，按下OK開始錄製影像.");
            alert_rec.SetIcon(Android.Resource.Drawable.IcDialogInfo);
            alert_rec.SetNegativeButton(" ok", new EventHandler<DialogClickEventArgs>((sender2, e2) => {
                if (!isRecing)
                {
                    btn.Text = "測謊中";
                    btn.Clickable = false;
                    Camera.StopPreview();
                    Camera.Unlock();
                    _MediaRecorder.SetCamera(Camera);
                    _MediaRecorder.SetMaxDuration(6000);
                    _MediaRecorder.SetVideoSource(VideoSource.Default);
                    _MediaRecorder.SetAudioSource(AudioSource.Mic);
                    _MediaRecorder.SetProfile(
                        CamcorderProfile.Get(CamcorderQuality.High));

                    _MediaRecorder.SetOutputFile(rec_video_uri_1);

                    _MediaRecorder.SetPreviewDisplay(_Holder.Surface);
                    _MediaRecorder.SetOrientationHint
                        (_SurfaceHolderCallBack.RotataDegrees);
                    _MediaRecorder.Prepare();
                    _MediaRecorder.Start();
                    RunUpdateLoop_first(btn, e);
                }
                else
                {
                    btn.Clickable = true;
                    btn.Text = "開始測謊";
                    _MediaRecorder.Stop();
                    Camera.Reconnect();
                    Camera.StartPreview();
                    timerViewer.Text = "Recording Time: 0.0 s";
                }
                isRecing = !isRecing;
            }));
            alert_rec.Show();

        }


        // 攝影計時器
        // First
        private async void RunUpdateLoop_first(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            Double count = 0.0;
            while (count <= 6.1)
            {
                if (count <= 6)
                {
                    await Task.Delay(100);
                    count = count + 0.1;
                    timerViewer.Text = string.Format("Recording Time: {0,2:0.0} s", count);
                }
                else
                {
                    btn.Clickable = true;
                    btn.Text = "開始測謊";
                    _MediaRecorder.Stop();
                    Camera.Reconnect();
                    Camera.StartPreview();
                    timerViewer.Text = "Recording Time: 0.0 s";
                    isRecing = !isRecing;
                    AlertDialog.Builder alert_rec = new AlertDialog.Builder(this);  

                    alert_rec.SetTitle("Recording Video_2");
                    alert_rec.SetMessage("第2題請要求受測者說謊回答問題，按下OK開始錄製影像.");
                    alert_rec.SetIcon(Android.Resource.Drawable.IcDialogInfo);
                    alert_rec.SetNegativeButton(" ok", new EventHandler<DialogClickEventArgs>((sender2, e2) => {
                        if (!isRecing)
                        {
                            btn.Text = "測謊中";
                            btn.Clickable = false;
                            Camera.StopPreview();
                            Camera.Unlock();
                            _MediaRecorder.SetCamera(Camera);
                            _MediaRecorder.SetMaxDuration(6000);
                            _MediaRecorder.SetVideoSource(VideoSource.Default);
                            _MediaRecorder.SetAudioSource(AudioSource.Mic);
                            _MediaRecorder.SetProfile(
                                CamcorderProfile.Get(CamcorderQuality.High));

                            _MediaRecorder.SetOutputFile(rec_video_uri_2);

                            _MediaRecorder.SetPreviewDisplay(_Holder.Surface);
                            _MediaRecorder.SetOrientationHint
                                (_SurfaceHolderCallBack.RotataDegrees);
                            _MediaRecorder.Prepare();
                            _MediaRecorder.Start();
                            RunUpdateLoop_second(btn, e);
                        }
                        else
                        {
                            btn.Clickable = true;
                            btn.Text = "開始測謊";
                            _MediaRecorder.Stop();
                            Camera.Reconnect();
                            Camera.StartPreview();
                            timerViewer.Text = "Recording Time: 0.0 s";
                        }
                        isRecing = !isRecing;
                    }));
                    alert_rec.Show();
                    return;
                }
            }
        }

        // Second
        private async void RunUpdateLoop_second(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            Double count = 0.0;
            while (count <= 6.1)
            {
                if (count <= 6)
                {
                    await Task.Delay(100);
                    count = count + 0.1;
                    timerViewer.Text = string.Format("Recording Time: {0,2:0.0} s", count);
                }
                else
                {
                    btn.Clickable = true;
                    btn.Text = "開始測謊";
                    _MediaRecorder.Stop();
                    Camera.Reconnect();
                    Camera.StartPreview();
                    timerViewer.Text = "Recording Time: 0.0 s";
                    isRecing = !isRecing;
                    AlertDialog.Builder alert_rec = new AlertDialog.Builder(this);  
                    alert_rec.SetTitle("Recording Video_3");
                    alert_rec.SetMessage("第3題請詢問想測謊的問題，按下OK開始錄製影像.");
                    alert_rec.SetIcon(Android.Resource.Drawable.IcDialogInfo);
                    alert_rec.SetNegativeButton(" ok", new EventHandler<DialogClickEventArgs>((sender2, e2) => {
                        if (!isRecing)
                        {
                            btn.Text = "測謊中";
                            btn.Clickable = false;
                            Camera.StopPreview();
                            Camera.Unlock();
                            _MediaRecorder.SetCamera(Camera);
                            _MediaRecorder.SetMaxDuration(6000);
                            _MediaRecorder.SetVideoSource(VideoSource.Default);
                            _MediaRecorder.SetAudioSource(AudioSource.Mic);
                            _MediaRecorder.SetProfile(
                                CamcorderProfile.Get(CamcorderQuality.High));

                            _MediaRecorder.SetOutputFile(rec_video_uri_3);

                            _MediaRecorder.SetPreviewDisplay(_Holder.Surface);
                            _MediaRecorder.SetOrientationHint
                                (_SurfaceHolderCallBack.RotataDegrees);
                            _MediaRecorder.Prepare();
                            _MediaRecorder.Start();
                            RunUpdateLoop_End(btn, e);
                        }
                        else
                        {
                            btn.Clickable = true;
                            btn.Text = "開始測謊";
                            _MediaRecorder.Stop();
                            Camera.Reconnect();
                            Camera.StartPreview();
                            timerViewer.Text = "Recording Time: 0.0 s";
                        }
                        isRecing = !isRecing;
                    }));
                    alert_rec.Show();
                    return;
                }
            }
        }

        // End
        private async void RunUpdateLoop_End(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            Double count = 0.0;
            while (count <= 6.1)
            {
                if (count <= 6)
                {
                    await Task.Delay(100);
                    count = count + 0.1;
                    timerViewer.Text = string.Format("Recording Time: {0,2:0.0} s", count);
                }
                else
                {
                    btn.Clickable = true;
                    btn.Text = "開始測謊";
                    _MediaRecorder.Stop();
                    Camera.Reconnect();
                    Camera.StartPreview();
                    timerViewer.Text = "Recording Time: 0.0 s";
                    isRecing = !isRecing;
                    AlertDialog.Builder alert_upload = new AlertDialog.Builder(this);
                    alert_upload.SetTitle("Uploading Files");
                    alert_upload.SetMessage("按下OK上傳三個測謊檔並分析!!");
                    alert_upload.SetIcon(Android.Resource.Drawable.IcDialogInfo);
                    alert_upload.SetNegativeButton(" ok", new EventHandler<DialogClickEventArgs>((sender2, e2) => {
                        // Show uploading progress:
                        progressDialog = new ProgressDialog(this);
                        progressDialog.SetMessage("上傳中，請稍等...");
                        progressDialog.Show();
                        // Call uploading function
                        new UploadFileAsync().Execute("");
                        }));
                    alert_upload.Show();
                    return;
                }
            }
        }

        // 查閱影像函式定義
        void View_Click(object sender, EventArgs e)
        {
            this.Finish();
            this.StartActivity(typeof(PlayActivity));
        }

        // AutoUpload Class:
        // Implement the upload class:
        private class UploadFileAsync : AsyncTask<String, int, String>
        {
            private HttpStatus serverResponseCode;
            protected override string RunInBackground(params string[] @params)
            {
                for (int i = 0; i <= 2; i++)
                {
                    try
                    {
                        String sourceFileUri = MainActivity.rec_video_uri[i];

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
                }
                progressDialog.Dismiss();

                return "Finished";
            }
        }
        // End of upload class.
    }

    // Class SurfaceHolderCallBack 控制一些錄影的旋轉問題:     
    public class SurfaceHolderCallBack : Java.Lang.Object, ISurfaceHolderCallback
        {
            public int RotataDegrees { get; set; }
            Camera Camera;
            Activity _Context;

            public SurfaceHolderCallBack(Activity context, Camera camera, MediaRecorder mediaRecorder)
            {
                Camera = camera;
                _Context = context;
            }

            public void SurfaceChanged(ISurfaceHolder holder, Android.Graphics.Format format, int width, int height)
            {
            }

            public void SurfaceCreated(ISurfaceHolder holder)
            {
                Camera.SetPreviewDisplay(holder);
                Camera.CameraInfo camInfo = new Camera.CameraInfo();
                Camera.GetCameraInfo(0, camInfo);
                var rotation = _Context.WindowManager.DefaultDisplay.Rotation;
                int degrees = 0;
                switch (rotation)
                {
                    case
                    SurfaceOrientation.Rotation0:
                        degrees = 0; break;
                    case SurfaceOrientation.Rotation90:
                        degrees = 90; break;
                    case SurfaceOrientation.Rotation180:
                        degrees = 180; break;
                    case SurfaceOrientation.Rotation270:
                        degrees = 270; break;
                }
                RotataDegrees = (camInfo.Orientation - degrees + 360) % 360;
                Camera.SetDisplayOrientation(RotataDegrees);
                Camera.StartPreview();

            }



        public void SurfaceDestroyed(ISurfaceHolder holder)
            {
            }
    } // End of Class SurfaceHolderCallBack


}

