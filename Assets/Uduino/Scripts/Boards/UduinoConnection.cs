using System;
using System.Collections;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;

namespace Uduino
{
    public class UduinoConnection
    {
        public UduinoManager _manager = null;
        public UduinoDevice connectedDevice = null;

        public static UduinoConnection GetFinder(UduinoManager manager, Platform p, ConnectionMethod m)
        {
            UduinoConnection connection = null;
#if UNITY_EDITOR || UNITY_STANDALONE //IF it's on the editor
            if (manager.activeExtentionsMap.ContainsValue(true))
            {
                if (manager.ExtensionIsPresentAndActive("UduinoDevice_DesktopBluetoothLE"))
                    connection = System.Reflection.Assembly.GetExecutingAssembly().CreateInstance("Uduino.UduinoConnection_DesktopBluetoothLE") as UduinoConnection;
                else if (manager.ExtensionIsPresentAndActive("UduinoDevice_Wifi"))
                    connection = System.Reflection.Assembly.GetExecutingAssembly().CreateInstance("Uduino.UduinoConnection_Wifi") as UduinoConnection;
                else
                    connection = new UduinoConnection_DesktopSerial();
            } else
            {
                return null;
            }

#elif UNITY_ANDROID //Get the  Android Serial Plugin
            if(manager.ExtensionIsPresentAndActive("UduinoDevice_AndroidBluetoothLE")) {
              connection = new UduinoConnection_AndroidBluetoothLE();
            } else if(manager.ExtensionIsPresentAndActive("UduinoDevice_AndroidSerial")){
              connection = new UduinoConnection_AndroidSerial();
            } else if (manager.ExtensionIsPresentAndActive("UduinoDevice_Wifi"))
              connection = new UduinoConnection_Wifi();
            else { Log.Error("Uduino for Android is not active ! Activate it in the Inspector Panel.");  }
#endif
            Log.Debug("Starting Uduino with type: " + connection.GetType());

            return connection;
        }

        public UduinoConnection(UduinoManager manager = null)
        {
            _manager = manager;
        }

        virtual protected void Setup() { }

        public virtual void FindBoards(UduinoManager manager)
        {
            _manager = manager;
            Interface.Instance.SetConnection(this);
        }

        public virtual UduinoDevice OpenUduinoDevice(string id = null)
        {
            Log.Debug("No Uduino board type setup");
            return null;
        }

        public virtual void DetectUduino(UduinoDevice uduinoDevice)
        {
            if (uduinoDevice.boardStatus == BoardStatus.Closed)
                return;

            if (_manager.ReadingMethod == HardwareReading.Thread && Application.isPlaying)
            {
                try
                {
                    Thread _detectionThread = null;
                    _detectionThread = new Thread(() => DetectUduinoThread(uduinoDevice));
                    _detectionThread.Start();
                }
                catch (System.Exception e)
                {
                    Log.Error(e);
                }
            } else
            {
                _manager.StartCoroutine(DetectUduinoCoroutine(uduinoDevice));
            }
        }

        /// <summary>
        /// Detect Uduino device from a thread
        /// </summary>
        /// <param name="uduinoDevice"></param>
        public void DetectUduinoThread(UduinoDevice uduinoDevice)
        {
#if UNITY_ANDROID
            if (_manager.ReadingMethod == HardwareReading.Thread)
                AndroidJNI.AttachCurrentThread(); // Sepcific android related code
#endif
            uduinoDevice.boardStatus = BoardStatus.Finding;
            int tries = 0;
            Thread.Sleep(Mathf.FloorToInt(UduinoManager.Instance.delayBeforeDiscover* 1000));
            do
            {
                if (TryToFind(uduinoDevice, true))
                    return;
                Thread.Sleep(100);
                //Wait one frame. Todo : use yield return new WaitForSeconds(0.5f); ?
                // yield return null;    //Wait one frame. Todo : use yield return new WaitForSeconds(0.5f); ?
            } while (uduinoDevice.getStatus() != BoardStatus.Found && tries++ < _manager.DiscoverTries-1 && !_manager.isApplicationQuiting);

            BoardNotFound(uduinoDevice);
        }

        
        /// <summary>
        /// Find a board connected to a specific port
        /// </summary>
        /// <param name="uduinoDevice">uduinoDevice</param>
        public IEnumerator DetectUduinoCoroutine(UduinoDevice uduinoDevice)
        {
            uduinoDevice.boardStatus = BoardStatus.Finding;
            int tries = 0;
            yield return new WaitForSeconds(UduinoManager.Instance.delayBeforeDiscover);
            do
            {
                if (TryToFind(uduinoDevice))
                    break;
               yield return new WaitForSeconds(0.1f);    //Wait one frame. Todo : use yield return new WaitForSeconds(0.5f); ?
                // yield return null;    //Wait one frame. Todo : use yield return new WaitForSeconds(0.5f); ?
            } while (uduinoDevice.getStatus() != BoardStatus.Found && tries++ < _manager.DiscoverTries -1);

            BoardNotFound(uduinoDevice);
        }

        bool TryToFind(UduinoDevice uduinoDevice, bool callAsync = false)
        {
            if (uduinoDevice.getStatus() == BoardStatus.Finding)
            {
                string reading = uduinoDevice.ReadFromArduino("identity", instant: true);
                Log.Debug("Trying to get name on <color=#2196F3>[" + uduinoDevice.identity + "]</color>.", true);
                if (reading == null) reading = uduinoDevice.lastRead;
                if (reading != null && reading.Split(new char[0])[0] == "uduinoIdentity")
                {
                    string name = reading.Split(new char[0])[1];
                    uduinoDevice.name = name;
                    if (callAsync)
                    {
                        _manager.InvokeAsync(() =>
                        {
                            uduinoDevice.UduinoFound();
                            _manager.AddUduinoBoard(name, uduinoDevice);
                        });
                    }
                    else
                    {
                        uduinoDevice.UduinoFound();
                        _manager.AddUduinoBoard(name, uduinoDevice);
                    }
                    BoardFound(name);
                    return true;
                }
                else
                {
                    Log.Debug("Impossible to get name on <color=#2196F3>[" + uduinoDevice.identity + "]</color>. Retrying.");
                }
            }
            return false;
        }

        public virtual bool BoardNotFound(UduinoDevice uduinoDevice)
        {
            if (uduinoDevice.getStatus() != BoardStatus.Found)
            {
                Log.Debug("Impossible to get name on <color=#2196F3>[" + uduinoDevice.identity + "]</color>. Closing.");
                uduinoDevice.Close();
                uduinoDevice = null;
                return false;
            } else
            {
                return true;
            }
        }

        public virtual void BoardFound(string name) { }
        public virtual void ScanForDevices() { Log.Debug("Should not be here"); }
        public virtual void Discover() { }
        public virtual void PluginReceived(string message) { }
        public virtual void PluginWrite(string message) { }
        public virtual bool ConnectPeripheral(string uuid, string name) { return false; }

        public virtual bool Disconnect() { Log.Debug("Disconnect."); return false; }
        public virtual void Stop() {  }

        public virtual void CloseDevices()
        {

            if (connectedDevice != null)
            {
                UduinoManager.Instance.CloseDevice(connectedDevice);
                connectedDevice = null;
            }
        }

        public void CreateDebugCanvas()
        {
            if (_manager.displayAndroidTextGUI && UduinoDebugCanvas.Instance == null)
            {
                Log.Info("Creating GUI Text Debug");
                GameObject debugCanvas = new GameObject();
                debugCanvas.AddComponent<UduinoDebugCanvas>();
            }
        }

   
    }
}