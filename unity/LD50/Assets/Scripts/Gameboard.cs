using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Gameboard : MonoBehaviour {
  private float _elapsed;
  private GameCubeCoordinates _coords;
  private Player _player;
  private Vector3 _inputDirection;

  public Player playerPrefab;
  public AttackBar attackBar;
  public PlayerCamera playerCamera;
  public int gridSize = 30;
  public float tickTimeStep = 0.5f;
  public int objectiveRadius = 1;

  public const string TriggerButton = "Trigger";

  public static Gameboard instance = null;

  public Material EmptyMaterial;
  public Material ObjectiveMaterial;
  public Material SpreadAlphaMaterial;
  public Material BlockageMaterial;

  public bool unityEditorShowStartup = false;
  public bool ShowStartup {
    get {
#if UNITY_EDITOR
      return unityEditorShowStartup;
#else
      return true;
#endif
    }
  }

  public bool DebugEnabled {
    get {
#if UNITY_EDITOR
      return true;
#else
      return false;
#endif
    }
  }

  public void End() {
  }

  public void Quit() {
#if UNITY_EDITOR
    UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
  }

  public void Reload() {
    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
  }

  private void OnNotEnoughAttackPower() {
    Debug.LogFormat("Not enough attack power!");
  }

  private void OnAttack() {
    Debug.LogFormat("Attack!");
  }

  private void OnAttackPowerUpdate(AttackPowerInfo info) {
    attackBar.SetValue(info.Amount, info.Color);
  }

  private void BuildMap() {
    _coords.Construct(gridSize);
    /*
    // Remove 25% of Coordinates except 0,0,0
    foreach (Vector3 cube in coords.GetCubesFromContainer("all")) {
      if (cube == Vector3.zero)
        continue;

      if (Random.Range(0.0f, 100.0f) < 5.0f)
        coords.RemoveCube(cube);
    }

    // Remove Coordinates not reachable from 0,0,0
    coords.RemoveCubes(
        coords.BooleanDifferenceCubes(
            coords.GetCubesFromContainer("all"),
            coords.GetReachableCubes(Vector3.zero, gridSize)
        )
    );
    */

    // Display Coordinates
    //coords.ShowCoordinatesInContainer("all");

    // Construct Examples
    ConstructExamples();
    ConstructPlayer();
  }

  private void ConstructExamples() {
    List<Vector3> allCubes = _coords.GetCubesFromContainer("all");

    // Line between the first and last cube coordinate
    _coords.AddCubesToContainer(_coords.GetLineBetweenTwoCubes(allCubes[0], allCubes[allCubes.Count - 1]), "line");

    // Reachable, 3 coordinates away from 0.0.0
    _coords.AddCubesToContainer(_coords.GetReachableCubes(Vector3.zero, 3), "reachable");

    // Spiral, 3 coordinates away from 0.0.0
    _coords.AddCubesToContainer(_coords.GetSpiralCubes(Vector3.zero, 10), "spiral");

    // Path between the first and last cube coordinate
    _coords.AddCubesToContainer(_coords.GetPathBetweenTwoCubes(allCubes[0], allCubes[allCubes.Count - 1]), "path");
  }

  private void ConstructPlayer() {
    Vector3 startingCube = new Vector3(0, 0, 0);
    _player = Instantiate(playerPrefab, _coords.ConvertCubeToWorldPosition(startingCube), playerPrefab.transform.rotation);
    _player.Init(startingCube, _coords);
    playerCamera.Init(_player);
  }

  private void Tick() {
    foreach (Vector3 cube in _coords.GetCubesFromContainer("all")) {
      GameCoordinate coord = _coords.GetCoordinateFromContainer(cube, "all");
      coord.Tick(cube, _coords);
    }
    foreach (Vector3 cube in _coords.GetCubesFromContainer("all")) {
      GameCoordinate coord = _coords.GetCoordinateFromContainer(cube, "all");
      coord.ApplyTick();
    }
  }

  private void Awake() {
    if (instance == null) {
      instance = this;
    } else if (instance != this) {
      Destroy(gameObject);
      return;
    }
    _coords = gameObject.GetComponent<GameCubeCoordinates>();
  }

  private void ShowExample(string key) {
    _coords.HideCoordinatesInContainer("all");
    _coords.ShowCoordinatesInContainer(key);
  }

  void Start() {
    BuildMap();
    _elapsed = 0.0f;
    _player.OnNotEnoughAttackPower.AddListener(OnNotEnoughAttackPower);
    _player.OnAttack.AddListener(OnAttack);
    _player.OnAttackPowerUpdate.AddListener(OnAttackPowerUpdate);
  }

  void FixedUpdate() {
    _elapsed += Time.fixedDeltaTime;
    if (_elapsed > tickTimeStep) {
      _elapsed -= tickTimeStep;
      Tick();
    }
    if (_inputDirection.sqrMagnitude > 0) {
      _player.Move(_inputDirection, _coords);
    }
  }

  void Update() {
    if (Gameboard.instance.DebugEnabled) {
      if (Input.GetKeyDown(KeyCode.F1)) {
        BuildMap();
        return;
      }
      if (_coords.GetCoordinatesFromContainer("all").Count == 0)
        return;
      if (Input.GetKeyDown(KeyCode.F2)) {
        _coords.ShowCoordinatesInContainer("all");
      }
      if (Input.GetKeyDown(KeyCode.F5))
        ShowExample("line");

      if (Input.GetKeyDown(KeyCode.F6))
        ShowExample("reachable");

      if (Input.GetKeyDown(KeyCode.F7))
        ShowExample("spiral");

      if (Input.GetKeyDown(KeyCode.F8))
        ShowExample("path");

      if (Input.GetKeyDown(KeyCode.F9))
        Tick();

      if (Input.GetMouseButtonDown(1)) {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 200f)) {
          Vector3 cube = _coords.ConvertWorldPositionToCube(hit.point);
          GameCoordinate coordinate = _coords.GetCoordinateFromContainer(cube, "all");
          if (coordinate) {
            coordinate.ClearRadius(cube, _coords, 2);
          }
        }
      }
    }

    if (Input.GetButtonUp(TriggerButton)) {
      _player.Trigger(_coords);
    }

    float horizontalInput = Input.GetAxisRaw("Horizontal");
    float verticalInput = Input.GetAxisRaw("Vertical");
    _inputDirection = new Vector3(horizontalInput, 0.0f, verticalInput);
  }
}
