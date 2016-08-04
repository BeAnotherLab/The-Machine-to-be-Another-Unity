/* ArduinoConnector by Alan Zucconi
 * http://www.alanzucconi.com/?p=2979
 */
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.IO.Ports;

public class arduinoControl : MonoBehaviour {

	/* The baudrate of the serial port. */
	[Tooltip("The baudrate of the serial port")]
	public int baudrate = 57600;
	public float pitchOffset, yawOffset;

	private SerialPort stream;

	void Start(){
	}

	public void Open (int p) {
		string[] ports = SerialPort.GetPortNames ();
		string port = ports[p];
		stream = new SerialPort(port, baudrate);
		stream.Open();
	}

	public void setPitch(float value){
		float sum;
		sum = value + pitchOffset;
		if ((value + pitchOffset) > 180) sum = 179.5f;
		if ((value + pitchOffset) < 0) sum = 0.5f;
		WriteToArduino("Pitch " + sum);
	}

	public void setYaw(float value) {
		float sum;
		sum = value + yawOffset;
		if ((value + yawOffset) > 180) sum = 179.5f;
		if ((value + yawOffset) < 0) sum = 0.5f;
		WriteToArduino("Yaw " + sum);
	}
		
	public void WriteToArduino(string message)
	{
		// Send the request
		stream.WriteLine(message);
		stream.BaseStream.Flush();
	}

	public string ReadFromArduino(int timeout = 0)
	{
		stream.ReadTimeout = timeout;
		try
		{
			return stream.ReadLine();
		}
		catch (TimeoutException)
		{
			return null;
		}
	}

	public IEnumerator AsynchronousReadFromArduino(Action<string> callback, Action fail = null, float timeout = float.PositiveInfinity)
	{
		DateTime initialTime = DateTime.Now;
		DateTime nowTime;
		TimeSpan diff = default(TimeSpan);

		string dataString = null;

		do
		{
			// A single read attempt
			try
			{
				dataString = stream.ReadLine();
			}
			catch (TimeoutException)
			{
				dataString = null;
			}

			if (dataString != null)
			{
				callback(dataString);
				yield return null;
			} else
				yield return new WaitForSeconds(0.05f);

			nowTime = DateTime.Now;
			diff = nowTime - initialTime;

		} while (diff.Milliseconds < timeout);

		if (fail != null)
			fail();
		yield return null;
	}

	void getPlayerPrefs(){
		pitchOffset = PlayerPrefs.GetFloat ("pitchOffset");
		yawOffset = PlayerPrefs.GetFloat ("yawOffset");
	}

	public void Close()
	{
		stream.Close();
	}

	void Update() {
			
	}
}