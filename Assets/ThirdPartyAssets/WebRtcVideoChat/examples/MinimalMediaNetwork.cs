using Byn.Awrtc;
using Byn.Awrtc.Unity;
using UnityEngine;

namespace Byn.Unity.Examples
{
    /// <summary>
    /// This example results in the same behavior as MinimalCall but
    /// uses instead the MinimalMediaNetwork interface.
    /// While the ICall interface isn't yet completed you can use the 
    /// IMediaNetwork interface to gain access to some additional features 
    /// such as sending byte[] reliable / unreliable and creating custom 
    /// typologies. 
    /// IMediaNetwork is not aware of network topologies at all and is simply
    /// a collection of peer connections. StartServer will allocate an address 
    /// via the Signaling Server and thus allow incoming connections and 
    /// Connect() will create a new outgoing connection. 
    /// MediaConfig will be set for all peers e.g. if video and audio is 
    /// set to active it will stream it to all incoming and outgoing connections.
    /// 
    /// You can IMediaNetwork for example to create a server / client structure 
    /// where one user streams to multiple users or for conference calls where
    /// each connection can be controlled manually. 
    /// 
    /// Note that IMediaNetwork is mainly for internal purposes. It might
    /// behave differently on different platforms and change in further updates.
    /// </summary>
    public class MinimalMediaNetwork : MonoBehaviour
    {
        IMediaNetwork sender;
        private bool mSenderConfigured = false;
        IMediaNetwork receiver;
        private bool mReceiverConfigured = false;

        private NetworkConfig netConf;
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
            //STEP1: instance setup
            address = Application.productName + "_MinimalMediaNetwork";

            netConf = new NetworkConfig();
            netConf.SignalingUrl = ExampleGlobals.Signaling;
            netConf.IceServers.Add(new IceServer(ExampleGlobals.StunUrl));
            SetupReceiver();
        }

        private void SetupReceiver()
        {
            //STEP2: Setup the receiver. See UpdateReceiver() for event handling
            Debug.Log("receiver setup");
            MediaConfig mediaConf1 = new MediaConfig();
            //first one only receives
            mediaConf1.Video = false;
            mediaConf1.Audio = false;

            receiver = UnityCallFactory.Instance.CreateMediaNetwork(netConf);
            receiver.Configure(mediaConf1);
        }

        private void UpdateReceiver()
        {
            //STEP3: Updating the receiver. Will be called ever frame
            //IMediaNetwork uses polling instead of events
            receiver.Update();

            //check if the configuration state changed
            if (receiver.GetConfigurationState() == MediaConfigurationState.Failed)
            {
                //did configuration fail? error
                Debug.Log("receiver configuration failed " + receiver.GetConfigurationError());
                receiver.ResetConfiguration();
            }
            else if (receiver.GetConfigurationState() == MediaConfigurationState.Successful
                && mReceiverConfigured == false)
            {
                //configuration successful.
                mReceiverConfigured = true;
                //StartServer corresponds to ICall.Listen
                receiver.StartServer(address);
            }

            //Dequeue network events
            NetworkEvent evt;
            while (receiver.Dequeue(out evt))
            {
                
                if (evt.Type == NetEventType.ServerInitialized)
                {
                    //triggered if StartServer completed
                    Debug.Log("receiver: server initialized.");
                    //receiver is ready -> create sender
                    SenderSetup();
                }
                else if (evt.Type == NetEventType.ServerInitFailed)
                {
                    //either network problem or address in use
                    Debug.LogError("receiver: server init failed");
                }
                else if (evt.Type == NetEventType.NewConnection)
                {
                    //triggered if a new connection is established
                    Debug.Log("receiver: New connection with id " + evt.ConnectionId);
                }
            }
            receiver.Flush();
        }
        private void SenderSetup()
        {
            //STEP4: receiver is ready -> start the sender 
            Debug.Log("sender setup");
            sender = UnityCallFactory.Instance.CreateMediaNetwork(netConf);
            MediaConfig mediaConf2 = new MediaConfig();
            mediaConf2.Video = false;
            mediaConf2.Audio = true;
            sender.Configure(mediaConf2);
        }


        private void UpdateSender()
        {
            //STEP5: Sender update loop. Same as receiver but is calling Connect
            //once configure completed.
            sender.Update();

            NetworkEvent evt;

            if (sender.GetConfigurationState() == MediaConfigurationState.Failed)
            {
                //did configuration fail? error
                Debug.Log("sender configuration failed " + sender.GetConfigurationError());
                sender.ResetConfiguration();
            }
            else if (sender.GetConfigurationState() == MediaConfigurationState.Successful
                && mSenderConfigured == false)
            {
                mSenderConfigured = true;

                //connecting to to the receiver
                sender.Connect(address);
            }

            while (sender.Dequeue(out evt))
            {
                if (evt.Type == NetEventType.NewConnection)
                {
                    Debug.Log("sender: New connection with id " + evt.ConnectionId);
                }
                else if (evt.Type == NetEventType.ConnectionFailed)
                {
                    Debug.LogError("sender: connection failed");
                }
            }
            sender.Flush();
        }


        private void OnDestroy()
        {
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


        // Update is called once per frame
        void Update()
        {
            if (receiver != null)
                UpdateReceiver();

            if (sender != null)
                UpdateSender();
        }
    }
}