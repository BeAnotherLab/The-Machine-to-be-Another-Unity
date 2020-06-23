using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityPsychBasics
{
        public class TaskSettings : MonoBehaviour {

        public string sceneBeforeLastCondition, sceneAfterLastCondition;

        public static TaskSettings instance;

        public int currentTask;

        [SerializeField]
        public bool withinScene, shuffleBool, useImageBool, useAnalogueScaleBool, useMouseBool;

        public string minVASLabel, midVASLabel, maxVASLabel;
        public List<string> likertItems = new List<string>();

        public int numberOfConditions;

        public List<bool> shuffle = new List<bool>();
        public List<bool> useImage = new List<bool>();
        public List<bool> analogueScale = new List<bool>();
        public List<bool> useMouseClickSelector = new List<bool>();

        private TaskManager _taskManager;
        private ScaleManager _scaleSettings;
        private MouseClickResponse _mouseClickResponse;
        
        private void Awake() {
            if (instance == null)
                instance = this;
        }

        private void Start() {
            _taskManager = TaskManager.instance;
            _scaleSettings = ScaleManager.instance;

            _mouseClickResponse = MouseClickResponse.instance;

           
            if (withinScene)
                SetWithinScene(false);
            else
                SetForSeparateScenes();

            SetScaleSettings();
        }

        private void SetScaleSettings(){
            _scaleSettings.minVASLabel = minVASLabel;
            _scaleSettings.midVASLabel = midVASLabel;
            _scaleSettings.maxVASLabel = maxVASLabel;

            for (int i = 0; i < likertItems.Count; i++) {
                _scaleSettings.likertItems.Add(likertItems[i]);
            }
        }

        public void LoadBeforeLast() {
            if (!withinScene)
                _taskManager.LoadScene(sceneBeforeLastCondition);
            else 
                SetWithinScene(false);                         
        }

        public void LoadAfterLast() {
            if (!withinScene)
                _taskManager.LoadScene(sceneAfterLastCondition);
            else
                SetWithinScene(true);
        }

        private void SetForSeparateScenes(){
            _mouseClickResponse.ActivateSelector(shuffleBool);
            _taskManager.useImages = useImageBool;
            _taskManager.useAnalogueScale = useAnalogueScaleBool;
            _taskManager.shuffle = shuffleBool;

            _taskManager.InitializeValuesListsAndObjects();
        }

        private void SetWithinScene(bool isLast) {


            if (currentTask < useImage.Count) {

                _mouseClickResponse.ActivateSelector(useMouseClickSelector[currentTask]);
                _taskManager.useImages = useImage[currentTask];
                _taskManager.useAnalogueScale = analogueScale[currentTask];
                _taskManager.shuffle = shuffle[currentTask];
                
                _taskManager.InitializeValuesListsAndObjects();

                currentTask++;
            }

            else {
                if(!isLast)
                    _taskManager.LoadScene(sceneBeforeLastCondition);
                else
                    _taskManager.LoadScene(sceneAfterLastCondition);
            }

        }

                
    }
}
