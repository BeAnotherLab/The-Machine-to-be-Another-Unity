using UnityEngine;

//-----------------------------------------------------------------------------
// Copyright 2012-2018 RenderHeads Ltd.  All rights reserverd.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProLiveCamera.Demos
{
	public class AVProLiveCameraMaterialMappingDemo : MonoBehaviour
	{
		public Transform _sphere;
		private float _t = 0.0f;
		public float _speed = 1.0f;

		void Update()
		{
			if (_sphere != null)
			{
				_t += Time.deltaTime * _speed;
				float t = Mathf.PingPong(_t, 5.0f) / 5.0f;
				t = Mathf.SmoothStep(0, 1, t);
				//t = Mathf.SmoothStep(0, 1, t);
				//t = Mathf.SmoothStep(0, 1, t);
				float x = Mathf.Lerp(25.33046f, -25.0f, t);
				_sphere.position = new Vector3(x, _sphere.position.y, _sphere.position.z);
			}
		}
	}
}