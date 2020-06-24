using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityPsychBasics
{
    public class CustomTaskSettings : MonoBehaviour 
    {

        public string sceneBeforeLastCondition, sceneAfterLastCondition;

        public static CustomTaskSettings instance;

        public int currentTask;

        public bool withinScene, shuffleBool, useImageBool, useAnalogueScaleBool;

        public string minVASLabel, midVASLabel, maxVASLabel;
        public List<string> likertItems = new List<string>();

        public List<bool> shuffle = new List<bool>();
        public List<bool> useImage = new List<bool>();
        public List<bool> analogueScale = new List<bool>();

        private void Awake() 
        {
            if (instance == null) instance = this;
        }

        private void Start() 
        {
            if (withinScene) SetWithinScene(false);
            else { //set for separate scenes
                CustomTaskManager.instance.useImages = useImageBool;
                CustomTaskManager.instance.useAnalogueScale = useAnalogueScaleBool;
                CustomTaskManager.instance.shuffle = shuffleBool;
                CustomTaskManager.instance.InitializeValuesListsAndObjects();
            } 

            CustomScaleManager.instance.minVASLabel = minVASLabel;
            CustomScaleManager.instance.midVASLabel = midVASLabel;
            CustomScaleManager.instance.maxVASLabel = maxVASLabel;

            for (int i = 0; i < likertItems.Count; i++) 
                CustomScaleManager.instance.likertItems.Add(likertItems[i]);
        }

        public void LoadBeforeLast() 
        {
            if (!withinScene) CustomTaskManager.instance.LoadScene(sceneBeforeLastCondition);
            else SetWithinScene(false); 
        }

        public void LoadAfterLast() 
        {
            if (!withinScene) CustomTaskManager.instance.LoadScene(sceneAfterLastCondition);
            else SetWithinScene(true);
        }
        
        private void SetWithinScene(bool isLast) 
        {
            if (currentTask < useImage.Count) {
                CustomTaskManager.instance.useImages = useImage[currentTask];
                CustomTaskManager.instance.useAnalogueScale = analogueScale[currentTask];
                CustomTaskManager.instance.shuffle = shuffle[currentTask];
                CustomTaskManager.instance.InitializeValuesListsAndObjects();
                currentTask++;
            }
            else {
                if (!isLast) CustomTaskManager.instance.LoadScene(sceneBeforeLastCondition);
                else CustomTaskManager.instance.LoadScene(sceneAfterLastCondition);
            }
        }
    }
}
