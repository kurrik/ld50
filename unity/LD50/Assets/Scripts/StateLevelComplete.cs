using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateLevelComplete : GameStateMonoBehavior {
  public const string AdvanceButton = "Trigger";
  public TMPro.TextMeshProUGUI textBox;
  public float minTimeSeconds = 1.0f;
  private float menuLoadedTime;
  private bool exit;

  public void SetText(string text) {
    textBox.text = text;
  }

  public override void OnCurrentEnter() {
    Time.timeScale = 0.0f;
    menuLoadedTime = Time.realtimeSinceStartup;
    exit = false;
  }

  public override void StateUpdate(GameStateManager states) {
    if (Input.GetButtonUp(AdvanceButton)) {
      exit = true;
    }
    float elapsed = Time.realtimeSinceStartup - menuLoadedTime;
    if (exit && elapsed >= minTimeSeconds) {
      states.PopState();
      gameObject.SetActive(false);
      Time.timeScale = 1.0f;
    }
  }
}
