using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Byn.Awrtc.Browser
{
    public class BrowserCallFactory : IAwrtcFactory
    {
        //Checks if the network and media component are available
        public static bool IsAvailable()
        {
#if UNITY_WEBGL
            try
            {
                //js side will check if all needed functions are available and if the browser is supported
                return BrowserWebRtcNetwork.IsAvailable() && CAPI.Unity_MediaNetwork_IsAvailable();
            }
            catch (EntryPointNotFoundException)
            {
                //method is missing entirely
            }
#endif
            return false;
        }
        public static bool HasUserMedia()
        {
#if UNITY_WEBGL
            try
            {
                return CAPI.Unity_MediaNetwork_HasUserMedia();
            }
            catch (EntryPointNotFoundException)
            {
                //method is missing entirely
            }
#endif
            return false;
        }


        public IWebRtcNetwork CreateBasicNetwork(string websocketUrl, IceServer[] lIceServers = null)
        {
            return new BrowserWebRtcNetwork(websocketUrl, lIceServers);
        }

        public ICall CreateCall(NetworkConfig config)
        {
            return new BrowserWebRtcCall(config);
        }

        public IMediaNetwork CreateMediaNetwork(NetworkConfig config)
        {
            return new BrowserMediaNetwork(config);
        }

        public void Dispose()
        {

        }


        public bool CanSelectVideoDevice()
        {
            return CAPI.Unity_DeviceApi_LastUpdate() > 0;
        }

        public string[] GetVideoDevices()
        {
            int bufflen = 1024;
            byte[] buffer = new byte[bufflen];
            uint len = CAPI.Unity_DeviceApi_Devices_Length();
            string[] arr = new string[len];
            for (int i = 0; i < len; i++)
            {
                CAPI.Unity_DeviceApi_Devices_Get(i, buffer, bufflen);
                arr[i] = Encoding.UTF8.GetString(buffer);
                Debug.Log("device read: " + arr[i]);
            }
            return arr;
        }

        //Not available at all in WebGL. All calls just map into a java script library
        //thus a signaling network would need to be implemeted in java script
        public ICall CreateCall(NetworkConfig config, IBasicNetwork signalingNetwork)
        {
            throw new NotSupportedException("Custom signaling networks are not supported in WebGL. It needs to be implemented in java script.");
        }
    }
}
