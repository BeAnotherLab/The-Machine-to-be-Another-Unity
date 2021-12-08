using UnityEngine;

namespace ScriptableObjectArchitecture
{
	[System.Serializable]
	[CreateAssetMenu(
	    fileName = "UserStateGameEvent.asset",
	    menuName = SOArchitecture_Utility.GAME_EVENT + "Custom/UserState",
	    order = 120)]
	public sealed class UserStateGameEvent : GameEventBase<UserState>
	{
	}
}