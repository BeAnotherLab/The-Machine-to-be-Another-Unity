using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultipleTimings : MonoBehaviour
{

    #region Public Fields
    public List<List<float>> timingList = new List<List<float>>();

    public static MultipleTimings instance;
    #endregion

    #region Private Fields
    [Tooltip("0 = instructions end, 1 = mirror, 2 = wall, 3 = goodbye")] [SerializeField] private List<float> timeList1 = new List<float>();
    [Tooltip("0 = instructions end, 1 = mirror, 2 = wall, 3 = goodbye")] [SerializeField] private List<float> timeList2 = new List<float>();
    [Tooltip("0 = instructions end, 1 = mirror, 2 = wall, 3 = goodbye")] [SerializeField] private List<float> timeList3 = new List<float>();
    [Tooltip("0 = instructions end, 1 = mirror, 2 = wall, 3 = goodbye")] [SerializeField] private List<float> timeList4 = new List<float>();
    #endregion



    #region MonoBehaviour Methods
    private void Awake()
    {
        if (instance == null)
            instance = this;

        timingList.Add(timeList1);
        timingList.Add(timeList2);
        timingList.Add(timeList3);
        timingList.Add(timeList4);
    }

    #endregion

}
