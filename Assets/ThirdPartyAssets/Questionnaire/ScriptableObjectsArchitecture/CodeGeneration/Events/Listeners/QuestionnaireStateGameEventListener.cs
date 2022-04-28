using UnityEngine;

namespace ScriptableObjectArchitecture
{
	[AddComponentMenu(SOArchitecture_Utility.EVENT_LISTENER_SUBMENU + "QuestionnaireState")]
	public sealed class QuestionnaireStateGameEventListener : BaseGameEventListener<QuestionnaireState, QuestionnaireStateGameEvent, QuestionnaireStateUnityEvent>
	{
	}
}