using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Uduino
{
    [Serializable]
    public class Pin  
    {
        private UduinoManager manager = null;
        public UduinoManager Manager {
            get {
                if (manager == null)
                    return UduinoManager.Instance;
                else
                    return manager;
            }
            set { manager = value; }
        }

        [SerializeField]
        public UduinoDevice device = null;
        [SerializeField]
        public string arduinoName = null; // We keep arduino name to retreive the board when there are multiple boards connected

        [SerializeField]
        public PinMode pinMode;
        public PinMode prevPinMode; // is used for Custom Editor

        [SerializeField]
        public int currentPin = -1;
        public int prevPin = -1; // used for editor check

        [SerializeField]
        public bool isEditorPin = false;

        bool isInit = false;

        [SerializeField]
        public int sendValue = 0;
        public int prevSendValue = 0;
        public int lastReadValue = 0; //for Editor panel

        public Pin(UduinoDevice arduinoParent, int pin, PinMode mode)
        {
            Manager = UduinoManager.Instance;
            device = arduinoParent;
            currentPin = pin;
            pinMode = mode;

            if(device != null)
                arduinoName = device.name;
        }

        public void Init(bool useInit = false)
        {
            ChangePinMode(pinMode, useInit? "init" : null);
        }

        public virtual void WriteReadMessage(string message)
        {
            if(Manager != null)
                Manager.sendCommand(device, message);
            //TODO : ref to bundle? 
            //TODO : Add ref to arduinocard
        }

        public virtual bool WriteMessage(string message, string bundle = null)
        {
            if (Manager == null)
                return false;

            return Manager.sendCommand(device, message, bundle);
        }

        public bool PinTargetExists(UduinoDevice parentArduinoTarget, int currentPinTarget)
        {
            if (( device != null  || parentArduinoTarget == null || parentArduinoTarget == null || parentArduinoTarget == device)
                && currentPinTarget == currentPin )
                return true;
            else
                return false;
        }

        /// <summary>
        /// Override Pin mode
        /// </summary>
        /// <param name="mode">Mode</param>
        public void OverridePinMode(PinMode mode, bool useInit = false)
        {
            if (mode != pinMode)
            {
                pinMode = mode;
                isInit = false;
                Init(useInit);
            }
        }

        /// <summary>
        /// Change Pin mode
        /// </summary>
        /// <param name="mode">Mode</param>
        public void ChangePinMode(PinMode mode, string bundle = null)
        {
            if (!isInit || mode != pinMode)
            {
                pinMode = mode;
                WriteMessage(UduinoManager.BuildMessageParameters("s", currentPin, (int)pinMode), bundle);
                isInit = true;
            }
        }

        /// <summary>
        /// Send OptimizedValue
        /// </summary>
        /// <param name="sendValue">Value to send</param>
        public virtual int SendRead(string bundle = null, System.Action<string> action = null, bool digital = false)
        {
            string cmd = "r" + (digital ? "d" : "");
            if (bundle != null) cmd = "br";
            string valueAsString = Manager.Read(device, UduinoManager.BuildMessageParameters(cmd,currentPin), action: action, bundle: bundle);
            int returnedValue = ParseIntValue(valueAsString);

            if (returnedValue != -1)
                lastReadValue = returnedValue;

            return lastReadValue;
        }

        /// <summary>
        /// Send OptimizedValue
        /// </summary>
        /// <param name="sendValue">Value to send</param>
        public void SendPinValue(int sendValue, string typeOfPin, string bundle = null)
        {
            if (sendValue != prevSendValue)
            {
                this.sendValue = sendValue;
                WriteMessage(UduinoManager.BuildMessageParameters(typeOfPin, currentPin, sendValue), bundle);
                prevSendValue = sendValue;
            }
        }

        public void Destroy()
        {
            if(pinMode == PinMode.Output)
                WriteMessage(UduinoManager.BuildMessageParameters("d",currentPin,0),"destroy");
            else if (pinMode == PinMode.PWM || pinMode == PinMode.Input)
                WriteMessage(UduinoManager.BuildMessageParameters("a", currentPin, 0), "destroy");
            isInit = false;
        }

        public virtual void Draw()
        {
            //Function overrided by the Editor
        }

        public int ParseIntValue(string data)
        {
            if (data == null || data == "")
                return -1;

            string[] parts = data.Split(new string[] { UduinoManager.bundleDelimiter }, StringSplitOptions.None);
            int max = 0;
            if (parts.Length == 1) max = 1;
            else max = parts.Length - 1;
            try
            {
                for (int i = 0; i < max; i++) // Parse bundle message
                {
                    string[] subParts = parts[i].Split(new string[] { UduinoManager.parametersDelimiter }, StringSplitOptions.None);
                    if (subParts.Length != 2)
                        return -1;
                    int recivedPin = -1;
                    recivedPin = int.Parse(subParts[0]);

                    int value = int.Parse(subParts[1]);
                    if (recivedPin != -1)
                    {
                        if (recivedPin == currentPin)
                        {
                            return value;
                        } else
                        {
                            Manager.dispatchValueForPin(device, recivedPin, value);
                        }
                    } 
                }
            }
            catch (System.FormatException)
            {

            }
            return -1;
        }
    }
}