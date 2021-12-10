using System.Collections;
using System.Collections.Generic;
using ScriptableObjectArchitecture;
using UnityEngine;

public class QuestionnaireUI : MonoBehaviour
{
    public void ReadyToShowQuestionnaire(bool answer)
    {
        GetComponent<PanelDimmer>().Show(answer);
    }    
}
