using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadRotationTask : MonoBehaviour {
    
	public Transform head;
    public AudioSource sound;
    public bool activateRandomly;
    public bool beginTask;
    public int totalTrials;   
    [Tooltip("left min, left max, right min, right max")]
    public int[] bounds = new int[4];

	private bool isInRange;
	private string lastDirection;
    private Timer _timer;

    private string[] mouseResponse = new string[] {"left", "right"};
    private int count;

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
     
		        if(!isInRange){          
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

	public IEnumerator Trial(string side) {
       
        int trialOrNot;
        string answer = null;

        if (activateRandomly) trialOrNot = Random.Range(0, 2);
        else trialOrNot = 1;

        isInRange = true;    

		if(trialOrNot == 1) {
			sound.Play();
            _timer.stopwatch.Start();

            while (!Input.anyKeyDown) //while there is no key pressed, wait.
                yield return null;

            if (Input.GetMouseButton(0)) //if the pressed key is left
                if (side == mouseResponse[0]) answer = "correct";
                else answer = "incorrect";

            else if (Input.GetMouseButton(1))//if the pressed key is right
                if (side == mouseResponse[1]) answer = "correct";
                else answer = "incorrect";

            string invertion;
            if (ServoExperimentManager.instance.invertDirection) invertion = "inverted";
            else invertion = "not inverted";

            string[] variables = new string[] { count.ToString(),invertion, side, _timer.ElapsedTimeAndRestart(), answer };

            CsvWrite.instance.WriteLine(variables);//writes trial to file
            Debug.Log(string.Join(",",variables));

            while (Input.anyKey) //waits while key is still pressed before proceeding
                yield return null;
            
            count++;
		}

		else yield return null;//so not each head turn is a trial

        if (count == totalTrials) {
            Debug.Log("condition is over");
            beginTask = false;
            count = 0;
        }

		lastDirection = side;
		isInRange = false;		
	}

}
