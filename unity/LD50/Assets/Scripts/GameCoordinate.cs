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
  SuperTemporaryBlockage,
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
  private int objectiveRemainingCount;
  private float objectiveStartingElevation;
  private int rotationSteps;
  private int pointsValue;

  // Initializes the Coordinate given a cube coordinate and world transform position
  public void Init(Vector3 cube, Vector3 position, GameCubeCoordinates coords) {
    base.Init(cube, position);
    meshRenderer = gameObject.GetComponent<MeshRenderer>();
    float randomTypeValue = Random.Range(0.0f, 1.0f);
    LevelInfo info = coords.info;
    CoordinateConfig? presetConfig = GetPresetCoordinateConfig(cube, coords);
    if (presetConfig != null) {
      CoordinateConfig cfg = (CoordinateConfig)presetConfig;
      SetType(cfg.Type, info);
    } else if (isObjectivePosition(cube, coords)) {
      SetType(GameCoordinateType.Objective, info);
    } else if (info.UseStarterPositions && isStarterCubePosition(cube, coords)) {
      if (Random.Range(0.0f, 1.0f) < 0.5f) {
        SetType(GameCoordinateType.SpreadAlpha, info);
      } else {
        SetType(GameCoordinateType.SpreadBeta, info);
      }
    } else if (randomTypeValue < info.BlockagePercent) {
      SetType(GameCoordinateType.Blockage, info);
    } else {
      SetType(GameCoordinateType.Empty, info);
    }
  }

  public bool IsEnemyPiece() {
    return type == GameCoordinateType.SpreadAlpha || type == GameCoordinateType.SpreadBeta;
  }

  private CoordinateConfig? GetPresetCoordinateConfig(Vector3 cube, GameCubeCoordinates coords) {
    CoordinateConfig[] configs = coords.info.CoordinateConfigs;
    foreach (CoordinateConfig config in configs) {
      if (config.Cube == cube) {
        return config;
      }
    }
    return null;
  }

  private bool isObjectivePosition(Vector3 cube, GameCubeCoordinates coords) {
    Vector3 origin = Vector3.zero;
    if (coords.GetDistanceBetweenTwoCubes(cube, origin) <= Gameboard.instance.objectiveRadius) {
      return true;
    }
    return false;
  }

  private bool isStarterCubePosition(Vector3 cube, GameCubeCoordinates coords) {
    int radius = coords.radius;
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
      case GameCoordinateType.Objective:
        if (tickCount >= coords.info.ObjectiveIntervalTicks) {
          tickCount = 0;
          DecayObjective(cube, coords);
        }
        break;
      default:
        break;
    }
  }

  private void DecayObjective(Vector3 cube, GameCubeCoordinates coords) {
    objectiveRemainingCount -= 1;
    Gameboard.instance.TriggerObjectiveIcon(transform.position);
    float pct = (float)objectiveRemainingCount / (float)coords.info.ObjectiveIntervalCount;
    Vector3 newPosition = transform.position;
    newPosition.y = pct * coords.info.objectiveStartingElevation;
    transform.position = newPosition;
    if (objectiveRemainingCount <= 0) {
      Gameboard.instance.TriggerWin();
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
    rotationSteps = Random.Range(-3, 3);
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
        for (int i = rotate; i < 0; i++) {
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
        case GameCoordinateType.Objective:
          target.takeDamageOnTick = true;
          break;
      }
    }
  }

  public void ApplyTick(GameCubeCoordinates coords) {
    if (nextType != null) {
      SetType((GameCoordinateType)nextType, coords.info);
    } else if (takeDamageOnTick) {
      hitPoints -= 1;
      switch (type) {
        case GameCoordinateType.TemporaryBlockage:
          SetType(GameCoordinateType.DamagedTemporaryBlockage, coords.info);
          break;
        case GameCoordinateType.Objective:
          Gameboard.instance.TriggerLoss();
          break;
      }
      if (hitPoints <= 0) {
        switch (type) {
          case GameCoordinateType.DamagedTemporaryBlockage:
            SetType(GameCoordinateType.Empty, coords.info);
            break;
          case GameCoordinateType.SpreadBeta:
            SetType(GameCoordinateType.Empty, coords.info);
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
    int score = 0;
    List<Vector3> reachableCubes = coords.GetReachableCubes(cube, radius);
    reachableCubes.Add(cube);
    foreach (Vector3 neighborCube in reachableCubes) {
      GameCoordinate neighbor = coords.GetCoordinateFromContainer(neighborCube, "all");
      if (!neighbor) {
        continue;
      }
      int distance = (int)coords.GetDistanceBetweenTwoCubes(cube, neighborCube);
      int neighborValue = neighbor.pointsValue;
      switch (neighbor.type) {
        case GameCoordinateType.SpreadAlpha:
        case GameCoordinateType.SpreadBeta:
          if (distance == radius) {
            neighbor.SetType(GameCoordinateType.TemporaryBlockage, coords.info);
          } else {
            neighbor.SetType(GameCoordinateType.Empty, coords.info);
          }
          placed = true;
          break;
        case GameCoordinateType.Empty:
          if (distance == radius) {
            neighbor.SetType(GameCoordinateType.TemporaryBlockage, coords.info);
            placed = true;
          }
          break;
        case GameCoordinateType.DamagedTemporaryBlockage:
          neighbor.SetType(GameCoordinateType.TemporaryBlockage, coords.info);
          placed = true;
          break;
      }
      if (placed) {
        score += neighborValue;
      }
    }
    Gameboard.instance.TriggerScoreIncrement(cube, score);
    return placed;
  }

  private Vector3[] _starCoords =
  {
        // q, s, r
        new Vector3(1.0f, -1.0f, 0.0f),
        new Vector3(-1.0f, 1.0f, 0.0f),
        new Vector3(0.0f, 1.0f, -1.0f),
        new Vector3(0.0f, -1.0f, 1.0f),
        new Vector3(1.0f, 0.0f, -1.0f),
        new Vector3(-1.0f, 0.0f, 1.0f),
    };
  public bool ClearStar(Vector3 cube, GameCubeCoordinates coords) {
    StartCoroutine(HandleClearStar(cube, coords));
    return true;
  }

  private IEnumerator HandleClearStar(Vector3 cube, GameCubeCoordinates coords) {
    bool placed = false;
    int score = 0;
    LevelInfo info = coords.info;
    float r = info.BoardSize;
    List<Vector3> cubes = new List<Vector3>() { cube };
    foreach (Vector3 target in _starCoords) {
      cubes.AddRange(coords.GetPathBetweenTwoCubes(cube, target * r));
      yield return new WaitForEndOfFrame();
    }
    cubes.AddRange(coords.GetSpiralCubes(cube, (int)r / 2));
    foreach (Vector3 neighborCube in cubes) {
      GameCoordinate neighbor = coords.GetCoordinateFromContainer(neighborCube, "all");
      if (!neighbor) {
        continue;
      }
      int neighborValue = neighbor.pointsValue;
      switch (neighbor.type) {
        case GameCoordinateType.SpreadAlpha:
        case GameCoordinateType.SpreadBeta:
          neighbor.SetType(GameCoordinateType.SuperTemporaryBlockage, coords.info);
          placed = true;
          break;
        case GameCoordinateType.Empty:
          neighbor.SetType(GameCoordinateType.SuperTemporaryBlockage, coords.info);
          placed = true;
          break;
        case GameCoordinateType.TemporaryBlockage:
          neighbor.SetType(GameCoordinateType.SuperTemporaryBlockage, coords.info);
          placed = true;
          break;
        case GameCoordinateType.DamagedTemporaryBlockage:
          neighbor.SetType(GameCoordinateType.SuperTemporaryBlockage, coords.info);
          placed = true;
          break;
      }
      if (placed) {
        score += neighborValue;
      }
    }
    Gameboard.instance.TriggerScoreIncrement(cube, score);
  }

  public bool SetTemporaryBlockage(Vector3 cube, GameCubeCoordinates coords, int radius) {
    bool placed = false;
    List<Vector3> reachableCubes = coords.GetReachableCubes(cube, radius);
    reachableCubes.Add(cube);
    foreach (Vector3 neighborCube in reachableCubes) {
      GameCoordinate neighbor = coords.GetCoordinateFromContainer(neighborCube, "all");
      if (neighbor.type == GameCoordinateType.Empty) {
        neighbor.SetType(GameCoordinateType.TemporaryBlockage, coords.info);
        placed = true;
      }
    }
    return placed;
  }

  private void SetType(GameCoordinateType newType, LevelInfo info) {
    bool visible = true;
    float elevation = 0.0f;
    if (newType == type) {
      return;
    }
    pointsValue = 0;
    switch (newType) {
      case GameCoordinateType.SpreadAlpha:
        meshRenderer.material = Gameboard.instance.SpreadAlphaMaterial;
        pointsValue = 2;
        break;
      case GameCoordinateType.SpreadBeta:
        meshRenderer.material = Gameboard.instance.SpreadBetaMaterial;
        hitPoints = Gameboard.instance.spreadBetaHitPoints;
        pointsValue = 5;
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
        elevation = info.objectiveStartingElevation;
        objectiveRemainingCount = info.ObjectiveIntervalCount;
        break;
      case GameCoordinateType.TemporaryBlockage:
        meshRenderer.material = Gameboard.instance.TemporaryBlockageMaterial;
        elevation = 1.0f;
        hitPoints = Gameboard.instance.temporaryBlockageHitPoints;
        break;
      case GameCoordinateType.SuperTemporaryBlockage:
        meshRenderer.material = Gameboard.instance.TemporaryBlockageMaterial;
        elevation = 2.0f;
        hitPoints = Gameboard.instance.superTemporaryBlockageHitPoints;
        newType = GameCoordinateType.TemporaryBlockage;
        break;
      case GameCoordinateType.DamagedTemporaryBlockage:
        meshRenderer.material = Gameboard.instance.DamagedBlockageMaterial;
        elevation = 0.5f;
        pointsValue = 1;
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