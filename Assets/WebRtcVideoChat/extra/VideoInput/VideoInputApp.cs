using System.Collections;
using UnityEngine;

namespace Byn.Unity.Examples
{

    /// <summary>
    /// This app is the same as the callapp just refreshing the
    /// video list later to give the virtual video input some time to
    /// setup.
    /// 
    /// See VirtualCamera for documentation.
    /// </summary>
    public class VideoInputApp : CallApp
    {

        protected override void Start()
        {
            base.Start();

#if UNITY_WEBGL
        //doesn't work with WebGL
        Debug.LogError("VideoInput isn't supported in the browser.");
#else
            //need to fresh the ui a bit later as the virtual input needs a while to start
            //without this the virtual camera wouldn't be visible in the video device list
            StartCoroutine(CoroutineRefreshLater());
#endif
        }

        IEnumerator CoroutineRefreshLater()
        {
            yield return new WaitForSecondsRealtime(1);
            mUi.UpdateVideoDropdown();
        }
    }
}