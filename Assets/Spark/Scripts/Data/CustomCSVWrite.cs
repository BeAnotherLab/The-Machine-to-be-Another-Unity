using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace UnityPsychBasics {
	public class CustomCSVWrite : MonoBehaviour {

        public static CustomCSVWrite instance;

        [Tooltip("Note that 9 variables are coded in, edit script if this has to change, here you can only change the name")]
        [SerializeField] private List<string> varNames = new List<string>();
        
        [HideInInspector] public int item, condition;
        [HideInInspector] public float response;

        [SerializeField] private ExperimentData _experimentData;
        private List<string> varValues = new List<string>();
        
		//This allows the start function to be called only once.
		private void Awake()
		{
            if (instance == null) instance = this;
		}

		private void Start () 
		{
            foreach (var item in varNames) varValues.Add(null); //initialize varNames array
            WriteToFile(varNames); //write column names
        }
		
		public void LogTrial()
		{
			varValues[0] = _experimentData.subjectID;
			varValues[1] = SceneManager.GetActiveScene().name;
			varValues[2] = item.ToString();
			varValues[3] = response.ToString();
			
            WriteToFile(varValues);
        }

		public void WriteResult(int currentItem, float responseValue)
		{
			response = responseValue;
			item = currentItem;
			instance.LogTrial();
		}
		
		private void WriteToFile(List<string> stringList)
		{
            string stringLine = string.Join(",", stringList.ToArray());
            string path = "./Logs/" + _experimentData.subjectID + "_log.csv";
			System.IO.StreamWriter file = new System.IO.StreamWriter(path, true);
			file.WriteLine(stringLine);
			file.Close();	
		}

	}
}