using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Webkit;

namespace WebViewAndroid
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar",
            ScreenOrientation = ScreenOrientation.Portrait)]
    public class SubActivity : ParentActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_sub);

            wvMain = (WebView)FindViewById(Resource.Id.wvMain);
            InitWebView();

            string MainURL = Intent.GetStringExtra("url");
            wvMain.LoadUrl(MainURL);
        }
    }
}