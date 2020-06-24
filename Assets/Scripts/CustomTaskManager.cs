using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace UnityPsychBasics {

    public class CustomTaskManager : MonoBehaviour 
    {
        #region public variables
        
        public Text textUI;
        public Button _nextButton;
        public Scrollbar _scrollbar;
        public ToggleGroup _toggleGroup;
        public Image _image;

        [HideInInspector] public bool shuffle, useImages, useAnalogueScale;
        [HideInInspector] public bool setValueOutside;

        public static CustomTaskManager instance;

        #endregion

        
        #region private variables
        
        private List<string> _questionList = new List<string>();
        private List<Sprite> _imageList = new List<Sprite>();
        private List<int> _indexList = new List<int>();
        private int _currentItem;
        
        #endregion
        
        
        #region MonoBehavior methods
        
        private void Awake() 
        {
            if (instance == null) instance = this;
        }

        void Start() 
        {
            _nextButton.interactable = false;
            Timer.instance.stopwatch.Start();
        }

        #endregion


        #region Public methods

        public void InitializeValuesListsAndObjects() 
        {
            CustomScaleManager.instance.CreateToggles();

            if(setValueOutside) ShowGameObjects(new GameObject[]{ }); 
                      
            else {
                if (useAnalogueScale && useImages) 
                    ShowGameObjects(new GameObject[] {_scrollbar.gameObject, _image.gameObject, _nextButton.gameObject});
                else if (!useAnalogueScale && useImages)
                    ShowGameObjects(new GameObject[] { _toggleGroup.gameObject, _image.gameObject, _nextButton.gameObject });
                else if (useAnalogueScale && !useImages)
                    ShowGameObjects(new GameObject[] { _scrollbar.gameObject, textUI.gameObject, _nextButton.gameObject });
                else if (!useAnalogueScale && !useImages)
                    ShowGameObjects(new GameObject[] { _toggleGroup.gameObject, textUI.gameObject, _nextButton.gameObject });
            }

            if (useImages) {
                for (int i = 0; i < ImageRead.instance.imageSprites.Count; i++) _imageList.Add(ImageRead.instance.imageSprites[i]);
                if (shuffle) CreateShuffleList();
                SetImage();
            }

            else {
                CustomCSVRead.instance.SetFileToLoad();

                for (int i = 0; i < CustomCSVRead.instance.questionnaireInput.Count; i++)
                    _questionList.Add(CustomCSVRead.instance.questionnaireInput[i]);

                if (shuffle) CreateShuffleList();
                textUI.text = _questionList[_currentItem];
            }
        }
        
        public void OnResponseSelection() 
        {
            _nextButton.interactable = true;
        }

        public void OnNextButton() //TODO split into two methods, one for GUI, the other for CustomCSVWrite 
        {
            CustomCSVWrite.instance.WriteResult(_currentItem, ResponseValue());
            _nextButton.interactable = false;
            _scrollbar.value = 0.5f;
            
            if (!shuffle) {
                _currentItem++;

                if (useImages) {
                    if (_currentItem < _imageList.Count) SetImage();
                    else if (_currentItem == _imageList.Count) QuestionsExhausted();
                    Timer.instance.stopwatch.Start();
                }

                else {
                    if (_currentItem < _questionList.Count) textUI.text = _questionList[_currentItem];
                    else if (_currentItem == _questionList.Count) QuestionsExhausted();
                }
            }

            else {
                if (_indexList.Count != 0) {
                    _currentItem = ShuffleValue();
                    if (useImages) SetImage();
                    else textUI.text = _questionList[_currentItem];
                    Timer.instance.stopwatch.Start();
                }
                else if (_indexList.Count == 0) QuestionsExhausted();
            }
        }

        public void OutsideResponseValue(float outsideValue) 
        {
            CustomCSVWrite.instance.response = outsideValue;
        }

        public void LoadScene(string scene) 
        {
            SceneManager.LoadScene(scene);
        }
        
        #endregion


        #region Private methods

        private float ResponseValue() 
        {
            float currentValue = 0;

            if (!useAnalogueScale) {
                Toggle[] numberOfToggles = _toggleGroup.GetComponentsInChildren<Toggle>();

                for (int i = 0; i < numberOfToggles.Length; i++)
                    if (numberOfToggles[i].isOn)
                        currentValue = i;

                _toggleGroup.SetAllTogglesOff();
                _nextButton.interactable = false;
            }
            else currentValue = _scrollbar.value;

            return currentValue;
        }
        
        private void ShowGameObjects(GameObject[] objectToShow) //TODO use Canvas Groups instead
        {
            GameObject[] _gameObjectsToShow = {_toggleGroup.gameObject, _scrollbar.gameObject, _nextButton.gameObject, _image.gameObject, textUI.gameObject};

            foreach (GameObject _object in _gameObjectsToShow) {
                _object.SetActive(false);
                
                for (int i = 0; i < objectToShow.Length; i++) {
                    if (objectToShow[i] == _object)
                        _object.SetActive(true);
                }
            }
        }

        private void CreateShuffleList()
        {
            if(useImages)
                for (int i = 0; i < _imageList.Count; i++)
                    _indexList.Add(i);
            else
                for (int i = 0; i < _questionList.Count; i++)
                    _indexList.Add(i);

            _currentItem = ShuffleValue();
        }

        private int ShuffleValue() 
        {
            int randomIndex = Random.Range(0, _indexList.Count);
            int selectedItem = _indexList[randomIndex];
            _indexList.RemoveAt(randomIndex);

            return selectedItem;
        }

        private void SetImage()
        {
            _image.sprite = _imageList[_currentItem];
            _image.GetComponent<RectTransform>().sizeDelta = new Vector2(_image.sprite.rect.width, (float)_image.sprite.rect.height);
        }

        private void QuestionsExhausted() 
        {
            CustomCSVWrite.instance.condition++;
            _currentItem = 0;
            _questionList.Clear();
            _imageList.Clear();
            _indexList.Clear();

            _nextButton.interactable = false;
            
            if (CustomTaskSettings.instance == null) {
                Debug.Log("You must attach the LoadSceneAfterTask object somewhere in the scene and add Scene names to it");//else the call is ambiguous for the diagnostics library
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            }

            else {
                if (CustomCSVWrite.instance.condition < ConditionDictionary.selectedOrder.Length)
                    CustomTaskSettings.instance.LoadBeforeLast();

                else if (CustomCSVWrite.instance.condition == ConditionDictionary.selectedOrder.Length)
                    CustomTaskSettings.instance.LoadAfterLast();
            }
        }
        
        #endregion

    }
}
