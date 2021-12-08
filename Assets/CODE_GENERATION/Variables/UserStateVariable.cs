using UnityEngine;
using UnityEngine.Events;

namespace ScriptableObjectArchitecture
{
	[System.Serializable]
	public class UserStateEvent : UnityEvent<UserState> { }

	[CreateAssetMenu(
	    fileName = "UserStateVariable.asset",
	    menuName = SOArchitecture_Utility.VARIABLE_SUBMENU + "Custom/UserState",
	    order = 120)]
	public class UserStateVariable : BaseVariable<UserState, UserStateEvent>
	{
	}
}