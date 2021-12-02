using UnityEngine;

namespace ScriptableObjectArchitecture
{
	[AddComponentMenu(SOArchitecture_Utility.EVENT_LISTENER_SUBMENU + "UserStates")]
	public sealed class UserStatesGameEventListener : BaseGameEventListener<UserStates, UserStatesGameEvent, UserStatesUnityEvent>
	{
	}
}