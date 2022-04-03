using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateTerminal : GameStateMonoBehavior {
  public override void OnCurrentEnter() {
    Time.timeScale = 0.0f;
  }

  public override void StateUpdate(GameStateManager states) {
  }
}