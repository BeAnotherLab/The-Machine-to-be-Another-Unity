using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSequence", menuName = "Timeline/SequenceData", order = 1)]
public class SequenceData : ScriptableObject
{
    public List<SequenceStep> steps = new List<SequenceStep>();
}