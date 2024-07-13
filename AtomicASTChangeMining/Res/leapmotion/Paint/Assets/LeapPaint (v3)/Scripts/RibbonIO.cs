﻿using UnityEngine;
using System.Collections.Generic;
using System;
using Leap.zzOldPaint;
using Leap.Unity.Old_FlowRoutines_LeapPaintv3;

namespace Leap.Unity.LeapPaint_v3 {


  public class RibbonIO : MonoBehaviour {

    public HistoryManager _historyManager;
    public FileDisplayer _fileDisplayer;
    public FileManager _fileManager;

    public PinchStrokeProcessor _replayProcessor;

    private bool _isLoading = false;
    public bool IsLoading {
      get { return _isLoading; }
    }

    public Action<string> OnSaveSuccessful = (x) => { };

    private string GetHistoryAsJSON(out int strokeNumber) {
      Strokes strokes = new Strokes(_historyManager.GetStrokes());
      strokeNumber = strokes.strokes.Count;
      return JsonUtility.ToJson(strokes);
    }

    public void Save() {
      int strokeNumber = 0;
      string json = GetHistoryAsJSON(out strokeNumber);
      if(strokeNumber < 1) { Debug.LogWarning("Aborting Save: No Strokes in Scene!", this); return; }
      string fileName = _fileManager.fileNamePrefix + 
                          GetHighestFileNumber(_fileManager.fileNamePrefix);
      _fileManager.Save(fileName + ".json", json);
      _fileManager.Save(fileName + ".ply", PlyExporter.MakePly(_replayProcessor._ribbonParentObject));
      //_fileManager.Save(fileName + ".obj", ObjExporter.makeObj(true, "LeapPaintMesh", _replayProcessor._ribbonParentObject).ToString());
      Debug.Log("Created JSON for " + strokeNumber + " strokes.");
      OnSaveSuccessful.Invoke(fileName);
    }

    public void LoadViaDisplayerSelected() {
      string fileName = _fileDisplayer.GetSelectedFilename();
      string strokesJSON = _fileManager.Load(fileName);
      Strokes strokes = JsonUtility.FromJson<Strokes>(strokesJSON);
      Debug.Log("Loaded JSON for " + strokes.strokes.Count + " strokes.");

      _historyManager.ClearAll();

      for (int i = 0; i < strokes.strokes.Count; i++) {
        List<StrokePoint> stroke = strokes.strokes[i].strokePoints;
        _replayProcessor.ShortcircuitStrokeToRenderer(stroke);
      }
    }

    public void LoadAsync() {
      if (!_isLoading) {
        _isLoading = true;
        FlowRunner.StartNew(AsyncLoadViaDisplayerSelected());
      }
    }

    private IEnumerator<Flow> AsyncLoadViaDisplayerSelected() {
      string fileName = _fileDisplayer.GetSelectedFilename();

      yield return Flow.IntoNewThread();

      string strokesJSON = _fileManager.Load(fileName);
      Strokes strokes = JsonUtility.FromJson<Strokes>(strokesJSON);
      Debug.Log("Loaded JSON for " + strokes.strokes.Count + " strokes.");

      yield return Flow.IntoUpdate();

      _historyManager.ClearAll();

      for (int i = 0; i < strokes.strokes.Count; i++) {
        List<StrokePoint> stroke = strokes.strokes[i].strokePoints;
        _replayProcessor.ShortcircuitStrokeToRenderer(stroke);
        //yield return Flow.ForFrames(8); // one per 8 frames is a good "splash screen" speed
        yield return Flow.IfElapsed(2); // this is a good quick-load speed
      }

      _isLoading = false;
    }

    private int GetHighestFileNumber(string prefix = "Paint - ") {
      int highestNumber = 0;
      string[] files = _fileManager.GetFiles();
      foreach (string file in files) {
        string fileName = _fileManager.NameFromPath(file);
        highestNumber = Mathf.Max(highestNumber, _fileManager.TryGetNumberFromName(fileName, prefix));
      }
      return ++highestNumber;
    }

  }


}
