using System.Collections.Generic;
using UnityEngine;

public interface IGameState {
  void StateUpdate(GameStateManager states);
  void StateFixedUpdate(GameStateManager states);
  void Register(GameStateManager states);
  void Unregister(GameStateManager states);
  void OnCurrentEnter();
  void OnCurrentExit();
}

public abstract class GameStateMonoBehavior : MonoBehaviour, IGameState {
  public abstract void OnCurrentEnter();
  public abstract void OnCurrentExit();
  public abstract void Register(GameStateManager states);
  public abstract void StateUpdate(GameStateManager states);
  public abstract void StateFixedUpdate(GameStateManager states);
  public abstract void Unregister(GameStateManager states);
}

public class GameStateManager {
  private LinkedList<IGameState> states_;

  public GameStateManager() {
    states_ = new LinkedList<IGameState>();
  }

  public void PushState(IGameState state) {
    if (states_.Count > 0) {
      Current.OnCurrentExit();
    }
    states_.AddFirst(state);
    state.Register(this);
    state.OnCurrentEnter();
  }

  public bool PopState() {
    if (states_.Count > 1) {
      Current.OnCurrentExit();
      Current.Unregister(this);
      states_.RemoveFirst();
      if (states_.Count > 0) {
        Current.OnCurrentEnter();
      }
      return true;
    }
    Debug.LogWarningFormat("Attempted to pop state from stack of length {0}", states_.Count);
    return false;
  }

  public bool RemoveState(IGameState state) {
    if (states_.Count > 1) {
      if (Current == state) {
        Current.OnCurrentExit();
      }
      state.Unregister(this);
      var removed = states_.Remove(state);
      if (states_.Count > 0) {
        Current.OnCurrentEnter();
      }
      return removed;
    }
    return false;
  }

  public IGameState Current {
    get { return states_.Count > 0 ? states_.First.Value : null; }
  }

  public void StateUpdate() {
    if (states_.Count > 0) {
      Current.StateUpdate(this);
    }
  }

  public void StateFixedUpdate() {
    if (states_.Count > 0) {
      Current.StateFixedUpdate(this);
    }
  }
}