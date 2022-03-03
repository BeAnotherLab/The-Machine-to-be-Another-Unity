using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeController : MonoBehaviour
{
    #region Public Fields
    public GameObject image1, image2, text;
    #endregion

    #region MonoBehaviour Methods
    //show text from key welcome
    
    
    //ShowInstructionsText()
    //fade in images
    
    #endregion

    #region Public Methods
    public void FadeInImages(){ //used
        FadeLT(true,image1);
        FadeLT(true,image2);
    }

    public void FadeOutImages()
    {
        FadeLT(false,image1);
        FadeLT(false,image2);
    }

    public void FadeInText()
    {
        FadeLT(true,text);
    }

    public void FadeOutAll() //used
    {
        FadeLT(false,image1);
        FadeLT(false,image2);
        FadeLT(false,text);
    }

    #endregion

    #region Private Fields

    private void FadeLT(bool fadeIn, GameObject obj)
    {
        Color from = new Color();
        
        if(obj.GetComponent<Renderer>() != null)
            from = obj.GetComponent<Renderer>().material.color;

        else if(obj.GetComponent<SpriteRenderer>() != null)
            from = obj.GetComponent<SpriteRenderer>().material.color;

        else if (obj.GetComponent<Image>() != null)
            from = obj.GetComponent<Image>().material.color;

        var to = Color.white;
        if (!fadeIn) to = new Color(1,1,1,0);

        LeanTween.value(obj, from,  to, 1).setOnUpdate((val) =>
        {
            if (obj.GetComponent<Renderer>() != null)
                obj.GetComponent<Renderer>().material.color = val;
            else if (obj.GetComponent<SpriteRenderer>() != null)
                obj.GetComponent<SpriteRenderer>().material.color = val;
            else if (obj.GetComponent<Image>() != null)
                obj.GetComponent<Image>().material.color = val;
        });

    }
    #endregion
}
