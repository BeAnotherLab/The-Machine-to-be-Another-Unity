using Byn.Awrtc;
using Byn.Awrtc.Unity;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Byn.Unity.Examples
{
    /// <summary>
    /// This app is a more complex and realistic version of the MinimalCall example.
    /// Only code not yet mentioned in an earlier example is commented.
    /// It requires two game objects that have both the SimpleCall script attached. 
    /// One is set as sender (Sender ticked) and one set as receiver. 
    /// 
    /// Unlike the MinimalCall it uses a stun server thus it is able to connect via internet.
    /// (assuming router supports it and no filewalls blocks it)
    /// It can also be used with a Unity RawFrame UI element to show the local or remote video.
    /// Note that the video needs to be flipped (Y Scale -1) as Unity stores video upside down
    /// internally and the library avoid flipping it via the CPU to increase performance.
    /// 
    /// You can use this example in two different ways:
    /// * For easy testing make sure to place both sender & receiver gameobject in a single scene.
    ///   During start the receiver will automatically start and wait for an incoming call and
    ///   after a few seconds the sender will start and then connect
    /// * You can also place them in two different scenes and use two different systems. First one 
    ///   starts the scene with the receiver. Then the other can start the scene with the sender.
    ///   They will then connect via network. If you start this on two different systems that
    ///   aren't in the same LAN note that in some cases the firewall or router configuration
    ///   can block a connection. 
    ///   Make sure if you use two projects they have the same name as this is used as an address
    ///   for the two applications to find each other. 
    ///   
    /// </summary>
    public class SimpleCall : MonoBehaviour
    {
        /// <summary>
        /// As part of this scenario and to simplify local testing one SimpleCall needs to be
        /// sender and one needs to be receiver (set via UnityEditor)
        /// </summary>
        public bool _Sender = false;
        public RawImage _LocalImage;
        public RawImage _RemoteImage;
        //Not yet stable on all platforms but coming soon.
        private bool _TryI420 = false;



        /// <summary>
        /// A specific state during this example
        /// </summary>
        public enum SimpleCallState
        {
            Invalid,
            Config,
            Calling,
            InCall,
            Ended,
            Error
        }

        /// <summary>
        /// Used to keep track of the current state for error messages / user info
        /// </summary>
        SimpleCallState mState = SimpleCallState.Invalid;

        /// <summary>
        /// Interface representing a single call.
        /// </summary>
        private ICall mCall;


        void Start()
        {
            StartCoroutine(ExampleGlobals.RequestPermissions());
            UnityCallFactory.EnsureInit(OnCallFactoryReady, OnCallFactoryFailed);
        }

        void OnCallFactoryReady()
        {
            UnityCallFactory.Instance.RequestLogLevel(UnityCallFactory.LogLevel.Info);
            InitExample();
        }

        void OnCallFactoryFailed(string error)
        {
            string fullErrorMsg = typeof(CallApp).Name + " can't start. The " + typeof(UnityCallFactory).Name + " failed to initialize with following error: " + error;
            Debug.LogError(fullErrorMsg);
        }

        void InitExample()
        {
            //STEP1: setup
            Log("Starting SimpleCall example");

            NetworkConfig netConf = new NetworkConfig();
            netConf.SignalingUrl = "ws://signaling.because-why-not.com/callapp";

            //Set a stun server as ice server. We use a free google stun
            //server here. (blocked in China)
            //This is used by WebRTC to open a port in your router to allow 
            //incoming connections. (not all routers support this though and
            //some firewalls block it)
            netConf.IceServers.Add(new IceServer("stun:stun.l.google.com:19302"));

            mCall = UnityCallFactory.Instance.Create(netConf);
            if (mCall == null)
            {
                //this might happen if our configuration is invalid e.g. broken stun server url
                //(it won't notice if the stun server is offline though)
                Error("Call init failed");
                return;
            }
            Log("Call object created");

            mCall.CallEvent += Call_CallEvent;

            //Setup is completed. Now set media configuration
            if (_Sender)
            {
                //the sender side waits 5 seconds to give the receiver
                //time to register the address online and wait for the connection
                StartCoroutine(ConfigureDelayed(5));
            }
            else
            {
                //Receiver starts immediately
                Configure();
            }
        }

        /// <summary>
        /// This is setting the media used for this call.
        /// </summary>
        public void Configure()
        {
            //STEP2: configure media devices
            MediaConfig mediaConfig = new MediaConfig();
            if (_Sender)
            {
                //sender is sending audio and video
                mediaConfig.Audio = true;
                mediaConfig.Video = true;

                //We ask for 320x240 (should work fine
                //even on the weakest systems)
                //note that not all devices can actually
                //deliver the resolution we ask for
                //mediaConfig.IdealWidth = 320;
                //mediaConfig.IdealHeight = 240;
                mediaConfig.IdealWidth = 1280;
                mediaConfig.IdealHeight = 720;

            }
            else
            {
                //set to false to avoid 
                //echo & multiple calls trying to access the same camera
                mediaConfig.Audio = false;
                mediaConfig.Video = false;
            }


            if (_TryI420)
            {
                mediaConfig.Format = FramePixelFormat.I420p;
            }
            mCall.Configure(mediaConfig);
            mState = SimpleCallState.Config;
            Log("Configure media devices");
        }

        /// <summary>
        /// Used for running the example automatically. 
        /// The receiver needs some time to register its address on the server
        /// first before the sender can connect.
        /// </summary>
        /// <param name="startInSec">time in seconds</param>
        /// <returns>Unity coroutine</returns>
        private IEnumerator ConfigureDelayed(float startInSec)
        {
            yield return new WaitForSeconds(startInSec);
            Configure();
        }

        void Update()
        {
            //some platforms might take a few frames to initialize
            //Make sure to check first if mCall is initialized.
            if(mCall != null)
            {
                mCall.Update();
            }
        }

        private void OnDestroy()
        {
            Cleanup();
        }

        public void Cleanup()
        {
            if (mCall != null)
            {
                mCall.Dispose();
                mCall = null;
            }
        }

        private void Call_CallEvent(object sender, CallEventArgs args)
        {
            if (args.Type == CallEventType.ConfigurationComplete)
            {
                //STEP3: configuration completed -> try calling
                Call();
            }
            else if (args.Type == CallEventType.ConfigurationFailed)
            {
                Error("Accessing audio / video failed");
            }
            else if (args.Type == CallEventType.ConnectionFailed)
            {
                Error("ConnectionFailed");
            }
            else if (args.Type == CallEventType.ListeningFailed)
            {
                Error("ListeningFailed");
            }
            else if (args.Type == CallEventType.CallAccepted)
            {
                //STEP5: We are connected
                mState = SimpleCallState.InCall;
                Log("Connection established");
            }
            else if (args.Type == CallEventType.CallEnded)
            {
                mState = SimpleCallState.Ended;
                Log("Call ended.");
            }
            else if (args.Type == CallEventType.FrameUpdate)
            {
                //STEP6: until the end of the call we receive frames here
                //Note that this is being called after Configure already for local frames even before
                //a connection is established!
                //This is triggered each video frame for local and remote video images
                FrameUpdateEventArgs frameArgs = args as FrameUpdateEventArgs;


                if (frameArgs.ConnectionId == ConnectionId.INVALID)
                {
                    bool textureCreated = UnityMediaHelper.UpdateRawImage(_LocalImage, frameArgs.Frame);
                    if (textureCreated)
                    {
                        Texture2D tex = _LocalImage.texture as Texture2D;
                        Log("Local Texture(s) created " + tex.width + "x" + tex.height + " format: " + frameArgs.Frame.Format);
                    }

                }
                else
                {
                    bool textureCreated = UnityMediaHelper.UpdateRawImage(_RemoteImage, frameArgs.Frame);
                    if (textureCreated)
                    {
                        Texture2D tex = _RemoteImage.texture as Texture2D;
                        Log("Remote Texture(s) created " + tex.width + "x" + tex.height + " format: " + frameArgs.Frame.Format);
                    }
                }
            }
        }

        private void Call()
        {
            string address = Application.productName + "_SimpleCall";

            if (_Sender)
            {
                //STEP4: Sender calls (outgoing connection) 
                mCall.Call(address);
            }
            else
            {
                //STEP4: Receiver listens (waiting for incoming connection)
                mCall.Listen(address);
            }
            mState = SimpleCallState.Calling;
        }

        private void Error(string errormsg)
        {
            if (_Sender)
            {
                Debug.LogError("Sender: Error during state " + mState + ": " + errormsg);
            }
            else
            {
                Debug.LogError("Receiver: Error during state " + mState + ": " + errormsg);
            }
            mState = SimpleCallState.Error;
        }

        private void Log(string msg)
        {
            if (_Sender)
            {
                Debug.Log("Sender: " + msg);
            }
            else
            {
                Debug.Log("Receiver: " + msg);
            }
        }
    }
}