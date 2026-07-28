using UnityEngine;

public class PresenceTest : MonoBehaviour
{
    private bool _last;

    private void Start()
    {
        _last = OVRPlugin.userPresent;
        Debug.Log($"Initial presence: {_last}");
    }

    private void Update()
    {
        bool current = OVRPlugin.userPresent;

        if (current != _last)
        {
            _last = current;
            Debug.Log($"Presence changed: {current}");
        }
    }
}