﻿using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Leap.Unity.Animation;
using Leap.Unity.LeapPaint_v3;
using UnityEngine.Rendering.PostProcessing;

public class LobbyControl : MonoBehaviour {
  private const string HAS_EXPERIENCE_TUTORIAL_FILENAME = "HasExperiencedTutorial.txt";
  private const int FALSE_VALUE = 0;
  private const int TRUE_VALUE = 1;

  public static LobbySelectionState selectionState = LobbySelectionState.None;

  public static bool hasExperiencedTutorial {
    get {
      return File.Exists(Path.Combine(Application.streamingAssetsPath, HAS_EXPERIENCE_TUTORIAL_FILENAME));
    }
    set {
#if UNITY_EDITOR
      if (value) {
        Debug.LogWarning("Will not set the tutorial-experienced flag since we are in the editor.");
        return;
      }
#endif

      string path = Path.Combine(Application.streamingAssetsPath, HAS_EXPERIENCE_TUTORIAL_FILENAME);
      if (value) {
        Directory.CreateDirectory(Application.streamingAssetsPath);
        File.WriteAllText(path, "");
      } else {
        File.Delete(path);
      }
    }
  }

  public bool forceLobbyExperience;
  public string sceneToLoad;
  public PressableUI tutorialButton;
  public PressableUI sandboxButton;
  public float appearDelay = 0.5f;
  public float transitionTime;
  public AnimationCurve transitionCurve;

  [Header("Transition Settings")]
  public float fadeOutTime;
  public float fadeInTime;

  private Tween _buttonTween;

  void OnEnable() {
    if (!hasExperiencedTutorial && !forceLobbyExperience) {
      transitionWithoutButtons();
      return;
    }

    Tween.AfterDelay(appearDelay, () => {
      _buttonTween = Tween.Persistent().
                     Target(tutorialButton.transform).LocalScale(Vector3.zero, tutorialButton.transform.localScale).
                     Target(sandboxButton.transform).LocalScale(Vector3.zero, sandboxButton.transform.localScale).
                     Smooth(transitionCurve).
                     OverTime(transitionTime).
                     OnReachEnd(() => {
                       tutorialButton.enabled = true;
                       sandboxButton.enabled = true;
                     });

      _buttonTween.Play();
    });


  }

  public void OnSelectTutorial() {
    selectionState = LobbySelectionState.Tutorial;
    StartCoroutine(transitionMinimizeButtons());
  }

  public void OnSelectSandbox() {
    selectionState = LobbySelectionState.Sandbox;
    StartCoroutine(transitionMinimizeButtons());
  }

  private IEnumerator transitionMinimizeButtons() {
    var asyncOp = SceneManager.LoadSceneAsync(sceneToLoad);
    asyncOp.allowSceneActivation = false;

    tutorialButton.enabled = false;
    sandboxButton.enabled = false;

    PostProcessVolume volume = FindObjectOfType<PostProcessVolume>();
    var fadeTween = Tween.Persistent().
                          Value(0, 1, v => volume.weight = v).
                          OverTime(fadeInTime).
                          Play();

    _buttonTween.Play(Direction.Backward);
    yield return new WaitWhile(() => _buttonTween.isRunning);
    yield return new WaitWhile(() => fadeTween.isRunning);

    _buttonTween.Release();
    fadeTween.Release();

    asyncOp.allowSceneActivation = true;
  }

  private void transitionWithoutButtons() {
    var asyncOp = SceneManager.LoadSceneAsync(sceneToLoad);
    asyncOp.allowSceneActivation = true;
    _buttonTween.Release();
  }

  public enum LobbySelectionState {
    None,
    Tutorial,
    Sandbox
  }
}
