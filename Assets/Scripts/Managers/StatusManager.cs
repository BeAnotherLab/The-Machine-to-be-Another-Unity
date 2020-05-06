using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.UI;
using VRStandardAssets.Menu;
using VRStandardAssets.Utils;

public enum UserStatus { headsetOff, headsetOn, readyToStart }

public class StatusManager : MonoBehaviour {

    #region Public Fields

    public static StatusManager instance;

    public bool statusManagementOn;

    public UserStatus selfStatus;
    public UserStatus otherStatus;
    
    public List<List<float>> timingList = new List<List<float>>();
    
    #endregion


    #region Private Fields

    [SerializeField] private bool _autoStartAndFinishOn;
    [SerializeField] private bool _serialReady;
    
    [Tooltip("Instructions Timing")] private GameObject _instructionsGUI;

    [SerializeField]
    private float waitBeforeInstructions; 
    
    [Tooltip("Set configuration of event timing (1-4)")] [SerializeField] private int _timingConfiguration;
    private List<float> _selectedTimingList = new List<float>();
    //TODO make waitafterInstructions match the duration of the introduction audio

    private GameObject _mainCamera;

    private GameObject _confirmationMenu;

    [Tooltip("0 = instructions end, 1 = mirror, 2 = wall, 3 = goodbye")] [SerializeField] private List<float> timeList1 = new List<float>();
    [Tooltip("0 = instructions end, 1 = mirror, 2 = wall, 3 = goodbye")] [SerializeField] private List<float> timeList2 = new List<float>();
    [Tooltip("0 = instructions end, 1 = mirror, 2 = wall, 3 = goodbye")] [SerializeField] private List<float> timeList3 = new List<float>();
    [Tooltip("0 = instructions end, 1 = mirror, 2 = wall, 3 = goodbye")] [SerializeField] private List<float> timeList4 = new List<float>();
    
    #endregion


    #region MonoBehaviour Methods

    private void Awake()
    {
        if (instance == null) instance = this;

        _mainCamera = GameObject.Find("Main Camera");

        _confirmationMenu = GameObject.Find("ConfirmationMenu");

        timingList.Add(timeList1);
        timingList.Add(timeList2);
        timingList.Add(timeList3);
        timingList.Add(timeList4);
    }

    private void Start()
    {
        InstructionsTextBehavior.instance.ShowTextFromKey("waitingForSerial");
    }

    private void Update()
    {
        if ( statusManagementOn ) //status management is for both autonomous and manual swap
        {
            if (XRDevice.userPresence == UserPresenceState.NotPresent && selfStatus != UserStatus.headsetOff) 
                SelfRemovedHeadset();
            else if (XRDevice.userPresence == UserPresenceState.Present && selfStatus == UserStatus.headsetOff) //if we just put the headset on 
                SelfPutHeadsetOn(); 
            
            if (Input.GetKeyDown("o")) IsOver();
        } 
    }

    #endregion


    #region Public Methods

    public void SetAutoStartAndFinish(bool on, float waitTime = 5)
    {
        _autoStartAndFinishOn = on;
        waitBeforeInstructions = waitTime;
    }

    public void ThisUserIsReady() //called when user has aimed at the confirmation dialog and waited through the countdown.
    {
        if (statusManagementOn) OscManager.instance.SendThisUserStatus(UserStatus.readyToStart);

        EnableConfirmationGUI(false); //hide status confirmation GUI elements

        //start experience or wait for the other if they're not ready yet
        if (otherStatus == UserStatus.readyToStart) StartPlaying();
        else InstructionsTextBehavior.instance.ShowTextFromKey("waitForOther");

        selfStatus = UserStatus.readyToStart;
    }

    public void OtherUserIsReady()
    {
        otherStatus = UserStatus.readyToStart;
        if (selfStatus == UserStatus.readyToStart) StartPlaying();
    }

    public void SelfPutHeadsetOn()
    {
        selfStatus = UserStatus.headsetOn;
        OscManager.instance.SendThisUserStatus(UserStatus.headsetOn);
        if (otherStatus == UserStatus.headsetOn) HeadsetsOn();    
    }

    public void OtherPutHeadsetOn()
    {
        otherStatus = UserStatus.headsetOn;
        if (selfStatus == UserStatus.headsetOn) HeadsetsOn();
    }
    
    public void OtherLeft()
    {
        otherStatus = UserStatus.headsetOff;
        if (selfStatus == UserStatus.readyToStart)
        {
            //different than self is gone in case there is an audio for this case
            VideoFeed.instance.SetDimmed(true);

            InstructionsTextBehavior.instance.ShowTextFromKey("otherIsGone");

            StopAllCoroutines();

            StartCoroutine(WaitBeforeResetting()); //after a few seconds, reset experience.
        }
        else
        {
            InstructionsDisplay.instance.ShowWelcomeVideo();
        }
    }

    public void StopExperience(bool start = false)
    {
        if (!start) VideoFeed.instance.SetDimmed(true); //TODO somehow this messses with Video Feed dimming when called on Start?
        InstructionsTextBehavior.instance.ShowTextFromKey("idle");

        StopAllCoroutines();

        AudioPlayer.instance.StopAudioInstructions();

        //reset user status as it is not ready
        EnableConfirmationGUI(true);

        if (_serialReady) //TODO is check necessary? 
            ArduinoManager.instance.InitialPositions();

        InstructionsDisplay.instance.ShowWelcomeVideo();
    }

    public void DisableStatusManagement()
    {
        VideoFeed.instance.SetDimmed(true);
        InstructionsTextBehavior.instance.ShowInstructionText(false);

        StopAllCoroutines();

        statusManagementOn = false;
    }

    public void SerialFailure() //if something went wrong with the physical installation
    {
        StopAllCoroutines();
        
        VideoFeed.instance.SetDimmed(true);
        OscManager.instance.SendSerialStatus(false);
        AudioPlayer.instance.StopAudioInstructions();    
        InstructionsDisplay.instance.ShowTechnicalFailureMessage();
        Destroy(gameObject);
    }

    public void SerialReady()
    {
        OscManager.instance.SendSerialStatus(true);
        _instructionsGUI.SetActive(true);
        InstructionsTextBehavior.instance.ShowTextFromKey("idle");
        ArduinoManager.instance.InitialPositions();
        _serialReady = true;
    }

    public void SelfRemovedHeadset()
    {
        _confirmationMenu.GetComponent<VRInteractiveItem>().Out(); //notify the VR interactive element that we are not hovering any more
        if (selfStatus == UserStatus.readyToStart) StopExperience(); //if we were ready and we took off the headset
        if (selfStatus == UserStatus.headsetOn) InstructionsDisplay.instance.ShowWelcomeVideo(); //if we just had headset on
        selfStatus = UserStatus.headsetOff;
        OscManager.instance.SendThisUserStatus(selfStatus);
    }
    
    #endregion


    #region Private Methods
    
    private void HeadsetsOn()
    {
        InstructionsDisplay.instance.ShowWaitForTurnVideo();
    }
    
    private IEnumerator StartPlayingCoroutine()
    {      
        if (_autoStartAndFinishOn) //if we are in auto swap
        {
            StartCoroutine("GoodbyeCoroutine");
            AudioPlayer.instance.PlayAudioInstructions(_timingConfiguration);
        }

        StartCoroutine("MirrorCoroutine");
        StartCoroutine("WallCoroutine");

        yield return new WaitForFixedTime(waitBeforeInstructions);// wait before playing audio
        yield return new WaitForFixedTime(_selectedTimingList[0]);//duration of audio track to start video after
        
        if (_autoStartAndFinishOn) //if we are in auto swap
        {
            ArduinoManager.instance.SendCommand("wal_on"); //close curtain
            ArduinoManager.instance.SendCommand("mir_off"); //hide mirror
        }

        if (_autoStartAndFinishOn) VideoFeed.instance.SetDimmed(false);
        _instructionsGUI.SetActive(false);
    }

    private IEnumerator GoodbyeCoroutine()
    {
        yield return new WaitForFixedTime(_selectedTimingList[3] + waitBeforeInstructions);
        Debug.Log("READY TO STOP");
        IsOver();
    }

    private IEnumerator MirrorCoroutine()
    {
        yield return new WaitForFixedTime(waitBeforeInstructions + _selectedTimingList[1]);
        Debug.Log("READY FOR MIRROR");
        ArduinoManager.instance.SendCommand("mir_on"); //show mirror
    }

    private IEnumerator WallCoroutine()
    {
        yield return new WaitForFixedTime(waitBeforeInstructions + _selectedTimingList[2]);
        Debug.Log("READY FOR WALL");
        ArduinoManager.instance.SendCommand("wal_off"); //open curtain
        ArduinoManager.instance.SendCommand("mir_off"); //hide mirror
    }
    
    private void EnableConfirmationGUI(bool enable)
    {
        ConfirmationButton.instance.gameObject.SetActive(enable);

        if (enable)
            _mainCamera.GetComponent<Reticle>().Show();
        else
        {
            _mainCamera.GetComponent<Reticle>().Hide();
            _mainCamera.GetComponent<CustomSelectionRadial>().Hide();
        }
    }

    private void StartPlaying()
    {
        if (_serialReady)
        {
            _instructionsGUI.SetActive(true);
            InstructionsTextBehavior.instance.ShowTextFromKey("instructions");
            _selectedTimingList.Clear();
            foreach (int item in timingList[_timingConfiguration]) _selectedTimingList.Add(item);
            StartCoroutine("StartPlayingCoroutine");    
        }
    }

    private void IsOver() //called at the the end of the experience
    {
        VideoFeed.instance.SetDimmed(true);
        InstructionsTextBehavior.instance.ShowTextFromKey("finished");
        StopAllCoroutines();
    }

    private IEnumerator WaitBeforeResetting()
    {
        yield return new WaitForSeconds(4f); //make sure this value is inferior or equal to the confirmation radial time to avoid bugs
        StopExperience();
    }

    #endregion
}
 