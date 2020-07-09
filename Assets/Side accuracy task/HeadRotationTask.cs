using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadRotationTask : MonoBehaviour {
    
	public Transform head;
    public AudioSource sound;
    public bool activateRandomly;
    [HideInInspector]
    public bool beginTask;
    public int totalTrials;
    public float trialDuration;
    [Tooltip("left min, left max, right min, right max")]
    public int[] bounds = new int[4];

	private bool isInRange;
	private string lastDirection;
    private Timer _timer;
    private bool _hasResponded = false;

    private float _time;

    private string[] mouseResponse = new string[] {"left", "right"};
    private int count;
    private string invertion;

    public static HeadRotationTask instance;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start() {
		string[] directions = new string[] {"left", "right"};
		lastDirection = directions[Random.Range(0,2)];

        _timer = Timer.instance;
	}

    void Update() {

        if(beginTask) {
            if (count < totalTrials){
     
		        if(!isInRange) {          
			        if(lastDirection == "right")//last direction is used so that only when entering the range coming from the center
				        if(head.eulerAngles.y > bounds[0] && head.eulerAngles.y < bounds[1])
					        StartCoroutine(Trial("left")); 
			
			        if(lastDirection == "left") 
				        if(head.eulerAngles.y > bounds[2] && head.eulerAngles.y < bounds[3])
					        StartCoroutine(Trial("right"));			
		        }
            }
        }

    }

    public void WriteHeadRotation(){
        string[] orientationLine = new string[] { count.ToString(), invertion, head.eulerAngles.x.ToString(), head.eulerAngles.y.ToString(), head.eulerAngles.z.ToString(), Time.realtimeSinceStartup.ToString(), _hasResponded.ToString()};
        //CsvWrite.instance.WriteFastLine(orientationLine);
    }

	public IEnumerator Trial(string side) {

        if (ServoExperimentManager.instance.invertDirection) invertion = "inverted";
        else invertion = "not inverted";

        InvokeRepeating("WriteHeadRotation", 0, 0.1f);

        int trialOrNot;
        string answer = null;

        if (activateRandomly) trialOrNot = Random.Range(0, 2);
        else trialOrNot = 1;

        isInRange = true;    

		if(trialOrNot == 1) {
			sound.Play();
            _time = Time.realtimeSinceStartup;
            _timer.stopwatch.Start();

            while((Time.realtimeSinceStartup - _time < trialDuration) && !Input.anyKeyDown) {
                    yield return null;

                if (Input.GetMouseButton(0)) {//if the pressed key is left
                    if (side == mouseResponse[0]) answer = "incorrect";
                    else answer = "correct";
                    _hasResponded = true;
                }

                else if (Input.GetMouseButton(1)) {//if the pressed key is right
                    if (side == mouseResponse[1]) answer = "incorrect";
                    else answer = "correct";
                    _hasResponded = true;
                }

                else _hasResponded = false;
            }


            string[] variables = new string[] { count.ToString(),invertion, side, _timer.ElapsedTimeAndRestart(), answer };

            CsvWrite.instance.WriteLine(variables);//writes trial to file
            Debug.Log(string.Join(",",variables));

            while (Input.anyKey) //waits while key is still pressed before proceeding
                yield return null;

            if (_hasResponded)
                count++;
            else
                Debug.Log("missed trial");

            _hasResponded = false;
		}

		else yield return null;//so not each head turn is a trial

        if (count == totalTrials) {
            Debug.Log("condition is over");
            beginTask = false;
            ServoExperimentManager.instance.beginTask = false;
            count = 0;
        }

		lastDirection = side;
		isInRange = false;		
	}

}
