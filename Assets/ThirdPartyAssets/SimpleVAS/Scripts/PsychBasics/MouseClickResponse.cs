using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityPsychBasics {
    public class MouseClickResponse : MonoBehaviour {

        private float[] mouseResponse;// = new int[] {1, 2};
        
        public bool useMouseClickSelector;
        [HideInInspector] public bool orderLeft1Right2;
        public static MouseClickResponse instance;

        private void Awake()
        {
            if (instance == null) instance = this;
        }

        private void Start()
        {
            orderLeft1Right2 = BasicDataConfigurations.mouseClickOrder;
            if (orderLeft1Right2) mouseResponse = new float[] { 1, 2 };
            else mouseResponse = new float[] { 2, 1 };
        }

        private void Update () 
        {
            if(useMouseClickSelector) { 
                if(Input.GetMouseButtonDown(0))
                    TaskManager.instance.OutsideResponseValue(mouseResponse[0]);

                if(Input.GetMouseButtonDown(1)) 
                    TaskManager.instance.OutsideResponseValue(mouseResponse[1]);
                
                TaskManager.instance.OnNextButton();
            }
        }
        
        public void ActivateSelector(bool _useMouseClickSelector) 
        {
            useMouseClickSelector = _useMouseClickSelector;
            if (useMouseClickSelector) TaskManager.instance.setValueOutside = true;
        }
        
    }
}
