using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Translations", menuName = "Scriptable Objects/Translations")]
public class Translations : ScriptableObject //container for text translations that need to be shown by key
{
    public Dictionary<string, string> Value;
}
