/*
 * Uduino - Arduino-Unity Library
 * http://marcteyssier.com/uduino/
 */
using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
#if UDUINO_READY
using System.IO.Ports;
#endif


/// <summary>
///  todo : new pin, ajouter le parent device 
///  todo : allBoards is not working anymore
/// </summary>
namespace Uduino
{
    #region Enums
    public enum PinMode
    {
        Output,
        PWM,
        Input,
        Input_pullup,
        Servo
    };

    public enum AnalogPin { A0 = -10, A1 = -11, A2 = -12, A3 = -13, A4 = -14, A5 = -15, A6 = -16, A7 = -17 };

    public enum State
    {
        LOW,
        HIGH
    };

    public enum BoardStatus
    {
        Open,
        Finding,
        Found,
        Stopping,
        Closed
    };

    public enum LogLevel
    {
        None,
        Debug,
        Info,
        Warning,
        Error
    };

    public enum Platform
    {
        Auto,
        Desktop,
        Android
    };

    public enum ConnectionMethod
    {
        Default,
        Serial,
        Network,
        Bluetooth
    };

    public enum HardwareReading
    {
        Thread,
        Coroutines
    };

    public enum UduinoInterfaceType
    {
        None,
        Minimal,
        Full
    }


    #endregion

    [Serializable]
     public struct UduinoExtension {
        public string name;
        public bool isPresent;
        public bool isActive;
    }

    public class UduinoManager : MonoBehaviour {

        #region Singleton
        /// <summary>
        /// UduinoManager unique instance.
        /// Create  a new instance if any UduinoManager is present on the scene.
        /// Set the Uduinomanager only on the first time.
        /// </summary>
        /// <value>UduinoManager static instance</value>
        public static UduinoManager Instance
        {
            get {
                if (_instance != null)
                    return _instance;

                UduinoManager[] uduinoManagers = FindObjectsOfType(typeof(UduinoManager)) as UduinoManager[];
                if (uduinoManagers.Length == 0 )
                {
                    Log.Warning("UduinoManager not present on the scene. Creating a new one.");
                    UduinoManager manager = new GameObject("UduinoManager").AddComponent<UduinoManager>();
                    _instance = manager;
                    return _instance;
                }
                else
                {
                    return uduinoManagers[0];
                }
            }
            set {
                if (_instance == null)
                    _instance = value;
                else
                {
                   Log.Error("You can only use one UduinoManager. Destroying the new one attached to the GameObject " + value.gameObject.name);
                   Destroy(value);
                }
            }
        }
        private static UduinoManager _instance = null;
        #endregion

        #region Exceptions
        /// <summary>
        /// Exception triggered if the participant ID was not found when loading the data.
        /// </summary>
        public class BoardAlreadyExistException : Exception { public BoardAlreadyExistException(string msg) : base(msg) { } }
        #endregion

        #region Variables
        /// <summary>
        /// Dictionnary containing all the connected Arduino devices
        /// </summary>
        public Dictionary<string, UduinoDevice> uduinoDevices = new Dictionary<string, UduinoDevice>();

        /// <summary>
        /// List containing all active pins
        /// </summary>
        [SerializeField]
        public List<Pin> pins = new List<Pin>();

        /// <summary>
        /// List containing callbacks
        /// </summary>
        public Dictionary<string, Action<string>> callbacksList = new Dictionary<string, Action<string>>();


        /// <summary>
        /// List containing all the available extensions
        /// </summary>
        public Dictionary<string, string> existingExtensionsMap = new Dictionary<string, string>()
        {
            { "UduinoDevice_DesktopSerial", "Desktop Serial" },
            { "UduinoDevice_DesktopBluetoothLE", "Desktop BLE" },
            { "UduinoDevice_AndroidBluetoothLE", "Android BLE" },
            { "UduinoDevice_AndroidSerial", "Android Serial" },
            { "UduinoDevice_Wifi", "Wifi" },
        };
        [SerializeField]
        public IsActiveDictionnary activeExtentionsMap = new IsActiveDictionnary();
        [SerializeField]
        public IsPresentDictionnary presentExtentionsMap = new IsPresentDictionnary();

        /// <summary>
        /// Uduino BoardConnection type, when a plugin is used
        /// </summary>
        UduinoConnection boardConnection = null;

        /// <summary>
        /// Variables for the async trigger of functions
        /// </summary>
        private object _lockAsync = new object();

        private System.Action _callbacksAsync;

        /// <summary>
        /// Log level
        /// </summary>
        [SerializeField]
        public LogLevel debugLevel;

        /// <summary>
        /// BaudRate
        /// </summary>
        [SerializeField]
        private int baudRate = 9600;
        public int BaudRate {
            get { return baudRate; }
            set { baudRate = value; }
        }

        /// <summary>
        /// Enable the reading of serial port in a different Thread.
        /// Might be usefull for optimization and not block the runtime during a reading. 
        /// </summary>

        private HardwareReading readingMethod = HardwareReading.Thread;
        public HardwareReading ReadingMethod
        {
            get { return readingMethod; }
            set
            {
                if (Application.isPlaying && readingMethod != value)
                {
                    if (readingMethod == HardwareReading.Thread)
                    {
                        StopAllCoroutines();
                        StartThread();
                    }
                    else
                    {
                        StopThread();
                        foreach (UduinoDevice device in GetAllBoard())
                            StartCoroutine(CoroutineRead(device));
                    }
                }
                readingMethod = value;
            }
        }

        /// <summary>
        /// Limitation of the send rate
        /// Packing into bundles
        /// </summary>
        [SerializeField]
        private bool limitSendRate = false;
        public bool LimitSendRate
        {
            get { return limitSendRate; }
            set {
                if (limitSendRate == value)
                    return;
               if (Application.isPlaying)
               {
                    if (value && !autoSendIsRunning)
                    {
                        Log.Debug("Start auto read");
                        StartCoroutine("AutoSendBundle");
                        autoSendIsRunning = true;
                    }
                    else
                    {
                        Log.Debug("Stop auto read");
                        StopCoroutine("AutoSendBundle");
                        autoSendIsRunning = false;
                    }
               }
                limitSendRate = value;
            }
        }
        private bool autoSendIsRunning = false;

        public int readTimeout = 30;

        public int writeTimeout = 30;

        public int threadFrequency = 16; //16 for 60fps

        public bool alwaysRead = true;

        public bool readAfterCommand = true;

        public bool skipMessageQueue = false;

        public int messageQueueLength = 10;

        public int defaultArduinoBoardType = 0;

		public bool useCuPort = false;

        /// <summary>
        /// SendRateSpeed
        /// </summary>
        [SerializeField]
        private int sendRateSpeed = 20;
        public int SendRateSpeed
        {
            get { return sendRateSpeed; }
            set { sendRateSpeed = value; }
        }

        /// <summary>
        /// Number of tries to discover the attached serial ports
        /// </summary>
        [SerializeField]
        private int discoverTries = 10;
        public int DiscoverTries
        {
            get { return discoverTries; }
            set { discoverTries = value; }
        }

        /// <summary>
        /// Discover serial ports on Awake
        /// </summary>
        public bool autoDiscover = true;

        /// <summary>
        /// Discover serial ports on Awake
        /// </summary>
        public float delayBeforeDiscover = 0.5f;

        /// <summary>
        /// Delemiter between each parameters
        /// </summary>
        public static string parametersDelimiter = " ";

        /// <summary>
        /// Delemiter between each bundle
        /// </summary>
        public static string bundleDelimiter = "-";

        /// <summary>
        /// Stop all digital/analog pin on quit
        /// </summary>
        public bool stopAllOnQuit = true;

        /// <summary>
        /// Stop all digital/analog pin on quit
        /// </summary>
        public bool stopAllOnPause = false;

        /// <summary>
        /// Reconnect when error
        /// </summary>
        public bool autoReconnect = true;

        /// <summary>
        /// Auto reconnect delay, if reconnect is enabled
        /// </summary>
        public float autoReconnectDelay = 5.0f;

        /// <summary>
        /// If the board should be automatically reconnected
        /// </summary>
        public bool shouldReconnect = false;

        /// <summary>
        /// List of black listed ports
        /// </summary>
        [SerializeField]
        private List<string> blackListedPorts = new List<string>();
        public List<string> BlackListedPorts {
            get { return blackListedPorts; }
            set { blackListedPorts = value; }
        }
        #endregion

        #region Events and Callbacks

        [System.Serializable]
        public class eventValueReceived : UnityEvent<string, UduinoDevice> { }
        [System.Serializable]
        public class eventBoard : UnityEvent<UduinoDevice> { }
        
        [SerializeField]
        public eventValueReceived OnDataReceivedEvent;

        [SerializeField]
        public eventBoard OnBoardConnectedEvent;

        [SerializeField]
        public eventBoard OnBoardDisconnectedEvent;

        /// <summary>
        /// Create a delegate event to trigger the function OnValueReceived()
        /// Takes two parameters, the returned data and the device.
        /// </summary>
        public delegate void OnValueReceivedDelegate(string data, UduinoDevice device);
        public event OnValueReceivedDelegate OnValueReceived;

        public delegate void OnDataReceivedDelegate(string data, UduinoDevice device);
        public event OnDataReceivedDelegate OnDataReceived;

        /// <summary>
        /// Create a delegate event to trigger the function OnBoardConnected()
        /// Takes one parameter, the board.
        /// </summary>
        public delegate void OnBoardConnectedDelegate(UduinoDevice device);
        public event OnBoardConnectedDelegate OnBoardConnected;

        /// <summary>
        /// Create a delegate event to trigger the function OnBoardDisconnected()
        /// Takes one parameter, the returned data.
        /// </summary>
        public delegate void OnBoardDisconnectedDelegate(UduinoDevice device);
        public event OnBoardDisconnectedDelegate OnBoardDisconnected;
        #endregion

        #region Extensions variables 
        ///  Addons SETTINGS
        public bool displayAndroidTextGUI = false;

        Platform platformType = Platform.Auto;
        ConnectionMethod connectionMethod = ConnectionMethod.Default;
        
        //BLE Settings
        public bool autoConnectToLastDevice = true;
        public int bleScanDuration = 3;
        public UduinoInterfaceType interfaceType = UduinoInterfaceType.Full; // Full, Minimal, None

        //Wifi setting
        public string uduinoIpAddress = "192.168.x.x";
        public int uduinoWifiPort = 4222;
        #endregion

        #region Init
        void Awake()
        {
#if UDUINO_READY
            Instance = this;
            Interface.Instance.Create();

            FullReset();
            Log.SetLogLevel(debugLevel);

            if (autoDiscover)
                DiscoverWithDelay(0);

            //TODO bouger autosendbundle que quand une carte est détectée
            StopCoroutine("AutoSendBundle");

            if (limitSendRate)
                StartCoroutine("AutoSendBundle");
#else
            Debug.Log("Uduino is not setup ! Please setup Uduino before starting the game.");
#endif
        }

        public void DiscoverWithDelay(float delay = -1)
        {
           StartCoroutine("DelayedDiscover", delay);
        }

        IEnumerator DelayedDiscover(float delay = -1)
        {
            if (delay == -1) delay = delayBeforeDiscover;
            yield return new WaitForSeconds(delay);
            DiscoverPorts();
            if(autoReconnect)
             StartCoroutine("RestartIfBoardNotDetected");
        }

        IEnumerator RestartIfBoardNotDetected()
        {
            yield return new WaitForSeconds(autoReconnectDelay);
           if (uduinoDevices.Count == 0) shouldReconnect = true;
        }

        #endregion

        #region Ports discovery & Board creation
        /// <summary>
        /// Get the ports names, dependings of the current OS
        /// </summary>
        public void DiscoverPorts()
        {
            CloseAllDevices();
            if (boardConnection == null || !Application.isPlaying)
                boardConnection = UduinoConnection.GetFinder(this, platformType, connectionMethod);

            if (boardConnection != null)
            {
                boardConnection.FindBoards(this);
            } else
            {
                Log.Warning("You didn't select any platform. Disabling Uduino.");
                this.enabled = false;
            }
        }

        public void AddUduinoBoard(string name, UduinoDevice board)
        {
            lock (uduinoDevices)
            {
                try
                {
                    Log.Info("Board <color=#ff3355>" + name + "</color> <color=#2196F3>[" + board.getIdentity() + "]</color> detected.");
                    uduinoDevices.Add(name, board);
                }
                catch (Exception)
                {
                    throw new BoardAlreadyExistException("Board with the name " + name + " is already connected ! Try to change the name of one of the arduino board");
                }
                finally
                {
                    board.alwaysRead = alwaysRead;
                    board.readAfterCommand = readAfterCommand;
                    StartReading(board);
                    InitAllArduinos();
                    board.WriteToArduino("connected", instant: true);

                    if (OnBoardConnected != null)
                        OnBoardConnected(board);

                    OnBoardConnectedEvent.Invoke(board);
                }
            }
        }

        /// <summary>
        /// Debug ports state. TODO : Move to Editor script
        /// </summary>
        public void GetPortState()
        {
            if (uduinoDevices.Count == 0)
            {
                Log.Info("Trying to close and no port are currently open");
            }
            foreach (KeyValuePair<string, UduinoDevice> uduino in uduinoDevices)
            {
            #if UDUINO_READY
                Log.Info("" + uduino.Value.getIdentity() + " (" + uduino.Key + ")");
            #endif
            }
        }

        /// <summary>
        /// Return if a board is connected
        /// </summary>
        /// <returns>Has a board connected</returns>
        public bool hasBoardConnected()
        {
            return uduinoDevices.Count == 0 ? false : true;
        }

        /// <summary>
        /// Return the board with the specific name
        /// </summary>
        /// <param name="name">Board Name</param>
        /// <param name="device">Output device </param>
        /// <returns>If a board has been found</returns>
        public bool GetBoard(string name, out UduinoDevice[] devices)
        {
            if (UduinoTargetExists(name))
            {
                devices = new UduinoDevice[1];
                devices[0] = uduinoDevices[name];
                return true;
            }
            else
            {
                devices = new UduinoDevice[uduinoDevices.Count];
                uduinoDevices.Values.CopyTo(devices, 0);
                if (devices.Length == 0) return false;
                else return true;
            }
        }


        /// <summary>
        /// Return all Uduino boards
        /// </summary>
        /// <returns>Boars as a list</returns>
        UduinoDevice[] GetAllBoard()
        {
            UduinoDevice[] devices = new UduinoDevice[uduinoDevices.Count];
            uduinoDevices.Values.CopyTo(devices, 0);
            return devices;
        }

        /// <summary>
        /// Return the board with the specific name
        /// </summary>
        /// <param name="device">Board Name</param>
        /// <param name="devices">Output device </param>
        /// <returns>If a board has been found</returns>
        public bool GetBoard(UduinoDevice device , out UduinoDevice[] devices)
        {
            if (UduinoTargetExists(device))
            {
                devices = new UduinoDevice[1];
                devices[0] = device;
                return true;
            }
            else
            {
                devices = new UduinoDevice[uduinoDevices.Count];
                uduinoDevices.Values.CopyTo(devices, 0);
                if (devices.Length == 0) return false;
                else return true;
            }
        }

        /// <summary>
        /// Return the Uduinodevice with the specific name
        /// </summary>
        /// <param name="name">Board Name</param>
        /// <returns>The UduinoDevice</returns>
        public UduinoDevice GetBoard(string name)
        {
            if (UduinoTargetExists(name))
            {
                UduinoDevice device = null;
                uduinoDevices.TryGetValue(name, out device);
                return device;
            } else
            {
                Log.Error("No board with the name " + name + " is found in the board list.");
                return null;
            }
        }

        /// <summary>
        /// Verify if the target exists when we want to get a value
        /// </summary>
        /// <param name="target">Target Uduino Name</param>
        /// <returns>Re</returns>
        private bool UduinoTargetExists(string target)
        {
            if (target == "" || target == null) return false;
            if (uduinoDevices.ContainsKey(target))
                return true;
            else
            {
               // if (target != null && target != "" && target != "allBoards")
                   // Log.Warning("The object " + target + " cannot be found. Are you sure it's connected and correctly detected ?");
                return false;
            }
        }


        /// <summary>
        /// Verify if the target exists when we want to get a value
        /// </summary>
        /// <param name="target">Target Uduino Name</param>
        /// <returns>Re</returns>
        private bool UduinoTargetExists(UduinoDevice target)
        {
            if (target == null) return false;
            if (uduinoDevices.ContainsValue(target))
                return true;
            else
            {
                if (target != null)
                    Log.Warning("The object " + target + " cannot be found. Are you sure it's connected and correctly detected ?");
                return false;
            }
        }

        #endregion

        #region BoardType 
        /// <summary>
        /// Set the board type, when one board only is connected
        /// </summary>
        /// <param name="type">Type of the board</param>
        public void SetBoardType(string type)
        {
            SetBoardType(null, type);
        }

        /// <summary>
        /// Set the board type of a specific arduino board
        /// </summary>
        /// <param name="target">Target board</param>
        /// <param name="type">Board type</param>
        public void SetBoardType(UduinoDevice target, string type)
        {
            int boardTypeId = BoardsTypeList.Boards.GetBoardIdFromName(type);
            SetBoardType(target, boardTypeId);
        }

        /// <summary>
        /// Set the board type of a specific arduino board
        /// </summary>
        /// <param name="target">Target board</param>
        /// <param name="boardId">Board ID, in the BoardType registered List</param>
        void SetBoardType(UduinoDevice target, int boardTypeId)
        {
            UduinoDevice[] devices;
            if (GetBoard(target, out devices))
                foreach (UduinoDevice device in devices)
                    device._boardType = boardTypeId;
            else
                Log.Info("Setting board type to a non-existant board");
        }
        
        /// <summary>
        /// Get the pin from specific board type
        /// </summary>
        /// <param name="boardType">Board type</param>
        /// <param name="pin">Pin to find</param>
        /// <returns>Int of the pin</returns>
        public int GetPinNumberFromBoardType(string boardType, string pin)
        {
            return BoardsTypeList.Boards.GetBoardFromName(boardType).GetPin(pin);
        }

        /// <summary>
        /// Get the pin from specific board type
        /// </summary>
        /// <param name="boardType">Board type</param>
        /// <param name="pin">Pin to find</param>
        /// <returns>Int of the pin</returns>
        public int GetPinNumberFromBoardType(string boardType, int pin)
        {
            return BoardsTypeList.Boards.GetBoardFromName(boardType).GetPin(pin);
        }

        /// <summary>
        /// Get the specific pin from the current board, if the board is already set with SetBoardType
        /// </summary>
        /// <param name="pin">Pin to find</param>
        /// <returns>Int of the pin</returns>
        public int GetPinFromBoard(string pin)
        {
            if(uduinoDevices.Count == 0)
                return BoardsTypeList.Boards.GetBoardFromId(defaultArduinoBoardType).GetPin(pin);

            var e = uduinoDevices.GetEnumerator();
            e.MoveNext();
            UduinoDevice anElement = e.Current.Value;

            int currentBoardType = anElement._boardType;
            return BoardsTypeList.Boards.GetBoardFromId(currentBoardType).GetPin(pin);
        }

        /// <summary>
        /// Get the specific pin from the current board, if the board is already set with SetBoardType
        /// </summary>
        /// <param name="pin">Pin to find</param>
        /// <returns>Int of the pin</returns>
        public int GetPinFromBoard(int pin)
        {
            return GetPinFromBoard(pin+"");
        }

        /// <summary>
        /// Return true if at least one board is connected
        /// </summary>
        /// <returns>Is board connected</returns>
        public bool isConnected()
        {
            return uduinoDevices.Count != 0;
        }
        #endregion

        #region Simple commands : Pin setup
        /// <summary>
        /// Initialize an arduino pin
        /// </summary>
        /// <param name="pin">Pin to initialize</param>
        /// <param name="mode">PinMode to init pin</param>
        public void pinMode(int pin, PinMode mode)
        {
            pinMode(null, pin, mode);
        }

        /// <summary>
        /// Init a pin
        /// </summary>
        /// <param name="pin">Analog pin to initialize</param>
        /// <param name="mode">PinMode to init pin</param>
        public void pinMode(AnalogPin pin, PinMode mode)
        {
            pinMode(null, PinValueToBoardValue(pin), mode);
        }

        /// <summary>
        /// Init a pin
        /// </summary>
        /// <param name="pin">Analog pin to initialize</param>
        /// <param name="mode">PinMode to init pin</param>
        public void pinMode(UduinoDevice target, AnalogPin pin, PinMode mode)
        {
            pinMode(target, PinValueToBoardValue(pin, target._boardType), mode);
        }

        /// <summary>
        /// Create a new Pin and setup the mode if the pin is not registered.
        /// If the pin exsists, change only the mode
        /// </summary>
        /// <param name="string">Target Name</param>
        /// <param name="pin">Pin to init</param>
        /// <param name="mode">PinMode to init pin</param>
        public void pinMode(UduinoDevice target, int pin, PinMode mode)
        {
            bool pinExists = false;

            foreach (Pin pinTarget in pins)
            {
                if (pinTarget.PinTargetExists(target, pin))
                {
                    pinExists = true;
                    if ( pinTarget.pinMode != mode) {
                        Log.Debug("Override pinMode for the pin <color=#4CAF50>" + pin + "</color> from <color=#2e7d32>" + pinTarget.pinMode + "</color> to <color=#2e7d32>" + mode + "</color>.");
                        
                        // TODO : I think this is useless
                        if ((target == null && uduinoDevices.Count != 0) || (target != null && UduinoTargetExists(target)))
                        {
                            pinTarget.OverridePinMode(mode);
                         //   Debug.Log("start");
                        }
                        else
                        {
                            pinTarget.OverridePinMode(mode, true);
                           // Debug.Log("stop");
                        }
                    } else
                    {
                        Log.Debug("pinMode of <color=#4CAF50>" + pin + "</color> already set to <color=#2e7d32>" + mode + "</color>");
                    }

                }
            }
            if (!pinExists)
            {
                Pin newPin = new Pin(target, pin, mode);
                pins.Add(newPin);
                string arduinoTarget = target != null ? " on the arduino <color=#ff3355>" + target.name + "</color> " : " ";
                Log.Debug("Set pinMode of <color=#4CAF50>" + pin +"</color>" +  arduinoTarget + "to <color=#2e7d32>" + mode + "</color>");
                if ((target == null && uduinoDevices.Count != 0) || //if target is not set but at least one card is connected
                   (target != null && UduinoTargetExists(target)))
                    newPin.Init();
            }
        }
        /// <summary>
        /// Return the pin
        /// </summary>
        /// <param name="pin"></param>
        /// <param name="boardType"></param>
        /// <returns></returns>
        int PinValueToBoardValue(AnalogPin pin, int boardType = -1)
        {
            if(boardType == -1) boardType = defaultArduinoBoardType;
           return BoardsTypeList.Boards.GetBoardFromId(boardType).GetPin(Enum.GetName(pin.GetType(), pin));
        }

        //TODO : Add old delegate as obsolete
        #region Obsolete
        [System.Obsolete("UduinoManager.InitPin() is deprecated, please use pinMode() instead.")]
        public void InitPin(int pin, PinMode mode)
        {
            pinMode(null, pin, mode);
        }

        [System.Obsolete("UduinoManager.InitPin() is deprecated, please use pinMode() instead.")]
        public void InitPin(AnalogPin pin, PinMode mode)
        {
            pinMode(null, (int)pin, mode);
        }

        [System.Obsolete("UduinoManager.InitPin() is deprecated, please use pinMode() instead.")]
        public void InitPin(UduinoDevice target, int pin, PinMode mode)
        {
            pinMode(target, pin, mode);
        }

        [System.Obsolete("UduinoManager.InitPin() is deprecated, please use pinMode() instead.")]
        public void InitPin(string target, AnalogPin pin, PinMode mode)
        {
            pinMode((int)pin, mode);
        }
        #endregion

        // TODO : Test with multiple boards ! If dosnt' work, refactor that. 
        /// <summary>
        /// Init all Pins when the arduino boards are found
        /// </summary>
        public void InitAllPins()
        {
            foreach(Pin pin in pins)
            {
                pin.Init(true);
            }
            Log.Debug("Init all pins not already initialized.");
            SendBundle("init");
        }

        /*
        public void InitAllBoardType()
        {
            foreach (KeyValuePair<string, int> boardType in boardTypeNames)
            {
                SetBoardType(boardType.Key, boardType.Value);
            }
        }
        */

        public void InitAllArduinos()
        {
         //   InitAllBoardType();
            InitAllPins();
            InitAllCallbacks();
        }


        /// <summary>
        /// Set the callbacks
        /// </summary>
        public void InitAllCallbacks()
        {
            foreach (KeyValuePair<string, Action<string>> callback in callbacksList)
            {
                UduinoDevice[] devices;
                if (GetBoard(callback.Key, out devices) || callback.Key == "")
                    foreach (UduinoDevice device in devices)
                        device.callback = callback.Value;
            }
        }
        #endregion

        #region Simple commands : Write

        /// <summary>
        /// DigitalWrite or AnalogWrite to arduino
        /// </summary>
        /// <param name="target"></param>
        /// <param name="pin"></param>
        /// <param name="value"></param>
        void arduinoWrite(UduinoDevice target, int pin, int value, string typeOfPin, string bundle = null)
        {
            bool onPinExists = false;
            foreach (Pin pinTarget in pins)
            {
                if (pinTarget.PinTargetExists(target, pin))
                {
                    pinTarget.SendPinValue(value, typeOfPin, bundle);
                    onPinExists = true;
                }
            }
            if (!onPinExists)
                Log.Info("You are trying to send a message to the pin " + pin + " but this pin is not initialized. \r\nUse the function UduinoManager.Instance.InitPin(..)");
        }

        /// <summary>fa
        /// Write a digital command to the arduino
        /// </summary>
        /// <param name="target"></param>
        /// <param name="pin"></param>
        /// <param name="value"></param>
        public void digitalWrite(UduinoDevice target, int pin, int value, string bundle = null)
        {
            if (value <= 150) value = 0;
            else value = 255;
            arduinoWrite(target,pin,value,"d", bundle);
        }


        /// <summary>
        /// Digital write value to the board
        /// </summary>
        public void digitalWrite(int pin, int value, string bundle = null)
        {
            digitalWrite(null, pin, value, bundle);
        }

        /// <summary>
        /// Write a command on an Arduino
        /// </summary>
        public void digitalWrite(int pin, State state = State.LOW, string bundle = null)
        {
            arduinoWrite(null, pin, (int)state * 255,"d", bundle);
        }

        /// <summary>
        /// Write a command on an Arduino
        /// </summary>
        public void digitalWrite(UduinoDevice target, int pin, State state = State.LOW, string bundle = null)
        {
            arduinoWrite(target, pin, (int)state * 255, "d", bundle);
        }

        /// <summary>
        /// Write an analog value to Arduino
        /// </summary>
        /// <param name="pin">Arduino Pin</param>
        /// <param name="value">Value</param>
        public void analogWrite(int pin, int value, string bundle = null)
        {
            arduinoWrite(null, pin, value, "a", bundle);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target">Arduino board</param>
        /// <param name="pin">Arduino Pin</param>
        /// <param name="value">Value</param>
        public void analogWrite(UduinoDevice target, int pin, int value, string bundle = null)
        {
            arduinoWrite(target, pin, value, "a", bundle);
        }

        #endregion

        #region Simple commands: Read
        // Digital Read
        public int digitalRead(UduinoDevice target, int pin, string bundle = null)
        {
            int readVal = 0;
            foreach (Pin pinTarget in pins)
            {
                if (pinTarget.PinTargetExists(target, pin))
                {
                    readVal = pinTarget.SendRead(bundle, digital: true);
                }
            }
            return readVal;
        }

        public int digitalRead(int pin, string bundle = null)
        {
            return digitalRead(null, pin, bundle);
        }

        public int digitalRead(AnalogPin pin, string bundle = null)
        {
            return digitalRead(null, PinValueToBoardValue(pin), bundle);
        }

        public int digitalRead(UduinoDevice target, AnalogPin pin, string bundle = null)
        {
            return digitalRead(target, PinValueToBoardValue(pin, target._boardType), bundle);
        }

        // Analog read

        public int analogRead(UduinoDevice target, int pin, string bundle = null)
        {
            int readVal = -1;

            foreach (Pin pinTarget in pins)
            {
                if (pinTarget.PinTargetExists(target, pin))
                {
                    readVal = pinTarget.SendRead(bundle);
                }
            }

            return readVal;
        }

        public int analogRead(UduinoDevice target, AnalogPin pin, string bundle = null)
        {
            return analogRead(target, PinValueToBoardValue(pin, target._boardType), bundle);
        }

        public int analogRead(int pin, string bundle = null)
        {
            return analogRead(null, pin, bundle);
        }

        public int analogRead(AnalogPin pin, string bundle = null)
        {
            return analogRead(null, PinValueToBoardValue(pin), bundle);
        }

        /// <summary>
        /// Dispatch received value for a pin
        /// </summary>
        /// <param name="target"></param>
        /// <param name="pin"></param>
        /// <param name="message"></param>
        /// <returns>readVal value</returns>
        public int dispatchValueForPin(UduinoDevice target, int pin, int readVal)
        {
            foreach (Pin pinTarget in pins)
            {
                if (pinTarget.PinTargetExists(target, pin))
                {
                  //  Debug.LogError("There is a problem here !! On recoit une valeur de pin a laquelle on ne s'attends pas.");
                  pinTarget.lastReadValue = readVal;
                }
            }
            return readVal;
        }
        #endregion

        #region Commands

        //// TODO !!! Refaire ça en overload, pour éviter le if/else
        /// TODO !! Is it usefull to override that ? 

        /// <summary>
        /// Send a read command to a specific arduino.
        /// A read command will be returned in the OnValueReceived() delegate function
        /// </summary>
        /// <param name="target">Target device. Not defined means read all boards</param>
        /// <param name="message">Variable watched, if defined</param>
        /// <param name="action">Action callback</param>
        /// <param name="bundle">Bundle name if any</param>
        public string Read(UduinoDevice target = null, string message = null, Action<string> action = null, string bundle = null)
        {
            UduinoDevice[] devices;
            string readVal = "";

            if (bundle != null)
            {
                if(GetBoard(target,out devices))
                {
                    foreach(UduinoDevice device in devices)
                    {
                        device.callback = action;
                        device.AddToBundle(message, bundle);
                        readVal =  device.lastRead;
                    }
                }
            }
            else
            {
                if (GetBoard(target, out devices))
                {
                    foreach (UduinoDevice device in devices)
                    {
                        device.callback = action;
                        readVal = device.ReadFromArduino(message);
                    }
                }
            }
            return readVal;
        }

        /// <summary>
        /// Send a read command to all boards.
        /// A read command will be returned in the OnValueReceived() delegate function
        /// </summary>
        /// <param name="message">Variable watched, if defined</param>
        /// <param name="action">Action callback</param>
        /// <param name="bundle">Bundle name if any</param>
        public void Read(string message = null, Action<string> action = null, string bundle = null)
        {
            UduinoDevice[] devices;

            if (bundle != null)
            {
                if (GetBoard("", out devices))
                {
                    foreach (UduinoDevice device in devices)
                    {
                        device.callback = action;
                        device.AddToBundle(message, bundle);
                    }
                }
            }
            else
            {
                if (GetBoard("", out devices))
                {
                    foreach (UduinoDevice device in devices)
                    {
                        device.callback = action;
                        device.ReadFromArduino(message);
                    }
                }
            }
        }

        /// <summary>
        /// Read a value from arduino direc
        /// </summary>
        /// <param name="target">Target device name. Not defined means read everything</param>
        /// <param name="message">Variable watched, if defined</param>
        /// <param name="action">Action callback</param>
        /// <param name="bundle">Bundle name if any</param>
        public string DirectReadFromArduino(UduinoDevice targetDevice = null, string message = null,  Action<string> action = null, string bundle = null)
        {
            string val = "";
            UduinoDevice[] devices;

            if (bundle != null)
            {
                if (GetBoard(targetDevice, out devices))
                    foreach (UduinoDevice device in devices)
                        device.AddToBundle(message, bundle);
            }
            else
            {
                if (GetBoard(targetDevice, out devices))
                {
                    foreach (UduinoDevice device in devices)
                    {
                        device.callback = action;
                        val = device.ReadFromArduino(message);
                    }
                }
            }
            return val;
        }

        //TODO : Too much overload ? Bundle ? 
        public void Read(int pin, string target = null, Action<string> action = null) 
        {
            DirectReadFromArduino(action: action);
        }

        public void Read(int pin, Action<string> action = null)
        {
            DirectReadFromArduino(action: action);
        }

        #endregion

        #region Send advanced commands
        /// <summary>
        /// Send a command on an Arduino
        /// </summary>
        /// <param name="target">Target device</param>
        /// <param name="message">Message to write in the serial</param>
        /// <param name="bundle">Bundle name</param>
        public bool sendCommand(UduinoDevice target, string message = null, string bundle = null)
        {
            UduinoDevice[] devices;
            if (GetBoard(target, out devices))
            {
                foreach (UduinoDevice device in devices)
                {
                    if (bundle != null || limitSendRate)
                    {
                        if (limitSendRate) bundle = "LimitSend";
                        device.AddToBundle(message, bundle);
                        return true;
                    }
                    else
                    {
                        device.WriteToArduino(message);
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Send a command to all connected Arduinos
        /// </summary>
        /// <param name="command">Command to send</param>
        /// <param name="value">List of parameters, optinal</param>
        public void sendCommand(string command, params object[] value)
        {
            foreach (KeyValuePair<string, UduinoDevice> uduino in uduinoDevices)
                uduino.Value.WriteToArduino(command, BuildMessageParameters(value));
        }

        /// <summary>
        /// Send a command on an Arduino with a specific value  
        /// </summary>
        /// <param name="target">Target device</param>
        /// <param name="message">Message to write in the serial</param>
        /// <param name="value">Optional value</param>
        public void sendCommand(UduinoDevice target, string command, params object[] value)
        {
            target.WriteToArduino(command, BuildMessageParameters(value));
        }

        [System.Obsolete("UduinoManager.Write() is deprecated, please use sendCommand() instead.")]
        public void Write(string target, string command, params object[] value) {
            sendCommand(GetBoard(target), command, value);
        }

        [System.Obsolete("UduinoManager.Write() is deprecated, please use sendCommand() instead.")]
        public void Write(string target = null, string message = null, string bundle = null)
        {
            sendCommand(GetBoard(target), message, bundle);
        }

        /// <summary>
        /// Function to build a message with custom delimiter
        /// </summary>
        /// <param name="parameters">List of custom parameters</param>
        /// <returns>The parameters as string, delimited by the custom delimitier</returns>
        public static string BuildMessageParameters(params object[] parameters)
        {
            string outputMessage = null;

            for(int i =0;i < parameters.Length;i++)
            {
                outputMessage += parameters[i];
                if (i != parameters.Length - 1)
                    outputMessage += parametersDelimiter;
            }
            return outputMessage;
        }

        /// <summary>
        /// Set the readCallback of a device
        /// </summary>
        /// <param name="target"></param>
        /// <param name="callback"></param>
        public void SetReadCallback(UduinoDevice target, Action<string> callback)
        {
            UduinoDevice[] devices;
            if (GetBoard(target, out devices))
            {
                foreach (UduinoDevice device in devices)
                {
                    device.callback = callback;
                }
            } else
            {
                string targetName = "";
                if (target != null)
                    targetName = target.name;
                callbacksList.Add(targetName, callback);
            }
        }

        /// <summary>
        /// Set the readCallback of a device
        /// </summary>
        /// <param name="callback">Callback</param>
        public void SetReadCallback(Action<string> callback)
        {
            SetReadCallback(null, callback);
        }
        #endregion

        #region Bundle
        /// <summary>
        /// Send an existing message bundle to Arduino
        /// </summary>
        /// <param name="target">Target arduino</param>
        /// <param name="bundleName">Bundle name</param>
        public void SendBundle(UduinoDevice target, string bundleName)
        {
            UduinoDevice[] devices;
            if (GetBoard(target, out devices))
                foreach (UduinoDevice device in devices)
                    device.SendBundle(bundleName);
        }

        /// <summary>
        /// Send an existing message bundle to Arduino
        /// </summary>
        /// <param name="bundleName">Bundle name</param>
        public void SendBundle(string bundleName)
        {
            SendBundle(null, bundleName);
        }

        /// <summary>
        /// Automatically send bundles
        /// </summary>
        /// <returns>Delay before next sending</returns>
        IEnumerator AutoSendBundle()
        {
            while (true)
            {
                if (!LimitSendRate)
                    yield return null;

                yield return new WaitForSeconds(sendRateSpeed / 1000.0f);
                List<string> keys = new List<string>(uduinoDevices.Keys);
                foreach (string key in keys)
                    uduinoDevices[key].SendAllBundles();
            }
        }


        [System.Obsolete("UduinoManager.AlwaysRead() is deprecated and has no effect. Please use delegates functions or callbacks.")]
        public void AlwaysRead(string target, Action<string> action) { }
        [System.Obsolete("UduinoManager.AlwaysRead() is deprecated and has no effect. Please use delegates functions or callbacks.")]
        public void AlwaysRead(string target) { }

        #endregion

        #region Dictionnary Helper
        /// <summary>
        /// Check if an extension is present and is active
        /// </summary>
        /// <param name="extensionName">Extension name by id</param>
        /// <returns></returns>
        public bool ExtensionIsPresentAndActive(string extensionName)
        {
            bool isPresent = false;
            bool isActive = false;
            presentExtentionsMap.TryGetValue(extensionName, out isPresent);
            if(isPresent)
                activeExtentionsMap.TryGetValue(extensionName, out isActive);

            return isPresent && isActive;
        }
        #endregion

        #region Hardware reading
        /// <summary>
        /// Threading variables
        /// </summary>
        public Thread _thread = null;
        private bool threadRunning = true;
        private int threadRestartTrials = 0;

        /// <summary>
        /// Start the reading
        /// </summary>
        /// <param name="target">Optional Uduino device</param>
        public void StartReading(UduinoDevice target)
        {
            if (readingMethod == HardwareReading.Coroutines)
                StartCoroutine(CoroutineRead(target));
            else
                StartThread();
        }

        /// <summary>
        /// Initialisation of the Thread reading on Awake()
        /// </summary>
        public void StartThread(bool isForced = false)
        {
            if (isForced && threadRestartTrials > 10)
            {
                Log.Error("Thread cannot restart.");
                return;
            }

            if (Application.isPlaying && _thread == null && readingMethod == HardwareReading.Thread && !IsRunning())
            {
                try
                {
                    if(isForced)
                    {
                        Log.Warning("Resarting Thread");
                        threadRestartTrials++;
                    }
                    Log.Debug("Starting Uduino read/write thread.");
                    _thread = new Thread(new ThreadStart(ReadPorts));
                    threadRunning = true;
                    _thread.Start();
                    _thread.IsBackground = true;
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
            else
            {
                Log.Debug("Uduino read/write thread is already started.");
            }
        }

        public void StopThread()
        {
            threadRunning = false;
            _thread = null;
        }

        public bool IsRunning()
        {
            return threadRunning;
        }
        
        void Update()
        {
            //Async Call
            Action tmpAction = null;
            lock (_lockAsync)
            {
                if (_callbacksAsync != null)
                {
                    tmpAction = _callbacksAsync;
                    _callbacksAsync = null;
                }
            }
            if (tmpAction != null) tmpAction();

            // Threading Loop
            if (_thread != null && !isApplicationQuiting &&
                _thread.ThreadState == ThreadState.Stopped)
            {
                StopThread();
                StartThread(true);
            }

            if(autoReconnect && shouldReconnect)
            {
                StartCoroutine("DiscoverWithDelay", 5.0f);
                shouldReconnect = false;
                Log.Warning("No Board detected. Reconnecting.");
            }
        }

        /// <summary>
        ///  Read the Serial Port data in a new thread.
        /// </summary>
        public void ReadPorts()
        {
#if UNITY_ANDROID
            if (ExtensionIsPresentAndActive("UduinoDevice_AndroidSerial"))
                AndroidJNI.AttachCurrentThread(); // Sepcific android serial related code
#endif
            while (IsRunning() && !isApplicationQuiting)
            {
                lock (uduinoDevices)
                {
                    foreach (KeyValuePair<string, UduinoDevice> uduino in uduinoDevices)
                    {
                        uduino.Value.WriteToArduinoLoop();
                        uduino.Value.ReadFromArduinoLoop();
                    }
                }
                Thread.Sleep(threadFrequency);
                if (limitSendRate) Thread.Sleep((int)sendRateSpeed / 2);
            }
            _thread = null;
        }

        /// <summary>
        /// Read and write value for Arduino, used in Editor
        /// </summary>
        /// <param name="target"></param>
        public void ReadWriteArduino(UduinoDevice target)
        {
            if(target != null)
            {
                target.WriteToArduinoLoop();
                target.ReadFromArduinoLoop();

            }
            else
            {
                foreach (KeyValuePair<string, UduinoDevice> uduino in uduinoDevices)
                {
                    uduino.Value.WriteToArduinoLoop();
                    uduino.Value.ReadFromArduinoLoop();
                }
            }
        }

        /// <summary>
        /// Retreive the Data from the Serial Prot using Unity Coroutines
        /// </summary>
        /// <param name="target"></param>
        /// <returns>null</returns>
        public IEnumerator CoroutineRead(UduinoDevice target)
        {
            while (true)
            {
                if (target != null)
                {
                    target.WriteToArduinoLoop();
                    target.ReadFromArduinoLoop();
                }

                if (limitSendRate)
                    yield return new WaitForSeconds(sendRateSpeed / 1000.0f);
                else
                    yield return new WaitForSeconds(threadFrequency/1000.0f);
            }
        }

        /// <summary>
        /// Trigger an async event, from the thread read to the main thread
        /// </summary>
        /// <param name="data">Message received</param>
        /// <param name="device">Device who receive the message</param>
        public void TriggerEvent(string data, UduinoDevice device)
        {
            InvokeAsync(() =>
            {
                if (OnDataReceived != null)
                    OnDataReceived(data, device);
                if (OnValueReceived != null)
                {
                    Log.Warning("OnValueReceived is deprecated. Please use OnDataRecevied instead");
                    OnValueReceived(data, device);
                }
                OnDataReceivedEvent.Invoke(data, device);
            });
        }

        /// <summary>
        /// Invoke a function from a read thead to the main thread
        /// </summary>
        /// <param name="callback">Callback functions</param>
        public void InvokeAsync(Action callback)
        {
            lock (_lockAsync)
            {
                _callbacksAsync += callback;
            }
        }
        #endregion

        #region Close Ports
        [System.Obsolete("The function CloseAllPorts() is deprecated, please use CloseAllDevices() instead.")]
        public void CloseAllPorts() { CloseAllDevices(); }

        /// <summary>
        /// Close all opened serial ports
        /// </summary>
        public void CloseAllDevices()
        {
            if (uduinoDevices.Count == 0)
            {
               // Log.Debug("No boards are connected.");
                return;
            }

            lock (uduinoDevices) // the lock here is creating delays when closing
            {
                List<string> devicesNames = new List<string>(uduinoDevices.Keys);
                foreach (string deviceName in devicesNames)
                {
                    CloseDevice(uduinoDevices[deviceName]);
                }
                uduinoDevices.Clear();
            }

        }

        public void CloseDevice(string target)
        {
            UduinoDevice[] devices;
            if (GetBoard(target, out devices))
                foreach (UduinoDevice device in devices)
                    CloseDevice(device);
        }

        public void CloseDevice(UduinoDevice device)
        {
            try
            {
                if (device.boardStatus == BoardStatus.Closed)
                    return;

                // Delete the pins 
                if (stopAllOnQuit)
                    foreach (Pin pinTarget in pins)
                        if (pinTarget.device == device || pinTarget.device == null)
                            pinTarget.Destroy();

                //Send the bundle destroy
                device.SendBundle("destroy");
                device.Stopping();
                device.Close();

                if (!isApplicationQuiting)
                {
                    Interface.Instance.RemoveDeviceButton(device.name);
                    Interface.Instance.UduinoDisconnected(device.name);
                }

                if (OnBoardDisconnected != null)
                    OnBoardDisconnected(device);
                OnBoardDisconnectedEvent.Invoke(device);

                uduinoDevices.Remove(device.name);

                if (!isApplicationQuiting && uduinoDevices.Count == 0)
                    StopThread();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        public bool isApplicationQuiting = false;
        void OnApplicationQuit()
        {
            isApplicationQuiting = true;
            FullReset();
        }

        private void OnDestroy()
        {
            isApplicationQuiting = true;
            FullReset();
        }

        void OnDisable()
        {
            isApplicationQuiting = true;
            FullReset();
        }

        public void FullReset()
        {
            if (uduinoDevices.Count != 0)
                CloseAllDevices();

            if (boardConnection != null)
                boardConnection.Stop();

            StopAllCoroutines();
            DisableThread();

            boardConnection = null;
        }

        void DisableThread()
        {
            StopThread();
            if (_thread != null)
            {
                //    _thread.Join();
                //    _thread = null;
            }
            lock (uduinoDevices)
            {
                _thread = null;
            }
        }
#endregion
    }

#region Version
    public static class UduinoVersion
    {
        static int major = 3;
        static int minor = 0;
        static int patch = 3;
        static string update = "May 2019";

        public static string getVersion()
        {
            return major + "." + minor + "." + patch;
        }

        public static string lastUpdate()
        {
            return update;
        }
    }
#endregion

#region Utils 
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<TKey> keys = new List<TKey>();

        [SerializeField]
        private List<TValue> values = new List<TValue>();

        // save the dictionary to lists
        public void OnBeforeSerialize()
        {
            keys.Clear();
            values.Clear();
            foreach (KeyValuePair<TKey, TValue> pair in this)
            {
                keys.Add(pair.Key);
                values.Add(pair.Value);
            }
        }

        // load dictionary from lists
        public void OnAfterDeserialize()
        {
            this.Clear();

            if (keys.Count != values.Count)
                throw new System.Exception(string.Format("there are {0} keys and {1} values after deserialization. Make sure that both key and value types are serializable."));

            for (int i = 0; i < keys.Count; i++)
                this.Add(keys[i], values[i]);
        }
    }

    [Serializable] public class IsPresentDictionnary : SerializableDictionary<string, bool> { }
    [Serializable] public class IsActiveDictionnary : SerializableDictionary<string, bool> { }
#endregion
}