using Byn.Awrtc;
using Byn.Awrtc.Unity;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MenuScript : MonoBehaviour{

    private static bool sCreated = false;
    public RectTransform _StartMenu;
    public Button _ButtonMenu;

    private void Awake()
    {
        if(sCreated)
        {
            Destroy(this.gameObject);
        }
        sCreated = true;
    }

    void Start ()
    {
        DontDestroyOnLoad(this.gameObject);
        MenuUi();
    }

    private void PrintDeviceDebug()
    {
#if !UNITY_WEBGL && !UNITY_WSA
        var testFactory = new WebRtcCSharp.RTCPeerConnectionFactoryRef();
        Debug.Log(testFactory.GetDebugDeviceInfo_Old());
        testFactory.Dispose();
#endif
    }
    
	
	// Update is called once per frame
	void Update ()
    {
	}

    public void Initialize()
    {
        //just to trigger the init process
        var res = UnityCallFactory.Instance;


    }

    void ExampleUi()
    {

        _ButtonMenu.gameObject.SetActive(true);
        _StartMenu.gameObject.SetActive(false);

    }

    void MenuUi()
    {
        _ButtonMenu.gameObject.SetActive(false);
        _StartMenu.gameObject.SetActive(true);
    }

    private void LoadExample(string s)
    {
        ExampleUi();
        UnityEngine.SceneManagement.SceneManager.LoadScene(s);
    }

    public void LoadChatExample()
    {
        LoadExample("chatscene");
    }

    public void LoadCallExample()
    {
        LoadExample("callscene");
    }

    public void LoadConference()
    {
        LoadExample("conferencescene");
    }
    public void LoadMenu()
    {
        MenuUi();
        UnityEngine.SceneManagement.SceneManager.LoadScene("menuscene");
    }
}
