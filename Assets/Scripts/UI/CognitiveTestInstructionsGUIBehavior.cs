using UnityEngine;

public enum ExperimentStep { pre, intervention, post };

public class CognitiveTestInstructionsGUIBehavior : MonoBehaviour
{

   public static CognitiveTestInstructionsGUIBehavior instance;

   [SerializeField] private GameObject[] _slides;
   private int _slideIndex;

   private void Awake()
   {
      if (instance == null) instance = this;
   }

   public void Init()
   {
      _slides[0].SetActive(true);
   }
   
   public void Next()
   {
      _slides[_slideIndex].SetActive(false);

      if (_slideIndex < _slides.Length - 1 )
      {
         _slideIndex++;
         _slides[_slideIndex].SetActive(true);   
      }
      else
      {
         CognitiveTestManager.instance.StartTest(ExperimentStep.pre);
      }
   }
}
