/* ArduinoConnector by Alan Zucconi
 * http://www.alanzucconi.com/?p=2979
 */
using UnityEngine;
using System;
using System.Collections;
using System.IO.Ports;

public class arduinoControl : MonoBehaviour {

	/* The serial port where the Arduino is connected. */
	[Tooltip("The serial port where the Arduino is connected")]
	public string port = "COM3";
	/* The baudrate of the serial port. */
	[Tooltip("The baudrate of the serial port")]
	public int baudrate = 57600;

	private SerialPort stream;
	public float pitchOffset, yawOffset;

	void Start(){
		Open ();
		getPlayerPrefs ();
	}

	public void Open () {
		// Opens the serial port
		stream = new SerialPort(port, baudrate);
		stream.ReadTimeout = 50;
		stream.Open();
		//this.stream.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
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
		
	public void setPitchOffset(float value) {
		pitchOffset = value;
		PlayerPrefs.SetFloat ("pitchOffset", pitchOffset);
	}

	public void setYawOffset(float value) {
		yawOffset = value;
		PlayerPrefs.SetFloat ("yawOffset", yawOffset);
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