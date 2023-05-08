using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CsvWrite : MonoBehaviour {

    [Tooltip("id, age, gender, handedness")]
    public string[] participantData = new string[4];
    public bool log;

    public static CsvWrite instance;

    void Awake() {
        if (instance == null)
            instance = this;
    }

    void Start() {
        string[] header = new string[] { "subject ID", "age", "gender", "handedness", "question ID", "condition", "side", "time", "response" };
        if(log) WriteToFile(header);
    }

    public void WriteLine(string[] variables) {

        string[] variableList = new string[participantData.Length + variables.Length];//new array to concatenate participant data + input variables.
        participantData.CopyTo(variableList, 0);
        variables.CopyTo(variableList, participantData.Length);

       if(log) WriteToFile(variableList);
    }


    void WriteToFile(string[] varList) {

        string stringLine = string.Join(",", varList);

        System.IO.StreamWriter file = new System.IO.StreamWriter("./Logs/" + varList[0] + "_log.csv", true);
        file.WriteLine(stringLine);
        file.Close();
    }
}