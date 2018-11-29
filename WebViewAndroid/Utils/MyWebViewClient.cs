using System;
using Android.Runtime;
using Android.Webkit;

namespace WebViewAndroid
{
    public class MyWebViewClient : WebViewClient{
        private readonly ParentActivity activity = null;

        public MyWebViewClient(ParentActivity active)
        {
            activity = active;
        }

        public override bool ShouldOverrideUrlLoading(WebView view, String url)
        {
            view.LoadUrl(url);
            return true;
        }

        public override void OnReceivedError(WebView view, [GeneratedEnum] ClientError errorCode, string description, string failingUrl)
        {
            base.OnReceivedError(view, errorCode, description, failingUrl);
            activity.OnWebViewError();
        }

        public override void OnReceivedError(WebView view, IWebResourceRequest request, WebResourceError error)
        {
            base.OnReceivedError(view, request, error);
            activity.OnWebViewError();
        }
    }
}