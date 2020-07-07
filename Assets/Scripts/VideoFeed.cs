using System.Collections;
using UnityEngine;
using RockVR.Video;

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

    [SerializeField] private MeshRenderer _videoPlaybackMeshRenderer;
    
    #endregion


    #region Private Fields
   
    private Camera _mainCamera;

    //Camera params
    private float _turningRate = 90f;
    private float _tiltAngle;

    //Dim params
    private bool _dimmed;

    private MeshRenderer _meshRenderer;

    private string _currentDirection;
    
    #endregion


    #region MonoBehaviour Methods

    private void Awake()
    {
        if (instance == null) instance = this;

        _mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        _meshRenderer = GetComponent<MeshRenderer>();
    }

    void Start()
    {
        _tiltAngle = PlayerPrefs.GetFloat("tiltAngle");
        RecenterPose();
        if (dimOnStart) StartCoroutine(StartupDim());
        otherPose = new Quaternion();
    }

    // Update is called once per frame
    void Update()    
    {
        Quaternion nextOtherPose = new Quaternion();

        // Turn towards our target rotation.
        otherPose = Quaternion.RotateTowards(otherPose, nextOtherPose, _turningRate * Time.deltaTime);

        if (Input.GetKeyDown("b")) SetDimmed();
        if (Input.GetKeyDown("n")) RecenterPose();
        if (Input.GetKeyDown("r")) Rotate();

        if (!twoWayWap) //if servo setup
        {
            transform.position = _mainCamera.transform.position + _mainCamera.transform.forward * 35; //keep webcam at a certain distance from head.
            transform.rotation = _mainCamera.transform.rotation; //keep webcam feed aligned with head
            transform.rotation *= Quaternion.Euler(0, 0, 1) * Quaternion.AngleAxis(-utilities.toEulerAngles(_mainCamera.transform.rotation).x, Vector3.forward); //compensate for absence of roll servo
            transform.rotation *= Quaternion.Euler(0, 0, _tiltAngle) * Quaternion.AngleAxis(0, Vector3.up); //to adjust for webcam physical orientation
            transform.localScale = new Vector3(0.9f, 1, -1);
        }
        else //if two way swap
        {
            transform.rotation = otherPose; //Move image according to the other person's head orientation
            transform.localScale = new Vector3(0.9f, 1, -1);
            transform.rotation *= Quaternion.Euler(0, 0, _tiltAngle) * Quaternion.AngleAxis(0, Vector3.up); //to adjust for webcam physical orientation
        }
    }

    void OnDestroy()
    {
        PlayerPrefs.SetInt("cameraID", cameraID);
    }
    #endregion


    #region Public Methods

    public void SetDirection(string direction)
    {
        _currentDirection = direction;
    }
    
    public void MatchDirection(char desiredDirection)
    {
        if (desiredDirection == 'R' && _currentDirection == "Left")
        {
            FlipHorizontal();
            _currentDirection = "Right";
        } else if (desiredDirection == 'L' && _currentDirection == "Right")
        {
            FlipHorizontal();
            _currentDirection = "Left";
        }
    }
    
    public void CancelTweens()
    {
        LeanTween.cancelAll();
    }
    
    public void FlipHorizontal()
    {
        transform.parent.localScale = new Vector3(- transform.parent.localScale.x, transform.parent.localScale.y, transform.parent.localScale.z);
    }

    public void SetDimmed(bool dim)
    {
        float next = 1;
        if (dim) next = 0;

        float dimValue = _meshRenderer.material.color.a;

        LeanTween.value(dimValue, next, 1).setEaseInOutQuad().setOnUpdate((val) => {
            Color c = _meshRenderer.material.color;
            c.a = val;
            _meshRenderer.material.SetColor("_Color", c);
        });
    }

    public void SetDimmed()
    {
        _dimmed = !_dimmed;
        SetDimmed(_dimmed);
    }

    public void Rotate()
    {
        _tiltAngle += 90;
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
    }

    public void SwitchHeadtracking() //Use to map the pitch and yaw sliders to headtracking or not
    {
        useHeadTracking = !useHeadTracking;
    }

    public void ShowLiveFeed(bool show)
    {
        GetComponent<MeshRenderer>().enabled = show;
        _videoPlaybackMeshRenderer.enabled = !show;
    }
    
    #endregion

    #region Private Methods

    private IEnumerator StartupDim()
    {
        yield return new WaitForSeconds(2);
        SetDimmed(true);
    }

    #endregion
    
}
