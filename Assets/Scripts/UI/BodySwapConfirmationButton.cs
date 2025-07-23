using UnityEngine;

public class BodySwapConfirmationButton : MonoBehaviour //TODO inherit confirmation button?
{
    public void SelfUserStateChanged(UserState selfUserState)
    {
        if (selfUserState == UserState.readyToStart)
        {
            GetComponent<MeshRenderer>().enabled = false;
            GetComponent<MeshCollider>().enabled = false;    
        }
    }
}
