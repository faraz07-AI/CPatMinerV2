﻿using UnityEngine;

namespace Leap.Unity.LeapPaint_v3 {

  /// <summary>
  /// Generates a toroid mesh with a low-resolution (square) minor cross-section.
  /// </summary>
  [RequireComponent(typeof(MeshFilter))]
  public class RectToroid : MonoBehaviour {

    [SerializeField]
    private float _radius = 1F;
    [SerializeField]
    private float _radialThickness = 0.2F;
    [SerializeField]
    private float _verticalThickness = 0.02F;
    [SerializeField]
    private bool _alwaysUpdate = false;

    public float Radius {
      get { return _radius; }
      set {
        _radius = value;
        _isDirty = true;
      }
    }
    public float RadialThickness {
      get { return _radialThickness; }
      set {
        _radialThickness = value;
        _isDirty = true;
      }
    }
    public float VerticalThickness {
      get { return _verticalThickness; }
      set {
        _verticalThickness = value;
        _isDirty = true;
      }
    }

    private const int NUM_DIVISIONS = 64;

    private Mesh _mesh;
    private MeshFilter _meshFilter;
    private Vector3[] _verts;
    private int[] _tris;
    private bool _isDirty;

    #if UNITY_EDITOR
    protected void OnValidate() {
      UnityEditor.EditorApplication.delayCall += initMesh;
    }
    #endif
    protected void Start() {
      initMesh();
    }
    protected void Update() {
      if (_isDirty || _alwaysUpdate) {
        refreshVerts();
      }
    }

    private void initMesh() {
      if (this == null) return; // in case we were destroyed.

      _meshFilter = GetComponent<MeshFilter>();

      if (_mesh == null) {
        _meshFilter.mesh = _mesh = new Mesh();
        _mesh.name = "Ribbon Circle Mesh";
        _mesh.MarkDynamic();
      }

      _mesh.Clear();
      _verts = new Vector3[NUM_DIVISIONS * 4]; // 4 verts per division (rectangular cross section)
      _tris = new int[NUM_DIVISIONS * 24];  // 8 tris per division * 3 entries per tri
      refreshVerts();
      refreshTris();
    }

    private void refreshVerts() {
      Vector3 r = Vector3.right;
      Vector3 h = Vector3.up;
      Quaternion rot = Quaternion.AngleAxis((360F / NUM_DIVISIONS), Vector3.up);
      for (int i = 0; i < NUM_DIVISIONS * 4; i += 4) {
        _verts[i + 0] = (r * _radius) + (r * _radialThickness) + (h * _verticalThickness);
        _verts[i + 1] = (r * _radius) - (r * _radialThickness) + (h * _verticalThickness);
        _verts[i + 2] = (r * _radius) + (r * _radialThickness) - (h * _verticalThickness);
        _verts[i + 3] = (r * _radius) - (r * _radialThickness) - (h * _verticalThickness);
        r = rot * r;
      }

      _mesh.vertices = _verts;
      _mesh.RecalculateBounds();
    }

    private void refreshTris() {
      int numVerts = _verts.Length;
      int trisIdx = 0;
      for (int i = 0; i < numVerts; i += 4) {
        // 1
        _tris[trisIdx++] = i;
        _tris[trisIdx++] = i + 1;
        _tris[trisIdx++] = ((i - 3) + numVerts) % numVerts;
      
        // 2
        _tris[trisIdx++] = i;
        _tris[trisIdx++] = (i + 4) % numVerts;
        _tris[trisIdx++] = i + 1;
      
        // 3
        _tris[trisIdx++] = i + 1;
        _tris[trisIdx++] = i + 3;
        _tris[trisIdx++] = ((i - 1) + numVerts) % numVerts;
      
        // 4
        _tris[trisIdx++] = i + 1;
        _tris[trisIdx++] = (i + 5) % numVerts;
        _tris[trisIdx++] = i + 3;
      
        // 5
        _tris[trisIdx++] = i + 3;
        _tris[trisIdx++] = i + 2;
        _tris[trisIdx++] = ((i - 2) + numVerts) % numVerts;
      
        // 6
        _tris[trisIdx++] = i + 2;
        _tris[trisIdx++] = i + 3;
        _tris[trisIdx++] = (i + 7) % numVerts;

        // 7
        _tris[trisIdx++] = i;
        _tris[trisIdx++] = ((i - 4) + numVerts) % numVerts;
        _tris[trisIdx++] = i + 2;

        // 8
        _tris[trisIdx++] = i;
        _tris[trisIdx++] = i + 2;
        _tris[trisIdx++] = (i + 6) % numVerts;
      }

      _mesh.triangles = _tris;
      _mesh.RecalculateNormals();
    }

  }


}
