// Support for Editor.RequiresConstantRepaint()
#if UNITY_5_4_OR_NEWER || (UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_5)
	#define AVPROLIVECAMERA_UNITYFEATURE_EDITORAUTOREFRESH
#endif
using UnityEngine;
using UnityEditor;
using System.Collections;

//-----------------------------------------------------------------------------
// Copyright 2012-2018 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProLiveCamera.Editor
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(AVProLiveCamera))]
	public class AVProLiveCameraEditor : UnityEditor.Editor
	{
		private AVProLiveCamera _camera;
		private SerializedProperty _propFlipX;
		private SerializedProperty _propFlipY;
		private SerializedProperty _propAllowTransparency;
		private SerializedProperty _propHotSwap;
		private SerializedProperty _propFrameRates;
		private SerializedProperty _propSettings;

		private static bool _preview = true;
		private static bool _showSettings = false;

		private const string SettingsPrefix = "AVProLiveCamera-LiveCameraEditor-";


#if AVPROLIVECAMERA_UNITYFEATURE_EDITORAUTOREFRESH
		public override bool RequiresConstantRepaint()
		{
			return (_camera != null && _camera.isActiveAndEnabled && _camera.Device != null && _preview);
		}
#endif

		void OnEnable()
		{
			_camera = (this.target) as AVProLiveCamera;
			_propFlipX = serializedObject.FindProperty("_flipX");
			_propFlipY = serializedObject.FindProperty("_flipY");
			_propAllowTransparency = serializedObject.FindProperty("_allowTransparency");
			_propHotSwap = serializedObject.FindProperty("_updateHotSwap");
			_propFrameRates = serializedObject.FindProperty("_updateFrameRates");
			_propSettings = serializedObject.FindProperty("_updateSettings");
			LoadSettings();
		}

		void OnDisable()
		{
			_camera = null;
			SaveSettings();
		}

		private static void LoadSettings()
		{
			_preview = EditorPrefs.GetBool(SettingsPrefix + "ExpandPreview", true);
			_showSettings = EditorPrefs.GetBool(SettingsPrefix + "ExpandSettings", false);
		}

		private static void SaveSettings()
		{
			EditorPrefs.SetBool(SettingsPrefix + "ExpandPreview", _preview);
			EditorPrefs.SetBool(SettingsPrefix + "ExpandSettings", _showSettings);
		}

		public override void OnInspectorGUI()
		{
			if (_camera == null)
				return;

			serializedObject.Update();

			if (!Application.isPlaying)
			{
				SerializedProperty propPlayOnStart = serializedObject.FindProperty("_playOnStart");
				EditorGUILayout.PropertyField(propPlayOnStart);
				GUILayout.Space(8f);

				GUILayout.Label("Search for Capture Device:");
				EditorGUILayout.Space();

				GUILayout.BeginVertical();

				SerializedProperty propDeviceSelection = serializedObject.FindProperty("_deviceSelection");
				EditorGUILayout.PropertyField(propDeviceSelection, new GUIContent("Device Select By"));

				AVProLiveCamera.SelectDeviceBy newDeviceSelection = _camera._deviceSelection;
				switch (newDeviceSelection)
				{
					case AVProLiveCamera.SelectDeviceBy.Default:
						break;
					case AVProLiveCamera.SelectDeviceBy.Name:
						if (_camera._deviceSelection != newDeviceSelection && _camera._desiredDeviceNames.Count == 0)
						{
							serializedObject.FindProperty("_desiredDeviceNames").arraySize++;
							serializedObject.ApplyModifiedProperties();
							serializedObject.FindProperty("_desiredDeviceNames").GetArrayElementAtIndex(0).stringValue = "Enter Device Name";
							serializedObject.ApplyModifiedProperties();
						}
						EditorGUILayout.BeginVertical();
						for (int index = 0; index < _camera._desiredDeviceNames.Count; index++)
						{
							EditorGUILayout.BeginHorizontal();


							/*EditorGUILayout.BeginHorizontal(GUILayout.Width(96));
							if (index != 0)
							{
								if (GUILayout.Button("Up"))
								{
									string temp = _camera._desiredDeviceNames[index - 1];
									_camera._desiredDeviceNames[index - 1] = _camera._desiredDeviceNames[index];
									_camera._desiredDeviceNames[index] = temp;
									HandleUtility.Repaint();
								}				
							}
							if (index + 1 < _camera._desiredDeviceNames.Count)
							{
								if (GUILayout.Button("Down"))
								{
									string temp = _camera._desiredDeviceNames[index + 1];
									_camera._desiredDeviceNames[index + 1] = _camera._desiredDeviceNames[index];
									_camera._desiredDeviceNames[index] = temp;
									HandleUtility.Repaint();
								}
							}
							EditorGUILayout.EndHorizontal();*/

							if (GUILayout.Button("-"))
							{
								serializedObject.FindProperty("_desiredDeviceNames").DeleteArrayElementAtIndex(index);
								break;
							}
							else
							{
								SerializedProperty propDesiredDeviceName = serializedObject.FindProperty("_desiredDeviceNames").GetArrayElementAtIndex(index);
								EditorGUILayout.PropertyField(propDesiredDeviceName, new GUIContent(""), GUILayout.ExpandWidth(true));
							}
							EditorGUILayout.EndHorizontal();
						}
						if (GUILayout.Button("+"))
						{
							serializedObject.FindProperty("_desiredDeviceNames").arraySize++;
							this.Repaint();
						}
						EditorGUILayout.EndVertical();
						break;
					case AVProLiveCamera.SelectDeviceBy.Index:
						{
							SerializedProperty propDesiredDeviceIndex = serializedObject.FindProperty("_desiredDeviceIndex");
							EditorGUILayout.PropertyField(propDesiredDeviceIndex, new GUIContent(" "));
						}
						break;
				}
				GUILayout.EndVertical();
				EditorGUILayout.Space();



				GUILayout.BeginVertical();

				SerializedProperty propModeSelection = serializedObject.FindProperty("_modeSelection");
				EditorGUILayout.PropertyField(propModeSelection, new GUIContent("Mode Select By"));

				AVProLiveCamera.SelectModeBy newResolutionSelection = _camera._modeSelection;
				switch (newResolutionSelection)
				{
					case AVProLiveCamera.SelectModeBy.Default:
						break;
					case AVProLiveCamera.SelectModeBy.Resolution:
						if (_camera._modeSelection != newResolutionSelection && _camera._desiredResolutions.Count == 0)
						{
							serializedObject.FindProperty("_desiredResolutions").arraySize++;
						}
						EditorGUILayout.BeginVertical();

						GUILayout.Label("Resolution", EditorStyles.boldLabel);

						SerializedProperty propDesiredAnyResolution = serializedObject.FindProperty("_desiredAnyResolution");
						EditorGUILayout.PropertyField(propDesiredAnyResolution, new GUIContent("Automatic Resolution"));
						if (!propDesiredAnyResolution.boolValue)
						{
							for (int index = 0; index < _camera._desiredResolutions.Count; index++)
							{
								EditorGUILayout.BeginHorizontal();
								if (GUILayout.Button("-"))
								{
									serializedObject.FindProperty("_desiredResolutions").DeleteArrayElementAtIndex(index);
									break;
								}
								else
								{
									SerializedProperty propResX = serializedObject.FindProperty("_desiredResolutions").GetArrayElementAtIndex(index).FindPropertyRelative("x");
									EditorGUILayout.PropertyField(propResX, new GUIContent(""), GUILayout.Width(64f));

									EditorGUILayout.LabelField("x", "", GUILayout.Width(24));

									SerializedProperty propResY = serializedObject.FindProperty("_desiredResolutions").GetArrayElementAtIndex(index).FindPropertyRelative("y");
									EditorGUILayout.PropertyField(propResY, new GUIContent(""), GUILayout.Width(64f));
								}
								EditorGUILayout.EndHorizontal();
							}
							if (GUILayout.Button("+"))
							{
								serializedObject.FindProperty("_desiredResolutions").arraySize++;
								this.Repaint();
							}

							SerializedProperty propMaintainAspect = serializedObject.FindProperty("_maintainAspectRatio");
							EditorGUILayout.PropertyField(propMaintainAspect);
						}

						GUILayout.Label("Frame Rate", EditorStyles.boldLabel);

						SerializedProperty propDesiredFrameRate = serializedObject.FindProperty("_desiredFrameRate");
						EditorGUILayout.PropertyField(propDesiredFrameRate, new GUIContent("Frame Rate"));
						if (propDesiredFrameRate.floatValue <= 0f)
						{
							GUILayout.Label("Highest frame rate will be used");
						}

						GUILayout.Label("Format", EditorStyles.boldLabel);

						SerializedProperty propDesiredFormatAny = serializedObject.FindProperty("_desiredFormatAny");
						EditorGUILayout.PropertyField(propDesiredFormatAny, new GUIContent("Automatic Format"));

						if (!propDesiredFormatAny.boolValue)
						{
							SerializedProperty propDesiredFormat = serializedObject.FindProperty("_desiredFormat");
							EditorGUILayout.PropertyField(propDesiredFormat, new GUIContent("Specific Format"));
						}
						else
						{
							SerializedProperty propSupportsTransparency = serializedObject.FindProperty("_desiredTransparencyFormat");
							EditorGUILayout.PropertyField(propSupportsTransparency, new GUIContent("Transparency"));
						}

						EditorGUILayout.EndVertical();
						break;
					case AVProLiveCamera.SelectModeBy.Index:
						{
							SerializedProperty propDesiredModeIndex = serializedObject.FindProperty("_desiredModeIndex");
							EditorGUILayout.PropertyField(propDesiredModeIndex, new GUIContent(" "));
						}
						break;
				}
				GUILayout.EndVertical();
				EditorGUILayout.Space();



				GUILayout.BeginVertical();

				SerializedProperty propInputSelection = serializedObject.FindProperty("_videoInputSelection");
				EditorGUILayout.PropertyField(propInputSelection, new GUIContent("Video Input Select By"));

				AVProLiveCamera.SelectDeviceBy newVideoInputSelection = _camera._videoInputSelection;
				switch (newVideoInputSelection)
				{
					case AVProLiveCamera.SelectDeviceBy.Default:
						break;
					case AVProLiveCamera.SelectDeviceBy.Name:
						if (_camera._videoInputSelection != newVideoInputSelection && _camera._desiredVideoInputs.Count == 0)
						{
							_camera._desiredVideoInputs.Add(AVProLiveCameraPlugin.VideoInput.Video_Serial_Digital);
						}
						EditorGUILayout.BeginVertical();
						for (int index = 0; index < _camera._desiredVideoInputs.Count; index++)
						{
							EditorGUILayout.BeginHorizontal();
							if (GUILayout.Button("-"))
							{
								serializedObject.FindProperty("_desiredVideoInputs").DeleteArrayElementAtIndex(index);
								break;
							}
							else
							{
								SerializedProperty propDesiredVideoInput = serializedObject.FindProperty("_desiredVideoInputs").GetArrayElementAtIndex(index);
								EditorGUILayout.PropertyField(propDesiredVideoInput, new GUIContent(" "));
							}
							EditorGUILayout.EndHorizontal();
						}
						if (GUILayout.Button("+"))
						{
							serializedObject.FindProperty("_desiredVideoInputs").arraySize++;
							this.Repaint();
						}
						EditorGUILayout.EndVertical();
						break;
					case AVProLiveCamera.SelectDeviceBy.Index:
						{
							SerializedProperty propDesiredVideoInputIndex = serializedObject.FindProperty("_desiredVideoInputIndex");
							EditorGUILayout.PropertyField(propDesiredVideoInputIndex, new GUIContent(" "));
						}
						break;
				}
				GUILayout.EndVertical();
				EditorGUILayout.Space();

				SerializedProperty propDeinterlace = serializedObject.FindProperty("_deinterlace");
				EditorGUILayout.PropertyField(propDeinterlace);
			}
			else
			{
				if (_camera.Device != null)
				{
					_preview = GUILayout.Toggle(_preview, "Inspector Preview");

					AVProLiveCameraDevice device = _camera.Device;


					if (_preview && device.OutputTexture != null)
					{
						Rect textureRect = GUILayoutUtility.GetRect(64.0f, 64.0f, GUILayout.MinWidth(64.0f), GUILayout.MinHeight(64.0f));
						GUI.DrawTexture(textureRect, device.OutputTexture, ScaleMode.ScaleToFit, false);
						GUILayout.BeginHorizontal();
						GUILayout.FlexibleSpace();
						if (GUILayout.Button("Select Texture", GUILayout.ExpandWidth(false)))
						{
							Selection.activeObject = device.OutputTexture;
						}
						GUILayout.FlexibleSpace();
						GUILayout.EndHorizontal();
					}

					GUILayout.Label("Device: " + device.Name);
					GUILayout.Label(string.Format("Mode: {0}x{1} {2}", device.CurrentWidth, device.CurrentHeight, device.CurrentFormat));

					if (device.FramesTotal > 30)
					{
						GUILayout.Label("Displaying at " + device.DisplayFPS.ToString("F1") + " fps");
					}
					else
					{
						GUILayout.Label("Displaying at ... fps");
					}

					if (device.IsRunning)
					{
						GUILayout.BeginHorizontal();
						if (GUILayout.Button("Stop Camera"))
						{
							device.Close();
						}
						if (device.IsPaused)
						{
							if (GUILayout.Button("Unpause Stream"))
							{
								device.Play();
							}
						}
						else
						{
							if (GUILayout.Button("Pause Stream"))
							{
								device.Pause();
							}
						}
						GUILayout.EndHorizontal();
					}
					else
					{
						if (GUILayout.Button("Start Camera"))
						{
							_camera.Begin();
						}
					}

					GUI.enabled = device.CanShowConfigWindow();
					if (GUILayout.Button("Show Config Window"))
					{
						device.ShowConfigWindow();
					}
					GUI.enabled = true;

					_showSettings = GUILayout.Toggle(_showSettings, "Show Settings");
					if (_showSettings && device.NumSettings > 0)
					{
						EditorGUILayout.PrefixLabel("Device Settings", EditorStyles.boldLabel);
						EditorGUI.BeginChangeCheck();
						for (int j = 0; j < device.NumSettings; j++)
						{
							AVProLiveCameraSettingBase settingBase = device.GetVideoSettingByIndex(j);
							GUILayout.BeginHorizontal();
							GUI.enabled = !settingBase.IsAutomatic;
							if (GUILayout.Button("D", GUILayout.ExpandWidth(false)))
							{
								settingBase.SetDefault();
							}
							GUI.enabled = true;
							GUILayout.Label(settingBase.Name, GUILayout.ExpandWidth(false), GUILayout.MaxWidth(96.0f));
							GUI.enabled = !settingBase.IsAutomatic;
							switch (settingBase.DataTypeValue)
							{
								case AVProLiveCameraSettingBase.DataType.Boolean:
									AVProLiveCameraSettingBoolean settingBool = (AVProLiveCameraSettingBoolean)settingBase;
									settingBool.CurrentValue = GUILayout.Toggle(settingBool.CurrentValue, "", GUILayout.ExpandWidth(true));
									break;
								case AVProLiveCameraSettingBase.DataType.Float:
									AVProLiveCameraSettingFloat settingFloat = (AVProLiveCameraSettingFloat)settingBase;
									float sliderValue = GUILayout.HorizontalSlider(settingFloat.CurrentValue, settingFloat.MinValue, settingFloat.MaxValue, GUILayout.ExpandWidth(true));
									if (GUI.enabled)
										settingFloat.CurrentValue = sliderValue;

									GUILayout.Label(((long)settingFloat.CurrentValue).ToString(), GUILayout.Width(32.0f), GUILayout.ExpandWidth(false));
									GUI.enabled = settingBase.CanAutomatic;
									settingBase.IsAutomatic = GUILayout.Toggle(settingBase.IsAutomatic, "", GUILayout.Width(32.0f));
									GUI.enabled = true;
									break;

							}
							GUI.enabled = true;
							GUILayout.EndHorizontal();
						}
						if (GUILayout.Button("Defaults"))
						{
							for (int j = 0; j < device.NumSettings; j++)
							{
								AVProLiveCameraSettingBase settingBase = device.GetVideoSettingByIndex(j);
								settingBase.SetDefault();
							}
						}
						if (EditorGUI.EndChangeCheck() || (Time.frameCount % 30) == 0)
						{
							// This is an expensive function call so we want to limit it
							device.Update_Settings();
						}
					}
					//EditorGUILayout.Toggle("Running:", device.IsRunning);
				}

#if !AVPROLIVECAMERA_UNITYFEATURE_EDITORAUTOREFRESH
			if (_preview)
			{
				//HandleUtility.Repaint();
				this.Repaint();
			}			
#endif
			}

			GUI.enabled = true;

			EditorGUILayout.PropertyField(_propAllowTransparency);
			EditorGUILayout.PropertyField(_propFlipX);
			EditorGUILayout.PropertyField(_propFlipY);

			EditorGUILayout.Space();

			EditorGUILayout.PropertyField(_propHotSwap);
			EditorGUILayout.PropertyField(_propFrameRates);
			EditorGUILayout.PropertyField(_propSettings);

			if (GUI.changed)
			{
				EditorUtility.SetDirty(_camera);
			}

			serializedObject.ApplyModifiedProperties();
		}
	}
}