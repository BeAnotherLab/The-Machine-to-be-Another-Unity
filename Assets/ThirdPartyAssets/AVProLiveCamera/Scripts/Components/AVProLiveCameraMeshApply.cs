using UnityEngine;

//-----------------------------------------------------------------------------
// Copyright 2012-2018 RenderHeads Ltd.  All rights reserverd.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProLiveCamera
{
	[AddComponentMenu("AVPro Live Camera/Mesh Apply")]
	public class AVProLiveCameraMeshApply : MonoBehaviour
	{
		public MeshRenderer _mesh;
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
				if (_mesh != null)
				{
					Material[] materials = _mesh.materials;
					for (int i = 0; i < materials.Length; i++)
					{
						materials[i].mainTexture = texture;
					}
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