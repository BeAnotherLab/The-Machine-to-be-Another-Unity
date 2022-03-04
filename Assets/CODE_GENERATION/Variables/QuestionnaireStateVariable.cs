using UnityEngine;
using UnityEngine.Events;

namespace ScriptableObjectArchitecture
{
	[System.Serializable]
	public class QuestionnaireStateEvent : UnityEvent<QuestionnaireState> { }

	[CreateAssetMenu(
	    fileName = "QuestionnaireStateVariable.asset",
	    menuName = SOArchitecture_Utility.VARIABLE_SUBMENU + "Custom/QuestionnaireState",
	    order = 120)]
	public class QuestionnaireStateVariable : BaseVariable<QuestionnaireState, QuestionnaireStateEvent>
	{
	}
}