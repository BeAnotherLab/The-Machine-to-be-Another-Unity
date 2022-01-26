using UnityEngine;

namespace ScriptableObjectArchitecture
{
	[System.Serializable]
	public sealed class UserStateReference : BaseReference<UserState, UserStateVariable>
	{
	    public UserStateReference() : base() { }
	    public UserStateReference(UserState value) : base(value) { }
	}
}