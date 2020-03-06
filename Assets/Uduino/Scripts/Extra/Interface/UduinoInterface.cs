using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Uduino
{
    public class Interface
    {
        public static Interface Instance {
             get
            {
                if (_instance == null)
                    _instance = new Interface();
               return _instance;
            } 
        }
        private static Interface _instance = null;

        public UduinoInterface currentInterface = null;
        public bool isActive()
        {
            try
            {
                if (Application.isPlaying)
                {
                    if (UduinoManager.Instance.interfaceType == UduinoInterfaceType.None)
                        return false;
                    else
                    {
                        if (currentInterface == null) Create();
                        return true;
                    }
                }
            } catch {
                Debug.Log("Catch error");
                if (UduinoManager.Instance.interfaceType == UduinoInterfaceType.None)
                    return false;
                else
                {
                    if (currentInterface == null) Create();
                    return true;
                }

            }
           // else return false
                return false;
        }


        public void Create()
        {
            UduinoInterface[] _interfaces = MonoBehaviour.FindObjectsOfType(typeof(UduinoInterface)) as UduinoInterface[];
            if (_interfaces.Length == 0 && UduinoManager.Instance.interfaceType != UduinoInterfaceType.None)
            {
                // Creating an interface
                    UduinoInterface targetInterface = null;
                    bool useBLE = false;
                    #if UNITY_EDITOR || UNITY_STANDALONE
                    if (UduinoManager.Instance.ExtensionIsPresentAndActive("UduinoDevice_DesktopBluetoothLE"))
                        useBLE = true;
                    #elif UNITY_ANDROID
                    if (UduinoManager.Instance.ExtensionIsPresentAndActive("UduinoDevice_AndroidBluetoothLE"))
                    useBLE = true;
                    #endif
                    GameObject tmpInterface = null;
                    if (useBLE)
                    {
                        tmpInterface = (GameObject)Object.Instantiate(Resources.Load("UduinoInterface_Bluetooth"));
                    }
                    else
                    {
                        tmpInterface = (GameObject)Object.Instantiate(Resources.Load("UduinoInterface"));
                    }
                    targetInterface = tmpInterface.GetComponent<UduinoInterface>();
                    tmpInterface.gameObject.name = "UduinoInterface";
                    tmpInterface.transform.SetParent(UduinoManager.Instance.transform);
                    targetInterface.currentInterfaceType = UduinoManager.Instance.interfaceType;

                    currentInterface = targetInterface;
            }
            else  // If there is more than Zero interfaces
            {
                foreach(UduinoInterface ui in _interfaces)
                {
                    if (UduinoManager.Instance.interfaceType != UduinoInterfaceType.None && currentInterface == null)
                        currentInterface = ui;  
                    else
                        ui.Destroy();
                }
            }
        }

        public void SetConnection(UduinoConnection connection)
        {
            if (isActive()) currentInterface.SetConnection( connection);
        }
        public void AddDeviceButton(string name, string uuid = "")
        {
            if (isActive()) currentInterface.AddDeviceButton(name, uuid);
        }

        public void SendCommand(string t)
        {
            if (isActive()) currentInterface.SendCommand(t);
        }
        public void Read()
        {
            if (isActive()) currentInterface.Read();
        }
        public void SendValue()
        {
            if (isActive()) currentInterface.SendValue();
        }
        public void LastReceviedValue(string value)
        {
            if (isActive()) currentInterface.LastReceviedValue(value);
        }
        public void StartSearching()
        {
            if (isActive()) currentInterface.StartSearching();
        }
        public void StopSearching()
        {
            if (isActive()) currentInterface.StopSearching();
        }
        public void NoDeviceFound(bool active)
        {
            if (isActive()) currentInterface.NoDeviceFound(active);
        }
        public void DisplayError(string message)
        {
            if (isActive()) currentInterface.DisplayError(message);
        }
        public void DetectDevice()
        {
            if (isActive()) currentInterface.DetectDevice();
        }
        public void BoardNotFound(string message)
        {
            if (isActive()) currentInterface.BoardNotFound(message);
        }
        public void UduinoConnected(string name)
        {
            if (isActive()) currentInterface.UduinoConnected(name);
        }
        public void DisconnectUduino(string name)
        {
            if (isActive()) currentInterface.DisconnectUduino(name);
        }
        public void RemoveDeviceButton(string name)
        {
            if (isActive()) currentInterface.RemoveDeviceButton(name);
        }
        public void UduinoDisconnected(string name)
        {
            if (isActive()) currentInterface.UduinoDisconnected(name);
        }
        public void UduinoConnecting(string name)
        {
            if (isActive()) currentInterface.UduinoConnecting(name);
        }
    }

   
    public class UduinoInterface : MonoBehaviour
    {
        #region Singleton
    
        #endregion
        [HideInInspector]
        public UduinoInterfaceType currentInterfaceType = UduinoInterfaceType.None;
    
#region Public variables
        public UduinoConnection boardConnection = null;

        [Header("Full panel")]
        public GameObject fullUI;
        public GameObject errorPanel;
        public GameObject fullDevicePanel;
        public GameObject scanButtonFull;
        public GameObject notFound;
        public GameObject boardButton;

        [Header("Minimal Panel")]
        public GameObject minimalUI;
        public GameObject minimalErrorPanel;
        public GameObject minimalDevicePanel;
        public GameObject minimalScanButton;
        public GameObject minimalNotFound;
        public GameObject minimalBoardButton;

        [Header("Debug Panel")]
        public GameObject debugPanel;
        public Text sendValue;
        public Text lastReceivedValue;
#endregion

        void Awake()
        {
            OnAwake();
        }

        public void Create() { }

        public virtual void OnAwake()
        {
            switch (UduinoManager.Instance.interfaceType)
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
                    //  CreateInterface();
                    //DestroyImmediate(this.gameObject);
                    return;
            }
            debugPanel.SetActive(false);
            getDeviceButtonPrefab().SetActive(false);
        }

        public virtual void SetConnection(UduinoConnection connection)
        {
            boardConnection = connection;
        }

        public virtual void AddDeviceButton(string name, string uuid = "")
        {
            if (UduinoManager.Instance.interfaceType == UduinoInterfaceType.None)
                return;

            GameObject deviceBtn = GameObject.Instantiate(getDeviceButtonPrefab(), getPanel());
            deviceBtn.transform.name = name;
            deviceBtn.transform.Find("DeviceName").transform.GetComponent<Text>().text = name;
           // Button btn = deviceBtn.GetComponent<Button>();

            deviceBtn.SetActive(true);
            NoDeviceFound(false);

           deviceBtn.transform.Find("Disconnect").GetComponent<Button>().onClick.AddListener(() => this.DisconnectUduino(name));
        }

        public virtual void SendCommand(string t)
        {

            Debug.Log("Wrong !! ");
            boardConnection.PluginWrite(t + "\r\n");
        }

        public virtual void Read() { }

        public virtual void SendValue()
        {
            boardConnection.PluginWrite(sendValue.text);
        }

        public virtual void LastReceviedValue(string value)
        {
            lastReceivedValue.text = value;
        }

        public virtual void StartSearching()
        {
            NoDeviceFound(false);
            getScanButton().text = "Finding boards...";
        }

        public virtual void StopSearching()
        {
            getScanButton().text = "Discover Boards";
        }

#region Getting elements from differents UIs
        public Text getScanButton() {
            return UduinoManager.Instance.interfaceType == UduinoInterfaceType.Full ?
                    scanButtonFull.transform.Find("ScanText").GetComponent<Text>() :
                    minimalScanButton.transform.Find("ScanText").GetComponent<Text>();
        }
        public Slider getScanSlider() {
            return UduinoManager.Instance.interfaceType == UduinoInterfaceType.Full ?
                    scanButtonFull.transform.Find("Slider").GetComponent<Slider>() :
                    minimalScanButton.transform.Find("Slider").GetComponent<Slider>();
        }
        public GameObject getDeviceButtonPrefab() {
            return UduinoManager.Instance.interfaceType == UduinoInterfaceType.Full ? boardButton : minimalBoardButton;
        }
        public Transform getPanel() {
            return UduinoManager.Instance.interfaceType == UduinoInterfaceType.Full ?
                    fullDevicePanel.transform :
                    minimalDevicePanel.transform;
        }
        public GameObject getErrorPanel() {
            return UduinoManager.Instance.interfaceType == UduinoInterfaceType.Full ? errorPanel : minimalErrorPanel;
        }
        public GameObject getNotFound() {
            return UduinoManager.Instance.interfaceType == UduinoInterfaceType.Full ? notFound : minimalNotFound;
        }
        public GameObject getBoardButton(string name) {
            return getPanel().transform.Find(name).gameObject;
        }
#endregion


        public void Detect()
        {
            UduinoManager.Instance.DiscoverPorts();
        }

        public virtual void NoDeviceFound(bool active)
        {
            getNotFound().SetActive(active);
        }

        public virtual void DisplayError(string message)
        {
            if (message == "")
            {
                getErrorPanel().SetActive(false);
            }
            else
            {
                getErrorPanel().SetActive(true);
                getErrorPanel().transform.Find("Content").Find("ErrorMessage").Find("ErrorText").GetComponent<Text>().text = message;
            }
        }

        public virtual void DetectDevice()
        {
            boardConnection.Discover();
        }

        public virtual void BoardNotFound(string message)
        {
            StopSearching();
            debugPanel.SetActive(false);
            getNotFound().SetActive(true);
            getNotFound().GetComponent<Text>().text = message;
        }

        public virtual void DisconnectUduino(string name)
        {
            UduinoManager.Instance.CloseDevice(name);
        }

        public virtual void RemoveDeviceButton(string name)
        {
            BoardNotFound("Board disconnected");
            try
            {
                Destroy(getBoardButton(name));
            } catch(System.Exception e)
            {
                Log.Debug(e);
            }
        }

        public virtual void UduinoConnected(string name)
        {
            StopSearching();
            debugPanel.SetActive(true);
            getNotFound().SetActive(false);
        }

        public virtual void UduinoDisconnected(string name) { }

        public virtual void UduinoConnecting(string name) { }

        public void Destroy()
        {
            if (this.gameObject != null && this.gameObject != UduinoManager.Instance.gameObject)
                DestroyImmediate(this.gameObject);
            else
                DestroyImmediate(this);
        }
    }

}