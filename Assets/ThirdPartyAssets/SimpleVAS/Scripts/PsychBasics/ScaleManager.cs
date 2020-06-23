using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityPsychBasics
{
    public class ScaleManager : MonoBehaviour {

        public GameObject togglePrefab;
        public ToggleGroup toggleGroup;

        public GameObject scrollbar;

        [HideInInspector]
        public string minVASLabel, midVASLabel, maxVASLabel;

        [HideInInspector]
        public List<string> likertItems = new List<string>();

        private TaskManager _taskManager;
        private LabelNames _labelNames;

        public static ScaleManager instance;

        private void Awake()
        {
            if (instance == null)
                instance = this;

            _taskManager = TaskManager.instance;
            _labelNames = LabelNames.instance;
        }

        public void CreateToggles(){

            foreach(Transform child in toggleGroup.GetComponent<Transform>()){
                GameObject.Destroy(child.gameObject);
            }

            if (!_taskManager.useAnalogueScale)
                for (int i = 0; i < likertItems.Count; i++)
                    CreateToggle(i);

            else
                SetAnalogueScaleNames();
        }

        private void CreateToggle(int index) {

            GameObject _instanciatedPrefab = Instantiate(togglePrefab, new Vector3(0, 0, 0), Quaternion.identity);

            _instanciatedPrefab.transform.SetParent(toggleGroup.gameObject.transform, false);

            Toggle _toggle = _instanciatedPrefab.GetComponent<Toggle>();

            _toggle.GetComponentInChildren<Text>().text = likertItems[index];

             _toggle.group = toggleGroup;

            _toggle.onValueChanged.AddListener(delegate { ToggleAction(_toggle); });
        }

        private void SetAnalogueScaleNames(){
            foreach (Transform child in scrollbar.transform){
                if (child.name == "Left label")
                    child.GetComponent<Text>().text = minVASLabel;
                else if (child.name == "Middle label")
                    child.GetComponent<Text>().text = midVASLabel;
                else if (child.name == "Right label")
                    child.GetComponent<Text>().text = maxVASLabel;
            }

        }
        
        private void ToggleAction(Toggle changed){
            _taskManager.OnResponseSelection();
        }
    }
}
