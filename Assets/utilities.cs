using UnityEngine;
using System.Collections;

public static class utilities {

	public static Vector3 toEulerAngles(Quaternion q)
	{
		// Store the Euler angles in radians
		Vector3 pitchYawRoll = new Vector3();

		float sqw = q.w * q.w;
		float sqx = q.x * q.x;
		float sqy = q.y * q.y;
		float sqz = q.z * q.z;

		// If quaternion is normalised the unit is one, otherwise it is the correction factor
		float unit = sqx + sqy + sqz + sqw;
		float test = q.x * q.y + q.z * q.w;

		if (test > 0.4999f * unit)                              // 0.4999f OR 0.5f - EPSILON
		{
			// Singularity at north pole
			pitchYawRoll.y = 2f * (float)Mathf.Atan2(q.x, q.w);  // Yaw
			pitchYawRoll.x = Mathf.PI * 0.5f;                         // Pitch
			pitchYawRoll.z = 0f;                                // Roll
			return pitchYawRoll;
		}
		else if (test < -0.4999f * unit)                        // -0.4999f OR -0.5f + EPSILON
		{
			// Singularity at south pole
			pitchYawRoll.y = -2f * (float)Mathf.Atan2(q.x, q.w); // Yaw
			pitchYawRoll.x = -Mathf.PI * 0.5f;                        // Pitch
			pitchYawRoll.z = 0f;                                // Roll
			return pitchYawRoll;
		}
		else
		{
			pitchYawRoll.y = (float)Mathf.Atan2(2f * q.y * q.w - 2f * q.x * q.z, sqx - sqy - sqz + sqw) * 180/Mathf.PI;       // Yaw
			pitchYawRoll.x = (float)Mathf.Asin(2f * test / unit) * 180/Mathf.PI;                                              // Pitch
			pitchYawRoll.z = (float)Mathf.Atan2(2f * q.x * q.w - 2f * q.y * q.z, -sqx + sqy - sqz + sqw) * 180/Mathf.PI;      // Roll
		}

		return pitchYawRoll;
	}

}
