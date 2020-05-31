using UnityEngine;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace Byn.Awrtc.Unity
{
    /// <summary>
    /// UnityCallFactory allows to create new ICall objects and will dispose them
    /// automatically when unity shuts down. It will also handle most 
    /// global tasks that aren't related to specific call objects such as
    /// * Initialization (see EnsureInit)
    /// * Listing available video devices
    /// * logging
    /// 
    /// This class acts as a unity specific wrapper that will forward calls
    /// to platform specific implementations depending on which platform
    /// is selected in Unity Build Settings menu. 
    /// </summary>
    public class UnityCallFactory : MonoBehaviour
    {
        /// <summary>
        /// Set to true to allow directly accessing UnityCallFactory.Instance
        /// without calling one of the init methods first. This is available only to
        /// give users time to switch to the new init methods. 
        /// 
        /// If set to false an exception will be triggered if UnityCallFactory.Instance
        /// is accessed before initialization. This helps to ensure that user
        /// code will work on all platforms including those that
        /// do not allow synchronous initialization.
        /// 
        /// In future versions this value will be false by default!
        /// </summary>
        public static readonly bool ALLOW_SYNCHRONOUS_INIT = true;




        /// <summary>
        /// Will switch the Android OS into communication mode. This will
        /// cause let Android know we use voice calls. It should show
        /// the Call volume if the audio buttons are used and might
        /// trigger a general change in how the audio is piped within the OS.
        /// The exact behaviour is specific to each device.
        /// 
        /// Ideally, set this to false and call it yourself before you
        /// start an audio call and turn it off after the call finishes. 
        /// Some external libraries might behave differently if the 
        /// communication mode is active.
        /// 
        /// 
        /// For more info see AndroidHelper.SetCommunicationMode();
        /// 
        /// </summary>
        public static readonly bool ANDROID_SET_COMMUNICATION_MODE = true;

        /// <summary>
        /// Unity / Mono is missing root certificates needed to check if wss or https servers are
        /// trystworthy. By default we have to allow to continue unchecked or we can't use encrypted
        /// connections at all. Not checking the remote certificate means that we aren't protected
        /// against man in the middle attacks but at least the traffic is encrypted. 
        /// This problem doesn't exist in WebGL as the browsers websockets are used instead.
        /// 
        /// Set to false if you require full security for native platforms. You need to install your 
        /// certificate as trustworthy within Mono to make sure the SSL connections don't fail with
        /// an RemoteCertificateNotAvailable and RemoteCertificateChainErrors. Alternatively, you can
        /// add your own check by overwriting following allback:
        /// Byn.Awrtc.Native.WebsocketSharpFactory.CertCallbackOverride 
        /// 
        /// </summary>
        public static readonly bool ALLOW_UNCHECKED_REMOTE_CERTIFICATE = true;



        /// <summary>
        /// Keeps the device from sleeping / turning of the screen
        ///  while calls are active.
        /// 
        /// </summary>
        public static readonly bool KEEP_AWAKE = true;


        /// <summary>
        /// Will automatically toggle on the default logger. 
        /// Keep this on true unless you want to remove the default
        /// logger entirely and implement your own.
        /// </summary>
        public static readonly LogLevel DEFAULT_LOG_LEVEL = LogLevel.Warning;

        /// <summary>
        /// Turns on the internal low level log. This will
        /// heavily impact performance and might cause crashes. 
        /// Log appears in platform specific log output e.g.
        /// xcode on ios, logcat on android, console window for others
        /// Won't appear in the unity log!
        /// </summary>
        public static readonly bool NATIVE_VERBOSE_LOG = false;

        /// <summary>
        /// Keeps track of the original Android audio mode. Will be restored
        /// after the last call finished
        /// 
        /// </summary>
        private int mAndroidPreviousAudioMode;

        /// <summary>
        /// Keeps track of the original Unitys Screen.sleepTimeout value. After
        /// the last call is destroyed it will be reset to this value.
        /// </summary>
        private int mSleepTimeoutBackup;

        /// <summary>
        /// Will be set to true after the first call is created. False
        /// after the last is destroyed.
        /// While true some special flags will be set to optimize for 
        /// active calls.
        /// (native platforms only)
        /// </summary>
        private bool mHasActiveCall = false;

        /// <summary>
        /// Used to keep track of the init process
        /// </summary>
        public enum InitState
        {
            /// <summary>
            /// Singleton not yet initialized and init process not yet started
            /// </summary>
            Uninitialized = 0,
            /// <summary>
            /// Singleton might be created but the init process is not yet completed. Some
            /// features might not work. 
            /// Depending on the platform the user might see a pop up asking for access
            /// to media devices or network.
            /// </summary>
            Initializing = 1,
            /// <summary>
            /// Init process is complete.
            /// </summary>
            Initialized = 2,
            /// <summary>
            /// Init process has failed. See log for detailed information.
            /// </summary>
            Failed = 3,

            /// <summary>
            /// Indicates that the factory is in the process of shutting down and
            /// cleaning up the memory.
            /// 
            /// </summary>
            Disposing = 4,

            /// <summary>
            /// Destroyed
            /// </summary>
            Disposed = 5
        }
        private static InitState sInitState = InitState.Uninitialized;


        /// <summary>
        /// To keep track user scripts callbacks
        /// </summary>
        private class InitCallbacks
        {
            public readonly Action onSuccess;
            public readonly Action<string> onFailure;

            public InitCallbacks(Action success, Action<string> failure)
            {
                onSuccess = success;
                onFailure = failure;
            }
        }

        /// <summary>
        /// Buffers all user script callbacks that try to ensure the init process is completed
        /// via EnsureInit. Will be emptied after init completed or failed
        /// </summary>
        private static List<InitCallbacks> sInitCallbacks = new List<InitCallbacks>();
        /// <summary>
        /// Error message set if the init process failed to inform user scripts that try to access it later.
        /// </summary>
        private static string sInitFailedMessage = "";



        /// <summary>
        /// Set to true once the default logger registered its callback.
        /// Will remain true until shutdown
        /// </summary>
        private static LogLevel sActiveLogLevel = LogLevel.None;


        private static readonly string LOG_PREFIX = typeof(UnityCallFactory).Name + ": ";


        private static UnityCallFactory sInstance;

        public static UnityCallFactory Instance
        {
            get
            {
                //return null if someone tries to access the singleton after its lifetime
                if (sInitState == InitState.Disposing || sInitState == InitState.Disposed)
                {
                    Debug.LogWarning(LOG_PREFIX + " is already destroyed due to shutdown! Returning null.");
                    return null;
                }
                if (sInitState == InitState.Uninitialized)
                {
                    if (ALLOW_SYNCHRONOUS_INIT)
                    {
                        EnsureInit(null, null);
                        if (sInitState == InitState.Initializing)
                        {
                            Debug.LogWarning(LOG_PREFIX + "This platform requires an asynchronous init process. Use UnityCallFactory.EnsureInit to make sure "
                                + " you don't access the factory before the init process is completed!");
                        }
                    }
                    else
                    {
                        //synchronous init is turned off but user tries to access it before calling the init method
                        //This is a user error! Please make sure to call InitAsync before using the singleton!
                        throw new InvalidOperationException("Accessed " + typeof(UnityCallFactory).Name + ".Instance before calling any of the Init methods!");
                    }
                }
                return sInstance;
            }
        }


        private static void AddInitCallbacks(Action onSuccessCallback, Action<string> onFailureCallback)
        {
            sInitCallbacks.Add(new InitCallbacks(onSuccessCallback, onFailureCallback));
        }

        public static void EnsureInit(Action onSuccessCallback)
        {
            EnsureInit(onSuccessCallback, null);
        }
        public static void EnsureInit(Action onSuccessCallback, Action<string> onFailureCallback)
        {

            if (sInitState == InitState.Uninitialized)
            {
                AddInitCallbacks(onSuccessCallback, onFailureCallback);
                Init();
            }
            else if (sInitState == InitState.Initializing)
            {
                AddInitCallbacks(onSuccessCallback, onFailureCallback);
            }
            else if (sInitState == InitState.Initialized)
            {
                //we already completed. inform the user
                TriggerSuccessCallback(onSuccessCallback);
            }
            else if (sInitState == InitState.Failed)
            {
                TriggerFailedCallback(onFailureCallback, sInitFailedMessage);
            }
            else
            {
                TriggerFailedCallback(onFailureCallback, "UnityCallFactory already disposed / destroyed!");
            }

        }


        /// <summary>
        /// Don't call directly. Use EnsureInit instead/
        /// </summary>
        private static void Init()
        {
            if (sInitState != InitState.Uninitialized)
                throw new InvalidOperationException("Init can only be called once!");

            Debug.Log(LOG_PREFIX + "Starting init process for " + Application.platform);
            sInitState = InitState.Initializing;


            CreateSingleton();

            //InitAsync will finish complete here for platforms that support synchronous init
            //but will keep running for async platforms (uwp and webgl) until all dependencies are
            //loaded / access to devices are allowed.
            IEnumerator asyncInit = sInstance.CoroutineInitAsync();
            sInstance.StartCoroutine(asyncInit);

        }


        /// <summary>
        /// Convert exceptions to strings for now as not all WebGL apps support them
        /// </summary>
        /// <param name="e"></param>
        private void TriggerOnInitFailed(Exception e)
        {
            Debug.LogException(e);
            TriggerInitFailed("UnityCallFactory failed with following exception: " + e);
        }
        private static void TriggerInitFailed(string error)
        {
            Debug.LogError(LOG_PREFIX + "triggered TriggerInitFailed. It won't be able to run: " + error);
            sInitState = InitState.Failed;
            sInitFailedMessage = error;
            foreach (var v in sInitCallbacks)
            {
                if (v != null)
                    TriggerFailedCallback(v.onFailure, error);
            }
            sInitCallbacks.Clear();
        }


        private static void TriggerInitSuccess()
        {
            Debug.Log(LOG_PREFIX + "initialized successfully ");
            sInitState = InitState.Initialized;
            foreach (var v in sInitCallbacks)
            {
                if (v != null)
                    TriggerSuccessCallback(v.onSuccess);
            }
            sInitCallbacks.Clear();
        }

        private static void TriggerFailedCallback(Action<string> failedCallback, string error)
        {
            try
            {
                if (failedCallback != null)
                    failedCallback(error);
            }
            catch (Exception e)
            {
                Debug.LogError(LOG_PREFIX + "User code threw exception:");
                Debug.LogException(e);
            }
        }
        private static void TriggerSuccessCallback(Action successCallback)
        {
            try
            {
                if (successCallback != null)
                    successCallback();
            }
            catch (Exception e)
            {
                Debug.LogError(LOG_PREFIX + "User code threw exception:");
                Debug.LogException(e);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private IEnumerator CoroutineInitAsync()
        {
            if (sInitState != InitState.Initializing)
                throw new InvalidOperationException("Expected to be called once during the init process only!");


            //Debug.Log(LOG_PREFIX + "Async init process for " + Application.platform + " state " + sInitState);



            //Some platforms need to load a dynamic library and initialize it properly
            //this is done here.
            bool staticInitRes = false;
            try
            {
                staticInitRes = InitStatic();
            }
            catch (Exception e)
            {
                //This usually happens because files or folders were moved around / renamed / deleted from the project directory
                //Could also be due to invalid project settings
                //might be best to reimport the asset from scratch
                Debug.LogError(LOG_PREFIX + "Exception thrown during static init process. This usually means dynamic libraries are missing!");
                TriggerOnInitFailed(e);
                yield break;
            }
            if (staticInitRes == false)
            {
                //Tried to use it on a platform that isn't supported?
                string error = "Platform specific sync init process failed. UnityCallFactory can't be used. See log message above for details.";
                Debug.LogError(LOG_PREFIX + error);
                TriggerInitFailed(error);
                yield break;
            }

            //Waiting process for asynchronous initialization.
            //this will be skipped immediately on most platforms
            int waitCounter = 0;
            InitStatusUpdate state;
            while ((state = IsInitStaticComplete()) == InitStatusUpdate.Waiting /*|| waitCounter < 5*/)
            {
                if (waitCounter % 10 == 0)
                {
                    Debug.Log(LOG_PREFIX + "is still waiting to complete the init process");
                }
                yield return new WaitForSecondsRealtime(0.1f);
                waitCounter++;
            }

            //error during the asynchronous part?
            if (state == InitStatusUpdate.Error)
            {
                //Tried to use it on a platform that isn't supported?
                string error = "Platform specific async init process failed. UnityCallFactory can't be used. See log message above for details.";
                TriggerInitFailed(error);
                yield break;
            }


            try
            {
                sInstance.InitObj();
            }
            catch (Exception e)
            {
                //This could happen if someone mixed up different versions / files of the asset
                //e.g. the C# code for one version but the native plugin for another
                //or WebRTC failed completely trying to boot up e.g. due to driver errors,
                //OS / security software blocked access to network hardware
                Debug.LogError(LOG_PREFIX + "Failed to create the call factory. ");
                TriggerOnInitFailed(e);
                yield break;
            }
            TriggerInitSuccess();
        }

        private static void CreateSingleton()
        {
            GameObject singleton = new GameObject();
            UnityEngine.Object.DontDestroyOnLoad(singleton);
            singleton.name = typeof(UnityCallFactory).Name;
            sInstance = singleton.AddComponent<UnityCallFactory>();
        }

        /// <summary>
        /// Static init process that will load dynamic libraries and make sure
        /// it can be accessed. This will do the very first call into 
        /// platform specific libraries and might fail if files are 
        /// corrupted or lost or if the user tries to run the application on
        /// platforms that aren't supported.
        /// 
        /// In the future this call might be only the trigger the start of
        /// an asynchonous init process.
        /// </summary>
        /// <returns>Returns true on success.</returns>
        private static bool InitStatic()
        {
            bool successful;

#if !UNITY_WSA && !UNITY_WEBGL
            /*
            If any of the lines below causes an exception you likely are missing
            some important dynamic libraries (either C# dll or platform specific dll/framework files)
            try redownloading the asset and run from a new project
            */
            //If an exception leads you here your C# dll's are missing: 
            var versionCsPlugin = Awrtc.Native.NativeAwrtcFactory.GetVersion();
            //and an exception here means the C++ plugin is missing:
            var versionCppPlugin = Awrtc.Native.NativeAwrtcFactory.GetWrapperVersion();
            var versionWebRtc = Awrtc.Native.NativeAwrtcFactory.GetWebRtcVersion();

            Debug.Log("Version info: [" + versionCsPlugin + "] / [" + versionCppPlugin + "] / [" + versionWebRtc + "]");
#endif

            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                successful = StaticInitBrowser();
            }
            else if (Application.platform == RuntimePlatform.Android)
            {
                successful = StaticInitAndroid();
            }
            else if (IsNativePlatform())
            {
                //the other native platforms automatically load everything needed
                successful = true;

                //turn off the security check for certificates
                //Unity's mono version will always return failed
                //unless the user installs it manually for their specific certificate
                if (ALLOW_UNCHECKED_REMOTE_CERTIFICATE)
                {
#if (!UNITY_WEBGL && !UNITY_WSA) || UNITY_EDITOR
                    Byn.Awrtc.Native.WebsocketSharpFactory.CertCallbackOverride =
                        (object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate,
                        System.Security.Cryptography.X509Certificates.X509Chain chain,
                        System.Net.Security.SslPolicyErrors sslPolicyErrors) =>
                        {
                            //don't warn if no error is returned (never seems to be the case)
                            if (sslPolicyErrors == System.Net.Security.SslPolicyErrors.None)
                                return true;

                            Debug.LogWarning(LOG_PREFIX + "Unity/Mono has no root certificates and is unable to check if certificates are trustworthy. "
                                + "Continuing without check. If you need higher security set UnityCallFactory.ALLOW_UNCHECKED_REMOTE_CERTIFICATE = false and "
                                + " install your certificate in mono or add your own check here.");
                            return true;
                        };
#endif
                }
            }
            else if (Application.platform == RuntimePlatform.WSAPlayerX86)
            {
#if UNITY_WSA && !UNITY_EDITOR
                Byn.Awrtc.Uwp.RtcGlobal.Init();
#endif
                //this one is work in progress. 
                successful = true;
            }
            else
            {
                //fail
                successful = false;
                Debug.LogError(LOG_PREFIX + "Platform not supported: " + Application.platform);
            }
            return successful;
        }

        private enum InitStatusUpdate
        {
            Waiting,
            Initialized,
            Error
        }

        private static InitStatusUpdate IsInitStaticComplete()
        {
            if (Application.platform == RuntimePlatform.WSAPlayerX86)
            {
#if UNITY_WSA && !UNITY_EDITOR
                if(Byn.Awrtc.Uwp.RtcGlobal.InitState == Byn.Awrtc.Uwp.RtcGlobal.State.Initialized)
                {
                    Debug.Log(LOG_PREFIX + "WSA plugin Initialized ...");
                    return InitStatusUpdate.Initialized;
                }
                else if (Byn.Awrtc.Uwp.RtcGlobal.InitState == Byn.Awrtc.Uwp.RtcGlobal.State.Initializing
                || Byn.Awrtc.Uwp.RtcGlobal.InitState == Byn.Awrtc.Uwp.RtcGlobal.State.Uninitialized)
                {
                    Debug.Log(LOG_PREFIX + "WSA plugin is still initializing ...");
                    return InitStatusUpdate.Waiting;
                }
                else
                {
                    //either failed to start or resulted in an error
                    Debug.LogError(LOG_PREFIX + "WSA plugin failed to initialize. See console for more information.");
                    return InitStatusUpdate.Error;
                }
#else
                return InitStatusUpdate.Error;
#endif
            }
            else if(Application.platform == RuntimePlatform.WebGLPlayer)
            {
#if UNITY_WEBGL
                var status = Byn.Awrtc.Browser.CAPI.Unity_PollInitState();
                if(status == Browser.CAPI.InitState.Initializing)
                {
                    return InitStatusUpdate.Waiting;
                }else if(status == Browser.CAPI.InitState.Initialized)
                {
                    return InitStatusUpdate.Initialized;
                }
                Debug.LogError("WebGL plugin returned " + status);
#endif
                return InitStatusUpdate.Error;
            }else
            {
                //all other platforms use still static init
                //meaning if we got so far it is working
                return InitStatusUpdate.Initialized;
            }
        }

        /// <summary>
        /// Initializes the singleton object itself. Will be called after
        /// the static init completed successfully. 
        /// </summary>
        private void InitObj()
        {
            //TODO: at the moment we are missing the static init process
            //This should be called right after the all dynamic libraries
            // / platform specific libraries are loaded but before the
            // Unity side init
            if(DEFAULT_LOG_LEVEL != LogLevel.None)
            {
                RequestLogLevel(DEFAULT_LOG_LEVEL);
            }

#if UNITY_WEBGL && !UNITY_EDITOR
        
            mFactory = new Byn.Awrtc.Browser.BrowserCallFactory();
            
#elif UNITY_WSA && !UNITY_EDITOR
            
            mFactory = new Byn.Awrtc.Uwp.UwpCallFactory();
#else//native platforms only


            if (NATIVE_VERBOSE_LOG)
            {
                Native.NativeAwrtcFactory.SetNativeLogLevel(WebRtcCSharp.LoggingSeverity.LS_INFO);
                //this will route the log via SLog class to the Unity log. This can cause crashes
                //and doesn't work on all platforms
                //Awrtc.Native.NativeWebRtcCallFactory.SetNativeLogToSLog(WebRtcCSharp.LoggingSeverity.LS_NONE);
            }
            else
            {
                Native.NativeAwrtcFactory.SetNativeLogLevel(WebRtcCSharp.LoggingSeverity.LS_NONE);
            }
            //This switches to the old cricket video capturers. They are declared obsolete but
            //in some cases we might want to still use it:
            //WebRtcCSharp.RTCPeerConnectionFactory.SetObosoleteVideo(true);
            var factory = new Native.NativeWebRtcCallFactory();
            factory.LastCallDisposed += LeaveActiveCallState;
            mFactory = factory;


#if UNITY_IOS
            //workaround for WebRTC / Unity audio bug on ios
            //WebRTC will deactivate the audio session once all calls ended
            //This will keep it active as Unity relies on this session as well
            WebRtcCSharp.IosHelper.InitAudioLayer();
            WebRtcCSharp.IosHelper.IosKeepAudioActive(true);
#endif

#endif


        }


        /// <summary>
        /// Unity will call this during shutdown. It will make sure all ICall objects and the factory
        /// itself will be destroyed properly.
        /// </summary>
        protected void OnDestroy()
        {
            Dispose();
        }




        private IAwrtcFactory mFactory = null;
        /// <summary>
        /// Do not use. For debugging only.
        /// </summary>
        public IAwrtcFactory InternalFactory
        {
            get
            {
                return mFactory;
            }
        }


#if UNITY_EDITOR || (!UNITY_WEBGL && !UNITY_WSA)
        public Native.NativeVideoInput VideoInput
        {
            get
            {
                if (mFactory != null)
                {
                    var factory = mFactory as Native.NativeWebRtcCallFactory;
                    return factory.VideoInput;
                }
                return null;
            }
        }
#endif




        private void Awake()
        {
            //to ensure that the object isn't created manually
            if (sInitState != InitState.Initializing)
            {
                Debug.LogError(LOG_PREFIX + "Attempted to create the script UnityCallFactory manually."
                               + " This is not possible. Use UnityCallFactory.Init methods to create it!");
                Destroy(this);
            }


        }

        private void Start()
        {

        }


        private void Update()
        {

        }


        /// <summary>
        /// Check if this platform uses the bundled native c++ plugin
        /// </summary>
        /// <returns></returns>
        private static bool IsNativePlatform()
        {
            //do not access any platform specific code here
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            return true;
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            return true;
#elif UNITY_ANDROID
            return true;
#elif UNITY_IOS
			return true;
#else
            return false;
#endif
        }

        private static bool StaticInitBrowser()
        {
#if UNITY_WEBGL
            Debug.Log(LOG_PREFIX + "Initialize browser specific plugin");
            //check if the java script part is available
            if (Byn.Awrtc.Browser.BrowserCallFactory.IsAvailable() == false)
            {
                //plugin is now loaded via jspre. if we end up here
                //the awrtc.jslib has been excluded from the build
                Debug.LogError(LOG_PREFIX + "Failed to load the java script plugin. Make sure awrtc.jspre and jslib are included in the build. ");
                return false;
                //Old dynamic system for loading the plugin. Replaced by jspre
                /*
                    Debug.Log("Plugin not yet loaded.");
                    //js part is missing -> inject the code into the browser
                    Byn.Awrtc.Browser.BrowserCallFactory.InjectJsCode();
                    bool successful = Byn.Awrtc.Browser.BrowserCallFactory.IsAvailable();
                    if (successful == false)
                    {
                        Debug.LogError("Failed to access the java script library. This might be because of browser incompatibility or a missing java script plugin!");
                    }
                    return successful;
                */
            }
            //check if the browser side has the neccessary features to run this
            if (Byn.Awrtc.Browser.CAPI.Unity_WebRtcNetwork_IsBrowserSupported() == false)
            {
                Debug.LogError(LOG_PREFIX + "Browser is missing some essential WebRTC features. Try a modern version of Chrome or Firefox.");
                return false;
            }

            if (Byn.Awrtc.Browser.CAPI.Unity_MediaNetwork_HasUserMedia() == false)
            {
                //Please send your angry emails directly to Google. 
                Debug.LogWarning("Browser does not allow access to UserMedia. Local camera & microphone won't work." 
                    + "Some browsers (Chrome) only allow access to user media if the page is loaded using HTTPS / WSS only!");
            }

            Byn.Awrtc.Browser.CAPI.Unity_InitAsync(Browser.CAPI.InitMode.WaitForDevices);

            return true;
#else
            return false;
#endif
        }

        private static bool StaticInitAndroid()
        {
#if UNITY_ANDROID
            Debug.Log(LOG_PREFIX + "Initialize android specific plugin");
            bool successful;
            try
            {
                //get context from unity activity
                AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext");

                //get the java plugin
                AndroidJavaClass wrtc = new AndroidJavaClass("com.because_why_not.wrtc.WrtcAndroid");
                //Debug.Log("Using context " + context.GetHashCode());
                //initialize the java side
                bool javaSideSuccessful;
                javaSideSuccessful = wrtc.CallStatic<bool>("init", context);
                //2019-05-22: Any V0.984 and higher after this date won't need the workaround below.
                //Pre 2019-05-22: Workaround for Bug0117: If Application.Quit is used unity destroys the C# side
                //but java/C++ side remains initialized causing an error during the next init process.
                //In this case the error can be ignored with the side effect
                //that a real error might be ignored as well:
                //if(javaSideSuccessful == false)
                //{
                //    Debug.LogWarning("WORKAROUND to avoid problems with Application.Quit. Error of the java library ignored");
                //    javaSideSuccessful = true;
                //}
                if (javaSideSuccessful)
                {
                    Debug.Log(LOG_PREFIX + "android java plugin initialized");
                }
                else
                {
                    Debug.LogError(LOG_PREFIX + "Failed to initialize android java plugin. See android logcat output for error information!");
                    return false;
                }

                //initialize the c++ side
                bool cppSideSuccessful = WebRtcCSharp.RTCPeerConnectionFactory.InitAndroidContext(context.GetRawObject());
                if (cppSideSuccessful)
                {
                    Debug.Log(LOG_PREFIX + "android cpp plugin successful initialized.");
                }
                else
                {
                    Debug.LogError(LOG_PREFIX + "Failed to initialize android native plugin. See android logcat output for error information!");
                }
                successful = true;
            }
            catch (Exception e)
            {
                successful = false;
                Debug.LogError(LOG_PREFIX + "Android specific init process failed due to an exception.");
                Debug.LogException(e);
            }

            return successful;
#else
            return false;
#endif
        }

        /// <summary>
        /// Creates a new ICall object.
        /// Only use this method to ensure that your software will keep working on other platforms supported in 
        /// future versions of this library.
        /// </summary>
        /// <param name="config">Network configuration</param>
        /// <returns></returns>
        public ICall Create(NetworkConfig config = null)
        {
            ICall call = mFactory.CreateCall(config);
            if (call == null)
            {
                Debug.LogError("Creation of call object failed. Platform not supported? Platform specific dll not included?");
            }
            else
            {

                if (IsNativePlatform())
                {
                    EnterActiveCallState();
                }
            }
            return call;
        }
        public IMediaNetwork CreateMediaNetwork(NetworkConfig config)
        {
            return mFactory.CreateMediaNetwork(config);
        }

        public IWebRtcNetwork CreateBasicNetwork(string websocketUrl, IceServer[] iceServers = null)
        {
            return mFactory.CreateBasicNetwork(websocketUrl, iceServers);
        }

        /// <summary>
        /// Returns a list containing the names of all available video devices. 
        /// 
        /// They can be used to select a certian device using the class
        /// MediaConfiguration and the method ICall.Configuration.
        /// </summary>
        /// <returns>Returns a list of video devices </returns>
        public string[] GetVideoDevices()
        {
            if (mFactory != null)
                return mFactory.GetVideoDevices();
            return new string[] { };
        }

        /// <summary>
        /// True if the video device can be chosen by the application. False if the environment (the browser usually)
        /// will automatically choose a suitable device.
        /// </summary>
        /// <returns></returns>
        public bool CanSelectVideoDevice()
        {
            if (mFactory != null)
            {
                return mFactory.CanSelectVideoDevice();
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Can be used if the given device name is considered a front facing video device.
        /// Result will be false if backfacing or unknown.
        /// </summary>
        /// <returns>
        /// True for front facing. False for backfacing or unknown
        /// </returns>
        /// <param name="deviceName">Device name.</param>
        public bool IsFrontFacing(string deviceName)
        {

            if (Application.platform == RuntimePlatform.Android)
            {
                //On Android we need to use a custom implementation.
                //WebRTC might use a different camera API than unity thus the naming
                //can be different
                foreach (var dev in UnityCallFactory.Instance.GetVideoDevices())
                {
                    if (deviceName == dev)
                    {
                        if (AndroidHelper.IsFrontFacing(dev))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                foreach (var dev in WebCamTexture.devices)
                {
                    if (deviceName == dev.name)
                    {
                        if (dev.isFrontFacing)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            //unknown video device
            return false;
        }
        /// <summary>
        /// This method will try to guess the default device. For mobile devices this is the front facing
        /// camera and for others it is the first camera in the device list.
        /// In some cases it might return "" which will leave the decision which device to pick to the
        /// platform specific layer or the platform itself (e.g. based on users browser configuration).
        /// </summary>
        /// <returns>Returns a device name or "" if none could be determined.</returns>
        public string GetDefaultVideoDevice()
        {
            if (CanSelectVideoDevice())
            {
                var devices = GetVideoDevices();
                if (devices != null && devices.Length > 0)
                {
                    //return a front facing one
                    foreach (var dev in devices)
                    {
                        if (IsFrontFacing(dev))
                        {
                            //found a front facing device
                            return dev;
                        }
                    }
                    //failed to find a front facing device
                    //return the first
                    return devices[0];
                }
            }
            //user selection not supported
            //or no device found
            return "";

        }


        /// <summary>
        /// Used to switch Unity and the platform into a state more suitable
        /// for audio / video streaming
        /// 
        /// So far it will:
        /// * prevent the device from sleeping
        /// * switch Android into communication mode
        /// 
        /// Only supported for native platforms.
        /// Further updates might add more. 
        /// </summary>
        private void EnterActiveCallState()
        {
            if (mHasActiveCall == false)
            {
                if (KEEP_AWAKE)
                {
                    mSleepTimeoutBackup = Screen.sleepTimeout;
                    Screen.sleepTimeout = SleepTimeout.NeverSleep;
                }
#if UNITY_ANDROID && !UNITY_EDITOR
                if (ANDROID_SET_COMMUNICATION_MODE)
                {
                    Debug.Log(LOG_PREFIX + "Switching android audio mode to MODE_IN_COMMUNICATION");
                    mAndroidPreviousAudioMode = AndroidHelper.GetMode();
                    AndroidHelper.SetModeInCommunicaion();
                }
#endif
            }
            mHasActiveCall = true;
        }

        /// <summary>
        /// Cleanup after all calls finished.
        /// Called by the native factories LastCallDisposed handler.
        /// </summary>
        private void LeaveActiveCallState()
        {
            if (mHasActiveCall)
            {
                if (KEEP_AWAKE)
                {
                    Screen.sleepTimeout = mSleepTimeoutBackup;
                }

#if UNITY_ANDROID && !UNITY_EDITOR
                if (Application.platform == RuntimePlatform.Android)
                {
                    if (ANDROID_SET_COMMUNICATION_MODE)
                    {
                        Debug.Log(LOG_PREFIX + "Restore privous audio mode: " + mAndroidPreviousAudioMode);
                        AndroidHelper.SetMode(mAndroidPreviousAudioMode);
                    }
                }
#endif
            }
            mHasActiveCall = false;
        }


        /// <summary>
        /// Destroys the factory.
        /// </summary>
        public void Dispose()
        {

            //Watch out this call might also be triggered if a user manually created an object
            //without following the proper init process
            //check first if this is actually the singleton
            if (sInstance == this)
            {
                sInitState = InitState.Disposing;

                //Always log this. Disposing the internal factory will trigger a chain reaction of
                //destroying every single ICall, IMediaNetwork and IBasicNetwork instance and 
                //doing the same with its platform specific components. Native platforms will have
                //to wait for multiple threads to cleanup their memory before stopping them and
                //returning from the call. Plenty of space for bugs and crashes.
                if (mFactory != null)
                {
                    Debug.Log(LOG_PREFIX + "is being destroyed. All created calls will be destroyed as well!");
                    mFactory.Dispose();
                    mFactory = null;
                    Debug.Log(LOG_PREFIX + "destroyed.");
                }
                sInitState = InitState.Disposed;
            }
        }



        /// <summary>
        /// Mobile native only:
        /// Turns on/off the phones speaker.
        /// 
        /// </summary>
        /// <param name="state"></param>
        public void SetLoudspeakerStatus(bool state)
        {
#if UNITY_IOS
            WebRtcCSharp.IosHelper.SetLoudspeakerStatus(state);
#elif UNITY_ANDROID
            //see AndroidHelper for more detailed control over android specific audio settings
            AndroidHelper.SetSpeakerOn(state);
#else

            Debug.LogError(LOG_PREFIX + "GetLoudspeakerStatus is only supported on mobile platforms.");
#endif
        }
        /// <summary>
        /// Checks if the phones speaker is turned on. Only for mobile native platforms
        /// </summary>
        /// <returns></returns>
        public bool GetLoudspeakerStatus()
        {
#if UNITY_IOS
            return WebRtcCSharp.IosHelper.GetLoudspeakerStatus();
#elif UNITY_ANDROID
            //android will just crash if GetLoudspeakerStatus is used
            //workaround via java
            return AndroidHelper.IsSpeakerOn();
#else
            Debug.LogError(LOG_PREFIX + "GetLoudspeakerStatus is only supported on mobile platforms.");
            return false;
#endif
        }



        /// <summary>
        /// Will route the log messages from the C# side plugin's SLog class to
        /// the Unity log. This method will only change the log level if 
        /// a more detailed log level is requested than privously. 
        /// This is to allow multiple components that run in parallel to use this
        /// call without overwriting each others logging.
        /// 
        /// You could also make sure the log level in this class remains at
        /// None and instead implement your own logger using the class SLog.
        /// 
        /// There are some special cases due to platform specific code:
        /// 
        /// WebGL: The browser plugin code won't log trough the unity logging
        /// system and instead use the browser API directly. The log level will
        /// still be changed by this call but it won't use the "OnLog" callback
        /// 
        /// Native: By default the C++ side won't be logged via Unity as
        /// Unity can get buggy by triggering callbacks from parallel native
        /// threads.
        /// You can activate c++ side logging via the flag NATIVE_VERBOSE_LOG.
        /// Output of the native log is done based on the platform and 
        /// the OnLog callback is not used.
        /// (e.g. Xcode debug console, Android LogCat, visual studio debug console, ...)
        /// <param name="requestedLevel">Requested level log level.
        /// Will be ignored if the current log level is more detailed already.
        /// </param>
        /// </summary>
        public void RequestLogLevel(LogLevel requestedLevel)
        {
            if(sActiveLogLevel < requestedLevel)
            {
                Debug.Log("Changing log level to " + requestedLevel);
                if(sActiveLogLevel == LogLevel.None)
                {
                    //the log level was off completely -> register callback
                    //Do this only once to avoid overwriting a custom logger
                    //that might have been set after this call.
                    SLog.SetLogger(OnLog);
                }
                sActiveLogLevel = requestedLevel;
            }
#if UNITY_WEBGL && !UNITY_EDITOR
            UpdateLogLevel_WebGl();

#endif
        }

        /// <summary>
        /// Obsolete.
        /// Unlike the original version this call won't reduce the log level
        /// anymore. If just a single call requests verbose logging it 
        /// will stay active until the application shuts down.
        /// This is to make sure multiple components don't overwrite each
        /// others log level.
        /// 
        /// </summary>
        /// <param name="requireLogging"> true to turn on the default logger within
        /// UnityCallFactory. Set false if your component doesn't requre any
        /// logging.</param>
        /// <param name="requireVerboseLogging">true if your component requires verbose logging. 
        /// False if it doesn't.</param>
        [Obsolete("Use RequestLogLeve instead!")]
        public void SetDefaultLogger(bool requireLogging, 
                                     bool requireVerboseLogging = false)
        {
            if(requireLogging)
            {
                RequestLogLevel(LogLevel.Warning);
            }
            if (requireVerboseLogging)
            {
                RequestLogLevel(LogLevel.Info);
            }

        }

        /// <summary>
        /// Call to the java script WebGL plugin to update its logging 
        /// policity.
        /// </summary>
        private void UpdateLogLevel_WebGl()
        {
#if UNITY_WEBGL
            int logLevel = (int)sActiveLogLevel;
            Browser.CAPI.Unity_SLog_SetLogLevel(logLevel);
#endif
        }


        /// <summary>
        /// Default logging callback.
        /// </summary>
        /// <param name="msg">Message.</param>
        /// <param name="tags">Tags.</param>
        private static void OnLog(object msg, string[] tags)
        {
            if (sActiveLogLevel == LogLevel.None)
                return;

            StringBuilder builder = new StringBuilder();
            bool warning = false;
            bool error = false;
            builder.Append("TAGS:[");
            foreach (var v in tags)
            {
                builder.Append(v);
                builder.Append(",");
                if (v == SLog.TAG_ERROR || v == SLog.TAG_EXCEPTION)
                {
                    error = true;
                }
                else if (v == SLog.TAG_WARNING)
                {
                    warning = true;
                }
            }
            builder.Append("]");
            builder.Append(msg);
            if (error && sActiveLogLevel >= LogLevel.Error)
            {
                UnityLog(builder.ToString(), Debug.LogError);
            }
            else if (warning && sActiveLogLevel >= LogLevel.Warning)
            {
                UnityLog(builder.ToString(), Debug.LogWarning);
            }
            else if(sActiveLogLevel >= LogLevel.Info)
            {
                UnityLog(builder.ToString(), Debug.Log);
            }
        }


        private static void UnityLog(string s, Action<object> logcall)
        {
            if (s.Length > 2048 && Application.platform != RuntimePlatform.Android)
            {
                foreach (string splitMsg in SplitLongMsgs(s))
                {
                    logcall(splitMsg);
                }
            }
            else
            {
                logcall(s);
            }
        }

        /// <summary>
        /// Used internally to split up long log messages.
        /// This is to avoid having the messages being cut off
        /// by platforms like Android.
        /// </summary>
        /// <returns>The long msgs.</returns>
        /// <param name="s">S.</param>
        private static string[] SplitLongMsgs(string s)
        {
            const int maxLength = 2048;
            int count = s.Length / maxLength + 1;
            string[] messages = new string[count];
            for (int i = 0; i < count; i++)
            {
                int start = i * maxLength;
                int length = s.Length - start;
                if (length > maxLength)
                    length = maxLength;
                messages[i] = "[" + (i + 1) + "/" + count + "]" + s.Substring(start, length);

            }
            return messages;
        }



        /// <summary>
        /// Log level used to decide which internal logs to print to the
        /// unity logger. 
        /// </summary>
        public enum LogLevel
        {
            /// <summary>
            /// Logger turned off
            /// </summary>
            None = 0,
            /// <summary>
            /// Errors only
            /// </summary>
            Error = 1,
            /// <summary>
            /// Errors and warnings
            /// </summary>
            Warning = 2,
            /// <summary>
            /// Logs all log messages. This also
            /// includes Debug log messages if a debug build is used.
            /// </summary>
            Info = 3
        }
    }
}