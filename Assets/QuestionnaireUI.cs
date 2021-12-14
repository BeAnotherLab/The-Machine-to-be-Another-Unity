using System.Collections;
using System.Collections.Generic;
using ScriptableObjectArchitecture;
using UnityEngine;

public class QuestionnaireUI : MonoBehaviour
{
    [SerializeField] private GameObject _questionnairePanelGameObject;

    public void ReadyToShowQuestionnaire(bool answer)
    {
        _questionnairePanelGameObject.GetComponent<PanelDimmer>().Show(answer);
    }

    public void UserStateChanged(UserState selfState)
    {
        if (selfState == UserState.headsetOff) //hide if user left 
            _questionnairePanelGameObject.GetComponent<PanelDimmer>().Show(false);   
    }
}
