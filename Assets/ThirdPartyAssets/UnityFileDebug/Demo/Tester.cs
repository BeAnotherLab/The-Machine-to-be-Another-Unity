using UnityEngine;

public class Tester : MonoBehaviour
{
    private void Start()
    {
        throw new System.Exception("Even regular exceptions are logged");
    }
    void Update()
    {
        DebugFile.Log("CPU 1 has done action a", DLogType.AI);
        DebugFile.Log("Assertion created with DebugFile.Log", DLogType.Assert);
        DebugFile.Log("Song1 has started playing", DLogType.Audio);
        DebugFile.Log("Asset1 has loaded", DLogType.Content);
        DebugFile.Log("Couldn't find save file", DLogType.Error);
        DebugFile.Log("Database couldn't load", DLogType.Exception);
        DebugFile.Log("Showing game over UI", DLogType.GUI);
        DebugFile.Log("Player 1 pressed A", DLogType.Input);
        DebugFile.Log("Nearby enemy count: 3", DLogType.Log);
        DebugFile.Log("Condition 42 has been met", DLogType.Logic);
        DebugFile.Log("Connected to Player 2 at 192.168.0.0", DLogType.Network);
        DebugFile.Log("Physics collision with Ball1", DLogType.Physics);
        DebugFile.Log("Running on mobile", DLogType.System);
        DebugFile.Log("This is a bad thing that you should be aware of", DLogType.Warning);
        DebugFile.Log("Warning future me, this log will perplex you as it has me. It will depend on XYZ to be fixed before ABC can be implemented which where this log is from depends on to function properly. Thanks, past me", DLogType.Log);

        DebugFile.Assert(true == false);
        DebugFile.Assert(true == false, "true equals false");
        DebugFile.LogWarning("Warning");
        DebugFile.LogError("Error");
        DebugFile.LogAssertion("Conditionless assertion");
        DebugFile.LogFormat("<color=green>This is a green message!</color>");
    }
}
