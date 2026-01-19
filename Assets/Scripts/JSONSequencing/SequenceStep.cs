using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class SequenceStep
{
    public float time;
    public string textKey;
    public string audio;
    public string visual;
    public List<string> actions;
}