﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using Facebook.Unity;
using com.adjust.sdk;

namespace FateGames
{
    public class GameManager : MonoBehaviour
    {
        #region Properties
        private static int levelCount = 1;
        private bool isLevelCountSet = false;
        private bool locked = false;

        public bool Locked
        {
            get { return locked; }
        }

        public static int MONEY
        {
            get
            {
                return PlayerPrefs.GetInt("MONEY");
            }
            set
            {
                PlayerPrefs.SetInt("MONEY", value);
            }
        }

        public static int GEM
        {
            get
            {
                return PlayerPrefs.GetInt("GEM");
            }
            set
            {
                PlayerPrefs.SetInt("GEM", value);
            }
        }

        public static int KEY
        {
            get
            {
                return PlayerPrefs.GetInt("KEY");
            }
            set
            {
                PlayerPrefs.SetInt("KEY", value);
            }
        }

        public static int GOLD
        {
            get
            {
                return PlayerPrefs.GetInt("GOLD");
            }
            set
            {
                PlayerPrefs.SetInt("GOLD", value);
            }
        }

        public CursorType cursorType = CursorType.DEFAULT;
        public GameState State = GameState.NOT_STARTED;
        private static GameManager instance;
        private UIStartText uiStartText = null;
        #endregion

        #region Object Serializations
        [SerializeField] private string levelName = "Level";
        [SerializeField] private string[] successTexts = null;
        [SerializeField] private Color[] successTextColors = null;
        [Header("Prefabs")]
        [SerializeField] private GameObject uiPrefab = null;
        [SerializeField] private GameObject uiLoadingScreenPrefab = null;
        [SerializeField] private GameObject uiCompleteScreenPrefab = null;
        [SerializeField] private GameObject uiLevelTextPrefab = null;
        [SerializeField] private GameObject uiStartTextPrefab = null;
        [SerializeField] private GameObject confettiShowerPrefab = null;
        [SerializeField] private GameObject confettiBlastPrefab = null;
        [SerializeField] private GameObject successTextPrefab = null;
        [SerializeField] private GameObject instructionTextPrefab = null;
        [SerializeField] private GameObject[] emojiEffectPrefabs = null;
        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            AvoidDuplication();
            SetLevelCount();
            
            if (FB.IsInitialized)
            {
                FB.ActivateApp();
            }
            else
            {
                //Handle FB.Init
                FB.Init(() =>
                {
                    FB.ActivateApp();
                    AdjustCurrentLevel();
                });
            }
        }

        private void Start()
        {
            // import this package into the project:
            // https://github.com/adjust/unity_sdk/releases

#if UNITY_IOS
            /* Mandatory - set your iOS app token here */
            InitAdjust("yjwava7b32tc");
#elif UNITY_ANDROID
        /* Mandatory - set your Android app token here */
        InitAdjust("YOUR_ANDROID_APP_TOKEN_HERE");
#endif
        }

        private void Update()
        {
            if (!locked)
                CheckInput();
            if (Input.GetKeyDown(KeyCode.S))
            {
                TakeScreenshot();
            }
        }

        void OnApplicationPause(bool pauseStatus)
        {
            // Check the pauseStatus to see if we are in the foreground
            // or background
            if (!pauseStatus)
            {
                //app resume
                if (FB.IsInitialized)
                {
                    FB.ActivateApp();
                }
                else
                {
                    //Handle FB.Init
                    FB.Init(() =>
                    {
                        FB.ActivateApp();
                    });
                }
            }
        }

        #endregion

        #region Singleton
        public static GameManager Instance
        {
            get
            {
                GameManager instance = GameManager.instance;
                if (!instance)
                {
                    instance = new GameObject("GameManager").AddComponent<GameManager>();
                }
                return instance;
            }
        }

        private void AvoidDuplication()
        {
            if (!instance)
            {
                DontDestroyOnLoad(gameObject);
                instance = this;
            }
            else
                DestroyImmediate(gameObject);
        }

        #endregion

        private void CheckInput()
        {
            if (Input.GetKeyDown(KeyCode.M))
            {
                SwitchCursorType();
            }
            if (Input.GetKeyDown(KeyCode.X) && State == GameState.STARTED)
            {
                FinishLevel(true);
            }
            else if (Input.GetKeyDown(KeyCode.C) && State == GameState.STARTED)
            {
                FinishLevel(false);
            }
            else if (Input.GetMouseButtonDown(0) && State == GameState.NOT_STARTED)
            {
                print(levelName + " " + CurrentLevel + " started.");
                LogLevelStartedEvent(CurrentLevel);
                State = GameState.STARTED;
                if (uiStartText)
                    uiStartText.Hide();
                LevelManager.Instance.StartLevel();
            }
        }
        public void SwitchCursorType()
        {
            MouseCursor cursor = FindObjectOfType<MouseCursor>();
            if (cursorType == CursorType.DEFAULT)
            {
                cursorType = CursorType.HAND;
                cursor.Show();
                Cursor.visible = false;
            }
            else if (cursorType == CursorType.HAND)
            {
                cursorType = CursorType.NO_CURSOR;
                cursor.Hide();
                Cursor.visible = false;
            }
            else if (cursorType == CursorType.NO_CURSOR)
            {
                cursorType = CursorType.DEFAULT;
                cursor.Hide();
                Cursor.visible = true;
            }
        }

        private void InitAdjust(string adjustAppToken)
        {
            var adjustConfig = new AdjustConfig(
                adjustAppToken,
                AdjustEnvironment.Production, // AdjustEnvironment.Sandbox to test in dashboard
                true
            );
            adjustConfig.setLogLevel(AdjustLogLevel.Info); // AdjustLogLevel.Suppress to disable logs
            adjustConfig.setSendInBackground(true);
            new GameObject("Adjust").AddComponent<Adjust>(); // do not remove or rename

            // Adjust.addSessionCallbackParameter("foo", "bar"); // if requested to set session-level parameters

            //adjustConfig.setAttributionChangedDelegate((adjustAttribution) => {
            //  Debug.LogFormat("Adjust Attribution Callback: ", adjustAttribution.trackerName);
            //});

            Adjust.start(adjustConfig);

        }


        public void LogLevelStartedEvent(int level)
        {
            var parameters = new Dictionary<string, object>();
            parameters["level"] = level;
            FB.LogAppEvent(
                "Level Started",
                0,
                parameters
            );
        }

        public void LogLevelFailedEvent(int level)
        {
            var parameters = new Dictionary<string, object>();
            parameters["level"] = level;
            FB.LogAppEvent(
                "Level Failed",
                0,
                parameters
            );
        }

        public void LogLevelCompletedEvent(int level)
        {
            var parameters = new Dictionary<string, object>();
            parameters["level"] = level;
            FB.LogAppEvent(
                "Level Completed",
                0,
                parameters
            );
        }
        private void TakeScreenshot()
        {
            string folderPath = Directory.GetCurrentDirectory() + "/Screenshots/";

            if (!System.IO.Directory.Exists(folderPath))
                System.IO.Directory.CreateDirectory(folderPath);

            var screenshotName =
                                    "Screenshot_" +
                                    System.DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss") +
                                    ".png";
            ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(folderPath, screenshotName));
            Debug.Log(folderPath + screenshotName);
        }

        #region Level management

        public int CurrentLevel
        {
            get
            {
                return PlayerPrefs.GetInt("currentLevel");
            }
            set
            {
                PlayerPrefs.SetInt("currentLevel", value);
            }
        }

        public string LevelName
        {
            get
            {
                return levelName;
            }
        }

        private void AdjustCurrentLevel()
        {
            if (!isLevelCountSet)
                SetLevelCount();
            if (CurrentLevel == 0 || CurrentLevel > levelCount)
                CurrentLevel = 1;
            if (SceneManager.GetActiveScene().buildIndex == 0) // no level is loaded
            {
                LoadLevel(CurrentLevel);
            }

        }
        private void SetLevelCount()
        {
            // There will be only one scene (LevelLoader) in the build settings other than level scenes.
            levelCount = SceneManager.sceneCountInBuildSettings - 1;
            isLevelCountSet = true;
        }

        public void LoadCurrentLevel()
        {
            LoadLevel(CurrentLevel);
        }
        public void LoadLevel(int level)
        {
            LeanTween.cancelAll();
            InputManager.Clear();
            StartCoroutine(LoadLevelAsynchronously(level));
        }

        private IEnumerator LoadLevelAsynchronously(int level)
        {
            locked = true;
            UILoadingScreen loadingScreen = CreateLoadingScreen();
            AsyncOperation operation = SceneManager.LoadSceneAsync(level);
            while (!operation.isDone)
            {
                float progress = Mathf.Clamp01(operation.progress / .9f);
                yield return null;
            }
            if (loadingScreen)
                loadingScreen.Hide();
            //CreateUILevelText();
            CreateUIStartText();
            locked = false;
            State = GameState.NOT_STARTED;
        }

        public void FinishLevel(bool success)
        {
            if (success) LogLevelCompletedEvent(CurrentLevel);
            else LogLevelFailedEvent(CurrentLevel);
            State = GameState.FINISHED;
            if (success)
            {
                /*InstantiateConfettiBlast();
                InstantiateConfettiShower();*/

                LeanTween.delayedCall(1f, () =>
                {
                    CreateUICompleteScreen(success);
                    CurrentLevel += 1;
                    AdjustCurrentLevel();
                });
            }
            else CreateUICompleteScreen(success);
        }
        #endregion

        #region UI
        private void CreateUILevelText()
        {
            Transform parent = UICanvas.transform;
            GameObject go = Instantiate(uiLevelTextPrefab, parent);
            Text levelText = go.GetComponent<Text>();
            levelText.text = levelName + " " + CurrentLevel;
        }

        private void CreateUIStartText()
        {
            Transform parent = UICanvas.transform;
            GameObject go = Instantiate(uiStartTextPrefab, parent);
            Text levelText = go.GetComponent<Text>();
            levelText.text = "TAP TO PLAY";
            uiStartText = go.GetComponent<UIStartText>();
        }

        private void CreateUIStartText(float verticalAnchorPosition)
        {
            Transform parent = UICanvas.transform;
            GameObject go = Instantiate(uiStartTextPrefab, parent);
            Text levelText = go.GetComponent<Text>();
            RectTransform rectTransform = go.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(rectTransform.anchorMin.x, verticalAnchorPosition);
            rectTransform.anchorMax = new Vector2(rectTransform.anchorMax.x, verticalAnchorPosition);
            rectTransform.anchoredPosition = Vector2.zero;
            levelText.text = "TAP TO PLAY";
            uiStartText = go.GetComponent<UIStartText>();
        }

        public void CreateUICompleteScreen(bool success)
        {
            Transform parent = UICanvas.transform;
            GameObject go = Instantiate(uiCompleteScreenPrefab, parent);
            UICompleteScreen uiCompleteScreen = go.GetComponent<UICompleteScreen>();
            uiCompleteScreen.SetScreen(success, CurrentLevel);


        }

        private void InstantiateConfettiShower()
        {
            print("Instantiated confetti shower");
            Instantiate(confettiShowerPrefab, Camera.main.transform);

            //Instantiate(confettiShowerPrefab, GameObject.FindGameObjectWithTag("Second Camera").transform);
        }

        private void InstantiateConfettiBlast()
        {
            print("Instantiated confetti blast");
            Instantiate(confettiBlastPrefab, Camera.main.transform);
        }

        public Canvas UICanvas
        {
            get
            {
                return GameObject.Find("UI").transform.Find("Canvas").GetComponent<Canvas>();
            }
        }

        private UILoadingScreen CreateLoadingScreen()
        {
            UILoadingScreen uiLoadingScreen = FindObjectOfType<UILoadingScreen>();
            if (!uiLoadingScreen)
            {
                Transform parent = UICanvas.transform;
                GameObject go = Instantiate(uiLoadingScreenPrefab, parent);
                uiLoadingScreen = go.AddComponent<UILoadingScreen>();
            }
            return uiLoadingScreen;

        }

        #endregion

        #region Enumerators
        public enum GameState { STARTED, NOT_STARTED, PAUSED, FINISHED }
        public enum CursorType { DEFAULT, HAND, NO_CURSOR }
        #endregion



    }
}