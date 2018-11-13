using Android.Webkit;

namespace WebViewAndroid
{
    public class MyWebChromeClient : WebChromeClient
    {
        private readonly ParentActivity activity = null;

        public MyWebChromeClient(ParentActivity active)
        {
            activity = active;
        }

        public override bool OnJsAlert(WebView view, string url, string message, JsResult result)
        {
            Android.App.AlertDialog alertDialog = new Android.App.AlertDialog.Builder(activity)
                            .SetTitle("提示")
                            .SetMessage(message)
                            .SetPositiveButton("确定", (s, e) =>
                            {
                                result.Confirm();
                            })
                            .Create();
            alertDialog.Show();
            return true;
        }
    }
}