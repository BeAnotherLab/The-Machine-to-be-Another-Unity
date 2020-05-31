
using UnityEngine;
using System.Runtime.InteropServices;
using System;
using System.Text;

namespace Byn.Awrtc.Browser
{
    public class BrowserMediaNetwork : BrowserWebRtcNetwork, IMediaNetwork
    {



        public BrowserMediaNetwork(NetworkConfig lNetConfig)
        {
            if(lNetConfig.AllowRenegotiation)
            {
                SLog.LW("NetworkConfig.AllowRenegotiation is set to true. This is not supported in the browser version yet! Flag ignored.", this.GetType().Name);
            }
            string signalingUrl = lNetConfig.SignalingUrl;

            IceServer[] iceServers = null;
            if(lNetConfig.IceServers != null)
            {
                iceServers = lNetConfig.IceServers.ToArray();
            }


            //TODO: change this to avoid the use of json

            StringBuilder iceServersJson = new StringBuilder();
            BrowserWebRtcNetwork.IceServersToJson(iceServers, iceServersJson);
            /*
            Example:
            {"{IceServers":[{"urls":["turn:because-why-not.com:12779"],"username":"testuser13","credential":"testpassword"},{"urls":["stun:stun.l.google.com:19302"],"username":"","credential":""}], "SignalingUrl":"ws://because-why-not.com:12776/callapp", "IsConference":"False"}
             */

            string conf = "{\"IceServers\":" + iceServersJson.ToString() + ", \"SignalingUrl\":\"" + signalingUrl + "\", \"IsConference\":\"" + false + "\"}";
            SLog.L("Creating BrowserMediaNetwork config: " + conf, this.GetType().Name);
            mReference = CAPI.Unity_MediaNetwork_Create(conf);
        }


        private void SetOptional(int? opt, ref int value)
        {
            if(opt.HasValue)
            {
                value = opt.Value;
            }
        }
        public void Configure(MediaConfig config)
        {
            int minWidth = -1;
            int minHeight = -1;
            int maxWidth = -1;
            int maxHeight = -1;
            int idealWidth = -1;
            int idealHeight = -1;
            int minFrameRate = -1;
            int maxFrameRate = -1;
            int idealFrameRate = -1;

            SetOptional(config.MinWidth, ref minWidth);
            SetOptional(config.MinHeight, ref minHeight);
            SetOptional(config.MaxWidth, ref maxWidth);
            SetOptional(config.MaxHeight, ref maxHeight);
            SetOptional(config.IdealWidth, ref idealWidth);
            SetOptional(config.IdealHeight, ref idealHeight);

            SetOptional(config.MinFrameRate, ref minFrameRate);
            SetOptional(config.MaxFrameRate, ref maxFrameRate);
            SetOptional(config.IdealFrameRate, ref idealFrameRate);


            CAPI.Unity_MediaNetwork_Configure(mReference,
                config.Audio, config.Video,
                minWidth, minHeight,
                maxWidth, maxHeight,
                idealWidth, idealHeight,
                minFrameRate, maxFrameRate, idealFrameRate, config.VideoDeviceName
                );
        }

        public IFrame TryGetFrame(ConnectionId id)
        {
            int length = CAPI.Unity_MediaNetwork_TryGetFrameDataLength(mReference, id.id);
            if (length < 0)
                return null;

            int[] width = new int[1];
            int[] height = new int[1];
            byte[] buffer = new byte[length];

            bool res = CAPI.Unity_MediaNetwork_TryGetFrame(mReference, id.id, width, height, buffer, 0, buffer.Length);
            if (res)
                return new BufferedFrame(buffer, width[0], height[0], FramePixelFormat.ABGR, 0, true);
            return null;
        }

        public MediaConfigurationState GetConfigurationState()
        {
            int res = CAPI.Unity_MediaNetwork_GetConfigurationState(mReference);
            MediaConfigurationState state = (MediaConfigurationState)res;
            return state;
        }
        public override void Update()
        {
            base.Update();

        }
        public string GetConfigurationError()
        {
            if(GetConfigurationState() == MediaConfigurationState.Failed)
            {
                return "An error occurred while requesting Audio/Video features. Check the browser log for more details.";
            }else
            {
                return null;
            }

        }

        public void ResetConfiguration()
        {
            CAPI.Unity_MediaNetwork_ResetConfiguration(mReference);
        }

        public void SetVolume(double volume, ConnectionId remoteUserId)
        {
            CAPI.Unity_MediaNetwork_SetVolume(mReference, volume, remoteUserId.id);
        }

        public bool HasAudioTrack(ConnectionId remoteUserId)
        {
            return CAPI.Unity_MediaNetwork_HasAudioTrack(mReference, remoteUserId.id);
        }

        public bool HasVideoTrack(ConnectionId remoteUserId)
        {
            return CAPI.Unity_MediaNetwork_HasVideoTrack(mReference, remoteUserId.id);
        }

        public bool IsMute()
        {
            return CAPI.Unity_MediaNetwork_IsMute(mReference);
        }

        public void SetMute(bool val)
        {
            CAPI.Unity_MediaNetwork_SetMute(mReference, val);
        }
    }
}
