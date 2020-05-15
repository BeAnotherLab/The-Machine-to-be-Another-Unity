using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AffectiveTestManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    public void StartInstructions(string subjectID, string prepost)
    {
        _prepost = prepost;
        _subjectID = subjectID;
        var files = Directory.GetFiles(Application.dataPath);
        
        string filepath = Application.dataPath + "/" + "AffectiveTest" + subjectID + "_log.json";
        
        if (!File.Exists(filepath))
        {
            Debug.Log(" creating new file : " + filepath);
            _subjectID = subjectID;
            _filePath = filepath; 
            AffectiveTestInstructionsGUI.instance.Init();
            AffectiveTestSettingsGUI.instance.gameObject.SetActive(false); //hide settings GUI
            _currentStep = steps.instructions;       
        }
        else AffectiveTestSettingsGUI.instance.ShowExistingSubjectIDError();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
