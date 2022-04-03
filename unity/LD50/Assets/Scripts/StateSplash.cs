using UnityEngine;

public class StateSplash : GameStateMonoBehavior {
  public const string AdvanceButton = "Trigger";

  public void Start() {
    if (Gameboard.instance.ShowStartup) {
      Gameboard.instance.states.PushState(this);
      Time.timeScale = 0.0f;
    } else {
      gameObject.SetActive(false);
    }
  }

  public virtual void Advance() {
    stateManager.PopState();
    gameObject.SetActive(false);
    Time.timeScale = 1.0f;
  }

  public override void StateUpdate(GameStateManager states) {
    Debug.LogFormat("Update");
    if (Input.GetButtonUp(AdvanceButton)) {
      Advance();
    }
  }
}