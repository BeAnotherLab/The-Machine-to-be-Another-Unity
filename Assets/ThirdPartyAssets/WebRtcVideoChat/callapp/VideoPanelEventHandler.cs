using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.Events;

/// <summary>
/// Used to catch different events and forward them to the UI
/// </summary>
public class VideoPanelEventHandler : MonoBehaviour, IPointerClickHandler
{
    private CallAppUi mParent;
    private float mLastClick;

    private void Start()
    {
        mParent = this.GetComponentInParent<CallAppUi>();
        if(mParent == null)
        {
            Debug.LogError("Failed to find CallAppUi. Deactivating VideoPanelEventHandler");
            this.gameObject.SetActive(false);
            return;
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        //Check for two clicks short after each other. Should work
        //on mobile and desktop platforms
        if((eventData.clickTime - mLastClick) < 0.5f)
        {
            mParent.Fullscreen();
        }
        else
        {
            mParent.ShowOverlay();
        }
        mLastClick = eventData.clickTime;
    }
}
