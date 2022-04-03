using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Gameboard : MonoBehaviour {
  public static Gameboard instance = null;

  public int gridSize = 20;
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

  public void End() {
  }

  public void Quit() {
#if UNITY_EDITOR
    UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
  }

  public void Reload() {
    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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
