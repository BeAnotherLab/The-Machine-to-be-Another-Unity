using UnityEngine;

namespace ScriptableObjectArchitecture
{
	[System.Serializable]
	public sealed class QuestionnaireStateReference : BaseReference<QuestionnaireState, QuestionnaireStateVariable>
	{
	    public QuestionnaireStateReference() : base() { }
	    public QuestionnaireStateReference(QuestionnaireState value) : base(value) { }
	}
}