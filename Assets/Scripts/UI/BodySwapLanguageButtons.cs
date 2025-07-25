using UnityEngine;

public class BodySwapLanguageButtons : MonoBehaviour
{
    [SerializeField] private GameObject _buttons;
    
    public void SelfUserStateChanged(UserState selfUserState)
    {
        if (selfUserState == UserState.readyToStart)
        {
            _buttons.SetActive(false);
        }
    }
}
