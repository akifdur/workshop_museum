/*
 * Copyright (C) 2012 GREE, Inc.
 * 
 * This software is provided 'as-is', without any express or implied
 * warranty.  In no event will the authors be held liable for any damages
 * arising from the use of this software.
 * 
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 * 
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would be
 *    appreciated but is not required.
 * 2. Altered source versions must be plainly marked as such, and must not be
 *    misrepresented as being the original software.
 * 3. This notice may not be removed or altered from any source distribution.
 */

using System.Collections;
using Gree.UnityWebView;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Networking;

public class WebViewController : MonoBehaviour
{
    public string url;
    public int4 margins;

    [SerializeField]
    private WebViewObject webViewObject;

    private Coroutine _loadCoroutine;

    private void OnEnable() 
    {
        _loadCoroutine = StartCoroutine(LoadWebView(url));
        SetVisibility(true);
    }

    private void OnDisable() 
    {
        if (_loadCoroutine != null) StopCoroutine(_loadCoroutine);
        SetVisibility(false);
    }

    private void SetVisibility(bool visibility)
    {
        webViewObject.SetVisibility(visibility);
    }

    // Note: Load web view loads the page but won't make it visible.
    // to do this, you must run SetVisibility(true);
    private IEnumerator LoadWebView(string source)
    {
        webViewObject.Init(
            ld: (msg) =>
            {
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_IOS
#if true
                var js = @"
                    if (!(window.webkit && window.webkit.messageHandlers)) {
                        window.Unity = {
                            call: function(msg) {
                                window.location = 'unity:' + msg;
                            }
                        };
                    }
                ";
#else
                // NOTE: depending on the situation, you might prefer this 'iframe' approach.
                // cf. https://github.com/gree/unity-webview/issues/189
                var js = @"
                    if (!(window.webkit && window.webkit.messageHandlers)) {
                        window.Unity = {
                            call: function(msg) {
                                var iframe = document.createElement('IFRAME');
                                iframe.setAttribute('src', 'unity:' + msg);
                                document.documentElement.appendChild(iframe);
                                iframe.parentNode.removeChild(iframe);
                                iframe = null;
                            }
                        };
                    }
                ";
#endif
#elif UNITY_WEBPLAYER || UNITY_WEBGL
                var js = @"
                    window.Unity = {
                        call:function(msg) {
                            parent.unityWebView.sendMessage('WebViewObject', msg);
                        }
                    };
                ";
#else
                var js = "";
#endif
                webViewObject.EvaluateJS(js + @"Unity.call('ua=' + navigator.userAgent)");
            },
            //transparent: false,
            //zoom: true,
            //ua: "custom user agent string",
            //radius: 0,  // rounded corner radius in pixel
            //// android
            //androidForceDarkMode: 0,  // 0: follow system setting, 1: force dark off, 2: force dark on
            //// ios
            //enableWKWebView: true,
            wkContentMode: 2,  // 0: recommended, 1: mobile, 2: desktop
            wkAllowsLinkPreview: true,
            //// editor
            separated: true
            );
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        webViewObject.bitmapRefreshCycle = 1;
        webViewObject.devicePixelRatio = 1;  // 1 or 2
#endif
        // cf. https://github.com/gree/unity-webview/pull/512
        // Added alertDialogEnabled flag to enable/disable alert/confirm/prompt dialogs. by KojiNakamaru · Pull Request #512 · gree/unity-webview
        //webViewObject.SetAlertDialogEnabled(false);

        // cf. https://github.com/gree/unity-webview/pull/728
        //webViewObject.SetCameraAccess(true);
        //webViewObject.SetMicrophoneAccess(true);

        // cf. https://github.com/gree/unity-webview/pull/550
        // introduced SetURLPattern(..., hookPattern). by KojiNakamaru · Pull Request #550 · gree/unity-webview
        //webViewObject.SetURLPattern("", "^https://.*youtube.com", "^https://.*google.com");

        // cf. https://github.com/gree/unity-webview/pull/570
        // Add BASIC authentication feature (Android and iOS with WKWebView only) by takeh1k0 · Pull Request #570 · gree/unity-webview
        //webViewObject.SetBasicAuthInfo("id", "password");

        //webViewObject.SetScrollbarsVisibility(true);

        webViewObject.SetMargins(margins.x, margins.y, margins.z, margins.w);

#if !UNITY_WEBPLAYER && !UNITY_WEBGL
        var src = System.IO.Path.Combine(Application.streamingAssetsPath, source);
        var dst = System.IO.Path.Combine(Application.temporaryCachePath, source);
        byte[] result;
        if (src.Contains("://")) 
        {   
            // for Android
            // NOTE: a more complete code that utilizes UnityWebRequest can be found in https://github.com/gree/unity-webview/commit/2a07e82f760a8495aa3a77a23453f384869caba7#diff-4379160fa4c2a287f414c07eb10ee36d
            var unityWebRequest = UnityWebRequest.Get(src);
            yield return unityWebRequest.SendWebRequest();
            result = unityWebRequest.downloadHandler.data;
        } 
        else 
        {
            result = System.IO.File.ReadAllBytes(src);
        }
        System.IO.File.WriteAllBytes(dst, result);
        webViewObject.LoadURL("file://" + dst.Replace(" ", "%20"));
#else
        webViewObject.LoadURL("StreamingAssets/" + Url.Replace(" ", "%20"));
#endif
        yield break;
    }
}