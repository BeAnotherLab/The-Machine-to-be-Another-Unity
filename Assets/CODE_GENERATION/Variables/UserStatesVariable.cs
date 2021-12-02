using UnityEngine;
using UnityEngine.Events;

namespace ScriptableObjectArchitecture
{
	[System.Serializable]
	public class UserStatesEvent : UnityEvent<UserStates> { }

	[CreateAssetMenu(
	    fileName = "UserStatesVariable.asset",
	    menuName = SOArchitecture_Utility.VARIABLE_SUBMENU + "Custom/UserStates",
	    order = 120)]
	public class UserStatesVariable : BaseVariable<UserStates, UserStatesEvent>
	{
	}
}