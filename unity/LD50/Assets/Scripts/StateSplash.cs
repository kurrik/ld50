using UnityEngine;

public class StateSplash : MonoBehaviour, IGameState {
  public const string AdvanceButton = "Trigger";

  private GameStateManager stateManager;

  public void Register(GameStateManager states) {
    stateManager = states;
  }

  public void Unregister(GameStateManager states) {
    stateManager = null;
  }

  public void OnCurrentEnter() { }
  public void OnCurrentExit() { }

  public void Start() {
    if (Gameboard.instance.ShowStartup) {
      Gameboard.instance.states.PushState(this);
      Time.timeScale = 0.0f;
    } else {
      gameObject.SetActive(false);
    }
  }

  public void Advance() {
    stateManager.PopState();
    gameObject.SetActive(false);
    Time.timeScale = 1.0f;
  }

  public void StateUpdate(GameStateManager states) {
    if (Input.GetButtonUp(AdvanceButton)) {
      Advance();
    }
  }

  public void StateFixedUpdate(GameStateManager states) {
  }
}