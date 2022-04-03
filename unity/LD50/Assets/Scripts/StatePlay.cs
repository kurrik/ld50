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

  public StateMenu stateMenu;
  public StateLevelComplete stateLevelComplete;
  public StateTerminal stateGameOver;
  public StateTerminal stateGameWon;

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

  private void Start() {
    stateMenu.gameObject.SetActive(false);
    stateLevelComplete.gameObject.SetActive(false);
    stateGameOver.gameObject.SetActive(false);
    stateGameWon.gameObject.SetActive(false);
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
  }

  public override void StateFixedUpdate(GameStateManager states) {
  }

  private void SetGameState(GameStateMonoBehavior state) {
    if (stateManager != null) {
      state.gameObject.SetActive(true);
      Debug.LogFormat("Menu enabled");
      stateManager.PushState(state);
    }
  }
}