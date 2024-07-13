using UnityEngine;
using UnityEngine.SceneManagement;

using System;
using System.Collections;
using System.Collections.Generic;

namespace Calcflow.UserStatistics
{

    public class ApplicationTracking : MonoBehaviour
    {

        void Awake()
        {
            StatisticsTracking.StartEvent("Application", "Calcflow", null, false);
            SceneManager.activeSceneChanged += SceneChanged;
        }

        void SceneChanged(Scene previousScene, Scene nextScene)
        {
            StatisticsTracking.EndAllStartedEvents();
            StatisticsTracking.StartEvent("Scene", nextScene.name);
        }

        void OnApplicationQuit()
        {
            StatisticsTracking.EndAllStartedEvents();
            StatisticsTracking.EndEvent("Application", "Calcflow");
            StatisticsTracking.StopTracking();
        }

        public void DoRequest(string url, string data)
        {
            StartCoroutine(RequestCoroutine(url, data));
        }

        IEnumerator RequestCoroutine(string url, string data)
        {
            using (WWW www = new WWW(url + data))
            {
                yield return www;
            }
            yield return null;
        }

    }

}
