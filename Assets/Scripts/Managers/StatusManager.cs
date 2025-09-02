using System.Collections;
using ScriptableObjectArchitecture;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.XR.OpenXR.Features.Extensions.PerformanceSettings;
using Debug = DebugFile;
using UnityEngine.XR.OpenXR.NativeTypes;

public class StatusManager : MonoBehaviour //TODO instructions text stuff needs not be handled here
{
    #region Public Fields
    
    public delegate void OnStopAllAudios();
    public static OnStopAllAudios StopAudiosInstructions = delegate { };

    public delegate void OnSendArduinoCommand(string command);
    public static OnSendArduinoCommand SendArduinoCommand;
    
    #endregion
    
    //using protected to make them accessible to children 
    #region Private Fields 
    [SerializeField] private BoolGameEvent _dimGameEvent;
    [SerializeField] private GameEvent _standbyGameEvent;
    [SerializeField] private BoolGameEvent _curtainOnEvent;
    
    [SerializeField] private StringGameEvent _setInstructionsTextGameEvent;
    
    #endregion

    #region Monobehaviour Methods

    private void OnEnable()
    {
        ArduinoManager.SerialFailure += SerialFailure;
        ArduinoManager.SerialReady += WaitThenStandby;
        OscManager.ReceiveSerialFailure += SerialFailure;
        OscManager.ReceiveSerialReady += Standby;
        UserStateManager.OtherLeft += WaitThenStandby;
        UserStateManager.ThisUserLeft += Standby;
    }

    private void OnDisable()
    {
        ArduinoManager.SerialFailure -= SerialFailure;
        ArduinoManager.SerialReady -= WaitThenStandby;
        OscManager.ReceiveSerialFailure -= SerialFailure;
        OscManager.ReceiveSerialReady -= Standby;
        UserStateManager.OtherLeft -= WaitThenStandby;
        UserStateManager.ThisUserLeft -= Standby;
    }

    private void Start()
    {
        _setInstructionsTextGameEvent.Raise("waitForSerial"); 
        XrPerformanceSettingsFeature.SetPerformanceLevelHint(PerformanceDomain.Cpu, PerformanceLevelHint.Boost);
    }

    #endregion
    
    #region Public Methods 
    
    public void StartExperience() //called by timeline TODO use signals instead!
    {
        _dimGameEvent.Raise(false);
        Debug.Log("experience started", DLogType.Logic);
    }
    
    public void MirrorOn() //called by sequencer / timeline
    {
        SendArduinoCommand("mir_on"); 
        Debug.Log("mirrors on", DLogType.Logic);
    }

    public void CloseWall() //called by sequencer / timeline
    {
        Debug.Log("wall on", DLogType.Logic);        
        _curtainOnEvent.Raise(true);
    }
    
    public void WallOff() //called by sequencer / timeline 
    {
        _curtainOnEvent.Raise(false);
        Debug.Log("wall off", DLogType.Logic);
    }

    public void MirrorOff()
    {
        SendArduinoCommand("mir_off"); //hide mirror
        Debug.Log("mirror off", DLogType.Logic);

    }
    
    public void EndExperience() //called at the the end of the experience TODO rename to EndExperience
    {
        _dimGameEvent.Raise(true);
        Debug.Log("experienced finished", DLogType.Logic);

    }
    
    #endregion
    
    #region Private Methods   

    
    private void Standby()
    {
        Debug.Log("Standby");
        _setInstructionsTextGameEvent.Raise("idle");
        StopAudiosInstructions();
        _dimGameEvent.Raise(true);
        _standbyGameEvent.Raise();
        _curtainOnEvent.Raise(false);
        SendArduinoCommand("mir_off"); 
    }

    private void WaitThenStandby()
    {
        StartCoroutine(WaitBeforeResetting());
    }
    
    private IEnumerator WaitBeforeResetting() //when other user left midexperience, wait to show a notificartion before resetting
    {
        Debug.Log("about to reset", DLogType.Logic);
        yield return new WaitForSeconds(4f); //make sure this value is inferior or equal to the confirmation radial time to avoid bugs
        Standby(); //if we were ready and we took off the headset go to initial state
    }

    private void SerialFailure() //if something went wrong with the physical installation
    {
        _dimGameEvent.Raise(true);
        StopAudiosInstructions(); 
        _setInstructionsTextGameEvent.Raise("systemFailure");
        Destroy(gameObject); //TODO should destroy a bunch more stuff to make sure experience ends ?
        Debug.Log("serial failure", DLogType.Error);
    }

    #endregion
    
}
