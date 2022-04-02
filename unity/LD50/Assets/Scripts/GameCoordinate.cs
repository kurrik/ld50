using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class GameCoordinate : Coordinate {

  private MeshRenderer meshRenderer;

  // Initializes the Coordinate given a cube coordinate and world transform position
  public override void Init(Vector3 cube, Vector3 position) {
    base.Init(cube, position);
    meshRenderer = gameObject.GetComponent<MeshRenderer>();
    meshRenderer.material.color = Random.ColorHSV();
    Hide();
  }

  // Hides the Coordinate
  public override void Hide() {
    meshRenderer.enabled = false;
  }

  // Shows the Coordinate
  public override void Show(bool bCollider = true) {
    meshRenderer.enabled = true;
  }
}

