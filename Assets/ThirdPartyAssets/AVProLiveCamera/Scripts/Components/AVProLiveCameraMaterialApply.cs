using UnityEngine;

//-----------------------------------------------------------------------------
// Copyright 2012-2018 RenderHeads Ltd.  All rights reserverd.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProLiveCamera
{
	[AddComponentMenu("AVPro Live Camera/Material Apply")]
	public class AVProLiveCameraMaterialApply : MonoBehaviour
	{
		public Material _material;
		public AVProLiveCamera _liveCamera;
		private Texture _lastTexture;

		void Start()
		{
			if (_liveCamera != null && _liveCamera.OutputTexture != null)
			{
				ApplyMapping(_liveCamera.OutputTexture);
			}
		}

		void Update()
		{
			if (_liveCamera != null && _liveCamera.OutputTexture != null)
			{
				ApplyMapping(_liveCamera.OutputTexture);
			}
			else
			{
				ApplyMapping(null);
			}
		}

		private void ApplyMapping(Texture texture)
		{
			if (_lastTexture != texture)
			{
				if (_material != null)
				{
					_material.mainTexture = texture;
				}
				_lastTexture = texture;
			}
		}

		public void OnDisable()
		{
			ApplyMapping(null);
		}
	}
}