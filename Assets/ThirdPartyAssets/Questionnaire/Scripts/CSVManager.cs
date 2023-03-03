using System.Collections.Generic;
using UnityEngine;

public class CSVManager : MonoBehaviour
{
    [SerializeField] private ExperimentData _experimentData;

    [SerializeField] private List<string> varNames = new List<string>();
    private List<string> varValues = new List<string>();

    private bool _newFile;
    
    private void Start ()
    {
        var fields = typeof(ExperimentData).GetFields();

        foreach (var field in fields) {
            varValues.Add(null); //initialize varNames array
            varNames.Add(field.Name);
        }

        _newFile = true;
    }
    
    private void WriteToFile(List<string> stringList)
    {
        string stringLine = string.Join(",", stringList.ToArray());
        string path = "./Logs/" + _experimentData.subjectID + "-" + _experimentData.otherID + "_log.csv";
        System.IO.StreamWriter file = new System.IO.StreamWriter(path, true);
        file.WriteLine(stringLine);
        file.Close();	
    }

    public void NewDataAvailable()
    {
        if (_newFile)
        {
            WriteToFile(varNames);
            _newFile = false;
        }
        var fields = typeof(ExperimentData).GetFields();
        for (int i=0; i<fields.Length; i++)
        {
            varValues[i] = fields[i].GetValue(_experimentData).ToString();
        }
        WriteToFile(varValues);
    }
    
    public void SetRole(int role)
    {
        if (role == 0) _experimentData.participantType = ParticipantType.leader;
        else if (role == 1) _experimentData.participantType = ParticipantType.follower;
        else if (role == 2) _experimentData.participantType = ParticipantType.free;
    }
}
