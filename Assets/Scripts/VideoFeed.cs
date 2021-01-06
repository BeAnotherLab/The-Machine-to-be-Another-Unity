using System.Collections;
using UnityEngine;
using RockVR.Video;
using UnityEngine.Serialization;

public class VideoFeed : MonoBehaviour //TODO turn to manager
{
    #region Public Fields

    public static VideoFeed instance;

    public float zoom;

    [HideInInspector]
    public Quaternion otherPose;

    public bool useHeadTracking = true; //used to decide whether to move the servos with the sliders or with the headtracking

    public int cameraID; //app must be reset for changes to be applied. first camera is for swap, second is for cognitive task

    public bool twoWayWap;

    public bool dimOnStart;

    public Transform targetTransform;
    
    #endregion


    #region Private Fields

    [SerializeField] private bool _loadTiltFromPlayerPrefs = true;

    [SerializeField] private MeshRenderer _videoPlaybackMeshRenderer;

    [SerializeField] private bool _editing;
    
    private Camera _mainCamera;

    //Camera params
    private float _turningRate = 90f;
    [SerializeField]
    private float _tiltAngle;

    //Dim params
    private bool _dimmed;

    private MeshRenderer _meshRenderer;

    private SubjectDirection _currentDirection;
    
    #endregion


    #region MonoBehaviour Methods

    private void Awake()
    {
        if (instance == null) instance = this;

        _mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
    }

    void Start()
    {
        if(_loadTiltFromPlayerPrefs) _tiltAngle = PlayerPrefs.GetFloat("tiltAngle");
        if (dimOnStart) StartCoroutine(StartupDim());
        otherPose = new Quaternion();
    }

    // Update is called once per frame
    void Update()    
    {
        Quaternion nextOtherPose = new Quaternion();

        // Turn towards our target rotation.
        otherPose = Quaternion.RotateTowards(otherPose, nextOtherPose, _turningRate * Time.deltaTime);

        if (Input.GetKeyDown("b") && !_editing ) ToggleDim();
        if (Input.GetKeyDown("n") && !_editing ) RecenterPose();
        if (Input.GetKeyDown("r") && !_editing ) Rotate();

        if (targetTransform != null)
        {
            if (!twoWayWap) //if servo setup
            {
                targetTransform.position = _mainCamera.transform.position + _mainCamera.transform.forward * 35; //keep webcam at a certain distance from head.
                targetTransform.rotation = _mainCamera.transform.rotation; //keep webcam feed aligned with head
                targetTransform.rotation *= Quaternion.Euler(0, 0, 1) * Quaternion.AngleAxis(-utilities.toEulerAngles(_mainCamera.transform.rotation).x, Vector3.forward); //compensate for absence of roll servo
                targetTransform.rotation *= Quaternion.Euler(0, 0, _tiltAngle) * Quaternion.AngleAxis(0, Vector3.up); //to adjust for webcam physical orientation
                targetTransform.localScale = new Vector3(0.9f, 1, -1);
            }
        }
    }

    void OnDestroy()
    {
        PlayerPrefs.SetInt("cameraID", cameraID);
    }
    #endregion


    #region Public Methods

    public void IsEditingText(bool editing)
    {    
        _editing = editing;
    }

    public void SetDirection(SubjectDirection direction)
    {
        _currentDirection = direction;
    }
    
    public void MatchDirection(char desiredDirection)
    {
        if (desiredDirection == 'R' && _currentDirection == SubjectDirection.left)
        {
            FlipHorizontal();
            _currentDirection = SubjectDirection.right;
        } else if (desiredDirection == 'L' && _currentDirection == SubjectDirection.right)
        {
            FlipHorizontal();
            _currentDirection = SubjectDirection.left;
        }
    }
    
    public void CancelTweens()
    {
        LeanTween.cancelAll();
    }
    
    public void FlipHorizontal()
    {
        //TODO This was broken by using network transform
        transform.parent.localScale = new Vector3(- transform.parent.localScale.x, transform.parent.localScale.y, transform.parent.localScale.z);
    }

    public void Dim(bool dim, bool fade = true)
    {
        if (targetTransform != null)
        {
            float next = 1;
            if (dim) next = 0;

            float dimValue = targetTransform.GetComponent<MeshRenderer>().material.color.a;

            float time = 0f;
            if (fade) time = 1;
            
            LeanTween.value(dimValue, next, time).setEaseInOutQuad().setOnUpdate((val) => {
                if (targetTransform != null)
                {
                    Color c = targetTransform.GetComponent<MeshRenderer>().material.color;
                    c.a = val;
                    targetTransform.GetComponent<MeshRenderer>().material.SetColor("_Color", c);                    
                }
            });    
        }
    }

    public void ToggleDim()
    {
        _dimmed = !_dimmed;
        Dim(_dimmed);
    }

    public void Rotate()
    {
        _tiltAngle += 90;
        if(twoWayWap) targetTransform.GetChild(0).transform.rotation = Quaternion.Euler(0,0, _tiltAngle);
        PlayerPrefs.SetFloat("tiltAngle", _tiltAngle);
    }

    public void SetZoom(float value)
    {
        zoom = value;
        PlayerPrefs.SetFloat("zoom", zoom);
    }

    public void RecenterPose()
    {
        UnityEngine.XR.InputTracking.Recenter();
        //The following will also move the camera positional reference.
        //taken from https://forum.unity.com/threads/openvr-how-to-reset-camera-properly.417509/#post-2792972
        Valve.VR.OpenVR.System.ResetSeatedZeroPose();
        Valve.VR.OpenVR.Compositor.SetTrackingSpace(Valve.VR.ETrackingUniverseOrigin.TrackingUniverseSeated);
    }

    public void SwitchHeadtracking() //Use to map the pitch and yaw sliders to headtracking or not
    {
        useHeadTracking = !useHeadTracking;
    }

    public void ShowLiveFeed(bool show)
    {
        if (targetTransform != null) targetTransform.GetComponent<MeshRenderer>().enabled = show;
        _videoPlaybackMeshRenderer.enabled = !show;
    }
    
    public IEnumerator StartupDim()
    {
        yield return new WaitForSeconds(2);
        Dim(true);
    }
    
    #endregion
}
