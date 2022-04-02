using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Coordinate : MonoBehaviour {
  virtual public void Init(Vector3 cube, Vector3 position) {
    this._cube = cube;
    this._position = position;
    gameObject.transform.position = _position;
  }

  abstract public void Hide();
  abstract public void Show(bool bCollider = true);

  public float gCost = 0.0f;
  public float hCost = 0.0f;
  public float fCost {
    get { return gCost + hCost; }
  }

  private Vector3 _position = Vector3.zero;
  public Vector3 position { get { return _position; } }

  private Vector3 _cube = Vector3.zero;
  public Vector3 cube { get { return _cube; } }
}