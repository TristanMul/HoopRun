#pragma warning disable 0649 

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using System;
using Watermelon.Core;

namespace Watermelon
{
    //Current version v1.0.2
    [DefaultExecutionOrder(5)]
    public class SceneLoader : MonoBehaviour
    {
        private static SceneLoader instance;

        [ValidateInput("ValidateFirstScene")]
        [Scenes]
        [SerializeField]
        private string firstScene;
        
        private static string currentScene;
        private string prevScene;

        public static string CurrentScene
        {
            get { return currentScene; }
#if UNITY_EDITOR
            set { currentScene = value; }
#endif
        }

        public static string PrevScene
        {
            get { return instance.prevScene; }
        }

        [SerializeField]
        private Image fadePanel;
        
        private List<SceneEvent> sceneOpenEvents = new List<SceneEvent>();
        private List<SceneEvent> sceneLeaveEvents = new List<SceneEvent>();
        
        public delegate void SceneLoaderCallback();

        private SceneLoaderCallback onSceneChanged;
        public static SceneLoaderCallback OnSceneChanged
        {
            get { return instance.onSceneChanged; }
            set { instance.onSceneChanged = value; }
        }

        private void Awake()
        {
            instance = this;

            DontDestroyOnLoad(gameObject);

            SceneManager.activeSceneChanged += OnActiveSceneChanged;
        }

        private void Start()
        {
#if !UNITY_EDITOR
            LoadScene(firstScene);
#else
            if (SceneManager.sceneCount == 1)
                LoadScene(firstScene);
#endif
        }

        public static void OverrideFirstScene(string scene)
        {
            instance.firstScene = scene;
        }

        public static void OnSceneOpened(string scene, SceneCallback callback, bool callOnce = false)
        {
            instance.sceneOpenEvents.Add(new SceneEvent(scene, callback, callOnce));
        }

        public static void OnSceneLeave(string scene, SceneCallback callback, bool callOnce = false)
        {
            instance.sceneLeaveEvents.Add(new SceneEvent(scene, callback, callOnce));
        }

        private void OnActiveSceneChanged(Scene prevScene, Scene currentScene)
        {
            int eventsCount = sceneOpenEvents.Count;
            for (int i = eventsCount - 1; i >= 0; i--)
            {
                if (sceneOpenEvents[i].scene == currentScene.name)
                {
                    sceneOpenEvents[i].callback.Invoke();

                    if (sceneOpenEvents[i].callOnce)
                        sceneOpenEvents.RemoveAt(i);
                }
            }
        }

        public static void ReloadScene(SceneTransition transition = SceneTransition.Fade)
        {
            LoadScene(currentScene, transition);
        }

        public static void LoadScene(string sceneName, SceneTransition transition = SceneTransition.Fade)
        {
            if (Application.CanStreamedLevelBeLoaded(sceneName))
            {
                string currentSceneName = currentScene;

                int eventsCount = instance.sceneLeaveEvents.Count;
                for (int i = eventsCount - 1; i >= 0; i--)
                {
                    if (instance.sceneLeaveEvents[i].scene == currentSceneName)
                    {
                        instance.sceneLeaveEvents[i].callback.Invoke();

                        if (instance.sceneLeaveEvents[i].callOnce)
                            instance.sceneLeaveEvents.RemoveAt(i);
                    }
                }

                Debug.Log("[SceneLoader]: Loading scene: " + sceneName);

                if (transition == SceneTransition.Fade)
                {
                    FadePanel(delegate
                    {
                        Tween.RemoveAll();

                        if (instance.onSceneChanged != null)
                        {
                            instance.onSceneChanged();
                        }

                        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
                    });
                }
                else
                {
                    if (instance.onSceneChanged != null)
                    {
                        instance.onSceneChanged();
                    }

                    SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
                }

                instance.prevScene = currentScene;
                currentScene = sceneName;
            }
            else
            {
                Debug.LogError("[SceneLoader]: Scene " + sceneName + " can't be found!");
            }
        }
        
        public static void HideFadePanel()
        {
            instance.fadePanel.color.SetAlpha(0);
            instance.fadePanel.raycastTarget = false;
            instance.fadePanel.gameObject.SetActive(false);
        }

        public static void FadePanel(Tween.TweenCallback callback)
        {
            instance.fadePanel.raycastTarget = true;
            instance.fadePanel.color.SetAlpha(0);
            instance.fadePanel.gameObject.SetActive(true);
            instance.fadePanel.DOFade(1, 0.5f, true).OnComplete(delegate
            {
                callback.Invoke();

                instance.fadePanel.DOFade(0, 0.5f, true).OnComplete(delegate
                {
                    instance.fadePanel.color.SetAlpha(0);
                    instance.fadePanel.raycastTarget = false;
                    instance.fadePanel.gameObject.SetActive(false);
                });
            });
        }
                
        public ValidatorAttribute.ValidateResult ValidateFirstScene(string scene)
        {
            if(string.IsNullOrEmpty(scene))
            {
                return new ValidatorAttribute.ValidateResult(ValidatorAttribute.ValidateType.Error, "Plese set scene!");
            }

            switch(scene)
            {
                case "Init":
                    return new ValidatorAttribute.ValidateResult(ValidatorAttribute.ValidateType.Error, "First scene can't be Init!");
                case "Unknown":
                    return new ValidatorAttribute.ValidateResult(ValidatorAttribute.ValidateType.Error, "First scene can't be Unknown!");
            }

            return new ValidatorAttribute.ValidateResult(ValidatorAttribute.ValidateType.Success, "Scene Loader is ready!");
        }

        public delegate void SceneCallback();

        public enum SceneTransition
        {
            None,
            Fade,
        }

        private class SceneEvent
        {
            public string scene;
            public SceneCallback callback;

            public bool callOnce;

            public SceneEvent(string scene, SceneCallback callback, bool callOnce = false)
            {
                this.scene = scene;
                this.callback = callback;
                this.callOnce = callOnce;
            }
        }
    }
}

//Changelog
//v1.0.0 - Base version
//v1.0.1 - Custom events, transition
//v1.0.2 - Fixed scene opening