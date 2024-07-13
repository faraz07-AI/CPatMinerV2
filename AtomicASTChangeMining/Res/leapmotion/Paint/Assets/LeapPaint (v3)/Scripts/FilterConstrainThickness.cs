﻿using UnityEngine;
using Leap.Unity.RuntimeGizmos;
using System;

namespace Leap.Unity.LeapPaint_v3 {


  public class FilterConstrainThickness : IBufferFilter<StrokePoint> {

    public int GetMinimumBufferSize() {
      return 2;
    }

    public void Process(RingBuffer<StrokePoint> data, RingBuffer<int> indices) {
      float maxThickness = data.GetLatest().thickness;

      for (int i = 1; i < data.Count - 1; i++) {
        var prevStroke = data.Get(i - 1);
        var currStroke = data.Get(i);
        var nextStroke = data.Get(i + 1);

        float thickness = maxThickness;
        thickness = Mathf.Min(thickness, getMaxThickness(currStroke, prevStroke));
        thickness = Mathf.Min(thickness, getMaxThickness(currStroke, nextStroke));

        currStroke.thickness = thickness;
        data.Set(i, currStroke);
      }
    }

    private float getMaxThickness(StrokePoint point, StrokePoint relativeTo) {
      Plane plane = new Plane(relativeTo.rotation * Vector3.forward, relativeTo.position);

      Ray currRay = new Ray(point.position, point.rotation * Vector3.right);
      float dist = 0;
      plane.Raycast(currRay, out dist);

      return Mathf.Abs(dist);
    }

    public void Reset() { }
  }

  public class FilterSmoothThickness : IBufferFilter<StrokePoint> {
    public const int NEIGHBORHOOD = 16;

    public int GetMinimumBufferSize() {
      return NEIGHBORHOOD * 2 + 1;
    }

    public void Process(RingBuffer<StrokePoint> data, RingBuffer<int> indices) {
      float maxThickness = data.GetLatest().thickness;

      for (int i = 0; i < data.Count; i++) {
        var currStroke = data.Get(i);
        float thickness = currStroke.thickness;

        for (int j = -NEIGHBORHOOD; j <= NEIGHBORHOOD; j++) {
          int index = i + j;
          if (index < 0 || index >= data.Count) continue;

          float percent = Mathf.Abs(j) / (NEIGHBORHOOD + 1.0f);

          thickness = Mathf.Min(thickness, data.Get(index).thickness + percent * maxThickness);
        }

        currStroke.thickness = thickness;
        data.Set(i, currStroke);
      }
    }

    public void Reset() { }
  }


}