using UnityEngine;

namespace ScriptableObjectArchitecture
{
	[System.Serializable]
	[CreateAssetMenu(
	    fileName = "UserStatesGameEvent.asset",
	    menuName = SOArchitecture_Utility.GAME_EVENT + "Custom/UserStates",
	    order = 120)]
	public sealed class UserStatesGameEvent : GameEventBase<UserStates>
	{
	}
}