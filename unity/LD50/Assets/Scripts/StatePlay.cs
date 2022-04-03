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
  public HUD HUD;

  // public GameLevelCompletedState gameLevelCompletedState;
  // public GameCompletedState gameCompletedState;
  // public GameEndState gameEndState;
  // public GamePauseState gamePauseState;

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
  private GameStateManager states;

  private void Start() {
  }

  public override void Register(GameStateManager s) {
    states = s;
  }

  public override void Unregister(GameStateManager s) {
    states = null;
  }

  public override void OnCurrentEnter() {
    HUD.gameObject.SetActive(true);
  }

  public override void OnCurrentExit() {
    HUD.gameObject.SetActive(false);
  }

  public override void StateUpdate(GameStateManager states) {
  }

  public override void StateFixedUpdate(GameStateManager states) {
  }

  private void SetGameState(GameStateMonoBehavior state) {
    state.gameObject.SetActive(true);
    if (states != null) {
      states.PushState(state);
    }
  }
}