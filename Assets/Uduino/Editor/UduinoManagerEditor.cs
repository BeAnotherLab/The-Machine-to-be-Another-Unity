using System;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;

namespace Uduino
{
#if UDUINO_READY

    #region Editor Pin
    [System.Serializable]
    public class EditorPin : Pin
    {
        UduinoManagerEditor editorManager = null;

        public static string[] arduinoUnoPins = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "A0", "A1", "A2", "A3", "A4", "A5" };

        public EditorPin(UduinoDevice arduinoParent, int pin, PinMode mode, UduinoManagerEditor m)
                : base(arduinoParent, pin, mode)
        {
            editorManager = m;
            device = arduinoParent;
            currentPin = pin;
            isEditorPin = true;
            ChangePinMode(mode);
        }

            public override int SendRead(string bundle = null, System.Action<string> action = null, bool digital = false)
        {
            int readVal = 1;
            if (editorManager != null)
            {
                editorManager.Read(device, UduinoManager.BuildMessageParameters("r" + (digital ? "d" : ""), currentPin), action: editorManager.ParseReadData);
            }

            return readVal;
        }

        public override bool WriteMessage(string message, string bundle = null)
        {
            if (editorManager != null)
            {
                editorManager.WriteMessage(device, message);
            }
            return true;
        }
    }

    #endregion

    #region Compilation check
    [InitializeOnLoad] // Make static initialiser be called as soon as the scripts are initialised in the editor (rather than just in play mode).
    public class ExitPlayModeOnScriptCompile
    {
        static bool hasStopped = false;
        static ExitPlayModeOnScriptCompile()
        {
            Unused(_instance);
            _instance = new ExitPlayModeOnScriptCompile();
        }

        private ExitPlayModeOnScriptCompile()
        {
            EditorApplication.update += OnEditorUpdate;
        }

        ~ExitPlayModeOnScriptCompile()
        {
            EditorApplication.update -= OnEditorUpdate;
            hasStopped = false;
            _instance = null;
        }
        private static void OnEditorUpdate()
        {
            if (Resources.FindObjectsOfTypeAll(typeof(UduinoManager)).Length == 0)
                return;

            if (EditorApplication.isPlaying && EditorApplication.isCompiling)
            {
                if (UduinoManager.Instance.enabled && !hasStopped)
                {
                    UduinoManager.Instance.StopThread();
                    UduinoManager.Instance.StopAllCoroutines();
                    Log.Warning("Stopping Uduino due to script compilation when Uduino is running.");
                    hasStopped = true;
                    //  EditorApplication.isPlaying = false;
                }
            }
        }
        private static void Unused<T>(T unusedVariable) { }
        private static ExitPlayModeOnScriptCompile _instance = null;
    }
    #endregion

    [CustomEditor(typeof(UduinoManager))]
    public class UduinoManagerEditor : Editor
    {
        public static UduinoManagerEditor Instance { get; private set; }

        #region Variables
        private UduinoManager manager = null;
        public UduinoManager Manager
        {
            get
            {
                if (Application.isPlaying) return UduinoManager.Instance;
                else return manager;
            }
            set
            {
                manager = value;
            }
        }
        string message = "";
        string messageValue = "";
        // string newBlackListedPort = "";
        string checkVersion = "";

        LogLevel debugLevel;

        static bool defaultPanel = true;
        static bool arduinoPanel = true;
        static bool eventsPanel = true;

        static bool advancedPanel = false;
        static bool blacklistedFoldout = false;

        //Style-relatedx
        Color headerColor = new Color(0.65f, 0.65f, 0.65f, 1);
        //Color backgroundColor = new Color(0.75f, 0.75f, 0.75f);
        Color defaultButtonColor;

        GUIStyle boldtext = null;
        GUIStyle olLight = null;
        GUIStyle olInput = null;
        GUIStyle customFoldtout = null;
        GUIStyle colTitleCenter = null;

        bool isUpToDate = false;
        bool isUpToDateChecked = false;
        bool isPlayAndPause = false;

        // Settings
        public string[] baudRates = new string[] { "9600", "19200", "38400", "57600", "115200", "Other" };
        int prevBaudRateIndex;
        public int baudRateIndex;

        #region Reorderable Lists
        private ReorderableList blackListedList = null;
        private Dictionary<string, ReorderableList> boardLists = new Dictionary<string, ReorderableList>();
        
        List<Pin> lastActivePinList = new List<Pin>();
        UduinoDevice lastClickedDevice = null; // Dirty hack when there are multiple boards

        void EnableLists()
        {
            blackListedList = new ReorderableList(Manager.BlackListedPorts, typeof(string), true, true, true, true);
            blackListedList.drawHeaderCallback += DrawBlackListedHeader;
            blackListedList.drawElementCallback += DrawBlackListedElement;
            blackListedList.onAddCallback += AddBlackListedPortItem;
            blackListedList.onRemoveCallback += RemoveBlackListedPortItem;

            /*
            editorPinsList = new ReorderableList(Manager.pins, typeof(EditorPin), true, true, true, true);
            editorPinsList.drawHeaderCallback += DrawEditorPinHeader;
            editorPinsList.drawElementCallback += DrawEditorPinElement;
            editorPinsList.onAddCallback += AddEditorPinItem;
            editorPinsList.onRemoveCallback += RemoveEditorPinItem;*/
        }

        void DisableLists()
        {
            // Make sure we don't get memory leaks etc.
            if (blackListedList != null)
            {
                blackListedList.drawHeaderCallback -= DrawBlackListedHeader;
                blackListedList.drawElementCallback -= DrawBlackListedElement;
                blackListedList.onAddCallback -= AddBlackListedPortItem;
                blackListedList.onRemoveCallback -= RemoveBlackListedPortItem;
            }
            foreach(KeyValuePair<string, ReorderableList> list in boardLists)
            {
                list.Value.drawHeaderCallback -= DrawEditorPinHeader;
                list.Value.drawElementCallback -= DrawEditorPinElement;
                list.Value.onAddCallback -= AddEditorPinItem;
                list.Value.onRemoveCallback -= RemoveEditorPinItem;
            }
        }

        #region Black listed list
        private void DrawBlackListedHeader(Rect rect) {
            //GUI.Label(new Rect(rect.x, rect.y, rect.width, rect.height), "Serial port name");
            EditorGUI.LabelField(rect, "Serial port name");
        }
        private void DrawBlackListedElement(Rect rect, int index, bool active, bool focused) {
            string item = Manager.BlackListedPorts[index];
            EditorGUI.BeginChangeCheck();
            //  GUI.Label(new Rect(rect.x + 118, rect.y, 30, 22), "", "ProfilerTimelineFoldout");
            item = GUI.TextField(new Rect(rect.x, rect.y +1, rect.width, 18), item);

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(target);
            }
            Manager.BlackListedPorts[index] = item;
        }
        private void AddBlackListedPortItem(ReorderableList list) {
            Manager.BlackListedPorts.Add("Serial port name");
            EditorUtility.SetDirty(target);
        }
        private void RemoveBlackListedPortItem(ReorderableList list) {
            Manager.BlackListedPorts.RemoveAt(list.index);
            EditorUtility.SetDirty(target);
        }
        void DrawBlackListedLayout() {
            if (blackListedList == null)
                blackListedList = new ReorderableList(Manager.BlackListedPorts, typeof(string), true, true, true, true);
            blackListedList.DoLayoutList();
        }
        #endregion

        #region Editor pin list

        public void DrawListForBoard(UduinoDevice board)
        {
            ReorderableList list = null;
            boardLists.TryGetValue(board.name, out list);

            if (list == null) // If it's non existantm create a new one
            {
                list = new ReorderableList(Manager.pins, typeof(EditorPin), false, true, true, true);
                list.drawHeaderCallback += DrawEditorPinHeader;
                list.drawElementCallback += DrawEditorPinElement;
                list.onAddCallback += AddEditorPinItem;
                list.onRemoveCallback += RemoveEditorPinItem;
                boardLists.Add(board.name, list);
            }

            lastClickedDevice = board; // HACK
            lastActivePinList = new List<Pin>(); // HACK

            for (int i = 0; i < Manager.pins.Count; ++i) {
                Pin pin = Manager.pins[i];
                if ((pin.device == board || pin.arduinoName == board.name || ( pin.device == null && pin.arduinoName == null) ) && pin.isEditorPin)
                    lastActivePinList.Add(pin);
            }

            
            list.list = lastActivePinList;
            list.DoLayoutList();
            
          //  Rect headerRect = GUILayoutUtility.GetRect(0, list.headerHeight, GUILayout.ExpandWidth(true));
          
            //GUI.Label(new Rect(headerRect.x, headerRect.y, headerRect.width, headerRect.height), "Test2", "RightLabel");
        }

        private void DrawEditorPinHeader(Rect rect)
        {
            GUI.Label(new Rect(rect.x, rect.y, 30, 22), "Pin", boldtext);
            GUI.Label(new Rect(rect.x + 18, rect.y, 85, 19), "Mode", boldtext);
            GUI.Label(new Rect(rect.x + 105, rect.y, 80, rect.height), "Action", boldtext);
            GUI.Label(new Rect(rect.x, rect.y, rect.width, rect.height), "(Editor pins)", "RightLabel");
         //   GUI.Label(new Rect(headerRect.x, headerRect.y + -15, headerRect.width - 65, headerRect.height), );
        }

        private void DrawPinUI(Rect rect, Pin pin, int boardType, Pin listPin = null)
        {
            CheckChanges(pin);

            EditorGUI.BeginChangeCheck();

            ArduinoBoardType currentBoard = BoardsTypeList.Boards.GetBoardFromId(boardType);
            string[] boardPins = currentBoard.GetPins();
            int[] boardPinValues = currentBoard.GetValues();

            pin.currentPin = EditorGUI.IntPopup(new Rect(rect.x, rect.y + 2, 35, 22), pin.currentPin, boardPins, boardPinValues);
            pin.pinMode = (PinMode)EditorGUI.EnumPopup(new Rect(rect.x + 40, rect.y + 2, 60, 19), pin.pinMode);
            float width = rect.width - 30 - 90;
            int leftOffset = 120;

            GUI.Label(new Rect(rect.x + 103, rect.y, 30, 22), "", "ProfilerTimelineFoldout");

            switch (pin.pinMode)
            {
                case PinMode.Output:
                    if (GUI.Button(new Rect(rect.x + leftOffset, rect.y + 2, width / 2, 15), "HIGH", "minibuttonLeft"))
                        pin.SendPinValue(255, "d");
                    if (GUI.Button(new Rect(rect.x + leftOffset + width / 2, rect.y + 2, width / 2, 15), "LOW", "minibuttonRight"))
                        pin.SendPinValue(0, "d");
                    break;
                case PinMode.Input_pullup:
                case PinMode.Input:
                    if (GUI.Button(new Rect(rect.x + leftOffset, rect.y + 2, width / 4, 16), "Read", "minibuttonLeft"))
                        pin.SendRead(digital: true);
                    UpdateReadPins(pin.device, pin.currentPin, pin.lastReadValue);
                    
                    GUI.Label(new Rect(rect.x + leftOffset + width / 4, rect.y + 2, width / 4 * 3, 14), pin.lastReadValue.ToString(), "TextFieldDropDownText");
                    EditorUtility.SetDirty(target);
                    break;
                case PinMode.PWM:
                case PinMode.Servo:
                    pin.sendValue = EditorGUI.IntSlider(new Rect(rect.x + leftOffset, rect.y, width, 16), pin.sendValue, 0, 255);
                    pin.SendPinValue(pin.sendValue, "a");
                    break;
            }


            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(target);
            }
        }

        private void DrawEditorPinElement(Rect rect, int index, bool active, bool focused)
        {
            Pin targetPin = (Pin)lastActivePinList[index];
            Pin pin = Manager.pins.Find(x => x == targetPin);
            DrawPinUI(rect, pin, lastClickedDevice._boardType, targetPin);
        }

        private void AddEditorPinItem(ReorderableList list)
        {
            Manager.pins.Add(new EditorPin(lastClickedDevice, 13, PinMode.Output, this)); // TODO : there is a problem here in the add button. Have to do it manually 
            EditorUtility.SetDirty(target);
        }

        private void RemoveEditorPinItem(ReorderableList list)
        {
            Pin targetPin = (Pin)list.list[list.index];
            Manager.pins.RemoveAt(Manager.pins.FindIndex(x => x == targetPin));
            /* for(int i = Manager.pins.Count -1; i >= 0; i--)
             {
                 if(targetp == Manager.pins[i])
                 {
                     Manager.pins.RemoveAt(i);
                     continue;
                 }
             }*/
            EditorUtility.SetDirty(target);
        }    
        #endregion

        private void OnDisable()
        {
            DisableLists();
        }

        #endregion

        #endregion

        void OnEnable()
        {
            if (manager == null)
                manager = (UduinoManager)target;

            FindExistingExtensions();
            DisplayActivePlugins(true);
            Instance = this;
            EnableLists();
            GetPanel();
        }

        #region Utils
        public string FirstToLower(string s)
        {
            if (string.IsNullOrEmpty(s))
                return string.Empty;

            char[] a = s.ToCharArray();
            a[0] = char.ToLower(a[0]);
            return new string(a);
        }
        #endregion

        #region Styles
        void SetColorAndStyles()
        {
            if (boldtext == null)
            {
                //Color and GUI
                defaultButtonColor = GUI.backgroundColor;
                if (!EditorGUIUtility.isProSkin)
                {
                    headerColor = new Color(165 / 255f, 165 / 255f, 165 / 255f, 1);
                    //  backgroundColor = new Color(193 / 255f, 193 / 255f, 193 / 255f, 1);
                }
                else
                {
                    headerColor = new Color(41 / 255f, 41 / 255f, 41 / 255f, 1);
                    //    backgroundColor = new Color(56 / 255f, 56 / 255f, 56 / 255f, 1);
                }

                boldtext = new GUIStyle(GUI.skin.label);
                boldtext.fontStyle = FontStyle.Bold;
                boldtext.alignment = TextAnchor.UpperCenter;

                olLight = new GUIStyle("OL Title");
                olLight.fontStyle = FontStyle.Normal;
                olLight.font = GUI.skin.button.font;
                olLight.fontSize = 9;
                olLight.alignment = TextAnchor.MiddleCenter;

                olInput = new GUIStyle("TE toolbar");
                olInput.fontStyle = FontStyle.Bold;
                olInput.fontSize = 10;
                olInput.alignment = TextAnchor.MiddleLeft;

                customFoldtout = new GUIStyle(EditorStyles.foldout);
                customFoldtout.fontStyle = FontStyle.Bold;

                colTitleCenter = new GUIStyle("OL Title");
                colTitleCenter.alignment = TextAnchor.MiddleCenter;
            }
        }

        void SetGUIBackgroundColor(string hex)
        {
            Color color = new Color();
            ColorUtility.TryParseHtmlString(hex, out color);
            GUI.backgroundColor = color;
        }
        void SetGUIBackgroundColor(Color color)
        {
            GUI.backgroundColor = color;
        }
        void SetGUIBackgroundColor()
        {
            GUI.backgroundColor = defaultButtonColor;
        }

        public void DrawLogo()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace(); // Nededed for lastRect
            EditorGUILayout.EndHorizontal();
        
             // load textures
            Texture tex = (Texture)EditorGUIUtility.Load("Assets/Uduino/Editor/Resources/uduino-logo.png");
            Texture tex2 = (Texture)EditorGUIUtility.Load("Assets/Uduino/Editor/Resources/arduino-bg.png");
            GUILayout.Space(0);
            Rect lastRect = GUILayoutUtility.GetLastRect();
            GUI.Box(new Rect(1, lastRect.y + 4, Screen.width, 27), tex);
            lastRect = GUILayoutUtility.GetLastRect();

            Color bgColor = new Color();
            ColorUtility.TryParseHtmlString("#ffffff", out bgColor);

            //draw bg
            EditorGUI.DrawRect(new Rect(lastRect.x - 15, lastRect.y + 5f, Screen.width + 1, 60f), bgColor);

            // draw text
            GUIStyle smallFont = new GUIStyle();
            smallFont.fontSize = 8;
            smallFont.alignment = TextAnchor.UpperRight;
            EditorGUI.LabelField(new Rect(lastRect.x - 15, lastRect.y + 5f, Screen.width + 1, 60f), "v." +UduinoVersion.getVersion()+ " ", smallFont);

            // draw Textures
            GUI.DrawTexture(new Rect(Screen.width / 2 - tex.width / 2 - 20, lastRect.y + 5, tex2.width, tex2.height), tex2, ScaleMode.ScaleToFit);
            GUI.DrawTexture(new Rect(Screen.width / 2 - tex.width / 2, lastRect.y + 10, tex.width, tex.height), tex, ScaleMode.ScaleToFit);

            GUI.color = Color.white;
            GUILayout.Space(60f);
        }

        public void DrawLine(int marginTop, int marginBottom, int height)
        {
            EditorGUILayout.Separator();
            GUILayout.Space(marginTop);
            Rect lastRect = GUILayoutUtility.GetLastRect();
            GUI.Box(new Rect(0f, lastRect.y + 4, Screen.width, height), "");
            GUILayout.Space(marginBottom);
        }

        public bool DrawHeaderTitle(string title, bool foldoutProperty, Color backgroundColor)
        {
            GUILayout.Space(0);
            Rect lastRect = GUILayoutUtility.GetLastRect();
            GUI.Box(new Rect(1, lastRect.y + 4, Screen.width, 27), "");
            lastRect = GUILayoutUtility.GetLastRect();
            EditorGUI.DrawRect(new Rect(lastRect.x - 15, lastRect.y + 5f, Screen.width + 1, 25f), headerColor);
            GUI.Label(new Rect(lastRect.x, lastRect.y + 10, Screen.width, 25), title);
            GUI.color = Color.clear;
            if (GUI.Button(new Rect(0, lastRect.y + 4, Screen.width, 27), ""))
            {
                foldoutProperty = !foldoutProperty;
            }
            GUI.color = Color.white;
            GUILayout.Space(30);
            if (foldoutProperty) { GUILayout.Space(5); }
            return foldoutProperty;
        }
        #endregion

        #region CheckCompatibility and CheckUpdate
        public void CheckCompatibility()
        {
#if UNITY_5_6 || UNITY_2017 || UNITY_2018 || UNITY_2019
            if (PlayerSettings.GetApiCompatibilityLevel(BuildTargetGroup.Standalone) == ApiCompatibilityLevel.NET_2_0_Subset)
#else
            if (PlayerSettings.apiCompatibilityLevel == ApiCompatibilityLevel.NET_2_0_Subset)
    #endif
            {
                SetGUIBackgroundColor("#ef5350");
                EditorGUILayout.HelpBox("Uduino works only with .NET 2.0 (not Subset).", MessageType.Error, true);
                if (GUILayout.Button("Fix Now", GUILayout.ExpandWidth(true)))
                {
    #if UNITY_5_6 || UNITY_2017 || UNITY_2018 || UNITY_2019
                    PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Standalone, ApiCompatibilityLevel.NET_2_0);
    #else
                    PlayerSettings.apiCompatibilityLevel = ApiCompatibilityLevel.NET_2_0;
    #endif
                    PlayerSettings.runInBackground = true;
                    Debug.LogWarning("Reimporting all assets.");
                    AssetDatabase.ImportAsset("Assets/Uduino/Scripts", ImportAssetOptions.ImportRecursive);
                    AssetDatabase.Refresh();
                }
                SetGUIBackgroundColor();
            }
        }
        

#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            string destinationFolder = "~/Documents/Arduino/libraries";
#else
        string destinationFolder = "";
#endif

        public void CheckUpdate()
        {
            if (AssetDatabase.IsValidFolder("Assets/Uduino/Arduino"))
            {
                EditorGUILayout.HelpBox("Uduino has been updated! To continue, select the Arduino libraries folder to add Uduino library.", MessageType.None, true);

    #if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
                EditorGUILayout.HelpBox("The Arduino libraries folder is located under: ~/Documents/Arduino/libraries", MessageType.Info, true);
    #else
                EditorGUILayout.HelpBox("The Arduino libraries folder is located under: C:/Users/<username>/Documents/Arduino/libraries", MessageType.Info, true);
    #endif

                EditorGUILayout.LabelField("Select Arduino libraries Folder");

                GUILayout.BeginHorizontal();

                GUILayout.BeginVertical();
                destinationFolder = EditorGUILayout.TextField(destinationFolder);
                GUILayout.EndVertical();
                GUILayout.BeginVertical();

                if (GUILayout.Button("Select path", GUILayout.ExpandWidth(true)))
                {
                    GUI.FocusControl("");
                    string path = EditorUtility.OpenFolderPanel("Set Arduino path", "", "");
                    if (path.Length != 0)
                    {
                        destinationFolder = path;
                    }
                }
                GUILayout.EndVertical();

                GUILayout.EndHorizontal();


                DrawLine(12, 0, 45);
                SetGUIBackgroundColor("#4FC3F7");

                if (GUILayout.Button("Update Uduino's Arduino library", GUILayout.ExpandWidth(true)))
                {
                    if (destinationFolder == "NOT SET")
                    {
                        if (EditorUtility.DisplayDialog("Move library folder", "You have to set a valid folder path", "Ok", "Cancel"))
                        { }
                    }
                    else if (EditorUtility.DisplayDialog("Move library folder", "Are you sure the Arduino libraries folder is " + destinationFolder + " ?", "Move", "Cancel"))
                    {
                        MoveArduinoFiles(destinationFolder);
                    }
                }
                SetGUIBackgroundColor();
            }
            EditorGUILayout.Separator();
        }
    
        void MoveArduinoFiles(string dest)
        {
            string sourceDirectory = Application.dataPath + "/Uduino/Arduino/libraries";
            string destinationDirectory = FirstToLower(dest);
            MoveDirectory(sourceDirectory, destinationDirectory);
        }

        public static void MoveDirectory(string source, string target)
        {
            var stack = new Stack<Folders>();
            stack.Push(new Folders(source, target));

            while (stack.Count > 0)
            {
                var folders = stack.Pop();
                Directory.CreateDirectory(folders.Target);
                foreach (var file in Directory.GetFiles(folders.Source, "*.*"))
                {
                    string targetFile = Path.Combine(folders.Target, Path.GetFileName(file));
                    if (Path.GetExtension(file) != ".meta")
                    {
                        if (File.Exists(targetFile))
                            File.Delete(targetFile);
                        File.Move(file, targetFile);
                    }
                }

                foreach (var folder in Directory.GetDirectories(folders.Source))
                {
                    stack.Push(new Folders(folder, Path.Combine(folders.Target, Path.GetFileName(folder))));
                }
            }
            Directory.Delete(source, true);
            FileUtil.DeleteFileOrDirectory("Assets/Uduino/Arduino");
        }

        public class Folders
        {
            public string Source { get; private set; }
            public string Target { get; private set; }

            public Folders(string source, string target)
            {
                Source = source;
                Target = target;
            }
        }
        #endregion

        #region Detect Plugins
        public static System.Type[] GetAllSubTypes(System.Type aBaseClass)
        {
           var result = new System.Collections.Generic.List<System.Type>();
            System.Reflection.Assembly[] AS = System.AppDomain.CurrentDomain.GetAssemblies();
            foreach (var A in AS)
            {
                try {
                    System.Type[] types = A.GetTypes();
                    foreach (var T in types)
                    {
                        if (T.IsSubclassOf(aBaseClass))
                            result.Add(T);
                    }
                } catch { }
            }
            return result.ToArray();
        }

        List<string> GetExtensionsSubTypes()
        {
            List<string> extensionsNames = new List<string>();
            foreach (var T in GetAllSubTypes(typeof(UduinoDevice)))
                extensionsNames.Add(T.Name);
            return extensionsNames;
        }

        public void FindExistingExtensions()
        {
            List<string> subTypes = GetExtensionsSubTypes();

            Manager.presentExtentionsMap = new IsPresentDictionnary();

            foreach (KeyValuePair<string, string> extensionType in Manager.existingExtensionsMap)
            {
                if(subTypes.Contains(extensionType.Key))
                    Manager.presentExtentionsMap[extensionType.Key] = true;
            }
        }


        public void DisplayActivePlugins(bool force = false)
        {
            foreach (KeyValuePair<string, bool> presentExtension in Manager.presentExtentionsMap)
            {
                bool isExtensionActive = false;
                Manager.activeExtentionsMap.TryGetValue(presentExtension.Key, out isExtensionActive);
                bool isActiveValue = isExtensionActive;
                if(!force)
                    isActiveValue = EditorGUILayout.Toggle(Manager.existingExtensionsMap[presentExtension.Key], isExtensionActive);

                if (isActiveValue != isExtensionActive || force)
                {
                    string path = "Assets/Uduino/Plugins/";
                    PluginImporter currentPlugin = null;
                    switch (presentExtension.Key)
                    {
                        case "UduinoDevice_AndroidBluetoothLE":
                            path += "AndroidBLE/UduinoAndroidBluetooth.aar";
                            currentPlugin = AssetImporter.GetAtPath(path) as PluginImporter;
                            if (currentPlugin == null) return;
                            currentPlugin.SetCompatibleWithPlatform(BuildTarget.Android, isActiveValue);
                            break;
                        case "UduinoDevice_AndroidSerial":
                            path += "AndroidSerial/Uduino_AndroidSerial.aar";
                            currentPlugin = AssetImporter.GetAtPath(path) as PluginImporter;
                            if (currentPlugin == null) return;

                            currentPlugin.SetCompatibleWithPlatform(BuildTarget.Android, isActiveValue);
                            break;
                        case "UduinoDevice_DesktopBluetoothLE":
                            path += "DesktopBLE/UduinoWinBluetoothLE.dll";
                            currentPlugin = AssetImporter.GetAtPath(path) as PluginImporter;
                            if (currentPlugin == null) return;
                            currentPlugin.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows, isActiveValue);
                            currentPlugin.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows64, isActiveValue);
                            break;
                    }
                    if(!force)
                        Manager.activeExtentionsMap[presentExtension.Key] = isActiveValue;
                }
            }
        }
        #endregion

        #region getPanel
        void SavePanel()
        {
            EditorPrefs.SetBool("uduinoPanelSettings", defaultPanel);
            EditorPrefs.SetBool("uduinoPanelArduino", arduinoPanel);
            EditorPrefs.SetBool("uduinoPanelEvents", eventsPanel);
            EditorPrefs.SetBool("uduinoPanelAdvanced", advancedPanel);
        }

        void GetPanel()
        {
            defaultPanel = EditorPrefs.GetBool("uduinoPanelSettings", defaultPanel);
            arduinoPanel = EditorPrefs.GetBool("uduinoPanelArduino", arduinoPanel);
            eventsPanel = EditorPrefs.GetBool("uduinoPanelEvents", eventsPanel);
            advancedPanel = EditorPrefs.GetBool("uduinoPanelAdvanced", advancedPanel);
        }
        #endregion

        public override void OnInspectorGUI()
        {
            if (manager == null)
                manager = (UduinoManager)target;

            Log.SetLogLevel(manager.debugLevel);
            SetColorAndStyles();

            DrawLogo();

            defaultPanel = DrawHeaderTitle("Uduino Settings", defaultPanel, headerColor);
            if (defaultPanel)
            {
                CheckCompatibility();
                DefaultPanel();
                CheckUpdate();
            }

            arduinoPanel = DrawHeaderTitle("Arduino", arduinoPanel, headerColor);
            if (arduinoPanel)
            {
                ArduinoSettings();
            }

            eventsPanel = DrawHeaderTitle("Events", eventsPanel, headerColor);
            if (eventsPanel)
            {
                ArduinoEvents();
            }

            advancedPanel = DrawHeaderTitle("Advanced", advancedPanel, headerColor);
            if (advancedPanel)
            {
                AdvancedSettings();
            }
           
            if (!Application.isPlaying)
                EditorUtility.SetDirty(target); //TODO : We add that here beacause we the values serialized are not updated
             
            #region Stop All pins on Pause
            if (Manager.stopAllOnPause && Application.isPlaying && EditorApplication.isPaused && !isPlayAndPause)
            {
                isPlayAndPause = true;
                foreach (Pin pinTarget in Manager.pins)
                  pinTarget.Destroy();
                Manager.SendBundle("destroy");
            }
            if (Manager.stopAllOnPause && (isPlayAndPause && !Application.isPlaying) || (isPlayAndPause && !EditorApplication.isPaused))
            {
                Manager.InitAllPins();
                isPlayAndPause = false;
            }

            SavePanel();
            #endregion
        }

        public void CheckCompatibilityErrors()
        {
            bool standalonebuildTarget = (
#if UNITY_2018 || UNITY_2019 || UNITY_2019
                EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneOSX ||
#else
                EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneOSXUniversal ||
                EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneOSXIntel64 ||
                #endif
                EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows ||
                EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows64 ||
                EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneLinux ||
                EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneLinux64);


            if (standalonebuildTarget && !Manager.ExtensionIsPresentAndActive("UduinoDevice_Wifi") &&
                !Manager.ExtensionIsPresentAndActive("UduinoDevice_DesktopBluetoothLE") && !Manager.ExtensionIsPresentAndActive("UduinoDevice_DesktopSerial"))
            {
                EditorGUILayout.HelpBox("To build for desktop, select at least one connection type.", MessageType.Warning, true);
            }
            else if ( ( Manager.ExtensionIsPresentAndActive("UduinoDevice_DesktopBluetoothLE") ||
                Manager.ExtensionIsPresentAndActive("UduinoDevice_DesktopSerial") ) 
                && !standalonebuildTarget)
            {
                GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
                EditorGUI.indentLevel--;
                EditorGUILayout.HelpBox("Build target set for Android but Uduino desktop is selected.", MessageType.Warning, true);
                if (GUILayout.Button("Switch build target to Standalone"))
                {
                    EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows);
                }
                EditorGUI.indentLevel++;
                GUILayout.EndVertical();
            }

            if (Manager.ExtensionIsPresentAndActive("UduinoDevice_AndroidBluetoothLE") && Manager.ExtensionIsPresentAndActive("UduinoDevice_AndroidSerial"))
            {
                EditorGUILayout.HelpBox("Having both Android BLE and Android Serial cause some conflits when building.", MessageType.Warning, true);
            }

            if (Manager.ExtensionIsPresentAndActive("UduinoDevice_AndroidBluetoothLE") &&
                PlayerSettings.Android.minSdkVersion != AndroidSdkVersions.AndroidApiLevel21 &&
                PlayerSettings.Android.targetSdkVersion != AndroidSdkVersions.AndroidApiLevel23)
            {
                EditorGUI.indentLevel--;

                GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
                SetGUIBackgroundColor();
                SetGUIBackgroundColor("#ef5350");
                EditorGUILayout.HelpBox("Change the Arduino settings for Uduino BLE", MessageType.Error, true);
                EditorGUI.indentLevel++;
                GUILayout.BeginHorizontal();
                SetGUIBackgroundColor("#4FC3F7");
                if (PlayerSettings.Android.minSdkVersion != AndroidSdkVersions.AndroidApiLevel21 && GUILayout.Button("Min SDK Version: 21"))
                {
                    PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel21;
                }
                if (PlayerSettings.Android.targetSdkVersion != AndroidSdkVersions.AndroidApiLevel23 &&  GUILayout.Button("Target SDK Version: 23"))
                {
                    PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel23;
                }
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();

                SetGUIBackgroundColor();
            }

            else if (Manager.ExtensionIsPresentAndActive("UduinoDevice_AndroidSerial") &&
               PlayerSettings.Android.minSdkVersion != AndroidSdkVersions.AndroidApiLevel19 &&
               PlayerSettings.Android.targetSdkVersion != AndroidSdkVersions.AndroidApiLevel23)
            {
                EditorGUI.indentLevel--;

                // GUILayout.BeginVertical("Box", GUILayout.ExpandWidth(true));
                GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
                SetGUIBackgroundColor();
                SetGUIBackgroundColor("#ef5350");
                EditorGUILayout.HelpBox("Change the Arduino settings for Uduino Android Serial", MessageType.Error, true);
                EditorGUI.indentLevel++;
                GUILayout.BeginHorizontal();
                SetGUIBackgroundColor("#4FC3F7");
                if (PlayerSettings.Android.minSdkVersion != AndroidSdkVersions.AndroidApiLevel19 && GUILayout.Button("Min SDK Version: 19"))
                {
                    PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel19;
                }
                if (PlayerSettings.Android.targetSdkVersion != AndroidSdkVersions.AndroidApiLevel23 && GUILayout.Button("Target SDK Version: 23"))
                {
                    PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel23;
                }
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();

                SetGUIBackgroundColor();
            }


        }


            #region Default Panel
        public void DefaultPanel()
        {
            GUILayout.Label("Connection type", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            DisplayActivePlugins();

            CheckCompatibilityErrors();

            EditorGUI.indentLevel--;

            GUILayout.Label("General", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            LogLevel tmpLogLevel = (LogLevel)EditorGUILayout.EnumPopup("Log Level", Manager.debugLevel);
            if (tmpLogLevel != Manager.debugLevel)
            {
                Manager.debugLevel = tmpLogLevel;
                EditorUtility.SetDirty(target);
            }
            Manager.interfaceType = (UduinoInterfaceType)EditorGUILayout.EnumPopup("Interface Type", Manager.interfaceType);
            Manager.ReadingMethod = (HardwareReading)EditorGUILayout.EnumPopup("Reading Method", Manager.ReadingMethod);
            EditorGUI.indentLevel--;

            GUILayout.Label("Arduino", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            if (baudRateIndex != baudRates.Length - 1)
                baudRateIndex = System.Array.IndexOf(baudRates, Manager.BaudRate.ToString());
            if (baudRateIndex == -1) baudRateIndex = baudRates.Length - 1;

            baudRateIndex = EditorGUILayout.Popup("Baud Rate", baudRateIndex, baudRates);
            if (baudRateIndex == baudRates.Length - 1) // if it's "Other" 
            {
                manager.BaudRate = EditorGUILayout.IntField("Speed", manager.BaudRate);
                prevBaudRateIndex = baudRateIndex;
            } else if (prevBaudRateIndex != baudRateIndex) {
                 int result = 9600;
                 int.TryParse(baudRates[baudRateIndex], out result);
                 manager.BaudRate = result;
                 prevBaudRateIndex = baudRateIndex;
            }
            EditorGUI.indentLevel--;

            // Extension settings
            if (Manager.ExtensionIsPresentAndActive("UduinoDevice_AndroidSerial"))
            {
                GUILayout.Label("Android Serial Options", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                Manager.displayAndroidTextGUI = EditorGUILayout.Toggle("Debug Textfield", Manager.displayAndroidTextGUI);
                EditorGUI.indentLevel--;
            }

            if (Manager.ExtensionIsPresentAndActive("UduinoDevice_AndroidBluetoothLE"))
            {
                GUILayout.Label("Android BLE Options", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                Manager.displayAndroidTextGUI = EditorGUILayout.Toggle("Debug Textfield", Manager.displayAndroidTextGUI);
                Manager.bleScanDuration = EditorGUILayout.IntField("Scan duration", Manager.bleScanDuration);
                EditorGUI.indentLevel--;
            }

            if (Manager.ExtensionIsPresentAndActive("UduinoDevice_DesktopBluetoothLE"))
            {
                GUILayout.Label("Destop BLE Options", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                Manager.bleScanDuration = EditorGUILayout.IntField("Scan duration", Manager.bleScanDuration);
                EditorGUI.indentLevel--;
            }

            if (Manager.ExtensionIsPresentAndActive("UduinoDevice_Wifi"))
            {
                GUILayout.Label("Wifi Options", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                Manager.uduinoIpAddress = EditorGUILayout.TextField("Board IP Address", Manager.uduinoIpAddress);
                Manager.uduinoWifiPort = EditorGUILayout.IntField("Connection port", Manager.uduinoWifiPort);
                EditorGUI.indentLevel--;
            }



            EditorGUILayout.Separator();
        }
            #endregion

            #region Arduino Settings
        public void ArduinoSettings()
        {
            if (Manager.uduinoDevices.Count == 0)
            {
                SetGUIBackgroundColor("#ef5350");
                GUILayout.BeginVertical("Box", GUILayout.ExpandWidth(true));
                GUILayout.Label("No Arduino connected", boldtext);
                GUILayout.EndVertical();
                SetGUIBackgroundColor();
            }
            else
            {
                foreach (KeyValuePair<string, UduinoDevice> uduino in Manager.uduinoDevices)
                {
                    // send extra queue 
                    if(!Application.isPlaying)
                    {
                        if(uduino.Value.writeQueue.Count > 0)
                        {
                            Manager.ReadWriteArduino(uduino.Value);
                        }
                    }

                    SetGUIBackgroundColor("#4FC3F7");
                    GUILayout.BeginVertical("Box", GUILayout.ExpandWidth(true));
                    GUILayout.Label(uduino.Key, boldtext);
                    GUILayout.EndVertical();
                    SetGUIBackgroundColor();

                    // Draz informations with the new style
                    GUILayout.Space(5f);

                    GUILayout.BeginVertical("RL Header");
                    GUILayout.Space(1f);
                    EditorGUILayout.LabelField("Board informations", EditorStyles.boldLabel);
                    GUILayout.EndVertical();

                    GUILayout.BeginVertical("RL Background");
                    GUILayout.Space(5f);
                    EditorGUILayout.TextField("Last read message", uduino.Value.lastRead);
                    EditorGUILayout.TextField("Last sent message", uduino.Value.lastWrite);
                    GUILayout.Space(5f);
                    GUILayout.EndVertical();
                    GUILayout.Space(10f);

                    #region Pin Active
                    if ( (uduino.Key.Contains("uduinoBoard") || uduino.Key.Contains("uduino")) && Application.isPlaying)
                    {
                        Rect headerRect = GUILayoutUtility.GetRect(0, -1, GUILayout.ExpandWidth(true));
                        headerRect.xMin += 6; headerRect.xMax -= 6; // Left right padding
                        Rect contentRect = GUILayoutUtility.GetRect(10, 0, GUILayout.ExpandWidth(true));
                        GUILayout.BeginVertical("RL Header");
                        GUILayout.Space(5f);
                        GUI.Label(new Rect(headerRect.x, headerRect.y, 30, 18), "Pin", boldtext);
                        GUI.Label(new Rect(headerRect.x + 18, headerRect.y, 85, 18), "Mode", boldtext);
                        GUI.Label(new Rect(headerRect.x + 105, headerRect.y, 80, 18), "Action", boldtext);
                        GUI.Label(new Rect(headerRect.x, headerRect.y, headerRect.width, 18), "(Scripted pins)", "RightLabel");
                        EditorGUILayout.LabelField("");
                        GUILayout.EndVertical();

                        GUILayout.BeginVertical("RL Background");
                        //GUILayout.Space(5f);
                        contentRect.yMin += 2; contentRect.yMax -= 5;
                        contentRect.xMin += 6; contentRect.xMax -= 6; // Left right padding
                                                                      //      contentRect.y += 21;
                        contentRect.y = contentRect.y + 19;

                        if (Manager.pins.Count != 0) // If a pin is active
                        {
                            bool hasScriptedPin = false;
                            foreach (Pin pin in Manager.pins)
                            {
                                if ((pin.device == uduino.Value || pin.device == null) && !pin.isEditorPin)
                                {
                                    DrawPinUI(contentRect,pin, uduino.Value._boardType);
                                    EditorGUILayout.LabelField("");
                                    hasScriptedPin = true;
                                    GUILayout.Space(3f);
                                    contentRect.y = contentRect.y + 21;
                                }
                            }
                            if (!hasScriptedPin)
                                GUILayout.Label("No arduino pins are currently setup by code.");
                        }
                        else // if no pins are active
                        {
                            GUILayout.Label("No arduino pins are currently setup by code.");
                        }
                        GUILayout.Space(5f);
                    
                        GUILayout.EndVertical();
                        GUILayout.Space(10f);
                    }
                    #endregion

                    #region Send Command
                    // GUILayout.Label("Send commands", EditorStyles.boldLabel);
                    //GUILayout.BeginVertical("Box");
                    if (uduino.Key.Contains("uduinoBoard") || uduino.Key.Contains("uduino")) // Display the informations for default Uduino Board
                    {
                        DrawListForBoard(uduino.Value);
                    }
                    else // If it's a "Normal" Arduino
                    {
                        GUILayout.BeginVertical("RL Header");
                        GUILayout.Space(1f);
                        EditorGUILayout.LabelField("Send command", EditorStyles.boldLabel);
                        GUILayout.EndVertical();

                        GUILayout.BeginVertical("RL Background");
                        GUILayout.Space(5f);

                        Rect contentRect = GUILayoutUtility.GetRect(0, 0, GUILayout.ExpandWidth(true));
                        contentRect.xMin += 6; contentRect.xMax -= 6; // Left right padding
                        GUILayout.Space(5f);
                        GUILayout.Label("");

                        message = GUI.TextField(new Rect(contentRect.x  , contentRect.y + 2, contentRect.width / 2, 14), message, "TextFieldDropDownText");

                        if (GUI.Button(new Rect(contentRect.x + contentRect.width / 2, contentRect.y + 2, contentRect.width / 4, 16), "Send", "minibuttonmid")) {
                            if (messageValue != "") Manager.sendCommand(uduino.Value, message + " " + messageValue);
                            else Manager.sendCommand(uduino.Value, message);
                            Manager.Read(uduino.Value);
                            Manager.ReadWriteArduino(uduino.Value);
                        }
                        if (GUI.Button(new Rect(contentRect.x + contentRect.width / 4 * 3, contentRect.y + 2, contentRect.width / 4, 16), "Read", "minibuttonright")) {
                            Manager.ReadWriteArduino(uduino.Value);
                        }

                        GUILayout.Space(5f);
                        GUILayout.EndVertical();
                        GUILayout.Space(10f);
                       
                    }
                    //  GUILayout.EndVertical();
                    #endregion

                    #region Board settings
                    //More setings
                    //  EditorGUILayout.Separator();
                    bool foldout = EditorPrefs.GetBool(uduino.Key);
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.GetControlRect(true, 16f, EditorStyles.foldout);
                    Rect foldRect = GUILayoutUtility.GetLastRect();
                    if (Event.current.type == EventType.MouseUp && foldRect.Contains(Event.current.mousePosition))
                    {
                        foldout = !foldout;
                        EditorPrefs.SetBool(uduino.Key, foldout);
                        GUI.changed = true;
                        Event.current.Use();
                    }

                    foldout = EditorGUI.Foldout(foldRect, foldout, "Other settings", customFoldtout);
                    if (foldout)
                    {
                        EditorGUI.indentLevel++;
                        uduino.Value._boardType = EditorGUILayout.Popup("Board Type", uduino.Value._boardType, BoardsTypeList.Boards.ListToNames());
                        uduino.Value.readTimeout = EditorGUILayout.IntField("Read timeout", uduino.Value.readTimeout);
                        uduino.Value.writeTimeout = EditorGUILayout.IntField("Write timeout", uduino.Value.writeTimeout);
                        uduino.Value.alwaysRead = EditorGUILayout.Toggle("Always read", uduino.Value.alwaysRead);

                        if (!uduino.Value.alwaysRead)
                            uduino.Value.readAfterCommand = EditorGUILayout.Toggle("Read after commands", uduino.Value.readAfterCommand);

                        EditorGUI.indentLevel++;
                        GUILayout.BeginVertical();
                      
                      
                        if (GUILayout.Button("Unset pins",GUILayout.ExpandWidth(true)))
                        {
                            foreach (Pin pin in Manager.pins.ToArray())
                            {
                                if(pin.device == null || pin.device == uduino.Value)
                                {
                                    pin.Destroy();
                                    Manager.pins.Remove(pin);
                                }
                            }
                            Manager.SendBundle("destroy");
                           // Manager.pins.Clear();
                        }
                        GUILayout.EndVertical();
                        if (Manager.uduinoDevices.Count > 1)
                        {
                            GUILayout.BeginVertical();
                            SetGUIBackgroundColor("#ef5350");
                            if (GUILayout.Button("Disconnect this board"))
                            {
                                Manager.CloseDevice(uduino.Value);
                            }
                            SetGUIBackgroundColor();
                            GUILayout.EndVertical();
                        }
                        EditorGUI.indentLevel--; EditorGUI.indentLevel--;
                    }
                    GUILayout.Space(5f);

                    if (uduino.Value.alwaysRead || uduino.Value.readAfterCommand)
                        EditorUtility.SetDirty(target);
            #endregion
                }
            }

            #region Discover/Close
            DrawLine(12, 0, 45);

            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical();
            SetGUIBackgroundColor("#4FC3F7");
            if (GUILayout.Button("Discover ports"))
            {
                Manager.DiscoverPorts();
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            SetGUIBackgroundColor("#ef5350");
            if (GUILayout.Button("Close ports"))
            {
                Manager.FullReset();
                //Manager.pins.Clear();
            }
            SetGUIBackgroundColor();
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            EditorGUILayout.Separator();
            #endregion
        }
            #endregion

            #region Events
        public void ArduinoEvents()
        {

            SerializedProperty valueEvents = serializedObject.FindProperty("OnDataReceivedEvent"); // <-- UnityEvent
            EditorGUILayout.PropertyField(valueEvents);
            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
            }

            SerializedProperty connectEvent = serializedObject.FindProperty("OnBoardConnectedEvent"); // <-- UnityEvent
            EditorGUILayout.PropertyField(connectEvent);
            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
            }

            SerializedProperty disconnectEvent = serializedObject.FindProperty("OnBoardDisconnectedEvent"); // <-- UnityEvent
            EditorGUILayout.PropertyField(disconnectEvent);
            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
            }


        }
            #endregion

            #region Advanced Settings
        public void AdvancedSettings()
        {
            GUILayout.Label("Serial Settings", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            Manager.defaultArduinoBoardType = EditorGUILayout.Popup("Board Type", Manager.defaultArduinoBoardType, BoardsTypeList.Boards.ListToNames());
            Manager.readTimeout = EditorGUILayout.IntField("Read timeout", Manager.readTimeout);
            Manager.writeTimeout = EditorGUILayout.IntField("Write timeout", Manager.writeTimeout);
            Manager.threadFrequency = EditorGUILayout.IntField("Thread Frequency", Manager.threadFrequency);
            if(Manager.defaultArduinoBoardType == BoardsTypeList.Boards.GetBoardIdFromName("Arduino Mega") && Manager.delayBeforeDiscover < 1)
                EditorGUILayout.HelpBox("For Arduino Mega, please increase the value \"Discover Delay\" to more than 1 second.", MessageType.Error, true);
            EditorGUI.indentLevel--;


            GUILayout.Label("Messages", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            if (manager.LimitSendRate = EditorGUILayout.Toggle("Limit Send Rate", Manager.LimitSendRate))
                if (manager.LimitSendRate)
                {
                    Manager.SendRateSpeed = EditorGUILayout.IntField("Send Rate speed", Manager.SendRateSpeed);
                    EditorGUILayout.Separator();
                }

            Manager.alwaysRead = EditorGUILayout.Toggle("Always read", Manager.alwaysRead);
            if(!Manager.alwaysRead)
                Manager.readAfterCommand = EditorGUILayout.Toggle("Read after commands", Manager.readAfterCommand);

            Manager.skipMessageQueue = EditorGUILayout.Toggle("Skip Queue", Manager.skipMessageQueue);
            if(!Manager.skipMessageQueue)
                Manager.messageQueueLength = EditorGUILayout.IntField("Queue size", Manager.messageQueueLength);

            EditorGUI.indentLevel--;

            GUILayout.Label("Discovery settings", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            Manager.autoDiscover = EditorGUILayout.Toggle("Discover on play", Manager.autoDiscover);
            if(Manager.autoDiscover)
                Manager.delayBeforeDiscover = EditorGUILayout.FloatField("Discover delay", Manager.delayBeforeDiscover);
            Manager.DiscoverTries = EditorGUILayout.IntField("Discovery tries", Manager.DiscoverTries);

            blacklistedFoldout = EditorGUI.Foldout(GUILayoutUtility.GetRect(40f, 40f, 16f, 16f, EditorStyles.foldout), blacklistedFoldout, "Blacklisted ports", true, EditorStyles.foldout);

            if (blacklistedFoldout)
            {
                EditorGUI.indentLevel--;
                DrawBlackListedLayout();
                EditorGUI.indentLevel++;
            }

            GUILayout.Label("On disconnect", EditorStyles.boldLabel);

            Manager.stopAllOnQuit = EditorGUILayout.Toggle("Reset pins", Manager.stopAllOnQuit);
            Manager.stopAllOnPause = EditorGUILayout.Toggle("Reset pins on pause", Manager.stopAllOnPause);
            Manager.autoReconnect = EditorGUILayout.Toggle("Auto reconnect", Manager.autoReconnect);
            Manager.autoReconnectDelay = EditorGUILayout.FloatField("Reconnect delay", Manager.autoReconnectDelay);


            GUILayout.Label("Advanced commands", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            string param = UduinoManager.parametersDelimiter == " " ? "space" : UduinoManager.parametersDelimiter;
            EditorGUILayout.LabelField("Parameters delimiter (" + param + ")");
            UduinoManager.parametersDelimiter = EditorGUILayout.TextField(UduinoManager.parametersDelimiter);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (UduinoManager.parametersDelimiter.Length > 1)
                EditorGUILayout.LabelField("The parameters delimiter should  be one character");
            if (UduinoManager.parametersDelimiter == "\n")
                EditorGUILayout.LabelField("The parameters delimiter should not be '\\n'");
            GUILayout.EndHorizontal();


            /*
            GUILayout.Label("Debug", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            //  SetGUIBackgroundColor("#4FC3F7");
            if (GUILayout.Button("Get port state"))
            {
                Manager.GetPortState();
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            //  SetGUIBackgroundColor("#ef5350");
            if (GUILayout.Button("Clear console"))
            {
                var logEntries = System.Type.GetType("UnityEditorInternal.LogEntries,UnityEditor.dll");
                var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                clearMethod.Invoke(null, null);
            }
            // SetGUIBackgroundColor();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            */

            GUILayout.Label("Update Uduino", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            EditorGUI.indentLevel--;
            EditorGUI.indentLevel--;
            EditorGUILayout.HelpBox("Current version: " + UduinoVersion.getVersion(), MessageType.None);

            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            // SetGUIBackgroundColor("#4FC3F7");
            if (GUILayout.Button("Check for update"))
            {
                string url = "https://marcteyssier.com/uduino/version/";
                try
                {
                    //TODO : Test when network not active
                    UnityWebRequest uwr = UnityWebRequest.Get(url);
#if UNITY_5_6
                    uwr.Send();
#else
                    uwr.SendWebRequest();
#endif
                    while (!uwr.isDone) { }
#if UNITY_5_6
                    if (uwr.isError)
#else
                    if (uwr.isNetworkError)
#endif
                        Debug.Log("Error while getting the data");
                    else
                        checkVersion = uwr.downloadHandler.text;


                    uwr.Dispose();
                    if (checkVersion.Length > 20)
                        checkVersion = "error";
                    isUpToDateChecked = true;
                    if (checkVersion == UduinoVersion.getVersion()) isUpToDate = true;
                    else isUpToDate = false;
                }
                catch (Exception e)
                {
                    Log.Debug("Error while connecting to the server.\r\n" + e);
                }
            }

            // SetGUIBackgroundColor();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            if (isUpToDateChecked)
            {
                if (isUpToDate) EditorGUILayout.HelpBox("Uduino is up to date (" + checkVersion + ")", MessageType.Info, true);
                else EditorGUILayout.HelpBox("Uduino is not up to date.\nGet the last version  (" + checkVersion + ") on the Asset Store.", MessageType.Error, true);
            }

            if (!AssetDatabase.IsValidFolder("Assets/Uduino/Arduino"))
            {
                GUILayout.BeginHorizontal();

                GUILayout.BeginVertical();

                EditorGUILayout.HelpBox("Arduino library not present", MessageType.None);
                GUILayout.EndVertical();

                GUILayout.BeginVertical();
                if (GUILayout.Button("  Download Library "))
                {
                    Application.OpenURL("https://marcteyssier.com/uduino/content/version/uduino_library.unitypackage");
                }
                // SetGUIBackgroundColor();
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }

            EditorGUILayout.Separator();
        }
            #endregion

        IEnumerator GetText()
        {
            using (UnityWebRequest www = UnityWebRequest.Get("http://www.my-server.com"))
            {
#if UNITY_2018 || UNITY_2019
                 yield return www.SendWebRequest();
#else
                yield return www.Send();
#endif
                Debug.Log(www.downloadHandler.text);
            }
        }

        void CheckChanges(Pin pin)
        {
            if (Application.isPlaying) {
                foreach (Pin pinTarget in Manager.pins) {
                    if (pinTarget.PinTargetExists(pin.device, pin.currentPin)) {
                        if (pin.pinMode != pin.prevPinMode) {
                            pinTarget.OverridePinMode(pin.pinMode);
                        }
                       // pinTarget.lastReadValue = pin.lastReadValue;
                      // pinTarget.sendValue = pin.sendValue;
                    }
                }
            }

            if (pin.currentPin != pin.prevPin && pin.currentPin != -1) {
                WriteMessage(pin.device, UduinoManager.BuildMessageParameters("s", pin.currentPin, (int)pin.pinMode));
                pin.prevPin = pin.currentPin;
            }

            if (pin.pinMode != pin.prevPinMode) {
                WriteMessage(pin.device, UduinoManager.BuildMessageParameters("s", pin.currentPin, (int)pin.pinMode));
                pin.prevPinMode = pin.pinMode;
            }

        }

        #region Read and write values to Pins
        public void ParseReadData(string data)
        {

            //  Debug.Log(""); //This yield is cool !
            int recivedPin = -1;
            int.TryParse(data.Split(new string[] { UduinoManager.parametersDelimiter }, StringSplitOptions.None)[0], out recivedPin);

            int value = 0;
            int.TryParse(data.Split(new string[] { UduinoManager.parametersDelimiter }, StringSplitOptions.None)[1], out value);

            if (recivedPin != -1)
            {
                foreach (Pin pinTarget in Manager.pins)
                {
                    if (pinTarget.PinTargetExists(null, recivedPin))
                    {
                        pinTarget.lastReadValue = value;
                    }
                }
            }
        }

        /// <summary>
        /// Update the state of a read pin
        /// </summary>
        /// <param name="target"></param>
        /// <param name="pin"></param>
        /// <param name="value"></param>
        void UpdateReadPins(UduinoDevice target, int pin, int value)
        {
            foreach (Pin pinTarget in Manager.pins)
            {
                if (pinTarget.PinTargetExists(target, pin))
                {
                    pinTarget.lastReadValue = value;
                }
            }
        }

        public void WriteMessage(UduinoDevice targetBoard, string message)
        {
            Manager.sendCommand(targetBoard, message);
            Manager.ReadWriteArduino(targetBoard);
        }

        public void Read(UduinoDevice target = null, string variable = null, System.Action<string> action = null)
        {
            Manager.DirectReadFromArduino(target, variable, action: action);
            Manager.ReadWriteArduino(target);
        }
        #endregion

    
    }

#else

        [CustomEditor(typeof(UduinoManager))]
        public class UduinoManagerEditor : Editor
        {

            public static UduinoManagerEditor Instance { get; private set; }

            #region Variables
            //Style-relatedx
            Color headerColor = new Color(0.65f, 0.65f, 0.65f, 1);
            //Color backgroundColor = new Color(0.75f, 0.75f, 0.75f);
            Color defaultButtonColor;

            GUIStyle boldtext = null;
            GUIStyle olLight = null;
            GUIStyle olInput = null;
            GUIStyle customFoldtout = null;
            #endregion

            const string define = "UDUINO_READY";

            bool defaultPanel = true;
            bool firstDone = false;
            bool secondDone = false;

            void OnEnable()
            {
                Instance = this;
                FindExistingExtensions();
                UduinoManager t = (UduinoManager)target;
                t.activeExtentionsMap["UduinoDevice_DesktopSerial"] = true;
            }

            #region Styles
            void SetColorAndStyles()
            {
                if (boldtext == null)
                {
                    //Color and GUI
                    defaultButtonColor = GUI.backgroundColor;
                    if (!EditorGUIUtility.isProSkin)
                    {
                        headerColor = new Color(165 / 255f, 165 / 255f, 165 / 255f, 1);
                        //  backgroundColor = new Color(193 / 255f, 193 / 255f, 193 / 255f, 1);
                    }
                    else
                    {
                        headerColor = new Color(41 / 255f, 41 / 255f, 41 / 255f, 1);
                        //    backgroundColor = new Color(56 / 255f, 56 / 255f, 56 / 255f, 1);
                    }

                    boldtext = new GUIStyle(GUI.skin.label);
                    boldtext.fontStyle = FontStyle.Bold;
                    boldtext.alignment = TextAnchor.UpperCenter;

                    olLight = new GUIStyle("OL Title");
                    olLight.fontStyle = FontStyle.Normal;
                    olLight.font = GUI.skin.button.font;
                    olLight.fontSize = 9;
                    olLight.alignment = TextAnchor.MiddleCenter;

                    olInput = new GUIStyle("TE toolbar");
                    olInput.fontStyle = FontStyle.Bold;
                    olInput.fontSize = 10;
                    olInput.alignment = TextAnchor.MiddleLeft;

                    customFoldtout = new GUIStyle(EditorStyles.foldout);
                    customFoldtout.fontStyle = FontStyle.Bold;

                }
            }

            void SetGUIBackgroundColor(string hex)
            {
                Color color = new Color();
                ColorUtility.TryParseHtmlString(hex, out color);
                GUI.backgroundColor = color;
            }
            void SetGUIBackgroundColor(Color color)
            {
                GUI.backgroundColor = color;
            }
            void SetGUIBackgroundColor()
            {
                GUI.backgroundColor = defaultButtonColor;
            }

            public void DrawLogo()
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace(); // Nededed for lastRect
                EditorGUILayout.EndHorizontal();

                Texture tex = (Texture)EditorGUIUtility.Load("Assets/Uduino/Editor/Resources/uduino-logo.png");
                Texture tex2 = (Texture)EditorGUIUtility.Load("Assets/Uduino/Editor/Resources/arduino-bg.png");
                GUILayout.Space(0);
                Rect lastRect = GUILayoutUtility.GetLastRect();
                GUI.Box(new Rect(1, lastRect.y + 4, Screen.width, 27), tex);
                lastRect = GUILayoutUtility.GetLastRect();

                Color bgColor = new Color();
                ColorUtility.TryParseHtmlString("#ffffff", out bgColor);

                EditorGUI.DrawRect(new Rect(lastRect.x - 15, lastRect.y + 5f, Screen.width + 1, 60f), bgColor);
                GUI.DrawTexture(new Rect(Screen.width / 2 - tex.width / 2 - 20, lastRect.y + 5, tex2.width, tex2.height), tex2, ScaleMode.ScaleToFit);

                GUI.DrawTexture(new Rect(Screen.width / 2 - tex.width / 2, lastRect.y + 10, tex.width, tex.height), tex, ScaleMode.ScaleToFit);

                GUI.color = Color.white;
                GUILayout.Space(60f);
            }

            public void DrawLine(int marginTop, int marginBottom, int height)
            {
                EditorGUILayout.Separator();
                GUILayout.Space(marginTop);
                Rect lastRect = GUILayoutUtility.GetLastRect();
                GUI.Box(new Rect(0f, lastRect.y + 4, Screen.width, height), "");
                GUILayout.Space(marginBottom);
            }

            public bool DrawHeaderTitle(string title, bool foldoutProperty, Color backgroundColor)
            {
                GUILayout.Space(0);
                Rect lastRect = GUILayoutUtility.GetLastRect();
                GUI.Box(new Rect(1, lastRect.y + 4, Screen.width, 27), "");
                lastRect = GUILayoutUtility.GetLastRect();
                EditorGUI.DrawRect(new Rect(lastRect.x - 15, lastRect.y + 5f, Screen.width + 1, 25f), headerColor);
                GUI.Label(new Rect(lastRect.x, lastRect.y + 10, Screen.width, 25), title);
                GUI.color = Color.clear;
                if (GUI.Button(new Rect(0, lastRect.y + 4, Screen.width, 27), ""))
                {
                    foldoutProperty = !foldoutProperty;
                }
                GUI.color = Color.white;
                GUILayout.Space(30);
                if (foldoutProperty) { GUILayout.Space(5); }
                return foldoutProperty;
            }


            #endregion
            public void FindExistingExtensions() { }

            public void CheckCompatibility()
            {
                var target = EditorUserBuildSettings.activeBuildTarget;
                var group = BuildPipeline.GetBuildTargetGroup(target);

#if UNITY_2018 || UNITY_2019
                if (PlayerSettings.GetApiCompatibilityLevel(group) != ApiCompatibilityLevel.NET_4_6)
#elif UNITY_5_6
                if (PlayerSettings.GetApiCompatibilityLevel(group) == ApiCompatibilityLevel.NET_2_0_Subset)
#else
            if (PlayerSettings.apiCompatibilityLevel == ApiCompatibilityLevel.NET_2_0_Subset)
#endif
                {
                    SetGUIBackgroundColor("#ef5350");
#if UNITY_2018 || UNITY_2019
                    EditorGUILayout.HelpBox("Uduino works with .NET 4.x", MessageType.Error, true);
#else
                EditorGUILayout.HelpBox("Uduino works with .NET 2.0 (not Subset).", MessageType.Error, true);
#endif
                    SetGUIBackgroundColor();

                    DrawLine(12, 0, 45);

                    SetGUIBackgroundColor("#4FC3F7");
                    if (GUILayout.Button("Fix Now", GUILayout.ExpandWidth(true)))
                    {
#if UNITY_2018 || UNITY_2019
                        PlayerSettings.scriptingRuntimeVersion = ScriptingRuntimeVersion.Latest;
                        PlayerSettings.SetApiCompatibilityLevel(group, ApiCompatibilityLevel.NET_4_6);
#elif UNITY_5_6
                        PlayerSettings.SetApiCompatibilityLevel(group, ApiCompatibilityLevel.NET_2_0);
#else
                        PlayerSettings.apiCompatibilityLevel = ApiCompatibilityLevel.NET_2_0;
#endif
                        PlayerSettings.runInBackground = true;
                    }
                    SetGUIBackgroundColor();
                }
                else
                {
                    //   EditorGUILayout.HelpBox("Project settings is already set to .NET 2.0", MessageType.Info, true);
                    StepDone();
                    secondDone = true;
                }
            }
            string destinationFolder = "NOT SET";

            public override void OnInspectorGUI()
            {
                SetColorAndStyles();

                DrawLogo();

                DrawHeaderTitle("Uduino Setup", defaultPanel, headerColor);

                //EditorGUILayout.HelpBox("Before getting started, you need to set-up Uduino.", MessageType.None);
                //EditorGUILayout.Space();

                DrawHeaderTitle("1. Add Uduino library in the Arduino folder", defaultPanel, headerColor);

                if (!AssetDatabase.IsValidFolder("Assets/Uduino/Arduino"))
                {
                    firstDone = true;
                    StepDone();
                }

                if (AssetDatabase.IsValidFolder("Assets/Uduino/Arduino"))
                {

                    EditorGUILayout.HelpBox("To use Uduino you will need the dedicated Arduino library. Select the Arduino libraries folder to add Uduino library.", MessageType.None, true);

#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
                EditorGUILayout.HelpBox("The Arduino libraries folder is located under: ~/Documents/Arduino/libraries ", MessageType.Info, true);
#else
                    EditorGUILayout.HelpBox("The Arduino libraries folder is located under: C:/Users/<username>/Documents/Arduino/libraries", MessageType.Info, true);
#endif

                    EditorGUILayout.LabelField("Select Arduino libraries Folder");

                    GUILayout.BeginHorizontal();

                    GUILayout.BeginVertical();
                    destinationFolder = EditorGUILayout.TextField(destinationFolder);
                    GUILayout.EndVertical();
                    GUILayout.BeginVertical();

                    if (GUILayout.Button("Select path", GUILayout.ExpandWidth(true)))
                    {
                        GUI.FocusControl("");
                        string path = EditorUtility.OpenFolderPanel("Set Arduino path", "", "");
                        if (path.Length != 0)
                        {
                            destinationFolder = path;
                        }
                    }
                    GUILayout.EndVertical();

                    GUILayout.EndHorizontal();


                    DrawLine(12, 0, 45);
                    SetGUIBackgroundColor("#4FC3F7");

                    if (GUILayout.Button("Add Uduino library to Arduino", GUILayout.ExpandWidth(true)))
                    {
                        if (destinationFolder == "NOT SET")
                        {
                            if (EditorUtility.DisplayDialog("Move library folder", "You have to set a valid folder path", "Ok", "Cancel"))
                            { }
                        }
                        else if (EditorUtility.DisplayDialog("Move library folder", "Are you sure the Arduino libraries folder is " + destinationFolder + " ?", "Move", "Cancel"))
                        {
                            MoveArduinoFiles(destinationFolder);
                        }
                    }
                    SetGUIBackgroundColor();
                }

                EditorGUILayout.Separator();

                DrawHeaderTitle("2. Change project settings", defaultPanel, headerColor);
                CheckCompatibility();

                EditorGUILayout.Separator();


                DrawHeaderTitle("3. Start using Uduino!", defaultPanel, headerColor);

                if (firstDone && secondDone)
                {
                    DrawLine(12, 0, 45);
                    GUILayout.BeginVertical();
                    SetGUIBackgroundColor("#4FC3F7");
                    if (GUILayout.Button("I'm ready !"))
                    {
                        AddProjectDefine();
                        AssetDatabase.Refresh();
                    }

                    GUILayout.EndVertical();
                    EditorGUILayout.Separator();

                }
                else
                {
                    if (!firstDone)
                        EditorGUILayout.HelpBox("Add Uduino library into your Arduino folder", MessageType.Warning, true);
                    if (!secondDone)
                        EditorGUILayout.HelpBox("Modify the project settings", MessageType.Warning, true);

                    DrawLine(12, 0, 45);
                    GUILayout.BeginVertical();
                    GUILayout.BeginHorizontal();

                    if (GUILayout.Button("Skip setup", GUILayout.MaxWidth(90)))
                    {
                        AddProjectDefine();
                        AssetDatabase.Refresh();
                    }
                    EditorGUI.BeginDisabledGroup(true);
                    if (GUILayout.Button("Not ready yet..."))
                    {

                    }
                    EditorGUI.EndDisabledGroup();

                    GUILayout.EndHorizontal();


                    GUILayout.EndVertical();
                    EditorGUILayout.Separator();

                }

                EditorGUILayout.Separator();
            }

            void StepDone()
            {
                SetGUIBackgroundColor("#00f908");
                EditorGUILayout.HelpBox("Step done !", MessageType.Info, true);
                SetGUIBackgroundColor();
            }

            void AddProjectDefine()
            {
                //TODO : Fix if the build target is changed !

                // Get defines.
                BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
                string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);

                // Append only if not defined already.
                if (defines.Contains(define))
                {
                    Debug.LogWarning("Selected build target (" + EditorUserBuildSettings.activeBuildTarget.ToString() + ") already contains <b>" + define + "</b> <i>Scripting Define Symbol</i>.");
                    return;
                }
                // Append.
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, (defines + ";" + define));
                Debug.LogWarning("<b>" + define + "</b> added to <i>Scripting Define Symbols</i> for selected build target (" + EditorUserBuildSettings.activeBuildTarget.ToString() + ").");
            }

            void MoveArduinoFiles(string dest)
            {
                string sourceDirectory = Application.dataPath + "/Uduino/Arduino/libraries";
                string destinationDirectory = FirstToLower(dest);
                MoveDirectory(sourceDirectory, destinationDirectory);
                AssetDatabase.Refresh();
            }

            public static void MoveDirectory(string source, string target)
            {
                var stack = new Stack<Folders>();
                stack.Push(new Folders(source, target));

                while (stack.Count > 0)
                {
                    var folders = stack.Pop();
                    Directory.CreateDirectory(folders.Target);
                    foreach (var file in Directory.GetFiles(folders.Source, "*.*"))
                    {
                        string targetFile = Path.Combine(folders.Target, Path.GetFileName(file));
                        if (File.Exists(targetFile)) File.Delete(targetFile);
                        File.Move(file, targetFile);
                    }

                    foreach (var folder in Directory.GetDirectories(folders.Source))
                    {
                        stack.Push(new Folders(folder, Path.Combine(folders.Target, Path.GetFileName(folder))));
                    }
                }
                Directory.Delete(source, true);
                FileUtil.DeleteFileOrDirectory("Assets/Uduino/Arduino");
            }

            public class Folders
            {
                public string Source { get; private set; }
                public string Target { get; private set; }

                public Folders(string source, string target)
                {
                    Source = source;
                    Target = target;
                }
            }

            public string FirstToLower(string s)
            {
                if (string.IsNullOrEmpty(s))
                    return string.Empty;

                char[] a = s.ToCharArray();
                a[0] = char.ToLower(a[0]);
                return new string(a);
            }
        }
#endif
        }