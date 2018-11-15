using System;
using System.IO;
using Android;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Provider;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Webkit;
using Android.Widget;
using Java.Interop;

namespace WebViewAndroid
{
    public class JSInterface: Java.Lang.Object
    {
        readonly ParentActivity activity;
        public JSInterface(ParentActivity act)
        {
            this.activity = act;
        }

        [Export]
        [JavascriptInterface]
        public void ShowInfo(string message)
        {
            Toast.MakeText(activity, message, ToastLength.Short).Show();
        }

        [Export]
        [JavascriptInterface]
        public bool WriteConfig(string szCfg)
        {
            bool bWrite = false;
            try
            {
                if (false == Directory.Exists(Utils.ROOT_PATH))
                {
                    //创建文件夹
                    Directory.CreateDirectory(Utils.ROOT_PATH);
                }

                FileStream fs = new FileStream(Utils.CONFIG_PATH, FileMode.Create, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs);
                sw.Write(szCfg);
                sw.Flush();
                sw.Close();
                fs.Close();
                bWrite = true;
            }
            catch(Exception e){
                if (e != null) { }
            }
            return bWrite;
        }

        [Export]
        [JavascriptInterface]
        public string ReadConfig()
        {
            string szInfo = "";
            try {
                FileStream fs = new FileStream(Utils.CONFIG_PATH, FileMode.Open, FileAccess.Read);
                StreamReader sr = new StreamReader(fs);
                szInfo = sr.ReadToEnd();
                sr.Close();
                fs.Close();
            }
            catch { }
            return szInfo;
        }

        [Export]
        [JavascriptInterface]
        public void Exit()
        {
            activity.Finish();
        }

        [Export]
        [JavascriptInterface]
        public void NotifyInfo(string szTitle, string szMsg, bool bVoice, bool bVibrate)
        {
            activity.NotifyInfo(szTitle, szMsg, bVoice, bVibrate);
        }

        [Export]
        [JavascriptInterface]
        public void ShowInSub(string szUrl)
        {
            szUrl = szUrl == null ? "" : szUrl.Trim();
            if (szUrl.Length > 0)
            {
                Intent localIntent = new Intent(activity, typeof(SubActivity));
                localIntent.PutExtra("url", szUrl);
                activity.StartActivityForResult(localIntent, 0);
            }
        }

        [Export]
        [JavascriptInterface]
        public void SubGoBack(string szJavaScript)
        {
            if (typeof(SubActivity).IsAssignableFrom(activity.GetType())) {
                szJavaScript = szJavaScript == null ? "" : szJavaScript.Trim();
                if (szJavaScript.Length > 0)
                {
                    Intent intent = new Intent();
                    intent.PutExtra("js", szJavaScript);
                    activity.SetResult(0, intent);
                }
                activity.Finish();
            }
        }

        [Export]
        [JavascriptInterface]
        public void SubGoBack()
        {
            SubGoBack("");
        }

        [Export]
        [JavascriptInterface]
        public void ChangeHref(string szURL)
        {
            activity.SetWebViewURL(szURL);
        }

        [Export]
        [JavascriptInterface]
        public void RemoveTmpImgFile(string szName)
        {
            try {
                string szPath = Utils.IMAGE_PATH + szName;
                File.Delete(szPath);
            }
            catch { }
        }

        [Export]
        [JavascriptInterface]
        public void StartLocation()
        {
            activity.StartLocationService();
        }

        [Export]
        [JavascriptInterface]
        public void StopLocation()
        {
            activity.StopLocationService();
        }

        [Export]
        [JavascriptInterface]
        public void OpenCameraActivity()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                Permission p = ContextCompat.CheckSelfPermission(activity,
                    Manifest.Permission.Camera);
                if (p != Permission.Granted)
                {
                    ActivityCompat.RequestPermissions(activity,
                        new string[] { Manifest.Permission.Camera }, activity.CAMERA_RESULTCODE);
                    return;
                }
                else
                {

                    OpenCamra();//调用具体方法
                }
            }
            else
            {
                OpenCamra();//调用具体方法
            }
        }

        public void OpenCamra()
        {
            Intent i = new Intent(MediaStore.ActionImageCapture);
            activity.tmpUploadFile = new Java.IO.File(Utils.IMAGE_PATH
                        + string.Format("{0}.jpg", Guid.NewGuid().ToString()));
            i.PutExtra(MediaStore.ExtraOutput, Android.Net.Uri.FromFile(activity.tmpUploadFile));
            activity.StartActivityForResult(i, activity.FILECHOOSER_RESULTCODE);
        }

        [Export]
        [JavascriptInterface]
        public void SetString(string szKey, string szValue)
        {
            Utils.SetString(szKey, szValue);
        }

        [Export]
        [JavascriptInterface]
        public string GetString(string szKey)
        {
            return Utils.GetString(szKey);
        }

        [Export]
        [JavascriptInterface]
        public void RemoveAllString()
        {
            Utils.RemoveAllString();
        }

        [Export]
        [JavascriptInterface]
        public string SaveToLocal(string szName, string szContent)
        {
            return Utils.DecodeBase64AndSave(szName, szContent);
        }
    }
}