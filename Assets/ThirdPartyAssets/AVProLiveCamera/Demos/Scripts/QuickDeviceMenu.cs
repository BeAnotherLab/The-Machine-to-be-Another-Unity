using UnityEngine;
using System.Collections.Generic;

//-----------------------------------------------------------------------------
// Copyright 2012-2018 RenderHeads Ltd.  All rights reserverd.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProLiveCamera.Demos
{
	public class QuickDeviceMenu : MonoBehaviour
	{
		public AVProLiveCamera _liveCamera;
		public AVProLiveCameraManager _liveCameraManager;
		public GUISkin _guiSkin;
		private Vector2 _scrollResolutions = Vector2.zero;
		private bool _isHidden = false;

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Space))
			{
				ToggleVisible();
			}
		}

		private void ToggleVisible()
		{
			_isHidden = !_isHidden;
			this.useGUILayout = !_isHidden;		// NOTE: this reduces garbage generation to zero
		}

		void OnGUI()
		{
			if (_isHidden)
			{
				return;
			}

			GUI.skin = _guiSkin;

			if (_liveCameraManager.NumDevices > 0)
			{
				GUILayout.BeginArea(new Rect(0f, 0f, Screen.width, Screen.height));
				if (GUILayout.Button("Press SPACE to hide/show QuickDeviceMenu (improves performance)", GUILayout.ExpandWidth(false)))
				{
					ToggleVisible();
				}
				GUILayout.EndArea();

				// NOTE: This is just a spacing element to leave space for the above message
				GUILayout.Label(" ");
				
				GUILayout.BeginHorizontal();

				// Select device
				GUILayout.BeginVertical();
				GUILayout.Button("SELECT DEVICE");
				for (int i = 0; i < _liveCameraManager.NumDevices; i++)
				{
					string name = _liveCameraManager.GetDevice(i).Name;

					GUI.color = Color.white;
					if (_liveCamera.Device != null && _liveCamera.Device.IsRunning)
					{
						if (_liveCamera.Device.Name == name)
						{
							GUI.color = Color.green;
						}
					}

					if (GUILayout.Button(name))
					{
						_liveCamera._deviceSelection = AVProLiveCamera.SelectDeviceBy.Index;
						_liveCamera._desiredDeviceIndex = i;
						_liveCamera.Begin();
					}
				}
				GUI.color = Color.white;
				GUILayout.EndVertical();

				if (_liveCamera.Device != null && _liveCamera.Device.IsRunning)
				{
					GUILayout.BeginVertical();
					GUILayout.Button("RESOLUTION");
					_scrollResolutions = GUILayout.BeginScrollView(_scrollResolutions, false, false, GUIStyle.none, GUI.skin.verticalScrollbar);
					List<string> usedNames = new List<string>(32);
					for (int i = 0; i < _liveCamera.Device.NumModes; i++)
					{
						AVProLiveCameraDeviceMode mode = _liveCamera.Device.GetMode(i);
						string name = string.Format("{0}x{1}", mode.Width, mode.Height);
						if (!usedNames.Contains(name))
						{
							GUI.color = Color.white;
							if (_liveCamera.Device.CurrentWidth == mode.Width && _liveCamera.Device.CurrentHeight == mode.Height)
							{
								GUI.color = Color.green;
							}

							usedNames.Add(name);
							if (GUILayout.Button(name))
							{
								_liveCamera._modeSelection = AVProLiveCamera.SelectModeBy.Index;
								_liveCamera._desiredModeIndex = i;
								_liveCamera.Begin();
							}
						}
					}
					GUI.color = Color.white;
					GUILayout.EndScrollView();
					GUILayout.EndVertical();

					// Select frame rate
					usedNames.Clear();
					GUILayout.BeginVertical();
					GUILayout.Button("FPS");
					for (int i = 0; i < _liveCamera.Device.NumModes; i++)
					{
						string matchName = string.Format("{0}x{1}", _liveCamera.Device.CurrentWidth, _liveCamera.Device.CurrentHeight);

						AVProLiveCameraDeviceMode mode = _liveCamera.Device.GetMode(i);

						string resName = string.Format("{0}x{1}", mode.Width, mode.Height);
						if (resName == matchName)
						{
							string name = string.Format("{0}", mode.FPS.ToString("F2"));
							if (!usedNames.Contains(name))
							{
								GUI.color = Color.white;
								if (_liveCamera.Device.CurrentFrameRate.ToString("F2") == mode.FPS.ToString("F2"))
								{
									GUI.color = Color.green;
								}

								usedNames.Add(name);
								if (GUILayout.Button(name))
								{
									_liveCamera._modeSelection = AVProLiveCamera.SelectModeBy.Index;
									_liveCamera._desiredModeIndex = i;
									_liveCamera.Begin();
								}
							}
						}
					}
					GUI.color = Color.white;
					GUILayout.EndVertical();

					// Select format
					usedNames.Clear();
					GUILayout.BeginVertical();
					GUILayout.Button("FORMAT");
					for (int i = 0; i < _liveCamera.Device.NumModes; i++)
					{
						string matchName = string.Format("{0}x{1}@{2}", _liveCamera.Device.CurrentWidth, _liveCamera.Device.CurrentHeight, _liveCamera.Device.CurrentFrameRate.ToString("F2"));

						AVProLiveCameraDeviceMode mode = _liveCamera.Device.GetMode(i);

						string resName = string.Format("{0}x{1}@{2}", mode.Width, mode.Height, mode.FPS.ToString("F2"));
						if (resName == matchName)
						{
							string name = string.Format("{0}", mode.Format);
							if (!usedNames.Contains(name))
							{
								GUI.color = Color.white;
								if (_liveCamera.Device.CurrentDeviceFormat == mode.Format)
								{
									GUI.color = Color.green;
								}

								usedNames.Add(name);
								if (GUILayout.Button(name))
								{
									_liveCamera._modeSelection = AVProLiveCamera.SelectModeBy.Index;
									_liveCamera._desiredModeIndex = i;
									_liveCamera.Begin();
								}
							}
						}
					}
					GUI.color = Color.white;
					GUILayout.EndVertical();
				}
				GUILayout.EndHorizontal();
			}
			else
			{
				GUILayout.Label("No webcam / capture devices found");
			}
		}
	}
}