using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

//-----------------------------------------------------------------------------
// Copyright 2015-2020 RenderHeads Ltd.  All rights reserverd.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProLiveCamera.Editor
{
	/// <summary>
	/// Editor for the AVProLiveCameraMaterialApply component
	/// </summary>
	[CanEditMultipleObjects]
	[CustomEditor(typeof(AVProLiveCameraMeshApply))]
	public class AVProLiveCameraMeshApplyEditor : UnityEditor.Editor
	{
		private readonly static GUIContent _guiTextTextureProperty = new GUIContent("Texture Property");
		//private readonly static string DefaultTextureUniformName = "_MainTex";
		private readonly static string HDRPTextureUniformName = "_BaseColorMap";

		private SerializedProperty _propLiveCamera;
		private SerializedProperty _propMesh;
		private SerializedProperty _propTexturePropertyName;
		private GUIContent[] _materialTextureProperties = new GUIContent[0];

		void OnEnable()
		{
			_propLiveCamera = serializedObject.FindProperty("_liveCamera");
			_propMesh = serializedObject.FindProperty("_mesh");
			_propTexturePropertyName = serializedObject.FindProperty("_texturePropertyName");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUI.BeginDisabledGroup(Application.isPlaying);

			EditorGUILayout.PropertyField(_propLiveCamera);
			EditorGUILayout.PropertyField(_propMesh);

			bool isHDRP = false;
			int texturePropertyIndex = -1;

			// TODO: don't do this every frame (expensive)
			if (_propMesh.objectReferenceValue != null)
			{
				MeshRenderer renderer = (MeshRenderer)(_propMesh.objectReferenceValue);
				Material[] materials = renderer.sharedMaterials;
				MaterialProperty[] matProps = MaterialEditor.GetMaterialProperties(materials);

				List<GUIContent> items = new List<GUIContent>(16);
				foreach (MaterialProperty matProp in matProps)
				{
					if (matProp.type == MaterialProperty.PropType.Texture)
					{
						if (matProp.name == _propTexturePropertyName.stringValue)
						{
							texturePropertyIndex = items.Count;
						}
						if (matProp.name == HDRPTextureUniformName)
						{
							isHDRP = true;
						}
						items.Add(new GUIContent(matProp.name));
					}
				}
				_materialTextureProperties = items.ToArray();
			}

			EditorGUILayout.Space();

			EditorGUILayout.PropertyField(_propTexturePropertyName, _guiTextTextureProperty);

			if (isHDRP && _propTexturePropertyName.stringValue != HDRPTextureUniformName)
			{
				EditorGUILayout.HelpBox("Select _BaseColorMap for HDRP", MessageType.Info);
			}
			if (texturePropertyIndex < 0)
			{
				EditorGUILayout.HelpBox("Texture property name '" + _propTexturePropertyName.stringValue + "' not found in material", MessageType.Warning);
			}

			int newTexturePropertyIndex = EditorGUILayout.Popup(texturePropertyIndex, _materialTextureProperties);
			if (newTexturePropertyIndex >=0 && newTexturePropertyIndex != texturePropertyIndex)
			{
				_propTexturePropertyName.stringValue = _materialTextureProperties[newTexturePropertyIndex].text;
			}

			EditorGUI.EndDisabledGroup();

			serializedObject.ApplyModifiedProperties();
		}
	}
}