using System.Collections;
using Byn.Awrtc;
using UnityEngine;

namespace Byn.Unity.Examples
{
    /// <summary>
    /// Keeps the global urls / data used for the example applications and unit tests
    /// </summary>
    public class ExampleGlobals
    {
        /// <summary>
        /// Recommended signaling protcol.
        /// wss only for WebGL to avoid possible issues with ws due to certificates / platform
        /// specific SSL issues.
        /// </summary>
        public static string SignalingProtocol
        {
            get{

                string protocol = "ws";
                if(Application.platform == RuntimePlatform.WebGLPlayer)
                {
                    protocol = "wss";
                }
                return protocol;
            }
        }
        /// <summary>
        /// Signaling. ws by default. wss for webgl
        /// </summary>
        public static string Signaling
        {
            get
            {
                return SignalingProtocol + "://signaling.because-why-not.com/test";
            }
        }

        /// <summary>
        /// Signaling for shared addresses (conference calls)
        /// </summary>
        public static string SharedSignaling
        {
            get
            {
                return SignalingProtocol + "://signaling.because-why-not.com/testshared";
            }
        }
        /// <summary>
        /// Signaling for the conference example app
        /// </summary>
        public static string SignalingConference
        {
            get
            {
                return SignalingProtocol + "://signaling.because-why-not.com/conferenceapp";
            }
        }
        /// <summary>
        /// Stun server
        /// </summary>
        public static readonly string StunUrl = "stun:stun.because-why-not.com:443";

        /// <summary>
        /// Turn server
        /// </summary>
        public static readonly string TurnUrl = "turn:turn.because-why-not.com:443";

        /// <summary>
        /// Turn server user (changed if overused)
        /// </summary>
        public static readonly string TurnUser = "testuser14";

        /// <summary>
        /// Turn server password (changed if userused)
        /// </summary>
        public static readonly string TurnPass = "pass14";


        public static IceServer DefaultIceServer
        {
            get{
                return new IceServer(TurnUrl, TurnUser, TurnPass);
            }
        }

        /// <summary>
        /// Backup stun server to keep essentials running during server maintenance
        /// </summary>
        public static readonly string BackupStunUrl = "stun:stun.l.google.com:19302";





        public static bool HasAudioPermission()
        {
#if UNITY_ANDROID && UNITY_2018_3_OR_NEWER
            if (Application.platform == RuntimePlatform.Android)
            {
                return UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Microphone);
            }
#endif
            //Assume true for all other platforms for now
            return true;
            
        }
        public static bool HasVideoPermission()
        {
#if UNITY_ANDROID && UNITY_2018_3_OR_NEWER
            if (Application.platform == RuntimePlatform.Android)
            {
                return UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Camera);
            }
#endif
            //Assume true for all other platforms for now
            return true;
            
        }

        public static IEnumerator RequestAudioPermission()
        {
#if UNITY_ANDROID && UNITY_2018_3_OR_NEWER
            if (!HasAudioPermission())
            {
                Debug.Log("Requesting microphone permissions");
                UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Microphone);
                //wait a while. In some Unity versions calling it twice during a single frame will cause one dialog to be 
                //omitted. Unity should stall here until the user either pressed allow or deny
                yield return new WaitForSeconds(0.1f);
                //might still return false here in rare cases even if the user pressed allowed. Looks like the only reliable way would
                //be polling permanently
                Debug.Log("microphone permission: " + HasAudioPermission());
            }
#endif
            yield return null;
        }


        public static IEnumerator RequestVideoPermission()
        {
#if UNITY_ANDROID && UNITY_2018_3_OR_NEWER
            if (!HasVideoPermission())
            {
                Debug.Log("Requesting camera permissions");
                UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Camera);
                yield return new WaitForSeconds(0.1f);
                Debug.Log("camera permission: " + HasVideoPermission());
            }
#endif
            yield return null;
        }


        /// <summary>
        /// Only works on Android. Won't do anything on other platorms but allows adding this feature later on.
        /// 
        /// This function is made for simple examples that might not have a UI but need to access audio or video.
        /// This method might not work with every Unity version. There seem to be cases in which either
        /// Microphone or Camera permissions are skipped on the first try if both are requested simultaneously. 
        /// </summary>
        public static IEnumerator RequestPermissions(bool audio = true, bool video = true)
        {
            if(audio)
            {
                yield return RequestAudioPermission();
            }
            if (video)
            {
                yield return RequestVideoPermission();
            }

            yield return null;

        }

    }
}