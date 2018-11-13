using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using System.Threading;

namespace WebViewAndroid
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar",
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : ParentActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			SetContentView(Resource.Layout.activity_main);

            wvMain = (WebView)FindViewById(Resource.Id.wvMain);

            InitWebView();

            // 检查网络状况
            new Thread(new ThreadStart(
                () =>
                {
                    while (true)
                    {
                        if (Utils.IsNetworkConnected(this))
                        {
                            RunOnUiThread(() => {
                                wvMain.LoadUrl(Utils.MainURL);
                            });
                            break;
                        }
                        else {
                            RunOnUiThread(()=>{
                                Toast.MakeText(this, "请打开网络链接!", ToastLength.Short).Show();
                            });
                            Thread.Sleep(5000);
                        }
                    }
                }
             )).Start();
        }
    }
}
