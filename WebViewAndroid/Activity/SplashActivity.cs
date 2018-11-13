
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;
using Java.IO;
using Java.Net;
using System;
using System.IO;
using System.Threading.Tasks;

namespace WebViewAndroid
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.Splash", MainLauncher = true, NoHistory = true,
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class SplashActivity: AppCompatActivity
    {
        MyHandler handler;
        ProgressDialog dlg;

        public class MyHandler : Handler {
            private SplashActivity sa;
            public MyHandler(SplashActivity sa) {
                this.sa = sa;
            }

            public override void HandleMessage(Message msg) {
                switch(msg.What)
                {
                    case 1:
                        Toast.MakeText(sa, "SD卡不可用", ToastLength.Short).Show();
                        break;
                    case 2:
                        Toast.MakeText(sa, "下载安装包失败", ToastLength.Short).Show();
                        break;
                    case 3:
                        Utils.InstallAPK(sa, Utils.APK_PATH);
                        break;
                }
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Utils.InitPath(this);

            handler = new MyHandler(this);
            dlg = new ProgressDialog(this);
            Utils.InitWS();
        }

        protected override void OnResume()
        {
            base.OnResume();
            
            CheckForUpdate();
        }

        private void GoMainPage() {
            Intent localIntent = new Intent(this, typeof(MainActivity));
            StartActivity(localIntent);
        }

        private void CheckForUpdate() {
            try {
                string szVersionCode = this.PackageManager.GetPackageInfo(this.PackageName, 0).VersionCode.ToString();
                string szURL = Utils.webService.GetAPKUrl(szVersionCode);
                szURL = szURL == null ? "" : szURL.Trim();
                if (szURL.Length > 0) {
                    Android.App.AlertDialog alertDialog = new Android.App.AlertDialog.Builder(this)
                            .SetTitle("提示")
                            .SetMessage("新版本已发布, 是否立即更新?")
                            .SetPositiveButton("确定", (s, e) =>
                            {
                                GoUpdatePage(szURL);
                            })
                            .SetNegativeButton("取消", (s, e) =>{
                                GoMainPage();
                            })
                            .Create();
                    alertDialog.SetCanceledOnTouchOutside(false);
                    alertDialog.Show();
                }
                else {
                    GoMainPage();
                }
            }
            catch {
                GoMainPage();
            }
        }

        private void GoUpdatePage(string szURL)
        {
            dlg.SetProgressStyle(ProgressDialogStyle.Horizontal);
            dlg.SetTitle("自动更新");
            dlg.SetMessage("正在下载...");
            dlg.SetCancelable(false);// 设置是否可以通过点击Back键取消
            dlg.SetCanceledOnTouchOutside(false);// 设置在点击Dialog外是否取消Dialog进度条
            dlg.Show();

            Task.Run(() => {
                try
                {
                    // 判断SD卡是否存在，并且是否具有读写权限
                    if (Utils.ExternalMemoryAvailable())
                    {
                        URL url = new URL(szURL);
                        // 创建连接
                        HttpURLConnection conn = (HttpURLConnection)url.OpenConnection();
                        conn.Connect();
                        // 获取文件大小
                        int length = conn.ContentLength;
                        // 创建输入流
                        Stream ipts = conn.InputStream;

                        Java.IO.File file = new Java.IO.File(Utils.ROOT_PATH);
                        // 判断文件目录是否存在
                        if (!file.Exists())
                        {
                            file.Mkdir();
                        }

                        file = new Java.IO.File(Utils.DOWNLOAD_PATH);
                        // 判断文件目录是否存在
                        if (!file.Exists())
                        {
                            file.Mkdir();
                        }

                        Java.IO.File apkFile = new Java.IO.File(Utils.APK_PATH);
                        FileOutputStream fos = new FileOutputStream(apkFile);
                        int count = 0;
                        // 缓存
                        byte[] buf = new byte[102400];
                        int iHistProgress = 0;
                        // 写入到文件中
                        do
                        {
                            int numread = ipts.Read(buf, 0, 102400);
                            count += numread;
                            // 计算进度条位置
                            int progress = (int)(((float)count / length) * 100);
                            // 更新进度
                            dlg.IncrementProgressBy(progress - iHistProgress);
                            iHistProgress = progress;
                            if (numread <= 0)
                            {
                                break;
                            }
                            // 写入文件
                            fos.Write(buf, 0, numread);
                        } while (true);
                        fos.Close();
                        ipts.Close();

                        // 下载完毕
                        handler.SendEmptyMessage(3);
                    }
                    else
                    {
                        handler.SendEmptyMessage(1);
                    }
                }
                catch
                {
                    handler.SendEmptyMessage(2);
                }
                // 取消下载对话框显示
                dlg.Dismiss();
            });
        }
    }
}