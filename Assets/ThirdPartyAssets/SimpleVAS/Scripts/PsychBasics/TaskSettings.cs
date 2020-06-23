using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityPsychBasics
{
    public class TaskSettings : MonoBehaviour 
    {

        public string sceneBeforeLastCondition, sceneAfterLastCondition;

        public static TaskSettings instance;

        public int currentTask;

        public bool withinScene, shuffleBool, useImageBool, useAnalogueScaleBool;

        public string minVASLabel, midVASLabel, maxVASLabel;
        public List<string> likertItems = new List<string>();

        public List<bool> shuffle = new List<bool>();
        public List<bool> useImage = new List<bool>();
        public List<bool> analogueScale = new List<bool>();
        public List<bool> useMouseClickSelector = new List<bool>();

        private void Awake() 
        {
            if (instance == null) instance = this;
        }

        private void Start() 
        {
            if (withinScene) SetWithinScene(false);
            else { //set for separate scenes
                MouseClickResponse.instance.ActivateSelector(shuffleBool);
                TaskManager.instance.useImages = useImageBool;
                TaskManager.instance.useAnalogueScale = useAnalogueScaleBool;
                TaskManager.instance.shuffle = shuffleBool;
                TaskManager.instance.InitializeValuesListsAndObjects();
            } 

            ScaleManager.instance.minVASLabel = minVASLabel;
            ScaleManager.instance.midVASLabel = midVASLabel;
            ScaleManager.instance.maxVASLabel = maxVASLabel;

            for (int i = 0; i < likertItems.Count; i++) 
                ScaleManager.instance.likertItems.Add(likertItems[i]);
        }

        public void LoadBeforeLast() 
        {
            if (!withinScene) TaskManager.instance.LoadScene(sceneBeforeLastCondition);
            else SetWithinScene(false); 
        }

        public void LoadAfterLast() 
        {
            if (!withinScene) TaskManager.instance.LoadScene(sceneAfterLastCondition);
            else SetWithinScene(true);
        }
        
        private void SetWithinScene(bool isLast) 
        {
            if (currentTask < useImage.Count) {
                MouseClickResponse.instance.ActivateSelector(useMouseClickSelector[currentTask]);
                TaskManager.instance.useImages = useImage[currentTask];
                TaskManager.instance.useAnalogueScale = analogueScale[currentTask];
                TaskManager.instance.shuffle = shuffle[currentTask];
                TaskManager.instance.InitializeValuesListsAndObjects();
                currentTask++;
            }
            else {
                if (!isLast) TaskManager.instance.LoadScene(sceneBeforeLastCondition);
                else TaskManager.instance.LoadScene(sceneAfterLastCondition);
            }
        }
    }
}
