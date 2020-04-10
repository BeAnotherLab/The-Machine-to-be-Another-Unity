#if UNITY_5_4_OR_NEWER || (UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_5)
	#define UNITY_FEATURE_UGUI
#endif

#if UNITY_FEATURE_UGUI
using System.Collections.Generic;
using UnityEngine.Serialization;
using UnityEngine;
using UnityEngine.UI;

//-----------------------------------------------------------------------------
// Copyright 2014-2018 RenderHeads Ltd.  All rights reserverd.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProLiveCamera
{
	[AddComponentMenu("AVPro Live Camera/uGUI Component")]
	public class AVProLiveCameraUGUIComponent : UnityEngine.UI.MaskableGraphic
	{
		[SerializeField]
		public AVProLiveCamera m_liveCamera;

		[SerializeField]
		public Rect m_UVRect = new Rect(0f, 0f, 1f, 1f);

		[SerializeField]
		public bool _setNativeSize = false;

		[SerializeField]
		public bool _keepAspectRatio = true;

		[SerializeField]
		public Texture _defaultTexture;

		private int _lastWidth;
		private int _lastHeight;

		protected AVProLiveCameraUGUIComponent()
		{ }


		/// <summary>
		/// Returns the texture used to draw this Graphic.
		/// </summary>
		public override Texture mainTexture
		{
			get
			{
				Texture result = Texture2D.whiteTexture;
				if (HasValidTexture())
				{
					result = m_liveCamera.OutputTexture;
				}
				else
				{
					if (_defaultTexture != null)
					{
						result = _defaultTexture;
					}
				}
				return result;
			}
		}

		public bool HasValidTexture()
		{
			return (m_liveCamera != null && m_liveCamera.OutputTexture != null);
		}

		void Update()
		{
			if (mainTexture == null)
			{
				return;
			}

			if (_setNativeSize)
			{
				SetNativeSize();
			}
			if (HasValidTexture())
			{
				if (mainTexture.width != _lastWidth ||
					mainTexture.height != _lastHeight)
				{
					_lastWidth = mainTexture.width;
					_lastHeight = mainTexture.height;
					SetVerticesDirty();
					SetMaterialDirty();
				}
			}
		}

		/// <summary>
		/// Texture to be used.
		/// </summary>
		public AVProLiveCamera source
		{
			get
			{
				return m_liveCamera;
			}
			set
			{
				if (m_liveCamera == value)
					return;

				m_liveCamera = value;
				//SetVerticesDirty();
				SetMaterialDirty();
			}
		}

		/// <summary>
		/// UV rectangle used by the texture.
		/// </summary>
		public Rect uvRect
		{
			get
			{
				return m_UVRect;
			}
			set
			{
				if (m_UVRect == value)
					return;
				m_UVRect = value;
				SetVerticesDirty();
			}
		}

		/// <summary>
		/// Adjust the scale of the Graphic to make it pixel-perfect.
		/// </summary>

		[ContextMenu("Set Native Size")]
		public override void SetNativeSize()
		{
			Texture tex = mainTexture;
			if (tex != null)
			{
				int w = Mathf.RoundToInt(tex.width * uvRect.width);
				int h = Mathf.RoundToInt(tex.height * uvRect.height);
				rectTransform.anchorMax = rectTransform.anchorMin;
				rectTransform.sizeDelta = new Vector2(w, h);
			}
		}

		// OnFillVBO deprecated by 5.2
		// OnPopulateMesh(Mesh mesh) deprecated by 5.2 patch 1
#if UNITY_5_4_OR_NEWER || (UNITY_5 && !UNITY_5_0 && !UNITY_5_1)
		/*		protected override void OnPopulateMesh(Mesh mesh)
			{			
				List<UIVertex> verts = new List<UIVertex>();
				_OnFillVBO( verts );

				var quad = new UIVertex[4];
				for (int i = 0; i < vbo.Count; i += 4)
				{
					vbo.CopyTo(i, quad, 0, 4);
					vh.AddUIVertexQuad(quad);
				}
				vh.FillMesh( toFill );
			}*/
		protected override void OnPopulateMesh(VertexHelper vh)
		{
			vh.Clear();

			List<UIVertex> aVerts = new List<UIVertex>();
			_OnFillVBO(aVerts);

			List<int> aIndicies = new List<int>(new int[] { 0, 1, 2, 2, 3, 0 });
			vh.AddUIVertexStream(aVerts, aIndicies);
		}
#else
		protected override void OnFillVBO(List<UIVertex> vbo)
		{
			_OnFillVBO( vbo );
		}
#endif

		/// <summary>
		/// Update all renderer data.
		/// </summary>
		protected void _OnFillVBO(List<UIVertex> vbo)
		{
			/*Texture tex = mainTexture;

			int texWidth = 4;
			int texHeight = 4;

			if (HasValidTexture())
			{

			}
			if (tex != null)
			{
				texWidth = tex.width;
				texHeight = tex.height;
			}*/

			{
				/*Vector4 v = Vector4.zero;

				int w = Mathf.RoundToInt(tex.width * uvRect.width);
				int h = Mathf.RoundToInt(tex.height * uvRect.height);

				float paddedW = ((w & 1) == 0) ? w : w + 1;
				float paddedH = ((h & 1) == 0) ? h : h + 1;

				v.x = 0f;
				v.y = 0f;
				v.z = w / paddedW;
				v.w = h / paddedH;

				v.x -= rectTransform.pivot.x;
				v.y -= rectTransform.pivot.y;
				v.z -= rectTransform.pivot.x;
				v.w -= rectTransform.pivot.y;

				v.x *= rectTransform.rect.width;
				v.y *= rectTransform.rect.height;
				v.z *= rectTransform.rect.width;
				v.w *= rectTransform.rect.height;*/

				Vector4 v = GetDrawingDimensions(_keepAspectRatio);

				vbo.Clear();

				var vert = UIVertex.simpleVert;

				vert.position = new Vector2(v.x, v.y);
				vert.uv0 = new Vector2(m_UVRect.xMin, m_UVRect.yMin);
				vert.color = color;
				vbo.Add(vert);

				vert.position = new Vector2(v.x, v.w);
				vert.uv0 = new Vector2(m_UVRect.xMin, m_UVRect.yMax);
				vert.color = color;
				vbo.Add(vert);

				vert.position = new Vector2(v.z, v.w);
				vert.uv0 = new Vector2(m_UVRect.xMax, m_UVRect.yMax);
				vert.color = color;
				vbo.Add(vert);

				vert.position = new Vector2(v.z, v.y);
				vert.uv0 = new Vector2(m_UVRect.xMax, m_UVRect.yMin);
				vert.color = color;
				vbo.Add(vert);
			}
		}

		//Added this method from Image.cs to do the keep aspect ratio
		private Vector4 GetDrawingDimensions(bool shouldPreserveAspect)
		{
			Vector4 returnSize = Vector4.zero;

			if (mainTexture)
			{
				var padding = Vector4.zero;
				var textureSize = new Vector2(mainTexture.width, mainTexture.height);

				Rect r = GetPixelAdjustedRect();

				int spriteW = Mathf.RoundToInt(textureSize.x);
				int spriteH = Mathf.RoundToInt(textureSize.y);

				var size = new Vector4(padding.x / spriteW,
										padding.y / spriteH,
										(spriteW - padding.z) / spriteW,
										(spriteH - padding.w) / spriteH);

				if (shouldPreserveAspect && textureSize.sqrMagnitude > 0.0f)
				{
					var spriteRatio = textureSize.x / textureSize.y;
					var rectRatio = r.width / r.height;

					if (spriteRatio > rectRatio)
					{
						var oldHeight = r.height;
						r.height = r.width * (1.0f / spriteRatio);
						r.y += (oldHeight - r.height) * rectTransform.pivot.y;
					}
					else
					{
						var oldWidth = r.width;
						r.width = r.height * spriteRatio;
						r.x += (oldWidth - r.width) * rectTransform.pivot.x;
					}
				}

				returnSize = new Vector4(r.x + r.width * size.x,
										  r.y + r.height * size.y,
										  r.x + r.width * size.z,
										  r.y + r.height * size.w);

			}

			return returnSize;
		}
	}
}

#endif