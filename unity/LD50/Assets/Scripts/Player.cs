using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Player : MonoBehaviour {
  private Vector3 _cube;
  private Vector3 _realPosition;

  private float _attackPower;
  public float attackPower {
    get { return _attackPower; }
  }

  public float playerSpeed = 0.5f;
  public float attackPowerRegenPerSecond = 0.1f;
  public float attack1Cost = 0.05f;

  public UnityEvent OnNotEnoughAttackPower = new UnityEvent();
  public UnityEvent OnAttack = new UnityEvent();

  void Start() {
    _attackPower = 0.0f;
  }

  public void Init(Vector3 cube, GameCubeCoordinates coords) {
    _cube = cube;
    GameCoordinate coordinate = coords.GetCoordinateFromContainer(cube, "all");
    transform.position = coordinate.transform.position;
    _realPosition = transform.position;
  }

  public void Move(Vector3 direction, GameCubeCoordinates coords) {
    Vector3 newPosition = _realPosition + playerSpeed * direction;
    Vector3 newCube = coords.ConvertWorldPositionToCube(newPosition);
    GameCoordinate coordinate = coords.GetCoordinateFromContainer(newCube, "all");
    if (coordinate) {
      transform.position = coordinate.transform.position;
      _realPosition = newPosition;
      _cube = newCube;
    }
  }

  public void Trigger(GameCubeCoordinates coords) {
    GameCoordinate coordinate = coords.GetCoordinateFromContainer(_cube, "all");
    if (coordinate) {
      if (_attackPower >= attack1Cost) {
        coordinate.ClearRadius(_cube, coords, 2);
        _attackPower -= attack1Cost;
        OnAttack.Invoke();
      } else {
        OnNotEnoughAttackPower.Invoke();
      }
    }
  }

  void FixedUpdate() {
    _attackPower = Mathf.Clamp(_attackPower + attackPowerRegenPerSecond * Time.fixedDeltaTime, 0.0f, 1.0f);
  }
}
