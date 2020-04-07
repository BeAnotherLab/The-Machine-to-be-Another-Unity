using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LanguageTextDictionary))]
public class LanguageDictionaryEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        LanguageTextDictionary dictionary = (LanguageTextDictionary) target;
        
        if (GUILayout.Button("english")) dictionary.LanguageChanged("english"); 
        if (GUILayout.Button("french")) dictionary.LanguageChanged("french");
        if (GUILayout.Button("deutsch")) dictionary.LanguageChanged("deutsch");
        if (GUILayout.Button("italian")) dictionary.LanguageChanged("italian");
    }
}
