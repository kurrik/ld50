using System.Collections;
using UnityEngine;

public struct CoordinateConfig {
  public Vector3 Cube;
  public GameCoordinateType Type;
}

public struct LevelInfo {
  public int Seed;
  public int BoardSize;
  public float BlockagePercent;
  public bool UseStarterPositions;
  public float TickTimestep;
  public CoordinateConfig[] CoordinateConfigs;
}

public class StatePlay : GameStateMonoBehavior {
  public const string MenuButton = "Menu";
  public const string TriggerButton = "Trigger";
  public HUD HUD;

  public StateMenu stateMenu;
  public StateLevelComplete stateLevelComplete;
  public StateTerminal stateGameOver;
  public StateTerminal stateGameWon;
  public Player playerPrefab;
  public GameCoordinate coordinatePrefab;
  public AttackBar attackBar;
  public PlayerCamera playerCamera;

  private int currentLevel = 0;
  private LevelInfo[] levels = {
    new LevelInfo(){
      Seed = 34927,
      BoardSize = 10,
      BlockagePercent = 0.1f,
      UseStarterPositions = false,
      TickTimestep = 1.0f,
      CoordinateConfigs = new CoordinateConfig[] {
        new CoordinateConfig() { Cube = new Vector3(0.0f,-10.0f,10.0f), Type = GameCoordinateType.SpreadAlpha },
        new CoordinateConfig() { Cube = new Vector3(-10.0f,10.0f,0.0f), Type = GameCoordinateType.SpreadAlpha },
        new CoordinateConfig() { Cube = new Vector3(10.0f,0.0f,-10.0f), Type = GameCoordinateType.SpreadAlpha },
      },
    },
    new LevelInfo(){
      Seed = 1228,
      BoardSize = 20,
      BlockagePercent = 0.2f,
      UseStarterPositions = false,
      TickTimestep = 0.5f,
      CoordinateConfigs = new CoordinateConfig[] {
        new CoordinateConfig() { Cube = new Vector3(20.0f,-20.0f,0.0f), Type = GameCoordinateType.SpreadAlpha },
        new CoordinateConfig() { Cube = new Vector3(0.0f,-20.0f,20.0f), Type = GameCoordinateType.SpreadBeta },
        new CoordinateConfig() { Cube = new Vector3(-20.0f,0.0f,20.0f), Type = GameCoordinateType.SpreadAlpha },
        new CoordinateConfig() { Cube = new Vector3(-20.0f,20.0f,0.0f), Type = GameCoordinateType.SpreadBeta },
        new CoordinateConfig() { Cube = new Vector3(0.0f,20.0f,-20.0f), Type = GameCoordinateType.SpreadAlpha },
        new CoordinateConfig() { Cube = new Vector3(20.0f,0.0f,-20.0f), Type = GameCoordinateType.SpreadBeta },
      },
    },
  };
  private LevelInfo level { get => levels[currentLevel]; }

  private float _tickTimeStep = 0.5f;
  private float _elapsed;
  private int _score;
  private GameCubeCoordinates _coords;
  private Player _player;
  private Vector3 _inputDirection;

  private void Awake() {

  }

  private void Start() {
    _score = 0; // Don't erase each time a level is loaded.
    _coords = gameObject.GetComponent<GameCubeCoordinates>();
    stateMenu.gameObject.SetActive(false);
    stateLevelComplete.gameObject.SetActive(false);
    stateGameOver.gameObject.SetActive(false);
    stateGameWon.gameObject.SetActive(false);
    LoadLevel();
  }

  private void LoadLevel(bool generateSeed = false) {
    _elapsed = 0.0f;
    _tickTimeStep = level.TickTimestep;
    if (_player) {
      playerCamera.Reset();
      _player.OnFailedAttack.RemoveAllListeners();
      _player.OnAttack.RemoveAllListeners();
      _player.OnAttackPowerUpdate.RemoveAllListeners();
      Destroy(_player.gameObject);
      _player = null;
    }
    int seed = level.Seed;
    if (generateSeed) {
      seed = Random.Range(0, 10000);
    }
    Debug.LogFormat("Using random seed {0}", seed);
    Random.InitState(seed);
    _coords.info = level;
    _coords.Construct(level.BoardSize);
    LoadPlayer();
  }

  private void LoadPlayer() {
    Vector3 startingCube = new Vector3(0, 0, 0);
    _player = Instantiate(playerPrefab, _coords.ConvertCubeToWorldPosition(startingCube), playerPrefab.transform.rotation);
    _player.Init(startingCube, _coords);
    playerCamera.Init(_player);
    _player.OnFailedAttack.AddListener(OnFailedAttack);
    _player.OnAttack.AddListener(OnAttack);
    _player.OnAttackPowerUpdate.AddListener(OnAttackPowerUpdate);
  }

  private void Tick() {
    foreach (Vector3 cube in _coords.GetCubesFromContainer("all")) {
      GameCoordinate coord = _coords.GetCoordinateFromContainer(cube, "all");
      coord.Tick(cube, _coords);
    }
    bool seenEnemyPiece = false;
    foreach (Vector3 cube in _coords.GetCubesFromContainer("all")) {
      GameCoordinate coord = _coords.GetCoordinateFromContainer(cube, "all");
      coord.ApplyTick();
      if (coord.IsEnemyPiece()) {
        seenEnemyPiece = true;
      }
    }
    if (!seenEnemyPiece) {
      TriggerLevelWin();
    }
  }

  private void OnFailedAttack() {
    // Debug.LogFormat("Failed attack!");
  }

  private void OnAttack(AttackType type) {
    switch (type) {
      case AttackType.Attack3:
      case AttackType.Attack4:
        TriggerShake();
        break;
    }
  }

  private void OnAttackPowerUpdate(AttackPowerInfo info) {
    attackBar.SetValue(info.Amount, info.Color);
  }

  private void SetGameState(GameStateMonoBehavior state) {
    if (stateManager != null) {
      state.gameObject.SetActive(true);
      stateManager.PushState(state);
    }
  }

  public override void OnCurrentEnter() {
    HUD.gameObject.SetActive(true);
  }

  public override void OnCurrentExit() {
    HUD.gameObject.SetActive(false);
  }

  public override void StateUpdate(GameStateManager states) {
    if (Input.GetButtonUp(MenuButton)) {
      stateMenu.SetText("Neat");
      SetGameState(stateMenu);
    }
    if (Gameboard.instance.DebugEnabled) {
      if (Input.GetKeyDown(KeyCode.F1)) {
        LoadLevel(true);
        return;
      }
      if (Input.GetKeyDown(KeyCode.F2)) {
        TriggerLevelWin();
      }
      if (Input.GetKeyDown(KeyCode.F3)) {
        TriggerLevelLoss();
      }
      if (Input.GetKeyDown(KeyCode.F5)) {
        TriggerShake();
      }
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

    if (Input.GetButtonUp(TriggerButton) && _player) {
      _player.Trigger(_coords);
    }

    float horizontalInput = Input.GetAxisRaw("Horizontal");
    float verticalInput = Input.GetAxisRaw("Vertical");
    _inputDirection = new Vector3(horizontalInput, 0.0f, verticalInput);
  }

  public override void StateFixedUpdate(GameStateManager states) {
    _elapsed += Time.fixedDeltaTime;
    if (_elapsed > _tickTimeStep) {
      _elapsed -= _tickTimeStep;
      Tick();
    }
    if (_inputDirection.sqrMagnitude > 0) {
      _player.Move(_inputDirection, _coords);
    }
  }

  public void TriggerLevelWin() {
    if (currentLevel == levels.Length - 1) {
      SetGameState(stateGameWon);
    } else {
      currentLevel += 1;
      SetGameState(stateLevelComplete);
      LoadLevel();
    }
  }

  public void TriggerLevelLoss() {
    SetGameState(stateGameOver);
  }

  public void TriggerScoreIncrement(Vector3 cube, int amount) {
    _score += amount;
    if (amount > 10) {
      TriggerShake();
    }
    HUD.SetScore(_score);
  }

  public void TriggerShake() {
    playerCamera.TriggerShake();
  }
}