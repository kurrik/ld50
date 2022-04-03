using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PlayerCamera : MonoBehaviour {
  private Player _player;
  private Camera _camera;
  private Vector3 _targetPosition;
  private Vector3 _startPosition;
  private Quaternion _startRotation;
  private float _zoomLevel;
  private Vector3 _velocity;
  private float _shakeElapsed;
  private bool _isShaking;
  private bool _isInitializing;

  public Vector3 playerOffset = new Vector3(0.0f, 128.0f, -90.0f);
  public float cameraSpeed = 0.1f;
  public float shakeDuration = 0.25f;
  public float shakeMagnitude = 0.02f;
  public float shakeFrequency = 4.0f;

  public void Init(Player player) {
    _isShaking = false;
    _isInitializing = true;
    _player = player;
    _camera = GetComponent<Camera>();
    _zoomLevel = 1.0f;
    UpdateTargetPosition();
  }

  public void Reset() {
    _isShaking = false;
    _player = null;
    transform.position = _startPosition;
    _targetPosition = _startPosition;
    _zoomLevel = 1.0f;
  }

  public void TriggerShake() {
    _isShaking = true;
    _shakeElapsed = 0.0f;
  }

  void Start() {
    _startPosition = transform.position;
    _startRotation = transform.rotation;
  }

  void Update() {
    //Debug.LogFormat("Going from {0} to {1}", transform.position, _targetPosition);
    transform.position = Vector3.SmoothDamp(transform.position, _targetPosition, ref _velocity, cameraSpeed);
    if (_isShaking) {
      _shakeElapsed += Time.deltaTime;
      if (_shakeElapsed >= shakeDuration) {
        _shakeElapsed = 0.0f;
        _isShaking = false;
        transform.rotation = _startRotation;
      } else {
        float pct = _shakeElapsed / shakeDuration;
        //float magnitude = Mathf.SmoothStep(shakeMagnitude, 0.0f, pct);
        float amount = Mathf.Sin(pct * shakeFrequency * 2.0f * Mathf.PI) * shakeMagnitude;
        transform.Rotate(Vector3.right, amount, Space.Self);
      }
    }
  }

  void FixedUpdate() {
    UpdateTargetPosition();
  }

  private void UpdateTargetPosition() {
    if (_player) {
      _zoomLevel = Mathf.Clamp(_player.amountMoved / _player.playerMaxAmountMoved, 0.0f, 1.0f) + 0.5f;
      // Debug.LogFormat("ZoomLevel {0}", _zoomLevel);
      _targetPosition = _player.transform.position + playerOffset * _zoomLevel;

      if (_isInitializing) {
        transform.position = _targetPosition;
        _isInitializing = false;
      }
    }
  }
}
