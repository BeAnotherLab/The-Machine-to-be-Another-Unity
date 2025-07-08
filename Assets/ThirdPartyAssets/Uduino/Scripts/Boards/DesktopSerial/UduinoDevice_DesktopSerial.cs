using UnityEngine;
using System;
using System.Collections;
#if UDUINO_READY
using System.IO.Ports;
#endif

namespace Uduino {

    public class UduinoDevice_DesktopSerial : UduinoDevice
    {

#if UDUINO_READY
        //Serial status
        public SerialPort serial = null;
        private string _port;
        private int _baudrate = 9600;
        private int _writeTimeout = 50;
        private int _readTimeout = 50;

        public short errorsTries = 0;

#if UNITY_EDITOR
        public override int readTimeout {
            get
            {
                if (serial != null) return serial.ReadTimeout;
                else return _readTimeout;
            } set {
                if (serial != null) {
                    try {
                        serial.ReadTimeout = value;
                    } catch (Exception e) {
                        Log.Error("Impossible to set ReadTimeout on <color=#2196F3>[" + _port + "]</color> : " + e);
                        Close();
                    }
                } else _readTimeout = value;
            }
        }

        public override int writeTimeout {
            get
            {
                if (serial != null) return serial.WriteTimeout;
                else return _writeTimeout;
            } set {
                if (serial != null) {
                    try {
                        serial.WriteTimeout = value;
                    } catch (Exception e) {
                        Log.Error("Impossible to set WriteTimeout on <color=#2196F3>[" + _port + "]</color> : " + e);
                        Close();
                    }
                } else _writeTimeout = value;
            }
        }
#endif
        //TODO : faire les fonctions set Rea
        public UduinoDevice_DesktopSerial(int baudrate = 9600) : base() { }

        public UduinoDevice_DesktopSerial(string port, int baudrate = 9600, int readTimeout = 100, int writeTimeout = 100, int boardType = 0) : base()
        {
            this._baudrate = baudrate;
            _readTimeout = readTimeout;
            _writeTimeout = writeTimeout;
            _boardType = boardType;
            this.identity = port;
            _port = port;
        }


        /// <summary>
        /// Open a specific serial port
        /// </summary>
        public override void Open()
        {
            try
            {
#if UNITY_EDITOR_WIN || (UNITY_STANDALONE_WIN && !UNITY_EDITOR_OSX)
                _port = "\\\\.\\" + _port;
#endif
                serial = new SerialPort(_port, _baudrate, Parity.None, 8, StopBits.One);
                serial.ReadTimeout = _readTimeout;
                serial.WriteTimeout = _writeTimeout;
                serial.Close();
                serial.Dispose();
                serial.Open();
                serial.DiscardInBuffer();
                serial.DiscardOutBuffer();
                serial.BaseStream.Flush();
                serial.DtrEnable = true;                  // Won't read from Leonardo without this
                boardStatus = BoardStatus.Open;
                //     serial.NewLine = "\r\n";
                Log.Info("Opening stream on port <color=#2196F3>[" + _port + "]</color>");
            }
            catch (Exception e)
            {
                if(e.Message == "Access is denied.\r\n")
                    Log.Error("The serial port of  <color=#2196F3>[" + _port + "]</color> is open in another application. Please close this application first (Arduino IDE ? ) or unplug/plug the device.");
                else
                    Log.Info("Error on port <color=#2196F3>[" + _port + "]</color> : " + e);
                Close();
            }
        }

        public override void UduinoFound()
        {
            base.UduinoFound();
            Interface.Instance.AddDeviceButton(name);
        }

        #region Public functions
        /// <summary>
        /// Return serial port 
        /// </summary>
        /// <returns>Current opened com port</returns>
        public string getPort()
        {
            return _port;
        }

        #endregion

        #region Commands
        /// <summary>
        /// Loop every thead request to write a message on the arduino (if any)
        /// </summary>
        public override bool WriteToArduinoLoop()
        {
            commandhasBeenSent = false;

            if (serial == null || !serial.IsOpen)
                return false;

            lock (writeQueue)
            {
                if (writeQueue.Count == 0)
                    return false;

                string message = (string)writeQueue.Dequeue();
                try
                {
                    try
                    {
                        serial.WriteLine(message);
                        serial.BaseStream.Flush();
                        Log.Info("<color=#4CAF50>" + message + "</color> sent to <color=#2196F3>[" + (name != "" ? name : _port) + "]</color>", true);
                    }
                    catch (Exception e)
                    {
                        writeQueue.Enqueue(message);
                        Log.Warning("Impossible to send the message <color=#4CAF50>" + message + "</color> to <color=#2196F3>[" + (name != "" ? name : _port) + "]</color>: " + e, true);
                        if(e.GetType() == typeof(System.IO.IOException) && boardStatus == BoardStatus.Found)
                        {
                            errorsTries++;
                            if(errorsTries > 5)
                            {
                                Debug.Log("Todo : Reconnect automatically ? ");
                            }
                        }
                        return false;
                    }
                }
                catch (Exception e)
                {
                    Log.Error("Error on port <color=#2196F3>[" + (name != "" ? name : _port) + "]</color>: " + e, true);
                    // Close();
                    return false;
                }
                commandhasBeenSent = true;
                WritingSuccess(message);
            }
            return true;
        }

        /// <summary>
        /// Read Arduino serial port
        /// </summary>
        /// <param name="message">Write a message to the serial port before reading the serial</param>
        /// <param name="instant">Read the message value now and not in the thread loop</param>
        /// <returns>Read data</returns>
        public override string ReadFromArduino(string message = null, bool instant = false)
        {
            if (serial == null || !serial.IsOpen)
                return null;

            return base.ReadFromArduino(message, instant);
        }


        public override bool ReadFromArduinoLoop(bool forceReading = false)
        {
            if (serial == null || !serial.IsOpen)
                return false;

            if (!base.ReadFromArduinoLoop(forceReading))
                return false; // If the conditions don't match, we return here

            // serial.DiscardOutBuffer();
           // serial.DiscardInBuffer(); // ! Creates bug in MacOS
         
            try
            {
                try
                {
                    int lineCount = 0;
                    while (lineCount < 5)
                    {
                        string readedLine = serial.ReadLine();
                        MessageReceived(readedLine);
                        lineCount++;
                    }
                    if (lineCount > 5)
                    {
                        serial.DiscardOutBuffer();
                        serial.DiscardInBuffer();
                    }
                    return true;
                    /*
                    int lineCount = 0;
                    string tempBuffer = "";
                    for (int i=0; i <150;i++)
                    {
                        char a = (char)serial.ReadByte();
                        tempBuffer += a.ToString();
                        if (tempBuffer.EndsWith("\r\n"))
                        {
                            string readedLine = tempBuffer.TrimEnd(Environment.NewLine.ToCharArray());
                            MessageReceived(readedLine);
                            tempBuffer = "";
                            lineCount++;
                            if (lineCount > 5)
                            {
                                serial.DiscardOutBuffer();
                                serial.DiscardInBuffer();
                                return true;
                            }
                        }
                    }
                    
                    return true;*/
                }
                catch (TimeoutException e)
                {
                    if (boardStatus == BoardStatus.Found)
                        Log.Debug("ReadTimeout. Are you sure something is written in the Serial of the board? \n" + e);
                }
            }
            catch (Exception e)
            {
                if (e.GetType() == typeof(System.IO.IOException) || e.InnerException.GetType() == typeof(System.IO.IOException))
                {
                    Log.Warning("Impossible to read message from arduino. Arduino has restarted."); // TODO : Reconnected
                }
                else
                    Log.Error(e);

                UduinoManager.Instance.InvokeAsync(() =>
                {
                    UduinoManager.Instance.CloseDevice(this);
                });

                if (UduinoManager.Instance.autoReconnect)
                    UduinoManager.Instance.shouldReconnect = true;
                  // TODO : Test with multiple board (it will detec all again). Should connect only to this one. 
            }
            return false;
        }
        #endregion

        #region Close
        /// <summary>
        /// Close Serial port 
        /// </summary>
        public override void Close()
        {
            base.Close();

            if (serial != null && serial.IsOpen)
            {
                Log.Warning("Closing port : <color=#2196F3>[" + _port + "]</color>");
                serial.Close();
                serial.Dispose();
                boardStatus = BoardStatus.Closed;
                serial = null;
            }
        }
        #endregion
#endif
    }
}