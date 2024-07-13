﻿using UnityEngine;
using System.Collections;
using Leap.Unity;

namespace Leap.Unity.LeapPaint_v3 {


  public class IndexTipColor : MonoBehaviour {

    private Vector3 _lastPos;

    public Renderer _tipMeshRenderer;
    public Color _startingColor = Color.white;
    public PaintCursor _cursor;

    [Header("Color Marble")]
    public Material _colorMarbleMaterial;
    [Tooltip("The material to use for the color marble when the index tip color is transparent.")]
    public Material _transparentMarbleMaterial;
    public Renderer _colorMarbleRenderer;

    [Header("SFX")]
    public SoundEffect _dipEffect;

    [Header("Color Palette Receiving State Feedback")]
    public ColorPalette handPalette;
    public ColorWearableUI colorWearableUI;

    private float _canPlayDipTime;

    private Color _paintColor;

    public bool IsClean {
      get { return GetColor().a < 0.001F; }
    }

    protected void Start() {
      this.SetColor(_startingColor);
    }

    protected void Update() {
      _tipMeshRenderer.material.color = new Color(_paintColor.r, _paintColor.g, _paintColor.b, _cursor.GetHandAlpha() * _paintColor.a);

      _velocity = this.transform.position.From(_lastPos) / Time.deltaTime;
      _lastPos = this.transform.position;

      if (handPalette != null && colorWearableUI != null) {
        if (_paintColor.a > 0.01F) {
          // Color palette should have "receiving" icon -- because in workstation mode
          // while the index tip has a color, the hand palette receives color from
          // the index tip rather than assigning it.
          if (colorWearableUI.IsWorkstation) {
            handPalette.SetSwatchMode(ColorSwatch.SwatchMode.ReceiveColor);
          }
        }
        else {
          // Color palette should reflect "assigning".
          handPalette.SetSwatchMode(ColorSwatch.SwatchMode.AssignColor);
        }
      }
    }

    public Color GetColor() {
      return _paintColor;
    }

    public void SetColor(Color color) {
      _paintColor = color;
      _tipMeshRenderer.material.color = new Color(color.r, color.g, color.b, _cursor.GetHandAlpha() * _paintColor.a);

      if (_paintColor.a < 0.01F) {
        // "Cleaned" finger of paint color.
        _colorMarbleRenderer.material = _transparentMarbleMaterial;
      }
      else {
        // Got a new paint color.
        _colorMarbleRenderer.material = _colorMarbleMaterial;
        _colorMarbleMaterial.color = color;
      }
    }

    #region Mixing Paint Colors

    private Vector3 _velocity = Vector3.zero;
    public Vector3 velocity {
      get {
        return _velocity;
      }
    }

    protected void OnTriggerStay(Collider other) {
      var rigidbody = other.attachedRigidbody;
      if (rigidbody == null) return;

      ColorMixingBasin mixingLiquid = rigidbody.GetComponent<ColorMixingBasin>();
      if (mixingLiquid != null && mixingLiquid.enabled) {
        if (IsClean) {
          this.SetColor(mixingLiquid.GetColor());
        }
        else {
          float handSpeed = velocity.magnitude;
          if (handSpeed < 0.1F) handSpeed = 0F;
          this.SetColor(mixingLiquid.MixWithIndexTipColor(this, handSpeed));
        }
      }
      ColorCleaningBasin cleaningLiquid = other.GetComponentInParent<ColorCleaningBasin>();
      if (cleaningLiquid != null && cleaningLiquid.enabled) {
        if(Time.time > _canPlayDipTime) {
          _dipEffect.PlayAtPosition(transform);
        }
        _canPlayDipTime = Time.time + 0.5f;
      
        this.SetColor(new Color(0F, 0F, 0F, 0F));
      }
    }
    public void DoOnTriggerStay(Collider other) {
      // Callable as an event hook in case the collider/rigidbody getting the OnTriggerStay event is on a different object.
      this.OnTriggerStay(other);
    }

    #endregion

  }


}
