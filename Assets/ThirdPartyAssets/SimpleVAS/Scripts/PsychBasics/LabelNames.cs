using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityPsychBasics
{
    public class LabelNames : MonoBehaviour {

        public Text left, middle, right;

        public static LabelNames instance;

        private TaskManager _taskManager;

	    void Awake () {
            if (instance == null)
                instance = this;
	    }

        private void Start() {
            _taskManager = TaskManager.instance;
        }
        
        public void UpdateLabelNames(string l, string m, string r){
            left.text = l;
            middle.text = m;
            right.text = r;
        }

    }
}
