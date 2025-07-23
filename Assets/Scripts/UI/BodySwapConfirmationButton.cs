using UnityEngine;

public class BodySwapConfirmationButton : MonoBehaviour
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
