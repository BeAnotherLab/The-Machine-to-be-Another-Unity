using UnityEngine;

namespace ScriptableObjectArchitecture
{
	[System.Serializable]
	[CreateAssetMenu(
	    fileName = "QuestionnaireStateGameEvent.asset",
	    menuName = SOArchitecture_Utility.GAME_EVENT + "Custom/QuestionnaireState",
	    order = 120)]
	public sealed class QuestionnaireStateGameEvent : GameEventBase<QuestionnaireState>
	{
	}
}