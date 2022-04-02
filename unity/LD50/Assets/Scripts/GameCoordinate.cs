using UnityEngine;

public enum GameCoordinateType {
  Empty,
  Basic,
  Starter,
}

[RequireComponent(typeof(MeshRenderer))]
public class GameCoordinate : Coordinate {

  private MeshRenderer meshRenderer;
  public GameCoordinateType type;
  public GameCoordinateType? nextType;

  // Initializes the Coordinate given a cube coordinate and world transform position
  public override void Init(Vector3 cube, Vector3 position, int radius) {
    base.Init(cube, position, radius);
    meshRenderer = gameObject.GetComponent<MeshRenderer>();
    float randomTypeValue = Random.Range(0.0f, 1.0f);
    if (isStarterCubePosition(cube, radius)) {
      SetType(GameCoordinateType.Starter);
    } else if (randomTypeValue < 0.2f) {
      SetType(GameCoordinateType.Basic);
    } else {
      SetType(GameCoordinateType.Empty);
    }
  }

  private bool isStarterCubePosition(Vector3 cube, int radius) {
    float x = Mathf.Abs(cube.x);
    float y = Mathf.Abs(cube.y);
    float z = Mathf.Abs(cube.z);
    if (x == radius && y == radius && z == 0) {
      return true;
    } else if (x == radius && y == 0 && z == radius) {
      return true;
    } else if (x == 0 && y == radius && z == radius) {
      return true;
    } else {
      return false;
    }
  }

  public void Tick(Vector3 cube, GameCubeCoordinates coords) {
    switch (type) {
      case GameCoordinateType.Starter:
        AffectPatternA(cube, coords);
        break;
      default:
        break;
    }
  }

  private Vector3[] _patternA =
  {
        // q, s, r
        new Vector3(0.0f, 1.0f, -1.0f),
        new Vector3(-1.0f, 0.0f, 1.0f),
        new Vector3(1.0f, -1.0f, 0.0f),
    };
  public void AffectPatternA(Vector3 cube, GameCubeCoordinates coords) {
    foreach (Vector3 offset in _patternA) {
      GameCoordinate neighbor = coords.GetCoordinateFromContainer(cube + offset, "all");
      if (neighbor && neighbor.type == GameCoordinateType.Empty) {
        neighbor.nextType = GameCoordinateType.Starter;
      }
    }
  }

  public void AffectNeighbors(Vector3 cube, GameCubeCoordinates coords) {
    foreach (Vector3 neighborCube in coords.GetNeighborCubes(cube)) {
      GameCoordinate neighbor = coords.GetCoordinateFromContainer(neighborCube, "all");
      if (neighbor.type == GameCoordinateType.Empty) {
        neighbor.nextType = GameCoordinateType.Starter;
      }
    }
  }

  public void ApplyTick() {
    if (nextType != null) {
      SetType((GameCoordinateType)nextType);
    }
  }

  public void ClearRadius(Vector3 cube, GameCubeCoordinates coords, int radius) {
    foreach (Vector3 neighborCube in coords.GetReachableCubes(cube, radius)) {
      GameCoordinate neighbor = coords.GetCoordinateFromContainer(neighborCube, "all");
      neighbor.SetType(GameCoordinateType.Empty);
    }
    SetType(GameCoordinateType.Empty);
  }

  private void SetType(GameCoordinateType newType) {
    switch (newType) {
      case GameCoordinateType.Starter:
        meshRenderer.material.color = Color.red;
        Show();
        break;
      case GameCoordinateType.Empty:
        Show();
        meshRenderer.material.color = Color.gray;
        break;
      case GameCoordinateType.Basic:
        meshRenderer.material.color = Random.ColorHSV();
        Show();
        break;
      default:
        // Uh oh?
        break;
    }
    type = newType;
    nextType = null;
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

