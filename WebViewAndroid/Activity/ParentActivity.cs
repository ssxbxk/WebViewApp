using Android.Annotation;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Graphics;
using Android.Locations;
using Android.Media;
using Android.Net;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using Java.IO;
using System;
using System.IO;

namespace WebViewAndroid
{
    public class ParentActivity: AppCompatActivity, ILocationListener
    {
        public WebView wvMain = null;
        public int FILECHOOSER_RESULTCODE = 100;
        public int CAMERA_RESULTCODE = 101;
        public Java.IO.File tmpUploadFile = null;

        private static string NAV_BAR_HEIGHT_RES_NAME = "navigation_bar_height";
        private static string NAV_BAR_HEIGHT_LANDSCAPE_RES_NAME = "navigation_bar_height_landscape";
        private static string SHOW_NAV_BAR_RES_NAME = "config_showNavigationBar";

        private NotificationManager nMgr;
        private bool mInPortrait;
        private MyWebViewClient myWebViewClient = new MyWebViewClient();
        private MyWebChromeClient myWebChromeClient;
        private LocationManager mLocationMgr;
        private JSInterface jsInterface;
        private bool GetLocation = false;
        private bool SuccessStart = false;
        private bool PreGetLocation = false;    // 用于保存暂停前是否获取经纬度的状态

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            //获取通知管理类
            nMgr = (NotificationManager)GetSystemService(NotificationService);

            Resources res = this.Resources;
            mInPortrait = (res.Configuration.Orientation == Android.Content.Res.Orientation.Portrait);

            InitLocation();
        }

        public void InitWebView() {
            wvMain.Settings.JavaScriptEnabled = true;
            wvMain.Settings.CacheMode = CacheModes.NoCache;

            //添加我们刚创建的类,并命名为wv 
            jsInterface = new JSInterface(this);
            wvMain.AddJavascriptInterface(jsInterface, "wv");
            wvMain.SetWebViewClient(myWebViewClient);

            myWebChromeClient = new MyWebChromeClient(this);
            wvMain.SetWebChromeClient(myWebChromeClient);
            wvMain.Settings.AllowFileAccess = true;
            wvMain.Settings.AllowFileAccessFromFileURLs = true;

            // 去掉通知栏高度
            int height = Resources.DisplayMetrics.HeightPixels;
            int iHeight = GetNavigationBarHeight(this);
            if (iHeight > 0)
            {
                wvMain.LayoutParameters.Height = height - iHeight / 2;
            }
        }

        public void NotifyInfo(string szTitle, string szMsg, bool bVoice, bool bVibrate)
        {
            //设置通知的图标以及显示的简介Title
            Notification notify = new Notification(Resource.Mipmap.ic_launcher, szTitle);
            //初始化点击通知后打开的活动
            PendingIntent pintent = PendingIntent.GetActivity(this, 0,
                new Intent(this, typeof(MainActivity)), PendingIntentFlags.UpdateCurrent);
            //设置通知的主体
            notify.SetLatestEventInfo(this, szTitle, szMsg, pintent);

            if (bVoice)
            {
                Android.Net.Uri ringUri = RingtoneManager.GetDefaultUri(RingtoneType.Notification);
                notify.Sound = ringUri;
                notify.Defaults = NotificationDefaults.Sound | notify.Defaults;
            }

            if (bVibrate)
            {
                notify.Vibrate = new long[] { 1000 };
                notify.Defaults = NotificationDefaults.Vibrate | notify.Defaults;
            }

            notify.LedARGB = Color.Green;
            //设置LED显示时间为1s
            notify.LedOnMS = 1000;
            //设置LED熄灭时间为1s
            notify.LedOffMS = 1000;
            notify.Flags = NotificationFlags.ShowLights | notify.Flags;
            notify.Defaults = NotificationDefaults.Lights | notify.Defaults;

            //发送通知
            nMgr.Notify(0, notify);
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Android.App.Result resultCode,
            Intent data)
        {
            if (wvMain != null && requestCode == 0 && resultCode == 0 && data != null)
            {
                string szJS = data.GetStringExtra("js");
                szJS = szJS == null ? "" : szJS.Trim();
                wvMain.LoadUrl(string.Format("javascript:{0};", szJS));
            }
            else if (requestCode == FILECHOOSER_RESULTCODE) {
                if (resultCode == Result.Ok && tmpUploadFile != null) 
                {
                    // 压缩一下图片, 否则太大了
                    Android.Net.Uri filePath = Android.Net.Uri.FromFile(tmpUploadFile);
                    Bitmap bitMap = BitmapFactory.DecodeFile(filePath.Path);

                    // 删除源文件
                    System.IO.File.Delete(filePath.Path);

                    int iWidth = 1024;
                    int iHeight = 576;
                    if (bitMap.Height > bitMap.Width)
                    {
                        int iTmp = iWidth;
                        iWidth = iHeight;
                        iHeight = iTmp;
                    }
                    
                    bitMap = Utils.ZoomImage(bitMap, iWidth, iHeight);
                    FileStream fos = new FileStream(filePath.Path, FileMode.OpenOrCreate);
                    bitMap.Compress(Bitmap.CompressFormat.Jpeg, 90, fos);
                    fos.Flush();
                    fos.Close();

                    // 转换成base64, 发送到js
                    string base64Img = Utils.GetImgStr(filePath.Path);
                    string szKey = Guid.NewGuid().ToString();
                    Utils.SetString(szKey, base64Img);
                    wvMain.LoadUrl("javascript:ReceiveImage('" + szKey + "');");

                    // 删除手机上的缓存文件
                    System.IO.File.Delete(filePath.Path);
                }
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions,
            [GeneratedEnum] Permission[] grantResults)
        {
            if (requestCode == CAMERA_RESULTCODE)
            {
                if (grantResults[0] == Permission.Granted)
                {
                    jsInterface.OpenCamra();
                }
                else {
                    Toast.MakeText(this, "请开启相机访问权限!", ToastLength.Long).Show();
                }
            }
        }

        protected int GetNavigationBarHeight(Context context)
        {
            Resources res = context.Resources;
            int result = 0;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.IceCreamSandwich)
            {
                if (HasNavBar(context))
                {
                    string key;
                    if (mInPortrait)
                    {
                        key = NAV_BAR_HEIGHT_RES_NAME;
                    }
                    else
                    {
                        key = NAV_BAR_HEIGHT_LANDSCAPE_RES_NAME;
                    }
                    return GetInternalDimensionSize(res, key);
                }
            }
            return result;
        }

        protected int GetInternalDimensionSize(Resources res, string key)
        {
            int result = 0;
            int resourceId = res.GetIdentifier(key, "dimen", "android");
            if (resourceId > 0)
            {
                result = res.GetDimensionPixelSize(resourceId);
            }
            return result;
        }

        [TargetApi(Value = 14)]
        protected bool HasNavBar(Context context)
        {
            Resources res = context.Resources;
            int resourceId = res.GetIdentifier(SHOW_NAV_BAR_RES_NAME, "bool", "android");
            if (resourceId != 0)
            {
                return res.GetBoolean(resourceId);
            }
            else
            { // fallback  
                return !ViewConfiguration.Get(context).HasPermanentMenuKey;
            }
        }


        public void SetWebViewURL(string szURL)
        {
            if (wvMain != null && szURL != null && szURL.Length > 0)
            {
                RunOnUiThread(()=>{
                    wvMain.LoadUrl(szURL);
                });
            }
        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            if (keyCode == Keycode.Back && e.Action == KeyEventActions.Down)
            {
                wvMain.LoadUrl("javascript:CustomGoBack();");
                return true;
            }
            else
                return base.OnKeyDown(keyCode, e);
        }

        private void InitLocation()
        {
            mLocationMgr = GetSystemService(Context.LocationService) as LocationManager;
        }

        public void StartLocationService()
        {
            GetLocation = true;
            string Provider = LocationManager.GpsProvider;
            /*
             * 选择最佳的Provider
            Criteria locationCriteria = new Criteria
            {
                Accuracy = Accuracy.Coarse,
                PowerRequirement = Power.Medium
            };

            string Provider = mLocationMgr.GetBestProvider(locationCriteria, true);
            */
            if (mLocationMgr.IsProviderEnabled(Provider))
            {
                mLocationMgr.RequestLocationUpdates(Provider, 5000, 10, this);
                SuccessStart = true;
            }
            else {
                Toast.MakeText(this, "请打开GPS定位功能", ToastLength.Short).Show();
            }
        }

        public void StopLocationService()
        {
            GetLocation = false;
            mLocationMgr.RemoveUpdates(this);
        }

        public void OnLocationChanged(Location location)
        {
            RunOnUiThread(() => {
                wvMain.LoadUrl("javascript:UpdateLocation(" +
                            location.Longitude + "," + location.Latitude + "," + location.Accuracy + ")");
            });
        }

        public void OnProviderDisabled(string provider)
        {
            RunOnUiThread(() => {
                Toast.MakeText(this, "请打开GPS定位功能", ToastLength.Short).Show();
            });
        }

        public void OnProviderEnabled(string provider)
        {
            RunOnUiThread(() => {
                Toast.MakeText(this, "定位中, 请稍候", ToastLength.Short).Show();
                // 这个代码没用, 如果程序运行的时候, 禁用了GPS, 再打开GPS的时候, 不会触发这个事件
                if (SuccessStart == false && GetLocation)
                {
                    StartLocationService();
                }
            });
        }

        public void OnStatusChanged(string provider, [GeneratedEnum] Availability status, Bundle extras)
        {
        }

        protected override void OnStop()
        {
            base.OnStop();
            PreGetLocation = GetLocation;
            if (PreGetLocation)
                StopLocationService();
        }

        protected override void OnResume()
        {
            base.OnResume();
            if (PreGetLocation)
                StartLocationService();
        }
    }
}