using System.Collections.Generic;

/// <summary>
/// For the moment this is not used
/// </summary>
namespace Uduino
{
    #region Board types
    public class ArduinoBoardType
    {
        public string name = "";
        public Dictionary<string, int> pins = new Dictionary<string, int>();

        //  TODO : If we add this as optional value, is it breaking ?
        public ArduinoBoardType(string name, int[] digitalRange, int[] analogRange, int[] otherAnalogPins)
        {
            this.name = name;
            for (int i = digitalRange[0]; i <= digitalRange[1]; i++)
            {
                pins.Add("" + i, i);
            }

            int tmpid = 0;
            for (int i = analogRange[0]; i <= analogRange[1]; i++)
            {
                pins.Add("A" + tmpid, i);
                tmpid++;
            }

            if (otherAnalogPins != null)
            {
                for (int i = 0; i <= otherAnalogPins.Length; i++)
                {
                    string key = "A" + (tmpid + i);
                    if (!pins.ContainsKey(key))
                        pins.Add(key, otherAnalogPins[i]);
                }
            }
        }

        public ArduinoBoardType(string name, Dictionary<string, int> pins)
        {
            this.name = name;
            this.pins = pins;
        }

        public string[] GetPins()
        {
            string[] keys = new string[pins.Keys.Count];
            pins.Keys.CopyTo(keys, 0);
            return keys;
        }

        public int[] GetValues()
        {
            int[] values = new int[pins.Values.Count];
            pins.Values.CopyTo(values, 0);
            return values;
        }

        /*
        public string PinValuetoPinName(int id)
        {
            string valueName = null;
            foreach (KeyValuePair<string,int> p in pins)
                if(p.Value == id)
                {
                    UnityEngine.Debug.Log(p.Value);
                    valueName = p.Key;
                    break;
                }

            if(valueName == null)
                Log.Error("The pin " + id + " does not exists for the " + name);
            return valueName;
        }*/
    
        public int GetPin(int id)
        {
            return GetPin(id + "");
        }

        public int GetPin(string id)
        {
            int outValue = -1;
            if (id[0] == 'd' || id[0] == 'D')
                id = id.Remove(0, 1);
            bool hasFound = pins.TryGetValue(id.ToUpper(), out outValue);
            if (!hasFound || outValue == -1)
                Log.Error("The pin " + id + " does not exists for the " + name);
            return outValue;
        }
    }
    public class BoardsTypeList
    {
        private static BoardsTypeList _boards = null;
        public static BoardsTypeList Boards
        {
            get
            {
                if (_boards != null)
                {
                    return _boards;
                }
                else
                {
                    _boards = new BoardsTypeList();
                    return _boards;
                }
            }
            set
            {
                if (Boards == null)
                    _boards = value;
            }
        }
        public List<ArduinoBoardType> boardTypes = new List<ArduinoBoardType>();

        BoardsTypeList()
        {
            ArduinoBoardType defaultArduinoBoard = new ArduinoBoardType("Default Arduino", new int[] { 0, 13 }, new int[] { 14, 19 }, null);
            ArduinoBoardType defaultATmega32U4 = new ArduinoBoardType("Defaut ATMega32u4", new int[] { 0, 13 }, new int[] { 18, 23 }, null);
            boardTypes.Add(new ArduinoBoardType("Arduino Uno", defaultArduinoBoard.pins));
            boardTypes.Add(new ArduinoBoardType("Arduino Duemilanove", defaultArduinoBoard.pins));
            boardTypes.Add(new ArduinoBoardType("Arduino Leonardo", defaultATmega32U4.pins));
            boardTypes.Add(new ArduinoBoardType("Arduino Pro Mini", defaultArduinoBoard.pins));
            boardTypes.Add(new ArduinoBoardType("Arduino Mega", new int[] { 0, 53 }, new int[] { 54, 69 }, null));
            boardTypes.Add(new ArduinoBoardType("Arduino Due", new int[] { 0, 53 }, new int[] { 54, 65 }, null));
            boardTypes.Add(new ArduinoBoardType("Arduino Nano", defaultArduinoBoard.pins));
            boardTypes.Add(new ArduinoBoardType("Arduino Micro", defaultATmega32U4.pins));
            //   boardTypes.Add(new ArduinoBoardType("Arduino Mini", new int[] { 0, 13 }, 7, null));
            boardTypes.Add(new ArduinoBoardType("Generic ATMega32B", defaultArduinoBoard.pins));
            boardTypes.Add(new ArduinoBoardType("Generic ATMega32u4", defaultATmega32U4.pins));
            boardTypes.Add(new ArduinoBoardType("NodeMCU", new Dictionary<string, int>() { { "D0", 16 }, { "D1", 5 }, { "D2", 4 }, { "D3", 0 }, { "D4", 2 }, { "D5", 14 }, { "D6", 12 }, { "D7", 13 }, { "D8", 15 }, { "RX", 3 }, { "TX", 1 }, { "S2", 9 }, { "S3", 10 }, { "A0", 17 } }));
            boardTypes.Add(new ArduinoBoardType("LoLin", GetBoardFromName("NodeMCU").pins));
            boardTypes.Add(new ArduinoBoardType("WeMos", new Dictionary<string, int>() { { "D0", 16 }, { "D1", 5 }, { "D2", 4 }, { "D3", 0 }, { "D4", 2 }, { "D5", 14 }, { "D6", 12 }, { "D7", 13 }, { "D8", 15 }, { "A0", 17 } }));
            boardTypes.Add(new ArduinoBoardType("Generic esp8266", new Dictionary<string, int>() { { "D0", 16 }, { "D1", 5 }, { "D2", 4 }, { "D3", 0 }, { "D4", 2 }, { "D5", 14 }, { "D6", 12 }, { "D7", 13 }, { "D8", 15 }, { "RX", 3 }, { "TX", 1 }, { "S2", 9 }, { "S3", 10 }, { "A0", 17 } }));

            //   boardTypes.Add(new ArduinoBoardType("Arduino Yun", 13, 6, new int[] {4,6,7,8,9,10,12}));
        }

        /// <summary>
        /// List the arduino boards as an array
        /// </summary>
        /// <returns>Array of arduino boards</returns>
        public string[] ListToNames()
        {
            List<string> names = new List<string>();
            boardTypes.ForEach(x => names.Add(x.name));
            return names.ToArray();
        }
        /// <summary>
        /// Return the arduino board type from a name
        /// </summary>
        /// <param name="name">Name of the board</param>
        /// <returns>ArduinoBoardType</returns>
        public ArduinoBoardType GetBoardFromName(string name)
        {
            return boardTypes.Find(x => x.name == name);
        }

        /// <summary>
        /// Return the arduino board type from an id
        /// </summary>
        /// <param boardId="boardId">Name of the board</param>  
        /// <returns>ArduinoBoardType</returns>
        public ArduinoBoardType GetBoardFromId(int boardId)
        {
            return boardTypes[boardId];
        }

        /// <summary>
        /// Return the arduino board ID from a name
        /// </summary>
        /// <param name="name">Name of the board</param>
        /// <returns>Aarduino board index in List</returns>
        public int GetBoardIdFromName(string name)
        {
            ArduinoBoardType board = boardTypes.Find(x => x.name == name);
            return boardTypes.IndexOf(board);
        }


        /// <summary>
        /// Add a new board type
        /// </summary>
        /// <param name="name">Name of the custom board</param>
        /// <param name="numberDigital">Number of digital pins</param>
        /// <param name="numberAnalog">Number of analog pin</param>
        /// <returns>Return new boardType</returns>
        public ArduinoBoardType addCustomBoardType(string name, int[] digitalRange, int[] analogRange, int[] otherAnalogPins)
        {
            ArduinoBoardType board = new ArduinoBoardType(name, digitalRange, analogRange, otherAnalogPins);
            boardTypes.Add(board);
            return board;
        }
    }
    #endregion
}