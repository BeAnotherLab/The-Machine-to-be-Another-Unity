#if UNITY_5_4_OR_NEWER || (UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_5)
	#define UNITY_FEATURE_UGUI
#endif

using UnityEngine;
using UnityEditor;
#if UNITY_FEATURE_UGUI
using UnityEngine.UI;
using UnityEditor.UI;

//-----------------------------------------------------------------------------
// Copyright 2014-2018 RenderHeads Ltd.  All rights reserverd.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProLiveCamera.Editor
{
	/// <summary>
	/// Editor class used to edit UI Images.
	/// </summary>
	[CustomEditor(typeof(AVProLiveCameraUGUIComponent), true)]
	[CanEditMultipleObjects]
	public class AVProLiveCameraUGUIComponentEditor : GraphicEditor
	{
		SerializedProperty m_Camera;
		SerializedProperty m_UVRect;
		SerializedProperty m_DefaultTexture;
		SerializedProperty m_SetNativeSize;
		SerializedProperty m_KeepAspectRatio;
		GUIContent m_UVRectContent;

		[MenuItem("GameObject/UI/AVPro Live Camera uGUI", false, 0)]
		public static void CreateGameObject()
		{
			GameObject parent = Selection.activeGameObject;
			RectTransform parentCanvasRenderer = (parent != null) ? parent.GetComponent<RectTransform>() : null;
			if (parentCanvasRenderer)
			{
				GameObject go = new GameObject("AVPro Live Camera");
				go.transform.SetParent(parent.transform, false);
				go.AddComponent<RectTransform>();
				go.AddComponent<CanvasRenderer>();
				go.AddComponent<AVProLiveCameraUGUIComponent>();
				Selection.activeGameObject = go;
			}
			else
			{
				EditorUtility.DisplayDialog("AVPro Live Camera", "You must make the AVPro Live Camera uGUI object as a child of a Canvas.", "Ok");
			}
		}

		public override bool RequiresConstantRepaint()
		{
			AVProLiveCameraUGUIComponent rawImage = target as AVProLiveCameraUGUIComponent;
			return (rawImage != null && rawImage.HasValidTexture());
		}

		protected override void OnEnable()
		{
			base.OnEnable();

			// Note we have precedence for calling rectangle for just rect, even in the Inspector.
			// For example in the Camera component's Viewport Rect.
			// Hence sticking with Rect here to be consistent with corresponding property in the API.
			m_UVRectContent = new GUIContent("UV Rect");

			m_Camera = serializedObject.FindProperty("m_liveCamera");
			m_UVRect = serializedObject.FindProperty("m_UVRect");
			m_SetNativeSize = serializedObject.FindProperty("_setNativeSize");
			m_KeepAspectRatio = serializedObject.FindProperty("_keepAspectRatio");

			m_DefaultTexture = serializedObject.FindProperty("_defaultTexture");

			SetShowNativeSize(true);
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(m_Camera);
			EditorGUILayout.PropertyField(m_DefaultTexture);
			AppearanceControlsGUI();
			EditorGUILayout.PropertyField(m_UVRect, m_UVRectContent);

			EditorGUILayout.PropertyField(m_SetNativeSize);
			EditorGUILayout.PropertyField(m_KeepAspectRatio);

			SetShowNativeSize(false);
			NativeSizeButtonGUI();

			serializedObject.ApplyModifiedProperties();
		}

		void SetShowNativeSize(bool instant)
		{
			base.SetShowNativeSize(m_Camera.objectReferenceValue != null, instant);
		}

		/// <summary>
		/// Allow the texture to be previewed.
		/// </summary>

		public override bool HasPreviewGUI()
		{
			AVProLiveCameraUGUIComponent rawImage = target as AVProLiveCameraUGUIComponent;
			return rawImage != null;
		}

		/// <summary>
		/// Draw the Image preview.
		/// </summary>

		public override void OnPreviewGUI(Rect drawArea, GUIStyle background)
		{
			AVProLiveCameraUGUIComponent rawImage = target as AVProLiveCameraUGUIComponent;
			Texture tex = rawImage.mainTexture;

			if (tex == null)
				return;

			// Create the texture rectangle that is centered inside rect.
			Rect outerRect = drawArea;
			EditorGUI.DrawTextureTransparent(outerRect, tex, ScaleMode.ScaleToFit);//, outer.width / outer.height);
																				   //SpriteDrawUtility.DrawSprite(tex, rect, outer, rawImage.uvRect, rawImage.canvasRenderer.GetColor());
		}

		/// <summary>
		/// Info String drawn at the bottom of the Preview
		/// </summary>

		public override string GetInfoString()
		{
			AVProLiveCameraUGUIComponent rawImage = target as AVProLiveCameraUGUIComponent;

			string text = string.Empty;
			if (rawImage.HasValidTexture())
			{
				text += string.Format("Video Size: {0}x{1}\n",
										Mathf.RoundToInt(Mathf.Abs(rawImage.mainTexture.width)),
										Mathf.RoundToInt(Mathf.Abs(rawImage.mainTexture.height)));
			}

			// Image size Text
			text += string.Format("Display Size: {0}x{1}",
					Mathf.RoundToInt(Mathf.Abs(rawImage.rectTransform.rect.width)),
					Mathf.RoundToInt(Mathf.Abs(rawImage.rectTransform.rect.height)));

			return text;
		}
	}
}
#endif