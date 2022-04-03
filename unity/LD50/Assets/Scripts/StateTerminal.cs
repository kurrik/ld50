using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateTerminal : GameStateMonoBehavior {
  public const string AdvanceButton = "Trigger";

  public override void OnCurrentEnter() {
    Time.timeScale = 0.0f;
  }

  public override void StateUpdate(GameStateManager states) {
    if (Input.GetButtonUp(AdvanceButton)) {
      Gameboard.instance.Quit();
    }
  }
}