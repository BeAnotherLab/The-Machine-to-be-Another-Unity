using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityPsychBasics
{
	public class DebriefingText : MonoBehaviour {

		public Text comments;

		// Use this for initialization
		void Start () {
			//WriteToFile ("subject ID", "hallucinations", "comments");
		}

		public void onNextButtonPressed(){

			if (BasicDataConfigurations.ID == null)	BasicDataConfigurations.ID = "na";
			WriteToFile (comments.text);
		}

		void WriteToFile(string a){

			string stringLine =  a;

			System.IO.StreamWriter file = new System.IO.StreamWriter("./Logs/" + BasicDataConfigurations.ID + "_debriefing.txt", true);
			file.WriteLine(stringLine);
			file.Close();	
		}
	}

}