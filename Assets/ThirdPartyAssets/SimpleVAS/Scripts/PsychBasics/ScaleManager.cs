using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityPsychBasics
{
    public class ScaleManager : MonoBehaviour 
    {

        public GameObject togglePrefab;
        public ToggleGroup toggleGroup;
        public GameObject scrollbar;

        [HideInInspector] public string minVASLabel, midVASLabel, maxVASLabel;
        [HideInInspector] public List<string> likertItems = new List<string>();

        public static ScaleManager instance;

        private void Awake()
        {
            if (instance == null) instance = this;
        }

        public void CreateToggles()
        {
            foreach(Transform child in toggleGroup.GetComponent<Transform>())
                Destroy(child.gameObject);

            if (!TaskManager.instance.useAnalogueScale)
                for (int i = 0; i < likertItems.Count; i++)
                {
                    var instanciatedPrefab = Instantiate(togglePrefab, Vector3.zero, Quaternion.identity);

                    instanciatedPrefab.transform.SetParent(toggleGroup.gameObject.transform, false);

                    Toggle toggle = instanciatedPrefab.GetComponent<Toggle>();
                    toggle.GetComponentInChildren<Text>().text = likertItems[i];
                    toggle.group = toggleGroup;
                    toggle.onValueChanged.AddListener(delegate {TaskManager.instance.OnResponseSelection(); });
                }

            else
            {
                foreach (Transform child in scrollbar.transform){
                    if (child.name == "Left label") child.GetComponent<Text>().text = minVASLabel;
                    else if (child.name == "Middle label") child.GetComponent<Text>().text = midVASLabel;
                    else if (child.name == "Right label") child.GetComponent<Text>().text = maxVASLabel;
                }
            }
        }
    }
}
