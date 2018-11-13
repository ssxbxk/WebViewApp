using System;
using Android.Webkit;

namespace WebViewAndroid
{
    public class MyWebViewClient : WebViewClient{
        public override bool ShouldOverrideUrlLoading(WebView view, String url)
        {
            view.LoadUrl(url);
            return true;
        }
    }
}