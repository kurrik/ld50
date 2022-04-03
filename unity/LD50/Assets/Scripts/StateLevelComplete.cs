using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateLevelComplete : GameStateMonoBehavior {
  public const string AdvanceButton = "Trigger";

  public override void OnCurrentEnter() {
    Time.timeScale = 0.0f;
  }

  public override void StateUpdate(GameStateManager states) {
    if (Input.GetButtonUp(AdvanceButton)) {
      states.PopState();
      gameObject.SetActive(false);
      Time.timeScale = 1.0f;
    }
  }
}
