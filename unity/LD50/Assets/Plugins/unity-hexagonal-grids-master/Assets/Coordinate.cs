using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Coordinate : MonoBehaviour {
  virtual public void Init(Vector3 cube, Vector3 position, int radius) {
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

public class TestCoordinate : Coordinate {

  private GameObject _outline = null;
  private MeshRenderer _meshRenderer;
  private MeshCollider _meshCollider;

  // Initializes the Coordinate given a cube coordinate and world transform position
  public override void Init(Vector3 cube, Vector3 position, int radius) {
    base.Init(cube, position, radius);

    HexMeshCreator.Instance.AddToGameObject(this.gameObject, HexMeshCreator.Type.Tile, true);
    _meshRenderer = gameObject.GetComponent<MeshRenderer>();
    _meshCollider = gameObject.GetComponent<MeshCollider>();

    _outline = new GameObject("Outline");
    _outline.transform.parent = gameObject.transform;

    HexMeshCreator.Instance.AddToGameObject(_outline, HexMeshCreator.Type.Outline, false);

    _outline.transform.position = position;

    Hide();
  }

  // Hides the Coordinate
  public override void Hide() {
    _meshRenderer.enabled = false;
    _meshCollider.enabled = false;
  }

  // Shows the Coordinate
  public override void Show(bool bCollider = true) {
    _meshRenderer.enabled = true;

    if (bCollider)
      _meshCollider.enabled = true;
  }
}
