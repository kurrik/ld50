using System.Collections;
using UnityEngine;

public struct CoordinateConfig {
  public Vector3 Cube;
  public GameCoordinateType Type;
}

public struct LevelInfo {
  public int Seed;
  public int BoardSize;
  public CoordinateConfig[] CoordinateConfigs;
}

public class StatePlay : GameStateMonoBehavior {
  public const string MenuButton = "Menu";
  public const string TriggerButton = "Trigger";
  public HUD HUD;
  public float tickTimeStep = 0.5f;

  public StateMenu stateMenu;
  public StateLevelComplete stateLevelComplete;
  public StateTerminal stateGameOver;
  public StateTerminal stateGameWon;
  public Player playerPrefab;
  public AttackBar attackBar;
  public PlayerCamera playerCamera;

  private int currentLevel = 0;
  private LevelInfo[] levels = {
    new LevelInfo(){
      Seed = 1234,
      CoordinateConfigs = new CoordinateConfig[] {
        new CoordinateConfig() { Cube = new Vector3(0.0f,0.0f,0.0f), Type = GameCoordinateType.SpreadAlpha },
      },
    },
  };
  private LevelInfo level { get => levels[currentLevel]; }

  private float _elapsed;
  private GameCubeCoordinates _coords;
  private Player _player;
  private Vector3 _inputDirection;

  private void Awake() {

  }

  private void Start() {
    _elapsed = 0.0f;
    _coords = gameObject.GetComponent<GameCubeCoordinates>();
    stateMenu.gameObject.SetActive(false);
    stateLevelComplete.gameObject.SetActive(false);
    stateGameOver.gameObject.SetActive(false);
    stateGameWon.gameObject.SetActive(false);
    LoadLevel();
  }

  private void LoadLevel() {
    _coords.Construct(20);
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
    foreach (Vector3 cube in _coords.GetCubesFromContainer("all")) {
      GameCoordinate coord = _coords.GetCoordinateFromContainer(cube, "all");
      coord.ApplyTick();
    }
  }

  private void OnFailedAttack() {
    Debug.LogFormat("Failed attack!");
  }

  private void OnAttack() {
    Debug.LogFormat("Attack!");
  }

  private void OnAttackPowerUpdate(AttackPowerInfo info) {
    attackBar.SetValue(info.Amount, info.Color);
  }

  private void SetGameState(GameStateMonoBehavior state) {
    if (stateManager != null) {
      state.gameObject.SetActive(true);
      Debug.LogFormat("Menu enabled");
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
        LoadLevel();
        return;
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

    if (Input.GetButtonUp(TriggerButton)) {
      _player.Trigger(_coords);
    }

    float horizontalInput = Input.GetAxisRaw("Horizontal");
    float verticalInput = Input.GetAxisRaw("Vertical");
    _inputDirection = new Vector3(horizontalInput, 0.0f, verticalInput);
  }

  public override void StateFixedUpdate(GameStateManager states) {
    _elapsed += Time.fixedDeltaTime;
    if (_elapsed > tickTimeStep) {
      _elapsed -= tickTimeStep;
      Tick();
    }
    if (_inputDirection.sqrMagnitude > 0) {
      _player.Move(_inputDirection, _coords);
    }
  }
}