﻿/*
	Created by Carl Emil Carlsen.
	Copyright 2016 Sixth Sensor.
	All rights reserved.
	http://sixthsensor.dk
*/

using UnityEngine;

namespace OscSimpl.Examples
{
	public class GettingStartedReceiving : MonoBehaviour
	{
		public OscIn oscIn;
		public Transform screen;
		public float speed;

		void Start()
		{
			// Ensure that we have a OscIn component.
			if( !oscIn ) oscIn = gameObject.AddComponent<OscIn>();

			// Start receiving from unicast and broadcast sources on port 7000.
			oscIn.Open(8015);
		}


		void OnEnable()
		{
			// You can "map" messages to methods in three ways:

			// 1) For messages with one argument, simply provide the address and
			// a method with one argument. In this case, OnTest1 takes a float argument.
			//oscIn.Map( "/test1", OnTest1 );

			// 2) The same can be achieved using a delgate.
			//oscIn.Map( "/test2", delegate( float value ){ Debug.Log( "Received: " + value ); });

			// 3) For messages with multiple arguments, provide the address and a method
			// that takes a OscMessage object argument, then process the message manually.
			// See the OnTest3 method.
			oscIn.Map( "/pose", OnTest3 );
		}


		void OnDisable()
		{
			// If you want to stop receiving messages you have to "unmap".

			// For mapped methods, simply pass them to Unmap.
	//		oscIn.Unmap( OnTest1 );
			oscIn.Unmap( OnTest3 );

			// For mapped delegates, pass the address. Note that this will cause all mappings 
			// made to that address to be unmapped.
			oscIn.Unmap( "/test2" );
		}

		
		void OnTest1( float value )
		{
	//		Debug.Log( "Received: " + value );
		}


		void OnTest3( OscMessage message )
		{
			// Get string arguments at index 0 and 1 safely.
			float x = 0, y = 0, z = 0, w = 0;

			if (message.TryGet (0, out x) && message.TryGet (1, out y) && message.TryGet (2, out z) && message.TryGet (3, out w)) {
				//Debug.Log ("Chino receive: " + x + " " + y + " " + z + " " + w);
			}

			Quaternion orientation = new Quaternion (x, y, z, w);
			Vector3 orientationInAngles = new Vector3 (x, y, z);
			orientationInAngles = orientation.eulerAngles;
			Vector3 orientationMapped = new Vector3 (x, y, z);

			Debug.Log ("Euler orientation: " + orientationInAngles);


			//angle mapping
			if (orientationInAngles.y < 180)
				orientationMapped.x = orientationInAngles.y + 180;
			else if (orientationInAngles.y > 180)
				orientationMapped.x =  orientationInAngles.y - 180;

			if (orientationInAngles.x < 180)
				orientationMapped.y = orientationInAngles.x + 180;
			else if (orientationInAngles.x > 180)
				orientationMapped.y =  orientationInAngles.x - 180;
			
			screen.transform.position = new Vector3((orientationMapped.x-180)*speed, (180-orientationMapped.y)*speed, 300); //((orientationInAngles.x/360) - 0.5f) * speed, 1);
			//GameObject.GetComponent<webcam> ().rece = ;

			Debug.Log (screen.transform.position.x);

			// If you wish to mess with the arguments yourself, you can.
			//foreach( object a in message.args ) if( a is string ) Debug.Log( "Received: " + a );

			// NEVER DO THIS AT HOME
			// Never cast directly, without ensuring that index is inside bounds and encapsulating
			// the cast in try-catch statement.
			//float value = (float) message.args[0]; // No no!
		}
	}
}