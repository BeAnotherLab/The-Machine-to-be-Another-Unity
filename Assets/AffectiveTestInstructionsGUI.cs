using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class AffectiveTestInstructionsGUI : MonoBehaviour
{
    public static AffectiveTestInstructionsGUI instance;
    
    [SerializeField] private Image selfFrame;
    [SerializeField] private GameObject selfImage;
    [SerializeField] private Image otherFrame;
    [SerializeField] private GameObject otherImage;

    private Dictionary<string, Sprite> _spritesDictionary;
    private Dictionary<string, AudioClip> _audioClipsDictionary;        
    
    // Start is called before the first frame update
    private void Start()
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>("AffectiveTaskImages");
        AudioClip[] clips = Resources.LoadAll<AudioClip>("AffectiveTaskAudioClips");

        _spritesDictionary = new Dictionary<string, Sprite>();
        _audioClipsDictionary = new Dictionary<string, AudioClip>();
        
        foreach (Sprite sprite in sprites) _spritesDictionary.Add(sprite.name, sprite);
        foreach (AudioClip clip in clips) _audioClipsDictionary.Add(clip.name, clip);
        
        int x = 0;

        //preload images


        //preload audios
    }
    
    public void ShowStimulus(JSONObject stimulus)
    {
        if (stimulus.GetField("perspective").str == "self")
        {
            selfFrame.color = Color.green;
            otherFrame.color = Color.red;
        }
        else
        {
            otherFrame.color = Color.green;
            selfFrame.color = Color.red;
        }
        
        stimulus.GetField("selfImage");
        stimulus.GetField("otherImage");
    }

    public void ShowRatingScale()
    {
        
    }
}
