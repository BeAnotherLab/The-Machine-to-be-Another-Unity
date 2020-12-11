using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeController : MonoBehaviour
{
    #region Public Fields
    public GameObject image1, image2, panel, text;
    #endregion

    #region MonoBehaviour Methods
    
    #endregion

    #region Public Methods
    public void FadeInImages(){
        FadeLT(true,image1);
        FadeLT(true,image2);
    }

    public void FadeOutImages()
    {
        FadeLT(false,image1);
        FadeLT(false,image2);
    }

    public void FadeInPanel()
    {
        FadeLT(true,panel);
    }

    public void FadeOutPanel()
    {
        FadeLT(false,panel);
    }

    public void FadeInText()
    {
        FadeLT(true,text);
    }

    public void FadeOutText()
    {
        FadeLT(false,text);
    }
    public void FadeOutAll()
    {
        FadeLT(false,image1);
        FadeLT(false,image2);
        FadeLT(false,panel);
        FadeLT(false,text);
    }

    public void FadeInAll()
    {
        FadeLT(true,image1);
        FadeLT(true,image2);
        FadeLT(true,panel);
        FadeLT(true,text);
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
