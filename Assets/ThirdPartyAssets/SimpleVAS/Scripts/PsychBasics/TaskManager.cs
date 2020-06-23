using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace UnityPsychBasics {

    public class TaskManager : MonoBehaviour {

        public Text textUI;
        public Button _nextButton;
        public Scrollbar _scrollbar;
        public ToggleGroup _toggleGroup;
        public Image _image;

        [HideInInspector]
        public bool shuffle, useImages, useAnalogueScale;

        [HideInInspector]
        public bool setValueOutside;

        public static TaskManager instance;

        private Timer _timer;
        private ScaleManager _scaleManager;

        private List<string> questionList = new List<string>();
        private List<Sprite> imageList = new List<Sprite>();
        private List<int> indexList = new List<int>();

        private int currentItem;

        private void Awake() {
            if (instance == null) instance = this;
        }

        void Start() {

            _nextButton.interactable = false;

            _timer = Timer.instance;
            _timer.stopwatch.Start();

        }

        public void InitializeValuesListsAndObjects() {

            _scaleManager = ScaleManager.instance;
            _scaleManager.CreateToggles();

            if(setValueOutside) 
                ShowGameObjects(new GameObject[]{  });
                      
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
                for (int i = 0; i < ImageRead.instance.imageSprites.Count; i++) imageList.Add(ImageRead.instance.imageSprites[i]);
                if (shuffle) CreateShuffleList();
                SetImage();
            }

            else {
                CsvRead.instance.SetFileToLoad();

                for (int i = 0; i < CsvRead.instance.questionnaireInput.Count; i++)
                    questionList.Add(CsvRead.instance.questionnaireInput[i]);

                if (shuffle) CreateShuffleList();
                textUI.text = questionList[currentItem];
            }
        }

        private void ShowGameObjects(GameObject[] objectToShow) //TODO use Canvas Grop instead
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

        private void CreateShuffleList(){

            if(useImages)
                for (int i = 0; i < imageList.Count; i++)
                    indexList.Add(i);
            else
                for (int i = 0; i < questionList.Count; i++)
                    indexList.Add(i);

            currentItem = ShuffleValue();
        }

        private int ShuffleValue() {

            int randomIndex = Random.Range(0, indexList.Count);
            int selectedItem = indexList[randomIndex];
            indexList.RemoveAt(randomIndex);

            return selectedItem;
        }


        public void OnResponseSelection() {
            _nextButton.interactable = true;
        }

        public void OnNextButton() {
            CsvWrite.instance.responseTime = _timer.ElapsedTimeAndRestart();
            _nextButton.interactable = false;
            CsvWrite.instance.item = currentItem;
            if (!setValueOutside) CsvWrite.instance.response = ResponseValue();
            CsvWrite.instance.LogTrial();
            _scrollbar.value = 0.5f;
            DoAfterSeletion();
        }

        public float ResponseValue() {
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

        public void OutsideResponseValue(float outsideValue){
            CsvWrite.instance.response = outsideValue;
        }

        private void DoAfterSeletion(){

            if (!shuffle) {
                currentItem++;

                if (useImages) {
                    if (currentItem < imageList.Count)
                        SetImage();

                    else if (currentItem == imageList.Count)
                        QuestionsExhausted();

                    _timer.stopwatch.Start();
                }

                else {
                    if (currentItem < questionList.Count)
                        textUI.text = questionList[currentItem];

                    else if (currentItem == questionList.Count)
                        QuestionsExhausted();
                }
            }

            else {
                if (indexList.Count != 0) {
                    currentItem = ShuffleValue();

                    if (useImages)
                        SetImage();

                    else
                        textUI.text = questionList[currentItem];

                    _timer.stopwatch.Start();
                }

                else if (indexList.Count == 0)
                    QuestionsExhausted();
            }
        }

        private void SetImage(){
            
            _image.sprite = imageList[currentItem];
            _image.GetComponent<RectTransform>().sizeDelta = new Vector2(_image.sprite.rect.width, (float)_image.sprite.rect.height);
        }


        private void QuestionsExhausted() {

            CsvWrite.instance.condition++;
            currentItem = 0;
            questionList.Clear();
            imageList.Clear();
            indexList.Clear();

            _nextButton.interactable = false;
            
            if (TaskSettings.instance == null) {
                UnityEngine.Debug.Log("You must attach the LoadSceneAfterTask object somewhere in the scene and add Scene names to it");//else the call is ambiguous for the diagnostics library
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            }

            else {
                if (CsvWrite.instance.condition < ConditionDictionary.selectedOrder.Length)
                    TaskSettings.instance.LoadBeforeLast();

                else if (CsvWrite.instance.condition == ConditionDictionary.selectedOrder.Length)
                    TaskSettings.instance.LoadAfterLast();
            }
        }

        public void LoadScene(string scene) {
            SceneManager.LoadScene(scene);
        }

    }

    }
