using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Android.Content;
using Android.Graphics;
using Android.Net;
using Android.Widget;
using Java.IO;
using Java.Util;

namespace WebViewAndroid
{
    public static class Utils
    {
        public static string MainURL = "http://192.168.0.73:8012/1.html";
        //public static string MainURL = "http://192.168.0.131:8083/Login.html";  // 樊响

        public static string WebServiceURL = "http://192.168.0.73:8009/ZHWebService.asmx";

        public static string ROOT_PATH = "";
        public static string IMAGE_PATH = "";
        public static string DOWNLOAD_PATH = "";
        public static string APK_PATH = "";
        public static string CONFIG_PATH = "";
        public static MobileTerminaWS.MobileTerminalWS webService = new MobileTerminaWS.MobileTerminalWS();

        private static Dictionary<string, string> DicCache = new Dictionary<string, string>();

        public static void InitPath(Context context) {
            ROOT_PATH = context.GetExternalFilesDir(Android.OS.Environment.DirectoryDocuments).ToString();
            DOWNLOAD_PATH = ROOT_PATH + "/download/";
            IMAGE_PATH = ROOT_PATH + "/cameraimage/";
            APK_PATH = DOWNLOAD_PATH + "com.dgys.hnmsawebapp.apk";
            CONFIG_PATH = ROOT_PATH + "/config.cfg";
        }

        public static bool ExternalMemoryAvailable() {
            return true;
        }

        public static void InitWS()
        {
            webService.Url = WebServiceURL;
            webService.MySoapHeaderValue = new MobileTerminaWS.MySoapHeader
            {
                Unarray = "Qh90JKZHHh3/PHOAtR92Yw=="
            };

            if (!Directory.Exists(DOWNLOAD_PATH))
                Directory.CreateDirectory(DOWNLOAD_PATH);
            if (!Directory.Exists(IMAGE_PATH))
                Directory.CreateDirectory(IMAGE_PATH);
        }

        public static void InstallAPK(Context context, string szPath)
        {
            try
            {
                // 通过Intent安装APK文件
                Intent intent = new Intent(Intent.ActionView);
                intent.SetDataAndType(Android.Net.Uri.Parse("file://" + szPath),
                    "application/vnd.android.package-archive");
                intent.SetFlags(ActivityFlags.NewTask);
                context.StartActivity(intent);
            }
            catch (Exception ex)
            {
                Toast.MakeText(context, "启动安装失败: " + ex.Message, ToastLength.Short).Show();
            }
        }

        public static bool IsNetworkConnected(Context context)
        {
            try {
                if (context != null)
                {
                    ConnectivityManager mConnectivityManager = (ConnectivityManager)context.GetSystemService(Context.ConnectivityService);
                    NetworkInfo mNetworkInfo = mConnectivityManager.ActiveNetworkInfo;
                    if (mNetworkInfo != null)
                    {
                        return mNetworkInfo.IsAvailable;
                    }
                }
            }
            catch { }
            return false;
        }

        /**
        * 将图片转换成Base64编码
        * @param imgFile 待处理图片
        * @return
        */
        public static String GetImgStr(String imgFile)
        {
            //将图片文件转化为字节数组字符串，并对其进行Base64编码处理
            InputStream inFile = null;
            byte[] data = null;
            //读取图片字节数组
            try
            {
                inFile = new FileInputStream(imgFile);
                data = new byte[inFile.Available()];
                inFile.Read(data);
                inFile.Close();
            }
            catch
            { }
            return "data:image/jpeg;base64," + Convert.ToBase64String(data);
        }

        public static void SetString(string szKey, string szValue)
        {
            if (DicCache.ContainsKey(szKey))
                DicCache.Remove(szKey);

            if (szValue.Length > 0)
                DicCache.Add(szKey, szValue);
        }

        public static string GetString(string szKey)
        {
            if (DicCache.ContainsKey(szKey))
                return DicCache[szKey];
            else
                return "";
        }

        public static void RemoveAllString() {
            DicCache.Clear();
        }

        /// <summary>
        /// 图片缩放处理
        /// </summary>
        /// <param name="bgimage">Bitmap文件</param>
        /// <param name="newWidth">新图片宽度</param>
        /// <param name="newHeight">新图片高度</param>
        /// <returns></returns>
        public static Bitmap ZoomImage(Bitmap bgimage, double newWidth, double newHeight)
        {
            // 获取这个图片的宽和高
            float width = bgimage.Width;
            float height = bgimage.Height;
            // 创建操作图片用的matrix对象
            Matrix matrix = new Matrix();
            // 计算宽高缩放率
            float scaleWidth = ((float)newWidth) / width;
            float scaleHeight = ((float)newHeight) / height;
            // 缩放图片动作
            matrix.PostScale(scaleWidth, scaleHeight);
            Bitmap bitmap = Bitmap.CreateBitmap(bgimage, 0, 0, (int)width,
                            (int)height, matrix, true);
            return bitmap;
        }
    }
}