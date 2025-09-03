using UnityEngine;
using UnityEditor;
using System.Collections;

//-----------------------------------------------------------------------------
// Copyright 2012-2018 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProLiveCamera.Editor
{
	[CustomEditor(typeof(AVProLiveCameraManager))]
	public class AVProLiveCameraManagerEditor : UnityEditor.Editor
	{
		private AVProLiveCameraManager _manager;

		void OnEnable()
		{
			_manager = (this.target) as AVProLiveCameraManager;
		}

		void OnDisable()
		{
			_manager = null;
		}

		public override void OnInspectorGUI()
		{
			if (_manager == null)
				return;

			if (!Application.isPlaying)
			{
				DrawDefaultInspector();
			}
			else
			{
				EditorGUILayout.Space();

				int numDevices = _manager.NumDevices;
				EditorGUILayout.PrefixLabel("Devices: ");
				for (int deviceIndex = 0; deviceIndex < numDevices; deviceIndex++)
				{
					EditorGUILayout.BeginHorizontal();
					AVProLiveCameraDevice device = _manager.GetDevice(deviceIndex);
					EditorGUILayout.LabelField(deviceIndex.ToString() + ") " + device.Name, "");
					if (device.IsRunning)
						EditorGUILayout.LabelField("Display at " + device.DisplayFPS.ToString("F1") + " FPS", "");
					else
						EditorGUILayout.LabelField("Stopped", "");
					EditorGUILayout.EndHorizontal();
				}
				EditorGUILayout.Space();
			}
		}
	}
}