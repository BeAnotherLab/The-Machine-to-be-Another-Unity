using UnityEngine;
using System.Collections;

namespace Byn.Awrtc.Unity
{
    /// <summary>
    /// Provides a range of static functions that help dealing with android specific issues.
    /// Main issue is that android treats the WebRTC audio as a voice cal. There are multiple problems
    /// because of this:
    /// 
    /// * the volume isn't set via "Media" but independently via the "Call volume" slider
    ///     (and unity blocks access to this slider)
    /// 
    /// * The volume is optimized for headphones or the users holding the phone directly onto their ears.
    /// -> Use SetSpeakerOn(true) to turn on the phones speaker for increased volume without headsets
    /// 
    /// 
    /// </summary>
    public class AndroidHelper
    {
        public readonly static string jclass_AndroidVideo = "com.because_why_not.wrtc.AndroidVideo";
        public readonly static string jclass_PermissionHelper = "com.because_why_not.wrtc.PermissionHelper";
        public static bool IsFrontFacing(string deviceName)
        {
            if (string.IsNullOrEmpty(deviceName))
                return false;
            AndroidJavaClass contextClass = new AndroidJavaClass(jclass_AndroidVideo);
            return contextClass.CallStatic<bool>("isFrontFacing", deviceName);
        }
        public static bool IsBackFacing(string deviceName)
        {
            if (string.IsNullOrEmpty(deviceName))
                return false;
            AndroidJavaClass contextClass = new AndroidJavaClass(jclass_AndroidVideo);
            return contextClass.CallStatic<bool>("isBackFacing", deviceName);
        }

        /// <summary>
        /// True switches the phone in speaker mode. This will also happen if headphones are connected.
        /// 
        /// This heavily increases the volume of the webrtc audio but often reduces the quality.
        /// 
        /// WARNING: This value is persistent even after restarting the app!!!
        /// </summary>
        /// <param name="value"></param>
        public static void SetSpeakerOn(bool value)
        {
            AndroidJavaObject audioManager = GetAudioManager();
            audioManager.Call("setSpeakerphoneOn", value);
        }

        /// <summary>
        /// Allows to check if the speakers are currently on.
        /// </summary>
        /// <returns></returns>
        public static bool IsSpeakerOn()
        {
            AndroidJavaObject audioManager = GetAudioManager();
            return audioManager.Call<bool>("isSpeakerphoneOn");
        }


        /// <summary>
        /// Wrapper for AndroidManager.getMode
        /// </summary>
        /// <returns></returns>
        public static int GetMode()
        {
            AndroidJavaObject audioManager = GetAudioManager();
            return audioManager.Call<int>("getMode");
        }

        /// <summary>
        /// Wrapper for AndroidManager.setMode
        /// </summary>
        /// <param name="mode"></param>
        public static void SetMode(int mode)
        {
            AndroidJavaObject audioManager = GetAudioManager();
            audioManager.Call("setMode", mode);
        }

        /// <summary>
        /// Checks if the current mode is set to InCommunication
        /// </summary>
        /// <returns></returns>
        public static bool IsModeInCommunication()
        {
            return GetMode() == GetAudioManagerFlag("MODE_IN_COMMUNICATION");
        }

        /// <summary>
        /// Switches Android to In-Communcation. 
        /// 
        /// This will show the Call Volume bar if the volume buttons are used and
        /// might also change the internal piping of audio within Android.
        /// 
        /// For more info see:
        /// https://developer.android.com/reference/android/media/AudioManager
        /// </summary>
        public static void SetModeInCommunicaion()
        {
            AndroidJavaObject audioManager = GetAudioManager();

            Debug.Log("mode before: " + audioManager.Call<int>("getMode"));
            SetMode(GetAudioManagerFlag("MODE_IN_COMMUNICATION"));
            Debug.Log("mode after: " + audioManager.Call<int>("getMode"));
        }
        public static int GetAudioManagerMode()
        {
            AndroidJavaObject audioManager = GetAudioManager();
            return audioManager.Call<int>("getMode");
        }

        /// <summary>
        /// Default mode in android applications. Call volume bar shouldn't be visible
        /// </summary>
        public static void SetModeNormal()
        {
            AndroidJavaObject audioManager = GetAudioManager();

            Debug.Log("mode before: " + audioManager.Call<int>("getMode"));
            SetMode(GetAudioManagerFlag("MODE_NORMAL"));
            Debug.Log("mode after: " + audioManager.Call<int>("getMode"));
        }

        /// <summary>
        /// Returns the volume level of the voice call / webrtc stream
        /// 
        /// </summary>
        /// <returns></returns>
        public static int GetStreamVolume()
        {
            AndroidJavaObject audioManager = GetAudioManager();
            return audioManager.Call<int>("getStreamVolume", GetAudioManagerFlag("STREAM_VOICE_CALL"));
        }

        /// <summary>
        /// Sets the volume level for the webrtc call / stream
        /// </summary>
        /// <param name="volume"></param>
        public static void SetStreamVolume(int volume)
        {
            AndroidJavaObject audioManager = GetAudioManager();
            audioManager.Call("setStreamVolume",
                GetAudioManagerFlag("STREAM_VOICE_CALL"), volume, GetAudioManagerFlag("FLAG_SHOW_UI") | GetAudioManagerFlag("FLAG_PLAY_SOUND"));
        }


        /// <summary>
        /// Doesn't seem to work yet.
        /// </summary>
        /// <param name="isMute"></param>
        public static void SetMute(bool isMute)
        {
            AndroidJavaObject audioManager = GetAudioManager();

            audioManager.Call("setStreamMute",
                    GetAudioManagerFlag("STREAM_VOICE_CALL"), isMute);
        }
        public static bool IsMute()
        {
            AndroidJavaObject audioManager = GetAudioManager();

            return audioManager.Call<bool>("isStreamMute",
                    GetAudioManagerFlag("STREAM_VOICE_CALL"));
        }

        private static AndroidJavaObject GetAudioManager()
        {
            AndroidJavaObject activity = GetActivity();
            AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext");

            AndroidJavaClass contextClass = new AndroidJavaClass("android.content.Context");
            AndroidJavaObject contextClass_AUDIO_SERVICE = contextClass.GetStatic<AndroidJavaObject>("AUDIO_SERVICE");

            AndroidJavaObject audioManager = context.Call<AndroidJavaObject>("getSystemService", contextClass_AUDIO_SERVICE);
            return audioManager;
        }

        private static AndroidJavaObject GetActivity()
        {
            AndroidJavaClass unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
            return activity;
        }

        private static int GetAudioManagerFlag(string flag)
        {
            AndroidJavaClass audioManagerClass = new AndroidJavaClass("android.media.AudioManager");
            return audioManagerClass.GetStatic<int>(flag);
        }



        public static bool CheckPermissionMicrophone()
        {
            AndroidJavaClass permissionHelper = new AndroidJavaClass(jclass_PermissionHelper);
            return permissionHelper.CallStatic<bool>("CheckPermissionMicrophone", GetActivity());
        }
        public static bool CheckPermissionCamera()
        {
            AndroidJavaClass permissionHelper = new AndroidJavaClass(jclass_PermissionHelper);
            return permissionHelper.CallStatic<bool>("CheckPermissionCamera", GetActivity());
        }
        public static bool CheckPermissionAudioSettings()
        {
            AndroidJavaClass permissionHelper = new AndroidJavaClass(jclass_PermissionHelper);
            return permissionHelper.CallStatic<bool>("CheckPermissionAudioSettings", GetActivity());
        }
        public static bool CheckPermissionNetwork()
        {
            AndroidJavaClass permissionHelper = new AndroidJavaClass(jclass_PermissionHelper);
            return permissionHelper.CallStatic<bool>("CheckPermissionNetwork", GetActivity());
        }

        public static bool HasRuntimePermissions()
        {
            AndroidJavaClass permissionHelper = new AndroidJavaClass(jclass_PermissionHelper);
            return permissionHelper.CallStatic<bool>("HasRuntimePermissions");
        }
        public static void RequestPermissions(bool microphone,
                                                     bool camera,
                                                     bool audioSettings,
                                                     int requestCode)
        {
            AndroidJavaClass permissionHelper = new AndroidJavaClass(jclass_PermissionHelper);
            permissionHelper.CallStatic("RequestPermissions",
                GetActivity(),
                microphone,
                camera,
                audioSettings,
                requestCode);
        }

        public static void OpenPermissionView()
        {
            AndroidJavaClass permissionHelper = new AndroidJavaClass(jclass_PermissionHelper);
            permissionHelper.CallStatic("OpenPermissionView",
                GetActivity());
        }

        /// <summary>
        /// Maps to AudioManager.setBluetoothScoOn
        /// 
        /// </summary>
        /// <param name="value"></param>
        private static void setBluetoothScoOn(bool value)
        {
            AndroidJavaObject audioManager = GetAudioManager();
            audioManager.Call("setBluetoothScoOn", value);
        }
        /// <summary>
        /// Maps to AudioManager.startBluetoothSco
        /// 
        /// </summary>
        /// <param name="value"></param>
        private static void startBluetoothSco()
        {
            AndroidJavaObject audioManager = GetAudioManager();
            audioManager.Call("startBluetoothSco");
        }
        /// <summary>
        /// Maps to AudioManager.stopBluetoothSco
        /// </summary>
        /// <param name="value"></param>
        private static void stopBluetoothSco()
        {
            AndroidJavaObject audioManager = GetAudioManager();
            audioManager.Call("stopBluetoothSco");
        }

        /// <summary>
        /// Gets the status of whether a headset is currently connected.
        /// </summary>
        /// <returns></returns>
        public static bool IsHeadsetOn()
        {
            //submitted by scott. Thanks! :)
            AndroidJavaObject audioManager = GetAudioManager();
            if (audioManager.Call<bool>("isWiredHeadsetOn")
                || audioManager.Call<bool>("isBluetoothA2dpOn"))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Turns on / off routing to a bluetooth speaker.
        /// Works only while in communication mode. If the
        /// mode turns off the results might be device specific.
        /// </summary>
        /// <param name="state">
        /// true - route to bluetooth
        /// false - turn off routing to bluetooth
        /// </param>
        public static void SetBluetoothOn(bool state)
        {
            if(state)
            {
                setBluetoothScoOn(true);
                startBluetoothSco();
            }
            else
            {
                setBluetoothScoOn(false);
                stopBluetoothSco();

            }

        }

    }
}
