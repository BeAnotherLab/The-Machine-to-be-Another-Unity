using UnityEngine;

namespace ScriptableObjectArchitecture
{
	[AddComponentMenu(SOArchitecture_Utility.EVENT_LISTENER_SUBMENU + "UserState")]
	public sealed class UserStateGameEventListener : BaseGameEventListener<UserState, UserStateGameEvent, UserStateUnityEvent>
	{
	}
}