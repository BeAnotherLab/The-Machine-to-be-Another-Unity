/* 
 * Copyright (C) 2018 Christoph Kutza
 * 
 * Please refer to the LICENSE file for license information
 */
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using Byn.Awrtc;
using Byn.Awrtc.Unity;

namespace Byn.Unity.Examples
{

    /// <summary>
    /// USE AT YOUR OWN RISK! This is just an example how it will look like. It isn't
    /// a fully functional app that will just work on all platforms!
    /// 
    /// Allows to test the future feature for conference calls / n to n connections.
    /// 
    /// The current conference support is still limited. So far supporting and
    /// optimizing the new mobile platforms has priority.
    /// 
    /// Typical problems:
    /// * handling multiple streams can be very CPU intensive (keep resolution low)
    /// * System can't handle failed direct connections
    ///     e.g. if 3 users join the same call but two of them 
    ///     just can't connect directly due to firewall / stun fails.
    ///     Use TURN if possible to reduce the risk of this happening!
    /// * Not tested on any mobile platforms yet!
    /// 
    /// 
    /// Note the signaling server server needs the correct flag in config.json
    ///     "address_sharing": true
    ///     !!!!
    ///     
    /// e.g.:
    /// 
    ///    "apps": [
    ///        {
    ///            "name": "ConferenceApp",
    ///            "path": "/conferenceapp",
    ///            "address_sharing": true
    ///        }
    ///        
    /// for url ws://because-why-not.com:12776/conferenceapp
    /// </summary>
    public class ConferenceApp : MonoBehaviour
    {
        /// <summary>
        /// Length limit of signaling server address
        /// </summary>
        private const int MAX_CODE_LENGTH = 256;

        #region UI
        /// <summary>
        /// Input field used to enter the room name.
        /// </summary>
        public InputField uRoomName;

        /// <summary>
        /// Input field to enter a new message.
        /// </summary>
        public InputField uMessageField;

        /// <summary>
        /// Output message list to show incoming and sent messages + output messages of the
        /// system itself.
        /// </summary>
        public MessageList uOutput;

        /// <summary>
        /// Join button to connect to a server.
        /// </summary>
        public Button uJoin;

        /// <summary>
        /// Send button.
        /// </summary>
        public Button uSend;


        /// <summary>
        /// Shutdown button. Disconnects all connections + shuts down the server if started.
        /// </summary>
        public Button uShutdown;

        /// <summary>
        /// Panel with the join button. Will be hidden after setup
        /// </summary>
        public GameObject uSetupPanel;

        /// <summary>
        /// Space used for video images
        /// </summary>
        public GameObject uVideoLayout;

        /// <summary>
        /// Prefab used for new user screen / video image
        /// </summary>
        public GameObject uVideoPrefab;


        /// <summary>
        /// Texture used to indicate users that don't stream video.
        /// </summary>
        public Texture2D uNoImgTexture;
        #endregion

        /// <summary>
        /// Call class handling all the functionality
        /// </summary>
        private ICall mCall;

        /// <summary>
        /// Configuration of audio / video functionality
        /// </summary>
        private MediaConfig config = new MediaConfig();

        /// <summary>
        /// Class used to keep track of each individual connection and its data / ui
        /// </summary>
        private class VideoData
        {
            public GameObject uiObject;
            public Texture2D texture;
            public RawImage image;

        }

        /// <summary>
        /// Dictionary to resolve connection ID with their specific data
        /// </summary>
        private Dictionary<ConnectionId, VideoData> mVideoUiElements = new Dictionary<ConnectionId, VideoData>();

        /// <summary>
        /// Unity start.
        /// </summary>
        private void Start()
        {
            //to trigger android permission requests
            StartCoroutine(ExampleGlobals.RequestPermissions());
            //use video and audio by default (the UI is toggled on by default as well it will change on click )
            config.Video = true;
            config.Audio = true;
            config.VideoDeviceName = UnityCallFactory.Instance.GetDefaultVideoDevice();
            
        }


        /// <summary>
        /// Creates the call object and uses the configure method to activate the 
        /// video / audio support if the values are set to true.
        /// </summary>
        /// <param name="useAudio">Uses the local microphone for the call</param>
        /// <param name="useVideo">Uses a local camera for the call. The camera will start
        /// generating new frames after this call so the user can see himself before
        /// the call is connected.</param>
        private void Setup(bool useAudio = true, bool useVideo = true)
        {
            Append("Setting up ...");

            //setup the server
            NetworkConfig netConfig = new NetworkConfig();
            netConfig.IceServers.Add(ExampleGlobals.DefaultIceServer);
            netConfig.SignalingUrl = ExampleGlobals.SignalingConference;
            netConfig.IsConference = true;
            mCall = UnityCallFactory.Instance.Create(netConfig);
            if (mCall == null)
            {
                Append("Failed to create the call");
                return;
            }

            Append("Call created!");
            mCall.CallEvent += Call_CallEvent;

            //setup local video element
            SetupVideoUi(ConnectionId.INVALID);
            mCall.Configure(config);


            SetGuiState(false);
        }

        /// <summary>
        /// Destroys the call object and shows the setup screen again.
        /// Called after a call ends or an error occurred.
        /// </summary>
        private void ResetCall()
        {
            foreach (var v in mVideoUiElements)
            {
                Destroy(v.Value.uiObject);
                Destroy(v.Value.texture);
            }
            mVideoUiElements.Clear();
            CleanupCall();
            SetGuiState(true);
        }

        /// <summary>
        /// Handler of call events.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Call_CallEvent(object sender, CallEventArgs e)
        {
            switch (e.Type)
            {
                case CallEventType.CallAccepted:
                    //Outgoing call was successful or an incoming call arrived
                    Append("Connection established");
                    OnNewCall(e as CallAcceptedEventArgs);
                    break;
                case CallEventType.CallEnded:
                    OnCallEnded(e as CallEndedEventArgs);
                    break;
                case CallEventType.ListeningFailed:
                    Append("Failed to listen for incoming calls! Server might be down!");
                    ResetCall();
                    break;

                case CallEventType.ConnectionFailed:
                    {
                        //this should be impossible to happen in conference mode!
                        ErrorEventArgs args = e as ErrorEventArgs;
                        Append("Error: " + args.Info);
                        Debug.LogError(args.Info);
                        ResetCall();
                    }
                    break;

                case CallEventType.FrameUpdate:
                    //new frame received from webrtc (either from local camera or network)
                    FrameUpdateEventArgs frameargs = e as FrameUpdateEventArgs;
                    UpdateFrame(frameargs.ConnectionId, frameargs.Frame);
                    break;
                case CallEventType.Message:
                    {
                        //text message received
                        MessageEventArgs args = e as MessageEventArgs;
                        Append(args.Content);
                        break;
                    }
                case CallEventType.WaitForIncomingCall:
                    {
                        //the chat app will wait for another app to connect via the same string
                        WaitForIncomingCallEventArgs args = e as WaitForIncomingCallEventArgs;
                        Append("Waiting for incoming call address: " + args.Address);
                        break;
                    }
            }

        }

        /// <summary>
        /// Event triggers for a new incoming call
        /// (in conference mode there is no difference between incoming / outgoing)
        /// </summary>
        /// <param name="args"></param>
        private void OnNewCall(CallAcceptedEventArgs args)
        {
            SetupVideoUi(args.ConnectionId);
        }

        /// <summary>
        /// Creates the connection specific data / ui
        /// </summary>
        /// <param name="id"></param>
        private void SetupVideoUi(ConnectionId id)
        {
            //create texture + ui element
            VideoData vd = new VideoData();
            vd.uiObject = Instantiate(uVideoPrefab);
            vd.uiObject.transform.SetParent(uVideoLayout.transform, false);
            vd.image = vd.uiObject.GetComponentInChildren<RawImage>();
            vd.image.texture = uNoImgTexture;
            mVideoUiElements[id] = vd;
        }

        /// <summary>
        /// User left. Cleanup connection specific data / ui
        /// </summary>
        /// <param name="args"></param>
        private void OnCallEnded(CallEndedEventArgs args)
        {
            VideoData data;
            if (mVideoUiElements.TryGetValue(args.ConnectionId, out data))
            {
                Destroy(data.texture);
                Destroy(data.uiObject);
                mVideoUiElements.Remove(args.ConnectionId);
            }
        }

        /// <summary>
        /// Updates the texture based on the given frame update.
        /// 
        /// </summary>
        /// <param name="tex"></param>
        /// <param name="frame"></param>
        private void UpdateTexture(ref Texture2D tex, IFrame frame)
        {
            //texture exists but has the wrong height /width? -> destroy it and set the value to null
            if (tex != null && (tex.width != frame.Width || tex.height != frame.Height))
            {
                Texture2D.Destroy(tex);
                tex = null;
            }
            //no texture? create a new one first
            if (tex == null)
            {
                tex = new Texture2D(frame.Width, frame.Height, TextureFormat.RGBA32, false);
                tex.wrapMode = TextureWrapMode.Clamp;
            }
            ///copy image data into the texture and apply
            tex.LoadRawTextureData(frame.Buffer);
            tex.Apply();
        }


        /// <summary>
        /// Updates the frame for a connection id. If the id is new it will create a
        /// visible image for it. The frame can be null for connections that
        /// don't sent frames.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="frame"></param>
        private void UpdateFrame(ConnectionId id, IFrame frame)
        {
            if (mVideoUiElements.ContainsKey(id))
            {
                VideoData videoData = mVideoUiElements[id];
                UpdateTexture(ref videoData.texture, frame);
                videoData.image.texture = videoData.texture;
            }
        }

        /// <summary>
        /// Destroys the call. Used if unity destroys the object or if a call
        /// ended / failed due to an error.
        /// 
        /// </summary>
        private void CleanupCall()
        {
            if (mCall != null)
            {

                Debug.Log("Destroying call!");
                mCall.Dispose();
                mCall = null;
                Debug.Log("Call destroyed");
            }
        }
        private void OnDestroy()
        {
            CleanupCall();
        }


        /// <summary>
        /// toggle audio on / off
        /// </summary>
        /// <param name="state"></param>
        public void AudioToggle(bool state)
        {
            config.Audio = state;
        }

        /// <summary>
        /// toggle video on / off
        /// </summary>
        /// <param name="state"></param>
        public void VideoToggle(bool state)
        {
            config.Video = state;
        }

        /// <summary>
        /// Adds a new message to the message view
        /// </summary>
        /// <param name="text"></param>
        private void Append(string text)
        {
            if (uOutput != null)
            {
                uOutput.AddTextEntry(text);
            }
            else
            {
                Debug.Log("Chat: " + text);
            }
        }

        /// <summary>
        /// The call object needs to be updated regularly to sync data received via webrtc with
        /// unity. All events will be triggered during the update method in the unity main thread
        /// to avoid multi threading errors
        /// </summary>
        private void Update()
        {
            if (mCall != null)
            {
                //update the call
                mCall.Update();
            }
        }

        #region UI 
        /// <summary>
        /// Shows the setup screen or the chat + video
        /// </summary>
        /// <param name="showSetup">true Shows the setup. False hides it.</param>
        private void SetGuiState(bool showSetup)
        {
            uSetupPanel.SetActive(showSetup);

            uSend.interactable = !showSetup;
            uShutdown.interactable = !showSetup;
            uMessageField.interactable = !showSetup;

        }

        /// <summary>
        /// Join button pressed. Tries to join a room.
        /// </summary>
        public void JoinButtonPressed()
        {
            Setup();
            EnsureLength();
            mCall.Listen(uRoomName.text);
        }

        /// <summary>
        /// Helper to enforce the length limit
        /// </summary>
        private void EnsureLength()
        {
            if (uRoomName.text.Length > MAX_CODE_LENGTH)
            {
                uRoomName.text = uRoomName.text.Substring(0, MAX_CODE_LENGTH);
            }
        }

        /// <summary>
        /// This is called if the send button
        /// </summary>
        public void SendButtonPressed()
        {
            //get the message written into the text field
            string msg = uMessageField.text;
            SendMsg(msg);
        }

        /// <summary>
        /// User either pressed enter or left the text field
        /// -> if return key was pressed send the message
        /// </summary>
        public void InputOnEndEdit()
        {
            if (Input.GetKey(KeyCode.Return))
            {
                string msg = uMessageField.text;
                SendMsg(msg);
            }
        }

        /// <summary>
        /// Sends a message to the other end
        /// </summary>
        /// <param name="msg"></param>
        private void SendMsg(string msg)
        {
            if (String.IsNullOrEmpty(msg))
            {
                //never send null or empty messages. webrtc can't deal with that
                return;
            }

            Append(msg);
            mCall.Send(msg);

            //reset UI
            uMessageField.text = "";
            uMessageField.Select();
        }



        /// <summary>
        /// Shutdown button pressed. Shuts the network down.
        /// </summary>
        public void ShutdownButtonPressed()
        {
            ResetCall();
        }
        #endregion
    }

}