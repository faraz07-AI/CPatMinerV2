﻿using UnityEngine;
using System.Collections;
using System;


namespace Leap.Unity.LeapPaint_v3 {

  public class MusicPlayer : MonoBehaviour {

    public AudioSource _musicSource;
    public AudioClip _music;

    private bool _shouldPlay = true;

    protected void Start() {
      string[] cmdLineArgs = Environment.GetCommandLineArgs();

      for (int i = 0; i < cmdLineArgs.Length; i++) {
			  if (cmdLineArgs[i].Equals("--noMusic")) {
          _shouldPlay = false;
        }
		  }

      if (_shouldPlay && !Application.isEditor) {
        StartMusic();
      }
    }

    public void StartMusic() {
      if (!_musicSource.isPlaying) {
        _musicSource.clip = _music;
        _musicSource.loop = true;
        _musicSource.Play();
      }
    }

    public void StopMusic() {
      if (_musicSource.isPlaying) {
        _musicSource.Stop();
      }
    }

    public void ToggleMusic() {
      if (_musicSource.isPlaying) {
        StopMusic();
      }
      else {
        StartMusic();
      }
    }

  }

}
