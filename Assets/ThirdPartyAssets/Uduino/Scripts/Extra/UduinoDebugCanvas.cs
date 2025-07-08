using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using SimpleJSON;

namespace Uduino
{
    public class UduinoDebugCanvas : MonoBehaviour
    {
        string uduinoLogContent = "";
        Queue uduinoLogQueue = new Queue();
        Text uduinoLogText = null;
        public static UduinoDebugCanvas Instance;

        void Awake()
        {

            string t = "{ \"devices\":[{\"uuid\":\"F0:07:2C:E5:EC:70\",\"name\":\"BlendMicro\"}]}";

            var parsedData = JSON.Parse(t);
            for (int i = 0; i < parsedData["devices"].Count; i++)
            {
                Debug.Log(parsedData["devices"][i]["name"].ToString());
            }

            Instance = this;
            CreateCanvasAndText();
            Application.logMessageReceived += HandleLog;
        }

        public void Log(string m)
        {
            uduinoLogText.text += "\n" + m;
        }

        void OnEnable()
        {
        }

        void OnDisable()
        {
            Application.logMessageReceived -= HandleLog;
        }

        public void Clear()
        {
            uduinoLogContent = "";
            uduinoLogQueue.Clear();
        }

        void HandleLog(string logString, string stackTrace, LogType type)
        {
            uduinoLogContent = logString;
            string newString = "\n [" + type + "] : " + uduinoLogContent;
            uduinoLogQueue.Enqueue(newString);
            if (type == LogType.Exception)
            {
                newString = "\n" + stackTrace;
                uduinoLogQueue.Enqueue(newString);
            }
            uduinoLogContent = string.Empty;
            foreach (string mylog in uduinoLogQueue)
            {
                uduinoLogContent += mylog;
            }
        }

        void Update()
        {
            if (uduinoLogText != null)
            {
                uduinoLogText.text = uduinoLogContent;
                if (uduinoLogContent.Split('\n').Length - 1 > Screen.height / 20) // Clear queue
                  Clear();
            }
        }

        void CreateCanvasAndText()
        {
            this.transform.name = "UduinoDebugCanvas";
            Canvas canvas = this.gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler cs = this.gameObject.AddComponent<CanvasScaler>();
            cs.scaleFactor = 1.0f;
            cs.dynamicPixelsPerUnit = 10f;
            canvas.sortingOrder = 500;

            GameObject textDebug = new GameObject();
            textDebug.name = "UduinoDebugText";
            textDebug.transform.parent = this.transform;
            uduinoLogText = textDebug.AddComponent<Text>();
            uduinoLogText.alignment = TextAnchor.UpperLeft;
            uduinoLogText.horizontalOverflow = HorizontalWrapMode.Wrap;
            uduinoLogText.verticalOverflow = VerticalWrapMode.Overflow;
            Font ArialFont = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
            uduinoLogText.font = ArialFont;
            uduinoLogText.fontSize = 20;
            uduinoLogText.enabled = true;
            uduinoLogText.color = Color.black;

            textDebug.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width,Screen.height);
            textDebug.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
            textDebug.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
            textDebug.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
            textDebug.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);

            this.gameObject.transform.localScale = new Vector3(
                                                 1.0f / this.transform.localScale.x * 0.1f,
                                                 1.0f / this.transform.localScale.y * 0.1f,
                                                 1.0f / this.transform.localScale.z * 0.1f);
        }


    }
}