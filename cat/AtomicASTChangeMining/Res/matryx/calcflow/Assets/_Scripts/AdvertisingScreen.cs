﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AdvertisingScreen : MonoBehaviour
{

    public int nextSceneIndex;

    void Awake()
    {
    }
    // Use this for initialization
    void Start()
    {
        Calcflow.UserStatistics.StatisticsTracking.Init();
        Invoke("LoadScene", 5.0f);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnLevelWasLoaded(int level)
    {
        //Invoke("LoadScene", 2.0f);
    }

    void LoadScene()
    {
        SceneManager.LoadScene(nextSceneIndex);
    }
}
