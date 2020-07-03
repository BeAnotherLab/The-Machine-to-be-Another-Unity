using Byn.Awrtc;
using Byn.Awrtc.Unity;
using UnityEngine;

namespace Byn.Unity.Examples
{
    /// <summary>
    /// This scenario uses two roles: a receiver and a sender. 
    /// The receiver call object will switch into listening mode and
    /// wait for an incoming call. Once the receiver is fully set 
    /// up a sender call object is created which then establishes a 
    /// direct connection. Once the connection is established you should
    /// be able to hear your own voice being replayed by the speaker. 
    /// 
    /// Both receiver & sender use the ICall interface. This interface 
    /// is the main interface of the library and was designed to allow 
    /// creating one-on-one audio &amp; video calls that work on all 
    /// platforms with only minimal use platform specific code.It is 
    /// also designed to be improved and maintained for a long time 
    /// into the future while new features are being added thus as 
    /// Conference calls.
    /// 
    /// The connection will be established between STEP6 and STEP7. 
    /// Here is what happens during that time:
    /// 1. The receiver is registered using a unique address on the 
    ///    signaling server
    /// 2. The sender is connecting to the signaling server and requests
    ///    to be connected to the address used by the receiver
    /// 3. The signaling server will now connect them indirectly and 
    ///    forward messages between both
    /// 4. sender & receiver will exchange WebRTC specific messages
    ///    containing details about if audio, video is used, what
    ///    data channels are being establishes, the codecs being used
    ///    and multiple IP addresses, port numbers and network protocols
    ///    that might possibly allow the two to connect directly
    ///    (or indirectly if TURN is available)
    /// 5. This step starts already in parallel with 4.
    ///    Sender & receiver will now try to connect using
    ///    the info exchanged. First it will try connections via 
    ///    LAN / WIFI. If this fails it will try to use STUN server to
    ///    open the routers port and connect via internet. If this also
    ///    fails it will try to use a TURN server to relay the data 
    ///    instead of using a direct connection. Note that this
    ///    example doesn't set a stun / turn server.
    /// 6. Once a direct connection is established and all data, audio
    ///    and video channels are ready the call object will report a
    ///    CallAccepted event.
    /// 7. The Update method of the call objects have to keep getting called
    ///    until the call ends.
    ///    The call will keep checking the state of the connection and forward
    ///    possible events that might be triggerd (e.g. CallEnded or
    ///    updated video frames if video is active)
    /// 8. The signaling connections will be cut after a few seconds and the 
    ///    address can be reused before the call finishes. 
    ///    
    /// Please follow the comments in the example to learn more.
    /// </summary>
    public class MinimalCall : MonoBehaviour
    {
        //Sender call object
        ICall sender;
        //Receiver call object
        ICall receiver;

        //Network configuration shared by both
        NetworkConfig netConf;

        //Address used to connect the right sender & receiver
        private string address;

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
            //STEP1: set up our member variables

            //first access to this will boot up the library and create a UnityCallFactory
            //game object which will manage everything in the background.
            if (UnityCallFactory.Instance == null)
            {
                //if it is null something went terribly wrong
                Debug.LogError("UnityCallFactory missing. Platform not supported / dll's missing?");
                return;
            }

            //Use this address to connect. Watch out: If you use a generic 
            //product name and someone else starts the app using the same name
            //you might end up connecting to someone else!
            address = Application.productName + "_MinimalCall";

            //Set signaling server url. This server is used to reserve the address, to find the 
            //other call object, to exchange connection information (ip, port + webrtc specific info)
            //which are then used later to create a direct connection.
            //"callapp" corresponds to a specific configuration on the server side
            //and also acts as a pool of possible users with addresses we can connect to. 
            netConf = new NetworkConfig();
            //e.g.: "ws://signaling.because-why-not.com/test"
            netConf.SignalingUrl = ExampleGlobals.Signaling;

            //possible stun / turn servers. not needed for local test. only for online connections
            //netConf.IceServers.Add(new IceServer("stun.l.google.com:19302"));
            SetupReceiver();
        }

        private void SetupReceiver()
        {
            //STEP2: Setup and create the receiver call
            Debug.Log("receiver setup");

            //receiver doesn't use video and audio
            MediaConfig mediaConf1 = new MediaConfig();
            mediaConf1.Video = false;
            mediaConf1.Audio = false;

            //this creates the receiver 
            receiver = UnityCallFactory.Instance.Create(netConf);

            //register our event handler. This is used to control all
            //further interaction with the call object later
            receiver.CallEvent += Receiver_CallEvent;
            //Ask for permission to access the media
            //(will always work as both is set to false)
            //See Receiver_CallEvent for next step
            receiver.Configure(mediaConf1);
        }

        /// <summary>
        /// Called by Unitys update loop. Will refresh the state of the call, sync the events with
        /// unity and trigger the event callbacks below.
        ///
        /// </summary>
        private void Update()
        {
            if (receiver != null)
                receiver.Update();

            if (sender != null)
                sender.Update();
        }

        /// <summary>
        /// Event handler for the receiver side.
        /// </summary>
        /// <param name="src">receiver mCall object</param>
        /// <param name="args">event specific arguments</param>
        private void Receiver_CallEvent(object src, CallEventArgs args)
        {

            if (args.Type == CallEventType.ConfigurationComplete)
            {
                //STEP3: Connect to the previously set signaling server
                //and try to listen for incoming calls on the set address.
                Debug.Log("receiver configuration done. Listening on address " + address);
                receiver.Listen(address);
            }
            else if (args.Type == CallEventType.WaitForIncomingCall)
            {
                //STEP4A: Our address is registered with the server now
                //we wait for the sender to connect 
                Debug.Log("receiver is ready to accept incoming calls");
                //setup the sender to connect
                SenderSetup();
            }
            else if (args.Type == CallEventType.ListeningFailed)
            {
                //STEP4B: Alternatively, we failed to listen.
                //e.g. due to no internet / server down / address in use
                //currently no specific error information are available.
                Debug.LogError("receiver failed to listen to the address");
            }
            else if (args.Type == CallEventType.CallAccepted)
            {
                //STEP7:
                //The sender connected successfully and a direct connection was
                //created.
                Debug.Log("receiver CallAccepted");
            }
        }

        /// <summary>
        /// Setting up the sender. This is called once the receiver is registered 
        /// at the signaling server and is ready to receive an incoming connection.
        /// </summary>
        private void SenderSetup()
        {
            //STEP5: sending up the sender
            Debug.Log("sender setup");

            sender = UnityCallFactory.Instance.Create(netConf);
            MediaConfig mediaConf2 = new MediaConfig();
            //keep video = false for now to keep the example simple & without UI
            mediaConf2.Video = false;
            //send audio. an echo will be heard to confirm it works
            mediaConf2.Audio = true;
            sender.CallEvent += Sender_CallEvent;

            //Set the configuration. It will trigger an ConfigurationComplete 
            // event once completed or ConnectionFailed if something went wrong.
            //
            //Note: platforms handle this very differently e.g.
            // * Windows & Mac will access the media devices and immediately trigger 
            //  ConfigurationComplete event
            // * iOS and Android might ask the user first for permission
            //   (or crash the app if it isn't allowed to access! Check your 
            //    Unity project setup!)
            // * WebGL behavior is browser specific. Currently, Chrome has a fixed
            //   audio & video device configured and just asks for access while 
            //   firefox lets the user decide which device to use once Configure is 
            //   called.
            // See Receiver_CallEvent for next step
            sender.Configure(mediaConf2);
        }
        private void Sender_CallEvent(object src, CallEventArgs args)
        {
            if (args.Type == CallEventType.ConfigurationComplete)
            {
                //STEP6: we got access to media devices
                Debug.Log("sender configuration done. Listening on address " + address);
                sender.Call(address);
            }
            else if (args.Type == CallEventType.ConfigurationFailed)
            {
                //STEP6: user might have blocked access? 
                Debug.LogError("sender failed to access the audio device");
            }
            else if (args.Type == CallEventType.ConnectionFailed)
            {
                //This can happen if the signaling connection failed or
                //if the direct connection failed e.g. due to firewall
                //See FAQ for more info how to find problems that cause this
                Debug.LogError("sender failed to connect");
            }
            else if (args.Type == CallEventType.CallAccepted)
            {
                //STEP7: Call Accepted
                Debug.Log("sender CallAccepted");
            }
            else if (args.Type == CallEventType.CallEnded)
            {
                //STEP8: CallEnded. Either network died or
                //one of the calls was destroyed/disposed
                Debug.Log("sender received CallEnded event");
            }
        }

        private void OnDestroy()
        {
            //STEP9: GameObject is being destroyed either due to user action, another script or
            //because Unity shuts down -> end the calls + cleanup memory
            //This step is extremely important and not cleaning up might
            //cause stalls or crashes. It will also keep the users webcam active thus
            //preventing other apps from accessing it
            if (receiver != null)
            {
                receiver.Dispose();
                receiver = null;
            }
            if (sender != null)
            {
                sender.Dispose();
                sender = null;
            }
        }
    }
}