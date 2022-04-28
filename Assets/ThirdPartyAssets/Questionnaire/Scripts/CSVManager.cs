using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

public class CSVManager : MonoBehaviour
{
    [SerializeField] private ResponseData _responseData;

    [SerializeField] private List<string> varNames = new List<string>();
    private List<string> varValues = new List<string>();
    private bool _newFile;
    
    private void Start ()
    {
        var fields = typeof(ResponseData).GetFields();

        foreach (var field in fields) {
            varValues.Add(null); //initialize varNames array
            varNames.Add(field.Name);
        }
    }
    
    private void WriteToFile(List<string> stringList)
    {
        string stringLine = string.Join(",", stringList.ToArray());
        string path = "./Logs/" + _responseData.subjectID + "-" + _responseData.pairID + "_log.csv";
        System.IO.StreamWriter file = new System.IO.StreamWriter(path, true);
        file.WriteLine(stringLine);
        file.Close();	
    }

    public void DataCollectionConsentGiven(bool given)  
    {
        _newFile = true;
    }
    
    public void NewDataAvailable()
    {
        if (_newFile)
        {
            WriteToFile(varNames);
            _newFile = false;
        }
        
        var fields = typeof(ResponseData).GetFields();
        for (int i=0; i<fields.Length; i++)
        {
            varValues[i] = fields[i].GetValue(_responseData).ToString();
        }
        WriteToFile(varValues);
    }
}
