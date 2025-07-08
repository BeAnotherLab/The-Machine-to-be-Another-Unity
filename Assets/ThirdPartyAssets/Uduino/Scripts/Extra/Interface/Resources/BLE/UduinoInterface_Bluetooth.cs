using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Uduino
{
    public class BLEDeviceButton_Interface
    {
        public Button button;
        public GameObject connecting;
        public GameObject connect;
        public Transform connected;
        public GameObject disconnect;

        public BLEDeviceButton_Interface(Button button)
        {
            this.button = button;
            Transform mainT = button.transform;
            this.connect = mainT.Find("Connect").gameObject;
            this.connecting = mainT.Find("Connecting").gameObject;
            this.connected = mainT.Find("Connected");
            this.disconnect = mainT.Find("Disconnect").gameObject;

            CanConnect();
        }

        public void CanConnect()
        {
            connect.SetActive(true);
            disconnect.SetActive(false);
            connecting.SetActive(false);
            connected.gameObject.SetActive(false);
            button.enabled = true;
        }

        public void Connecting()
        {
            connect.SetActive(false);
            connecting.SetActive(true);
            button.enabled = false;
            disconnect.SetActive(true);
        }

        public void Connected()
        {
            connecting.SetActive(false);
            connected.gameObject.SetActive(true);
        }

        public void Disconnected()
        {
            CanConnect();
        }
    }

    public class UduinoInterface_Bluetooth : UduinoInterface { 

        public Dictionary<string, BLEDeviceButton_Interface> devicesButtons = new Dictionary<string, BLEDeviceButton_Interface>();

        void Awake()
        {
            switch(UduinoManager.Instance.interfaceType)
            {
                case UduinoInterfaceType.Full:
                    minimalUI.SetActive(false);
                    fullUI.SetActive(true);
                    break;
                case UduinoInterfaceType.Minimal:
                    minimalUI.SetActive(true);
                    fullUI.SetActive(false);
                break;
                case UduinoInterfaceType.None:
                    minimalUI.SetActive(false);
                    fullUI.SetActive(false);
                    break;
            }
            StopTimer();
            ClearPanel();
            DisplayDebugPanel(false);
        }

        public override void Read()
        {

        }

        public override void SendValue()
        {
            boardConnection.PluginWrite(sendValue.text);
        }

        public override void LastReceviedValue(string value)
        {
            lastReceivedValue.text = value;
        }

        #region Start / Stop searching
        public void SearchDevices()
        {
            boardConnection.ScanForDevices();
        }

        public override void StartSearching()
        {
            ClearPanel();
            StartTimer();
            DisplayDebugPanel(false);
            NoDeviceFound(false);
            getScanButton().text = "Scanning...";
            devicesButtons.Clear();
        }

        public override void StopSearching()
        {
            getScanButton().text = "Scan for devices";
            getScanSlider().value = 0;
            getScanSlider().gameObject.SetActive(false);
        }

        void StartTimer()
        {
            StartCoroutine(StartSliderCountdown());
        }

        public IEnumerator StartSliderCountdown()
        {
            Slider slider = getScanSlider();
            slider.gameObject.SetActive(true);

            int currentCount = 0 ;
            while (currentCount < UduinoManager.Instance.bleScanDuration * 100)
            {
                yield return new WaitForSeconds(0.01f);
                slider.value = (float)((float)currentCount / (float)(UduinoManager.Instance.bleScanDuration * 100));
                currentCount++;
            }
            StopTimer();
        }

        public override void SendCommand(string t)
        {
            boardConnection.PluginWrite(t + "\r\n");
        }

        void StopTimer()
        {
            getScanSlider().value = 0;
            getScanSlider().gameObject.SetActive(false);
        }

        void ClearPanel()
        {
            foreach (Transform child in getPanel())
                if (child.gameObject.name != "NotFound")
                {
                    if (child.gameObject.name == "Device")
                        child.gameObject.SetActive(false);
                    else
                        Destroy(child.gameObject);
                }

            getErrorPanel().SetActive(false);
        }

        #endregion
 
        public override void AddDeviceButton(string name, string uuid)
        {
            if (UduinoManager.Instance.interfaceType == UduinoInterfaceType.None)
                return;

            GameObject deviceBtn = Instantiate(getDeviceButtonPrefab(), getPanel());
            deviceBtn.transform.name = name;
            deviceBtn.transform.Find("DeviceName").transform.GetComponent<Text>().text = name;
            Button btn = deviceBtn.GetComponent<Button>();
            deviceBtn.gameObject.SetActive(true);

            BLEDeviceButton_Interface deviceInterface = new BLEDeviceButton_Interface(btn);
            devicesButtons.Add(name, deviceInterface);

            // Add connect event
            btn.onClick.AddListener(() => boardConnection.ConnectPeripheral(uuid, name));

            // Add disconnect event
            deviceInterface.disconnect.GetComponent<Button>().onClick.AddListener(() => UduinoManager.Instance.CloseDevice(name));
        }


        public void DisplayDebugPanel(bool active)
        {
            debugPanel.SetActive(active);
        }

        public override void UduinoConnecting(string name)
        {
            BLEDeviceButton_Interface currentDeviceBtn = null;
            if (devicesButtons.TryGetValue(name, out currentDeviceBtn))
            {
                currentDeviceBtn.Connecting();
            }
            Log.Info("connecting to " + name);
        }

        public override void UduinoConnected(string name)
        {
            BLEDeviceButton_Interface currentDeviceBtn = null;
            if(devicesButtons.TryGetValue(name, out currentDeviceBtn)) {
                DisplayDebugPanel(true);
                currentDeviceBtn.Connected();
            }
        }

        public override void UduinoDisconnected(string name)
        {
            BLEDeviceButton_Interface currentDeviceBtn = null;
            if (devicesButtons.TryGetValue(name, out currentDeviceBtn))
            {
                DisplayDebugPanel(false);
                currentDeviceBtn.Disconnected();
            }
            else
            {
                // TODO : We close all of them if we don't find the good one, because sometimes NAME is send in behalf of identity
                foreach(KeyValuePair<string, BLEDeviceButton_Interface> a in devicesButtons)
                {
                    a.Value.Disconnected();
                }
            }
        }
    }
}