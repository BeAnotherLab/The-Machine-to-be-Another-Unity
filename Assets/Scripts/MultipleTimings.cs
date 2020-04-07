using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultipleTimings : MonoBehaviour
{

    #region Public Fields
    public List<List<int>> timingList = new List<List<int>>();

    public static MultipleTimings instance;
    #endregion

    #region Private Fields
    [SerializeField] private List<int> timeList1 = new List<int>();
    [SerializeField] private List<int> timeList2 = new List<int>();
    [SerializeField] private List<int> timeList3 = new List<int>();
    [SerializeField] private List<int> timeList4 = new List<int>();
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

    private void Start()
    {

    }
    #endregion

}
