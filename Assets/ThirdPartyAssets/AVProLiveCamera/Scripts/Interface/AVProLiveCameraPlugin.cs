using System.Text;
using System.Runtime.InteropServices;

//-----------------------------------------------------------------------------
// Copyright 2012-2018 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProLiveCamera
{
	public class AVProLiveCameraPlugin
	{
		public enum VideoFrameFormat
		{
			RAW_BGRA32,

			YUV_422_YUY2,
			YUV_422_UYVY,
			YUV_422_YVYU,
			YUV_422_HDYC,

			YUV_420_PLANAR_YV12,
			YUV_420_PLANAR_I420,

			RAW_RGB24,
			RAW_MONO8,

			RGB_10BPP,
			YUV_10BPP_V210,

			MPEG,
		}

		public enum VideoInput
		{
			None,
			Video_Tuner,
			Video_Composite,
			Video_SVideo,
			Video_RGB,
			Video_YRYBY,
			Video_Serial_Digital,
			Video_Parallel_Digital,
			Video_SCSI,
			Video_AUX,
			Video_1394,
			Video_USB,
			Video_Decoder,
			Video_Encoder,
			Video_SCART,
			Video_Black,
		}

		// Used by GL.IssuePluginEvent
		public const int PluginID = 0xFA20000;
		public enum PluginEvent
		{
			UpdateAllTextures = 0x0001000,
			UpdateOneTexture = 0x0002000,
		}

#if UNITY_5_4_OR_NEWER || (UNITY_5 && !UNITY_5_0 && !UNITY_5_1)
		[DllImport("AVProLiveCamera")]
		public static extern System.IntPtr GetRenderEventFunc();
#endif

		//////////////////////////////////////////////////////////////////////////
		// System Initialisation

		[DllImport("AVProLiveCamera")]
		public static extern bool Init(bool supportInternalConversion);


		[DllImport("AVProLiveCamera")]
		public static extern void Deinit();

		[DllImport("AVProLiveCamera")]
		private static extern System.IntPtr GetPluginVersion();

		public static string GetPluginVersionString()
		{
			return System.Runtime.InteropServices.Marshal.PtrToStringAnsi(GetPluginVersion());
		}

		//////////////////////////////////////////////////////////////////////////
		// Device Enumeration & Configuration

		[DllImport("AVProLiveCamera")]
		public static extern int GetNumDevices();

		[DllImport("AVProLiveCamera")]
		public static extern bool HasDeviceConfigWindow(int index);

		[DllImport("AVProLiveCamera")]
		public static extern bool ShowDeviceConfigWindow(int index);

		[DllImport("AVProLiveCamera", CharSet = CharSet.Unicode)]
		private static extern bool GetDeviceName(int index, StringBuilder nameBuffer, int nameBufferLength);

		public static bool GetDeviceName(int index, out string name)
		{
			StringBuilder nameStr = new StringBuilder(512);
			if (GetDeviceName(index, nameStr, nameStr.Capacity))
			{
				name = nameStr.ToString();
				return true;
			}
			name = string.Empty;
			return false;
		}

		[DllImport("AVProLiveCamera", CharSet = CharSet.Unicode)]
		private static extern bool GetDeviceGUID(int index, StringBuilder nameBuffer, int nameBufferLength);

		public static bool GetDeviceGUID(int index, out string name)
		{
			StringBuilder nameStr = new StringBuilder(512);
			if (GetDeviceGUID(index, nameStr, nameStr.Capacity))
			{
				name = nameStr.ToString();
				return true;
			}
			name = string.Empty;
			return false;
		}

		[DllImport("AVProLiveCamera")]
		public static extern bool IsDeviceConnected(int index);

		[DllImport("AVProLiveCamera")]
		public static extern bool UpdateDevicesConnected();


		[DllImport("AVProLiveCamera")]
		public static extern int GetNumModes(int index);

		[DllImport("AVProLiveCamera")]
		private static extern bool GetModeInfo(int deviceIndex, int modeIndex, out int width, out int height, out float fps, StringBuilder format);

		public static bool GetModeInfo(int deviceIndex, int modeIndex, out int width, out int height, out float fps, out string format)
		{
			StringBuilder formatStr = new StringBuilder(512);
			if (GetModeInfo(deviceIndex, modeIndex, out width, out height, out fps, formatStr))
			{
				format = formatStr.ToString();
				return true;
			}
			format = string.Empty;
			return false;
		}

		[DllImport("AVProLiveCamera")]
		public static extern int GetNumVideoInputs(int deviceIndex);

		[DllImport("AVProLiveCamera")]
		private static extern bool GetVideoInputName(int deviceIndex, int inputIndex, StringBuilder name);

		public static bool GetVideoInputName(int deviceIndex, int inputIndex, out string name)
		{
			StringBuilder nameStr = new StringBuilder(512);
			if (GetVideoInputName(deviceIndex, inputIndex, nameStr))
			{
				name = nameStr.ToString();
				return true;
			}
			name = string.Empty;
			return false;
		}

		[DllImport("AVProLiveCamera")]
		public static extern void SetVideoInputByIndex(int deviceIndex, int inputIndex);


		//////////////////////////////////////////////////////////////////////////
		// Device Settings

		[DllImport("AVProLiveCamera")]
		public static extern int GetNumDeviceVideoSettings(int deviceIndex);

		[DllImport("AVProLiveCamera")]
		private static extern bool GetDeviceVideoSettingInfo(int deviceIndex, int settingIndex, out int settingType, out int dataType, StringBuilder name, out bool canAutomatic);

		public static bool GetDeviceVideoSettingInfo(int deviceIndex, int settingIndex, out int settingType, out int dataType, out string name, out bool canAutomatic)
		{
			StringBuilder nameStr = new StringBuilder(512);
			if (GetDeviceVideoSettingInfo(deviceIndex, settingIndex, out settingType, out dataType, nameStr, out canAutomatic))
			{
				name = nameStr.ToString();
				return true;
			}
			name = string.Empty;
			return false;
		}

		[DllImport("AVProLiveCamera")]
		public static extern bool GetDeviceVideoSettingBoolean(int deviceIndex, int settingIndex, out bool defaultValue, out bool currentValue, out bool isAutomatic);

		[DllImport("AVProLiveCamera")]
		public static extern bool GetDeviceVideoSettingFloat(int deviceIndex, int settingIndex, out float defaultValue, out float currentValue, out float minValue, out float maxValue, out bool isAutomatic);

		[DllImport("AVProLiveCamera")]
		public static extern bool UpdateDeviceVideoSettingValue(int deviceIndex, int settingIndex, out float currentValue, out bool isAutomatic);

		[DllImport("AVProLiveCamera")]
		public static extern void ApplyDeviceVideoSettingValue(int deviceIndex, int settingIndex, float currentValue, bool isAutomatic);


		//////////////////////////////////////////////////////////////////////////
		// Open & Close Devices

		[DllImport("AVProLiveCamera")]
		public static extern bool StartDevice(int index, int modeIndex, int videoInputIndex);

		[DllImport("AVProLiveCamera")]
		public static extern void StopDevice(int index);

		//////////////////////////////////////////////////////////////////////////
		// Play & Pause

		[DllImport("AVProLiveCamera")]
		public static extern bool Play(int index);

		[DllImport("AVProLiveCamera")]
		public static extern void Pause(int index);

		[DllImport("AVProLiveCamera")]
		public static extern void Stop(int index);

		//////////////////////////////////////////////////////////////////////////
		// Current State of Device

		[DllImport("AVProLiveCamera")]
		public static extern int GetWidth(int index);

		[DllImport("AVProLiveCamera")]
		public static extern int GetHeight(int index);

		[DllImport("AVProLiveCamera")]
		public static extern float GetFrameRate(int index);

		[DllImport("AVProLiveCamera")]
		public static extern long GetFrameDurationHNS(int index);

		[DllImport("AVProLiveCamera")]
		public static extern int GetFormat(int index);

		[DllImport("AVProLiveCamera")]
		private static extern bool GetDeviceFormat(int index, StringBuilder format);

		public static string GetDeviceFormat(int deviceIndex)
		{
			string result = "Unknown";
			StringBuilder nameStr = new StringBuilder(512);
			if (GetDeviceFormat(deviceIndex, nameStr))
			{
				result = nameStr.ToString();
			}
			return result;
		}

		[DllImport("AVProLiveCamera")]
		public static extern bool IsFrameTopDown(int index);

		[DllImport("AVProLiveCamera")]
		public static extern uint GetLastFrame(int index);

		//////////////////////////////////////////////////////////////////////////
		// Frame Updating

		[DllImport("AVProLiveCamera")]
		public static extern bool SetActive(int index, bool active);

		[DllImport("AVProLiveCamera")]
		public static extern bool IsNextFrameReadyForGrab(int index);

		[DllImport("AVProLiveCamera")]
		public static extern int GetLastFrameUploaded(int handle);

		[DllImport("AVProLiveCamera")]
		public static extern bool UpdateTextureGL(int index, int textureID);

		[DllImport("AVProLiveCamera")]
		public static extern bool GetFramePixels(int index, System.IntPtr buffer, int bufferIndex, int bufferWidth, int bufferHeight);

		[DllImport("AVProLiveCamera")]
		public static extern void SetTexturePointer(int index, int bufferIndex, System.IntPtr texturePtr);

		[DllImport("AVProLiveCamera")]
		public static extern bool GetFrameAsColor32(int index, System.IntPtr bufferPtr, int bufferWidth, int bufferHeight);

		//////////////////////////////////////////////////////////////////////////
		// Live Stats

		[DllImport("AVProLiveCamera")]
		public static extern float GetCaptureFrameRate(int deviceIndex);

		[DllImport("AVProLiveCamera")]
		public static extern uint GetCaptureFramesDropped(int deviceIndex);

		//////////////////////////////////////////////////////////////////////////
		// Internal Frame Buffering

		[DllImport("AVProLiveCamera")]
		public static extern void SetFrameBufferSize(int deviceIndex, int read, int write);

		[DllImport("AVProLiveCamera")]
		public static extern long GetLastFrameBufferedTime(int deviceIndex);

		[DllImport("AVProLiveCamera")]
		public static extern System.IntPtr GetLastFrameBuffered(int deviceIndex);

		[DllImport("AVProLiveCamera")]
		public static extern System.IntPtr GetFrameFromBufferAtTime(int deviceIndex, long time);
	}
}