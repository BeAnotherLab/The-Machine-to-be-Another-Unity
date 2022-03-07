using System.Collections;
using System.Collections.Generic;
using ScriptableObjectArchitecture;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.XR;
using UnityEngine.UI;
using VRStandardAssets.Menu;
using VRStandardAssets.Utils;
using Uduino;
using UnityEngine.Serialization;
using UnityEngine.Timeline;
using Debug = DebugFile;


public class StatusManager : MonoBehaviour {

    #region Public Fields

    public static StatusManager instance;

    public bool presenceDetection; //TODD check if still necessary
    
    public UserStateVariable previousOtherState;
    public UserStateVariable otherState;
    
    public UserStateVariable previousSelfState;
    public UserStateVariable selfState;
    
    public UserStateGameEvent selfStateGameEvent;
    public UserStateGameEvent otherStateGameEvent;
    
    public PlayableDirector instructionsTimeline;
    
    #endregion


    #region Private Fields
    
    [SerializeField] private PlayableDirector _shortTimeline;
    [SerializeField] private PlayableDirector _longTimeline;
    [SerializeField] private GameObject _languageButtons;

    [SerializeField] private GameEvent _standbyGameEvent;
    [SerializeField] private GameEvent _InstructionsStartedGameEvent;
    [SerializeField] private BoolGameEvent _experienceFinishedGameEvent;

    [SerializeField] private StringGameEvent _setInstructionsTextGameEvent;
    [SerializeField] private QuestionnaireStateVariable _questionnaireState;

    [SerializeField] private TrackAsset _germanTrack;
    [SerializeField] private TrackAsset _englishTrack;
    
    private GameObject _mainCamera;
    private bool _readyForStandby; //when we use serial, only go to standby if Arduino is ready.
    private GameObject _confirmationMenu;
    private bool _experienceRunning;
    private bool _dimOutOnExperienceStart;
    private bool _showQuestionnaireThisRound;
    
    #endregion


    #region MonoBehaviour Methods

    private void Awake()
    {
        if (instance == null) instance = this;

        _mainCamera = GameObject.Find("Main Camera");
        _confirmationMenu = GameObject.Find("ConfirmationMenu");
        UduinoManager.Instance.OnBoardDisconnectedEvent.AddListener(delegate { SerialFailure(); });
        instructionsTimeline = _longTimeline; //use short experience by default
    }
    
    private void Start()
    {
        if (SwapModeManager.instance.ArduinoControl)
            _setInstructionsTextGameEvent.Raise("Waiting for serial...");
        else
            _readyForStandby = true; //if we're not using the serial control, we don't have to wait for the arduino

        selfState.Value = UserState.headsetOff;
        otherState.Value = UserState.headsetOff;
    }

    private void Update()
    {
        if (XRDevice.userPresence == UserPresenceState.NotPresent && selfState.Value != UserState.headsetOff)
        {
            previousSelfState.Value = selfState.Value;
            selfState.Value = UserState.headsetOff; //SelfRemovedHeadset();
            selfStateGameEvent.Raise(UserState.headsetOff);
        }
        else if (XRDevice.userPresence == UserPresenceState.Present && selfState.Value == UserState.headsetOff) //if we just put the headset on
        {
            previousSelfState.Value = selfState.Value;
            selfState.Value = UserState.headsetOn;
            selfStateGameEvent.Raise(UserState.headsetOn);
        }
            
        if (Input.GetKeyDown("o")) IsOver();
    }

    #endregion


    #region Public Methods

    public void SwitchLanguage(string language)
    {
        //StatusManager.instance.SwitchLanguageTrack(fileName);
    }
    
    public void StartExperience() //TODO remove?
    {
        InstructionsTextBehavior.instance.ShowInstructionText(false);
        if (_dimOutOnExperienceStart) VideoFeed.instance.Dim(false);
        else instructionsTimeline.Stop();
        Debug.Log("experience started");
    }

    public void MirrorOn()
    {
        //ArduinoManager.instance.SendCommand("mir_on");
        Debug.Log("mirrors on");
    }

    public void CloseWall()
    {
        ArduinoManager.instance.WallOn(true);
        //ArduinoManager.instance.SendCommand("mir_off"); //hide mirror
        Debug.Log("wall on");        
    }
    
    public void WallOn() //TODO rename
    {
        OpenCurtainCanvasController.instance.Show("Open Curtain");
        ArduinoManager.instance.WallOn(false);
        //ArduinoManager.instance.SendCommand("mir_off"); //hide mirror
        Debug.Log("wall off");
    }

    public void ThisUserIsReady() //called when user has aimed at the confirmation dialog and waited through the countdown.
    {
        OscManager.instance.SendThisUserStatus(UserState.readyToStart);

        //EnableConfirmationGUI(false); //hide status confirmation GUI elements
        _languageButtons.gameObject.SetActive(false); //hide language buttons;

        //start experience or wait for the other if they're not ready yet
        //if (otherState.Value == UserState.readyToStart) StartPlaying();
        //InstructionsTextBehavior.instance.ShowTextFromKey("waitForOther");

        InstructionsTextBehavior.instance.ShowInstructionText(false);
        
        Debug.Log("this user is ready", DLogType.Input);
    }

    public void OtherUserIsReady()
    {
        //if (selfState.Value == UserState.readyToStart) StartPlaying();
        Debug.Log("the other user is ready", DLogType.Input);
    }

    public void SelfPutHeadsetOn()
    {
        _setInstructionsTextGameEvent.Raise("Move the headset up and down until you can see this text clearly. \n When you are ready, look at the button below to begin.");
        OscManager.instance.SendThisUserStatus(UserState.headsetOn);
        Debug.Log("this user put on the headset", DLogType.Input);
    }

    public void OtherPutHeadsetOn()
    {
        Debug.Log("the other user put on the headset", DLogType.Input);
    }
    
    public void OtherLeft()
    {
        //if experience started
        if (previousOtherState.Value == UserState.readyToStart)
        {
            //only reset on other left if experience running, post finished, or doing pre questionnaire
            if (_experienceRunning || _questionnaireState.Value != QuestionnaireState.post) 
            {
                instructionsTimeline.Stop();
                _experienceRunning = false;    
                StartCoroutine(WaitBeforeResetting()); //after a few seconds, reset experience.
                selfState.Value = UserState.headsetOn;    
            }
        }
        Debug.Log("the other user removed the headset", DLogType.Input);
    }

    public void Standby(bool start = false, bool dimOutOnExperienceStart = true)
    {
        if (!start) VideoFeed.instance.Dim(true); //TODO somehow this messes with Video Feed dimming when called on Start?
            _setInstructionsTextGameEvent.Raise("Move the headset up and down until you can see this text clearly. \n When you are ready, look at the button below to begin.");

        instructionsTimeline.Stop();
        _experienceRunning = false;
        _showQuestionnaireThisRound = false;
        
        AudioManager.instance.StopAudioInstructions();

        InstructionsTextBehavior.instance.gameObject.GetComponent<FadeController>().FadeInText();
        InstructionsTextBehavior.instance.gameObject.GetComponent<FadeController>().FadeOutImages();
        
        //reset user status as it is not ready
        //EnableConfirmationGUI(true);
        _languageButtons.gameObject.SetActive(true); //show language buttons;

        if (_readyForStandby) //TODO is check necessary? 
            ArduinoManager.instance.InitialPositions();
        
        Debug.Log("ready to start");
        
        VideoFeed.instance.Dim(true);

        _dimOutOnExperienceStart = dimOutOnExperienceStart;
        Debug.Log("setting dimOutOnExperienceStat to " + _dimOutOnExperienceStart);
        
        _standbyGameEvent.Raise();
    }
    
    public void EnablePresenceDetection(bool enablePresenceDetection)
    {
        
    }
    
    public void SerialFailure() //if something went wrong with the physical installation
    {
        VideoFeed.instance.Dim(true);
        OscManager.instance.SendSerialStatus(false);
        AudioManager.instance.StopAudioInstructions();    
        _setInstructionsTextGameEvent.Raise("oops! There was an error with the system. Please come back later.");
        instructionsTimeline.Stop();
        _experienceRunning = false;
        Destroy(gameObject);
        Debug.Log("serial failure", DLogType.Error);
    }

    public void SerialReady(bool serialControlComputer = false)
    {
        if (serialControlComputer) //if this computer is the one connected to the Arduino board
        {
            ArduinoManager.instance.InitialPositions();
        }
        
        _setInstructionsTextGameEvent.Raise("Move the headset up and down until you can see this text clearly. \n When you are ready, look at the button below to begin.");
        _readyForStandby = true;
        Debug.Log("serial ready", DLogType.System);
    }    

    public void SelfRemovedHeadset()
    {
        _confirmationMenu.GetComponent<VRInteractiveItem>().Out(); //notify the VR interactive element that we are not hovering any more
        if (previousSelfState.Value == UserState.readyToStart 
            && _questionnaireState.Value != QuestionnaireState.post) { //reset unless doing post questionnaire
            Standby(false, _dimOutOnExperienceStart); //if we were ready and we took off the headset go to initial state
        }
        
        OscManager.instance.SendThisUserStatus(selfState);
        Debug.Log("this user removed his headset", DLogType.Input);
    }
    
    public void SetInstructionsTimeline(int index)
    {
        if (index == 0)
            instructionsTimeline = _shortTimeline;
        else if (index == 1)
            instructionsTimeline = _longTimeline;
    }

    public void SelfStateChanged(UserState newState)
    {
        if (newState == UserState.headsetOff) SelfRemovedHeadset();
        else if (newState == UserState.headsetOn) SelfPutHeadsetOn();
        else if (newState == UserState.readyToStart) ThisUserIsReady();
    }

    public void OtherStateChanged(UserState newState)
    {
        if (newState == UserState.headsetOff) OtherLeft();
        else if (newState == UserState.headsetOn) OtherPutHeadsetOn(); //TODO only if previous one was ready to start?
        else if (newState == UserState.readyToStart) OtherUserIsReady();
    }
    
    public void OnBothConsentsGiven(bool bothGiven)
    {
        _showQuestionnaireThisRound = bothGiven;
        if(!bothGiven) StartPlaying();
    }

    public void OnBothQuestionnaireFinished(QuestionnaireState state)
    {
        if (state == QuestionnaireState.pre) StartPlaying();
        else if (state == QuestionnaireState.post) StartCoroutine(WaitBeforeResetting()); 
    }
    
    public void SwitchLanguageTrack(string fileName)
    {
        TimelineAsset timelineAsset = (TimelineAsset) instructionsTimeline.playableAsset;
        _englishTrack = timelineAsset.GetOutputTrack(0);
        _germanTrack = timelineAsset.GetOutputTrack(1);
        _englishTrack.muted = fileName != "lng_en.json";
        _germanTrack.muted = fileName != "lng_de.json";
    }

    
    #endregion

    //pre questionnarie is cancelled if either self or other removes headset
    //Let self post questionnaire finish

    #region Private Methods

    /*private void EnableConfirmationGUI(bool enable)
    {
        if (enable)
            _mainCamera.GetComponent<Reticle>().Show();
        else
        {
            _mainCamera.GetComponent<Reticle>().Hide();
            _mainCamera.GetComponent<CustomSelectionRadial>().Hide();
        }
    }*/

    private void StartPlaying()
    {
        if (_readyForStandby)
        {
            OpenCurtainCanvasController.instance.Show("Close Curtain");
            instructionsTimeline.Play();
            _InstructionsStartedGameEvent.Raise();
            _experienceRunning = true;
        }
    }

    private void IsOver() //called at the the end of the experience
    {
        VideoFeed.instance.Dim(true);
        //InstructionsTextBehavior.instance.ShowTextFromKey("finished");
        instructionsTimeline.Stop();
		Debug.Log("experience finished");
        _experienceRunning = false;
        _experienceFinishedGameEvent.Raise(_showQuestionnaireThisRound);
    }

    private IEnumerator WaitBeforeResetting()
    {
        yield return new WaitForSeconds(4f); //make sure this value is inferior or equal to the confirmation radial time to avoid bugs
        Standby(false, _dimOutOnExperienceStart); //if we were ready and we took off the headset go to initial state
        SelfPutHeadsetOn();
        Debug.Log("about to reset");
    }

    #endregion
}
 