using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Uduino
{
    public class UduinoDevice
    {

        #region Variables
        public string name = "";
        public int _boardType = 0;

        public string lastRead = null;
        public string lastWrite = null;
        private Dictionary<string, List<string>> bundles = new Dictionary<string, List<string>>();

        public System.Action<string> callback = null;

        public BoardStatus boardStatus = BoardStatus.Closed;

        //Messages reading
        public Queue readQueue, writeQueue;
        public int maxQueueLength = 10;

        public bool alwaysRead = true;
        public bool readAfterCommand = false;

        public string identity = ""; // serial port name 

        public bool commandhasBeenSent = false;

        public UduinoConnection _connection;

#if UNITY_EDITOR
        virtual public int writeTimeout { get; set; }
        virtual public int readTimeout { get; set; }
#endif

#endregion

        #region delegates
        public delegate void OnBoardClosedEvent();
        public event OnBoardClosedEvent OnBoardClosed;

        public delegate void OnBoardFoundEvent();
        public event OnBoardFoundEvent OnBoardFound;
        #endregion

        #region Getters
        /// <summary>
        /// Return port status 
        /// </summary>
        /// <returns>BoardStatus</returns>
        public BoardStatus getStatus()
        {
            return boardStatus;
        }

        /// <summary>
        /// Return Identity 
        /// </summary>
        public string getIdentity()
        {
            return identity;
        }

        /// <summary>
        /// Set identity
        /// </summary>
        /// <param name="identity"></param>
        public void setIdentity(string identity)
        {
            this.identity = identity;
        }

        #endregion

        #region Init & Open
        public UduinoDevice(int boardType = -1)
        {
            if (boardType == -1)
                _boardType = UduinoManager.Instance.defaultArduinoBoardType;
            readQueue = Queue.Synchronized(new Queue());
            writeQueue = Queue.Synchronized(new Queue());
            maxQueueLength = UduinoManager.Instance.messageQueueLength;
        }

        public virtual void Open()
        {
            boardStatus = BoardStatus.Open;
        }

        public virtual void UduinoFound()
        {
            boardStatus = BoardStatus.Found;
#if UNITY_EDITOR
            if (Application.isPlaying) EditorUtility.SetDirty(UduinoManager.Instance);
#endif

            if (OnBoardFound != null)
                OnBoardFound();
        }
        #endregion

        #region public methods
        /// <summary>
        /// Add a message to the bundle
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <param name="bundle">Bundle Name</param>
        public void AddToBundle(string message, string bundle)
        {
            List<string> existing;
            if (!bundles.TryGetValue(bundle, out existing))
            {
                existing = new List<string>();
                bundles[bundle] = existing;
            }
            /* // TODO : Verify if message already in bundle !
            if (existing.FindIndex(o => string.Equals(message, o, System.StringComparison.OrdinalIgnoreCase)) > -1) { } else
            {
            }*/

            Log.Debug("Message <color=#4CAF50>" + message + "</color> added to the bundle " + bundle, true);
            existing.Add("," + message);
        }

        /// <summary>
        /// Send a Bundle to the arduino
        /// </summary>
        /// <param name="bundleName">Name of the bundle to send</param>
        public void SendBundle(string bundleName)
        {
            List<string> bundleValues;
            if (bundles.TryGetValue(bundleName, out bundleValues))
            {
                string fullMessage = "b" + UduinoManager.parametersDelimiter + bundleValues.Count;

                if (bundleValues.Count == 1) // If there is one message
                {
                    Log.Debug("Bundle <color=#4CAF50>" + bundleName + "</color> content sent to  <color=#2196F3>" + name + "</color>", true);

                    string message = bundleValues[0].Substring(1, bundleValues[0].Length - 1);
                    if (message.Contains("r")) ReadFromArduino(message);
                    else WriteToArduino(message);

                    return;
                }

                for (int i = 0; i < bundleValues.Count; i++)
                    fullMessage += bundleValues[i];

                if (fullMessage.Contains("r")) ReadFromArduino(fullMessage);
                else WriteToArduino(fullMessage);

                if (fullMessage.Length >= 128)  /// Max Length, matching avec arduino
                    Log.Warning("The bundle message is too big. Try to not send too many messages or increase UDUINOBUFFER in Uduino library.");

                bundles.Remove(bundleName);
            }
            else
            {
                if (bundleName != "init" && bundleName != "destroy")
                    Log.Info("You are tring to send the bundle \"" + bundleName + "\" but it seems that it's empty.");
            }
        }

        public void SendAllBundles()
        {
            Log.Debug("Send all bundles");
            List<string> bundleNames = new List<string>(bundles.Keys);
            foreach (string key in bundleNames)
                SendBundle(key);
        }
        #endregion


        /* Read Write */
        public virtual bool WriteToArduino(string message, object value = null, bool instant = false)
        {
            if (message == null || message == "")
                return false;

            if (value != null)
                message = message + UduinoManager.parametersDelimiter + value.ToString(); // TODO : Redo that with bundle builder ?

            if (message.Length > 140)
                Log.Error("The message length and parameters are too long. Reduce the length of the parameters or increase the value RECEIVE_MAX_BUFFER from the Uduino.h script ( Arduino/libraries/Uduino.h)");

            AddToArduinoWriteQueue(message);

            if (instant && !WriteToArduinoLoop()) // If the message is set up to instant and the writing is not working
                return false;

            return true;
        }
        public virtual bool WriteToArduinoLoop() { return false; }

        public virtual string ReadFromArduino(string message = null,  bool instant = false)
        {
            if (boardStatus == BoardStatus.Stopping)
                return null;

            AddToArduinoWriteQueue(message);
         
            if (instant && message != null)
            {
                ReadFromArduinoLoop(true);
            }

            lock (readQueue)
            {
                if (readQueue.Count == 0)
                    return null;

                string finalMessage = (string)readQueue.Dequeue();
                return finalMessage;
            }
        }
    
        public virtual bool ReadFromArduinoLoop(bool forceReading = false)
        {
            if (boardStatus == BoardStatus.Stopping)
                return false;

            if (forceReading)
            {
                WriteToArduinoLoop();
                return true;
            }

            if (!alwaysRead) // If the always read is disabled on this card and forceReading is not set
            {
                if (readAfterCommand && commandhasBeenSent)
                {
                    return true;
                }
                return false;
            }

            return true; // it can continue

            // ... 
            // Here is the platform specific code of child class
            // .. 
        }

        public virtual void AddToArduinoReadQueue(string message)
        {
            lock (readQueue)
                readQueue.Enqueue(message);
        }


        public virtual bool AddToArduinoWriteQueue(string message)
        {
           // Debug.Log("Writing " + message);
            // Debug.Log("Queue " + writeQueue.Count);
            if (message == null)
                return false;

            lock (writeQueue)
            {
                if (UduinoManager.Instance.skipMessageQueue) // Clear the queue if skipQueue
                    writeQueue.Clear();

                message += "\r\n";
                if (!writeQueue.Contains(message)) // Skip the message if it's already in the queue 
                {
                    if (writeQueue.Count < maxQueueLength)
                        writeQueue.Enqueue(message);
                    else Log.Debug("The queue is full. Send less frequently or increase queue length.");
                }
            }

           return false;
        }
        #region Callbacks
        /* Reading / Writing success */
        public virtual void MessageReceived(string message)
        {
            // if(message == "\r" ||message == "\r\n" ||message == "\n") return;
            ReadingSuccess(message);

            if (message != null && readQueue.Count < maxQueueLength)
                lock (readQueue)
                    readQueue.Enqueue(message);
        }

        public virtual void WritingSuccess(string message)
        {
            lastWrite = message;
        }

        public virtual void ReadingSuccess(string message)
        {
            lastRead = message;

            if (UduinoManager.Instance)
            {
                if (message.Split(' ')[0] == "uduinoIdentity")
                    return;

                UduinoManager.Instance.InvokeAsync(() =>
                {
                    if (callback != null)
                        callback(message);

                    UduinoManager.Instance.TriggerEvent(message, this);
                    #if UNITY_EDITOR
                    if (Application.isPlaying) EditorUtility.SetDirty(UduinoManager.Instance);
                    #endif
                });

                if (UduinoManager.Instance.IsRunning() == false && UduinoManager.Instance.ReadingMethod == HardwareReading.Thread && callback != null)
                {
                    if(System.Threading.Thread.CurrentThread != UduinoManager.Instance._thread)
                        callback(message);
                }
            }
        }
        #endregion

        #region Stop
        public virtual void Stopping()
        {
            WriteToArduino("disconnected", instant: true);
            boardStatus = BoardStatus.Stopping;
        }

        public virtual void Close()
        {
            if (boardStatus != BoardStatus.Closed && OnBoardClosed != null)
            {
                OnBoardClosed();
                OnBoardClosed = null;
            }
            ClearQueues();
            boardStatus = BoardStatus.Closed;
            if(_connection != null)
                _connection.Disconnect();
        }

        /// Specal Handler when application quit;
        private bool isApplicationQuitting = false;

        void OnDisable()
        {
            if (isApplicationQuitting) return;
            Close();
        }

        public void ClearQueues()
        {
            WriteToArduinoLoop();

            lock (readQueue)
                readQueue.Clear();
            lock (writeQueue)
                writeQueue.Clear();
        }

        void OnApplicationQuit()
        {
            isApplicationQuitting = true;
        }
        #endregion

    }
}