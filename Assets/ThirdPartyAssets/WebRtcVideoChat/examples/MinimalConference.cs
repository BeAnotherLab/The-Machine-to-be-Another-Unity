using UnityEngine;
using System;
using Byn.Awrtc;
using Byn.Awrtc.Unity;

namespace Byn.Unity.Examples
{
    /// <summary>
    /// Minimal conference call example.
    /// Note that this feature is still in development and not yet fully stable.
    /// Use at your own risk.
    /// 
    /// This example shows the use of the ICall interface for conference calls. By
    /// setting NetworkConfig.IsConference to true and using the signaling server
    /// flag "address_sharing" it allows to create N to N connections using a single address.
    /// All users in the conference are treated equally.
    /// </summary>
    public class MinimalConference : MonoBehaviour
    {
        ICall[] calls = new ICall[0];

        NetworkConfig netConf;
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
            calls = new ICall[3];

            address = Application.productName + "_MinimalConference";
            netConf = new NetworkConfig();

            //watch out the signaling server needs to be configured properly for this to work:
            //flag "address_sharing" needs to be set to true in config.json
            //e.g. "ws://signaling.because-why-not.com/testshared"
            netConf.SignalingUrl = ExampleGlobals.SharedSignaling;

            //
            netConf.IsConference = true;

            //The current version doesn't deal well with failed direct connections
            //thus a turn server is used to ensure users can connect.
            //
            netConf.IceServers.Add(new IceServer(ExampleGlobals.TurnUrl, 
                ExampleGlobals.TurnUser, 
                ExampleGlobals.TurnPass));
            SetupCalls();
        }

        private void SetupCalls()
        {
            MediaConfig mediaConf1 = new MediaConfig();

            mediaConf1.Video = false;
            mediaConf1.Audio = true;
            for (int i = 0; i < calls.Length; i++)
            {
                Debug.Log(i + " setup");
                calls[i] = UnityCallFactory.Instance.Create(netConf);
                calls[i].CallEvent += OnCallEvent;
                calls[i].Configure(mediaConf1);
            }
        }
        
        private void OnCallEvent(object src, CallEventArgs args)
        {
            ICall call = src as ICall;
            int index = Array.IndexOf(calls, call);

            if (args.Type == CallEventType.ConfigurationComplete)
            {
                Debug.Log(index + ": configuration done. Listening on address " + address);
                //ALL connections will call listen. The current conference call version
                //will connect all users that listen to the same address
                //resulting in an N to N / full mesh topology 
                call.Listen(address);
            }
            else if (args.Type == CallEventType.CallAccepted)
            {
                Debug.Log(index + ": CallAccepted");
            }
            else if (args.Type == CallEventType.ConfigurationFailed || args.Type == CallEventType.ListeningFailed)
            {
                Debug.LogError(index + ": failed");
            }
        }

        private void OnDestroy()
        {
            for (int i = 0; i < calls.Length; i++)
            {
                if (calls[i] != null)
                {
                    calls[i].Dispose();
                    calls[i] = null;
                }
            }
        }
        
        void Update()
        {
            for (int i = 0; i < calls.Length; i++)
            {
                if (calls[i] != null)
                {
                    calls[i].Update();
                }
            }
        }
    }
}