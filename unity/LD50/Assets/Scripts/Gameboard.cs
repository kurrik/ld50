using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Gameboard : MonoBehaviour {
  public static Gameboard instance = null;

  public int objectiveRadius = 1;
  public int temporaryBlockageHitPoints = 5;
  public int spreadBetaHitPoints = 5;

  public GameStateManager states;
  public StateSplash stateSplash;
  public StatePlay statePlay;

  public Material EmptyMaterial;
  public Material ObjectiveMaterial;
  public Material SpreadAlphaMaterial;
  public Material SpreadBetaMaterial;
  public Material BlockageMaterial;
  public Material TemporaryBlockageMaterial;
  public Material DamagedBlockageMaterial;

  public bool unityEditorShowStartup = false;
  public bool ShowStartup {
    get {
#if UNITY_EDITOR
      return unityEditorShowStartup;
#else
      return true;
#endif
    }
  }

  public bool DebugEnabled {
    get {
#if UNITY_EDITOR
      return true;
#else
      return false;
#endif
    }
  }

  public void Quit() {
#if UNITY_EDITOR
    UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
  }

  public void Reload() {
    Time.timeScale = 1.0f;
    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
  }

  public void TriggerWin() {
    statePlay.TriggerLevelWin();
  }

  public void TriggerLoss() {
    statePlay.TriggerLevelLoss();
  }

  public void TriggerScoreIncrement(Vector3 cube, int amount) {
    statePlay.TriggerScoreIncrement(cube, amount);
  }

  public void TriggerShake() {
    statePlay.TriggerShake();
  }

  private void Awake() {
    if (instance == null) {
      instance = this;
    } else if (instance != this) {
      Destroy(gameObject);
      return;
    }
    states = new GameStateManager();
    states.PushState(statePlay);
    if (stateSplash) {
      stateSplash.gameObject.SetActive(true);
    }
  }

  void FixedUpdate() {
    states.StateFixedUpdate();
  }

  void Update() {
    states.StateUpdate();
  }
}
