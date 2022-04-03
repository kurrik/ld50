using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMenu : GameStateMonoBehavior {
  public const string MenuButton = "Menu";
  public TMPro.TextMeshProUGUI customText;

  public void SetText(string text) {
    customText.text = text;
  }

  public override void OnCurrentEnter() {
    Time.timeScale = 0.0f;
  }

  public override void StateUpdate(GameStateManager states) {
    if (Input.GetButtonUp(MenuButton)) {
      CloseMenu();
    }
  }

  public void CloseMenu() {
    stateManager.PopState();
    gameObject.SetActive(false);
    Time.timeScale = 1.0f;
  }
}
