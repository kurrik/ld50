using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PlayerCamera : MonoBehaviour {
  private Player _player;
  private Camera _camera;
  private Vector3 _targetPosition;
  private Vector3 _startPosition;
  private float _zoomLevel;
  private Vector3 _velocity;

  public Vector3 playerOffset = new Vector3(0.0f, 128.0f, -90.0f);
  public float cameraSpeed = 0.1f;

  public void Init(Player player) {
    _player = player;
    _camera = GetComponent<Camera>();
    _zoomLevel = 1.0f;
    UpdateTargetPosition();
  }

  public void Reset() {
    _player = null;
    transform.position = _startPosition;
    _targetPosition = _startPosition;
    _zoomLevel = 1.0f;
  }

  void Start() {
    _startPosition = transform.position;
  }

  void Update() {
    //Debug.LogFormat("Going from {0} to {1}", transform.position, _targetPosition);
    transform.position = Vector3.SmoothDamp(transform.position, _targetPosition, ref _velocity, cameraSpeed);
  }

  void FixedUpdate() {
    UpdateTargetPosition();
  }

  private void UpdateTargetPosition() {
    if (_player) {
      _zoomLevel = Mathf.Clamp(_player.amountMoved / _player.playerMaxAmountMoved, 0.0f, 1.0f) + 0.5f;
      // Debug.LogFormat("ZoomLevel {0}", _zoomLevel);
      _targetPosition = _player.transform.position + playerOffset * _zoomLevel;
    }
  }
}
