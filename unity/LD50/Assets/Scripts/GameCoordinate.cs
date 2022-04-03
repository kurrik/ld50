using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum GameCoordinateType {
  Empty,
  SpreadAlpha,
  SpreadBeta,
  Objective,
  Blockage,
  TemporaryBlockage,
  DamagedTemporaryBlockage,
}

[RequireComponent(typeof(MeshRenderer))]
public class GameCoordinate : Coordinate {

  private MeshRenderer meshRenderer;
  public GameCoordinateType type;
  public GameCoordinateType? nextType;
  private int tickCount;
  private bool takeDamageOnTick;
  private int hitPoints;
  private int rotationSteps;

  // Initializes the Coordinate given a cube coordinate and world transform position
  public void Init(Vector3 cube, Vector3 position, GameCubeCoordinates coords) {
    base.Init(cube, position);
    meshRenderer = gameObject.GetComponent<MeshRenderer>();
    float randomTypeValue = Random.Range(0.0f, 1.0f);
    if (isObjectivePosition(cube, coords)) {
      SetType(GameCoordinateType.Objective);
    } else if (isStarterCubePosition(cube, coords.radius)) {
      if (Random.Range(0.0f, 1.0f) < 0.5f) {
        SetType(GameCoordinateType.SpreadAlpha);
      } else {
        SetType(GameCoordinateType.SpreadBeta);
      }
    } else if (randomTypeValue < 0.2f) {
      SetType(GameCoordinateType.Blockage);
    } else {
      SetType(GameCoordinateType.Empty);
    }
  }

  private bool isObjectivePosition(Vector3 cube, GameCubeCoordinates coords) {
    Vector3 origin = Vector3.zero;
    if (coords.GetDistanceBetweenTwoCubes(cube, origin) <= Gameboard.instance.objectiveRadius) {
      return true;
    }
    return false;
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
    tickCount += 1;
    switch (type) {
      case GameCoordinateType.SpreadAlpha:
        SpreadAlpha(cube, coords);
        break;
      case GameCoordinateType.SpreadBeta:
        SpreadBeta(cube, coords);
        break;
      default:
        break;
    }
  }

  private Vector3[] _spreadAlphaPattern =
  {
        // q, s, r
        new Vector3(0.0f, 1.0f, -1.0f),
        new Vector3(-1.0f, 0.0f, 1.0f),
        new Vector3(1.0f, -1.0f, 0.0f),
    };
  public void SpreadAlpha(Vector3 cube, GameCubeCoordinates coords) {
    Spread(cube, _spreadAlphaPattern, coords, GameCoordinateType.SpreadAlpha, 0);
  }

  private Vector3[] _spreadBetaPattern =
  {
        // q, s, r
        new Vector3(0.0f, 1.0f, -1.0f),
    };
  public void SpreadBeta(Vector3 cube, GameCubeCoordinates coords) {
    rotationSteps = Random.Range(-3, 2);
    Spread(cube, _spreadBetaPattern, coords, GameCoordinateType.SpreadBeta, rotationSteps);
  }

  private void Spread(Vector3 cube, Vector3[] pattern, GameCubeCoordinates coords, GameCoordinateType spreadType, int rotate) {
    foreach (Vector3 offset in pattern) {
      Vector3 adjustedOffset = offset;
      if (rotate > 0) {
        for (int i = 0; i < rotate; i++) {
          adjustedOffset = coords.RotateCubeCoordinatesRight(adjustedOffset);
        }
      } else if (rotate < 0) {
        for (int i = 0; i < rotate; i++) {
          adjustedOffset = coords.RotateCubeCoordinatesLeft(adjustedOffset);
        }
      }
      GameCoordinate target = coords.GetCoordinateFromContainer(cube + adjustedOffset, "all");
      if (!target) {
        continue;
      }
      switch (target.type) {
        case GameCoordinateType.Empty:
          target.nextType = spreadType;
          break;
        case GameCoordinateType.DamagedTemporaryBlockage:
        case GameCoordinateType.TemporaryBlockage:
          target.takeDamageOnTick = true;
          break;
      }
    }
  }

  public void ApplyTick() {
    if (nextType != null) {
      SetType((GameCoordinateType)nextType);
    } else if (takeDamageOnTick) {
      hitPoints -= 1;
      if (type == GameCoordinateType.TemporaryBlockage) {
        SetType(GameCoordinateType.DamagedTemporaryBlockage);
      }
      if (hitPoints <= 0) {
        switch (type) {
          case GameCoordinateType.DamagedTemporaryBlockage:
            SetType(GameCoordinateType.Empty);
            break;
          case GameCoordinateType.SpreadBeta:
            SetType(GameCoordinateType.Empty);
            break;
        }
      }
    }
    switch (type) {
      case GameCoordinateType.SpreadBeta:
        takeDamageOnTick = true;
        break;
      default:
        takeDamageOnTick = false;
        break;
    }
  }

  public bool ClearRadius(Vector3 cube, GameCubeCoordinates coords, int radius) {
    bool placed = false;
    List<Vector3> reachableCubes = coords.GetReachableCubes(cube, radius);
    reachableCubes.Add(cube);
    foreach (Vector3 neighborCube in reachableCubes) {
      GameCoordinate neighbor = coords.GetCoordinateFromContainer(neighborCube, "all");
      if (neighbor.type == GameCoordinateType.SpreadAlpha) {
        neighbor.SetType(GameCoordinateType.TemporaryBlockage);
        placed = true;
      } else if (neighbor.type == GameCoordinateType.SpreadBeta) {
        neighbor.SetType(GameCoordinateType.TemporaryBlockage);
        placed = true;
      } else if (neighbor.type == GameCoordinateType.DamagedTemporaryBlockage) {
        neighbor.SetType(GameCoordinateType.TemporaryBlockage);
        placed = true;
      }
    }
    return placed;
  }

  public bool SetTemporaryBlockage(Vector3 cube, GameCubeCoordinates coords, int radius) {
    bool placed = false;
    List<Vector3> reachableCubes = coords.GetReachableCubes(cube, radius);
    reachableCubes.Add(cube);
    foreach (Vector3 neighborCube in reachableCubes) {
      GameCoordinate neighbor = coords.GetCoordinateFromContainer(neighborCube, "all");
      if (neighbor.type == GameCoordinateType.Empty) {
        neighbor.SetType(GameCoordinateType.TemporaryBlockage);
        placed = true;
      }
    }
    return placed;
  }

  private void SetType(GameCoordinateType newType) {
    bool visible = true;
    float elevation = 0.0f;
    if (newType == type) {
      return;
    }
    switch (newType) {
      case GameCoordinateType.SpreadAlpha:
        meshRenderer.material = Gameboard.instance.SpreadAlphaMaterial;
        break;
      case GameCoordinateType.SpreadBeta:
        meshRenderer.material = Gameboard.instance.SpreadBetaMaterial;
        hitPoints = Gameboard.instance.spreadBetaHitPoints;
        break;
      case GameCoordinateType.Empty:
        meshRenderer.material = Gameboard.instance.EmptyMaterial;
        break;
      case GameCoordinateType.Blockage:
        //meshRenderer.material.color = Random.ColorHSV();
        meshRenderer.material = Gameboard.instance.BlockageMaterial;
        elevation = 1.0f;
        break;
      case GameCoordinateType.Objective:
        meshRenderer.material = Gameboard.instance.ObjectiveMaterial;
        elevation = 2.0f;
        break;
      case GameCoordinateType.TemporaryBlockage:
        meshRenderer.material = Gameboard.instance.TemporaryBlockageMaterial;
        elevation = 1.0f;
        hitPoints = Gameboard.instance.temporaryBlockageHitPoints;
        break;
      case GameCoordinateType.DamagedTemporaryBlockage:
        meshRenderer.material = Gameboard.instance.DamagedBlockageMaterial;
        elevation = 0.5f;
        break;
      default:
        // Uh oh?
        break;
    }
    type = newType;
    tickCount = 0;
    nextType = null;
    if (visible) {
      Show();
    } else {
      Hide();
    }
    if (elevation != transform.position.y) {
      Vector3 newPosition = transform.position;
      newPosition.y = elevation;
      transform.position = newPosition;
    }
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

