using UnityEngine;
using System.Collections;

public class VideoFeed : MonoBehaviour
{
    #region Public Fields

    [HideInInspector]
    public float zoom = 39.5f;
    [HideInInspector]
    public Quaternion otherPose;

    public bool useHeadTracking = true; //used to decide whether to move the servos with the sliders or with the headtracking

    public int cameraID; //app must be reset for changes to be applied

    #endregion


    #region Private Fields

    [SerializeField]
    private bool _twoWaySwap = true;

    private MeshRenderer _meshRenderer;
    private Camera _mainCamera;

    //Camera params
    private WebCamTexture _camTex;
    private float _turningRate = 90f;
    private float _tiltAngle = 0;

    //Dim params
    private float _dimLevel = 1;
    private bool _dimmed = false;
    private float _dimRate = 0.08f;

    #endregion


    #region MonoBehaviour Methods

    private void Awake()
    {
        _mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        _meshRenderer = GetComponent<MeshRenderer>();
    }

    void Start()
    {
        InitCamera();
        recenterPose();
        otherPose = new Quaternion();
    }

    // Update is called once per frame
    void Update()
    {
        Quaternion nextOtherPose = new Quaternion();

        // Turn towards our target rotation.
        otherPose = Quaternion.RotateTowards(otherPose, nextOtherPose, _turningRate * Time.deltaTime);

        if (Input.GetKeyDown("b")) setDimmed();

        if (!_twoWaySwap) //if servo setup
        {
            transform.position = _mainCamera.transform.position + _mainCamera.transform.forward * 35; //keep webcam at a certain distance from head.
            transform.rotation = _mainCamera.transform.rotation; //keep webcam feed aligned with head
            transform.rotation *= Quaternion.Euler(0, 0, 1) * Quaternion.AngleAxis(-utilities.toEulerAngles(_mainCamera.transform.rotation).x, Vector3.forward); //compensate for absence of roll servo
            transform.rotation *= Quaternion.Euler(0, 0, _tiltAngle) * Quaternion.AngleAxis(_camTex.videoRotationAngle, Vector3.up); //to adjust for webcam physical orientation
            transform.localScale = new Vector3(0.9f, 1, -1);
        }
        else //if two way swap
        {
            transform.rotation = otherPose; //Move image according to the other person's head orientation
            transform.localScale = new Vector3(0.9f, 1, -1);
        }

        _meshRenderer.material.mainTexture = _camTex;
        setDimLevel();
    }

    void OnDestroy()
    {
        _camTex.Stop();
        PlayerPrefs.SetInt("cameraID", cameraID);
    }
    #endregion


    #region Public Methods

    public void setDimmed()
    {
        _dimmed = !_dimmed;
    }

    public void setDimmed(bool dim)
    {
        _dimmed = dim;
    }

    public void setCameraOrientation()
    {
        _tiltAngle += 90;
        PlayerPrefs.SetFloat("tiltAngle", _tiltAngle);
    }

    public void setZoom(float value)
    {
        zoom = value;
        PlayerPrefs.SetFloat("zoom", zoom);
    }

    public void recenterPose()
    {
        UnityEngine.XR.InputTracking.Recenter();
    }

    public void switchHeadtracking()
    {
        useHeadTracking = !useHeadTracking;
    }

    #endregion


    #region Private Methods

    //TODO use coroutines or LeanTween to avoid setting the dim in the Update() method
    private void setDimLevel()
    {
        float next;
        float range = 20;

        if (_dimmed) next = 1;
        else next = 0;

        _dimLevel += _dimRate * (next - _dimLevel);
        Color c = new Color(_dimLevel * range, _dimLevel * range, _dimLevel * range);
        _meshRenderer.material.SetColor("_Color", c);
    }

    //TODO allow camera to be set in runtime
    private void InitCamera()
    {
        cameraID = PlayerPrefs.GetInt("cameraID");
        WebCamDevice[] devices = WebCamTexture.devices;
        string deviceName = devices[cameraID].name;
        _camTex = new WebCamTexture(deviceName, 1920, 1080);//, 1920, 1080, FPS); //PERFORMANCE DEPENDS ON FRAMERATE AND RESOLUTION
        _camTex.Play();
    }

    #endregion
}
