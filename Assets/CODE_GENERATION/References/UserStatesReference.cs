using UnityEngine;

namespace ScriptableObjectArchitecture
{
	[System.Serializable]
	public sealed class UserStatesReference : BaseReference<UserStates, UserStatesVariable>
	{
	    public UserStatesReference() : base() { }
	    public UserStatesReference(UserStates value) : base(value) { }
	}
}