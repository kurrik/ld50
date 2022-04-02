using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum AttackType {
  None,
  Attack1,
  Attack2,
  Attack3,
  Attack4,
}

public struct AttackPowerInfo {
  public float Amount;
  public Color Color;
  public AttackType Type;
}

public class Player : MonoBehaviour {
  private Vector3 _cube;
  private Vector3 _realPosition;

  private AttackPowerInfo _attackPowerInfo = new AttackPowerInfo();
  public float attackPower {
    get { return _attackPowerInfo.Amount; }
    set {
      _attackPowerInfo.Amount = value;
      if (value > attack4Cost) {
        _attackPowerInfo.Color = attack4Color;
        _attackPowerInfo.Type = AttackType.Attack4;
      } else if (value > attack3Cost) {
        _attackPowerInfo.Color = attack3Color;
        _attackPowerInfo.Type = AttackType.Attack3;
      } else if (value > attack2Cost) {
        _attackPowerInfo.Color = attack2Color;
        _attackPowerInfo.Type = AttackType.Attack2;
      } else if (value > attack1Cost) {
        _attackPowerInfo.Color = attack1Color;
        _attackPowerInfo.Type = AttackType.Attack1;
      } else {
        _attackPowerInfo.Color = noAttackColor;
        _attackPowerInfo.Type = AttackType.None;
      }
    }
  }

  private float _amountMoved;
  public float amountMoved {
    get { return _amountMoved; }
  }

  public float playerMaxAmountMoved = 10.0f;
  public float playerAmountMovedDecay = 0.5f;

  public float playerSpeed = 0.5f;
  public float attackPowerRegenPerSecond = 0.1f;
  public Color noAttackColor = Color.gray;
  public float attack1Cost = 0.05f;
  public Color attack1Color = Color.blue;
  public float attack2Cost = 0.15f;
  public Color attack2Color = Color.green;
  public float attack3Cost = 0.3f;
  public Color attack3Color = Color.yellow;
  public float attack4Cost = 0.5f;
  public Color attack4Color = Color.red;

  public UnityEvent OnNotEnoughAttackPower = new UnityEvent();
  public UnityEvent OnAttack = new UnityEvent();
  public UnityEvent<AttackPowerInfo> OnAttackPowerUpdate = new UnityEvent<AttackPowerInfo>();

  void Start() {
    attackPower = 0.0f;
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
      _amountMoved += (newPosition - _realPosition).magnitude;
      _realPosition = newPosition;
      _cube = newCube;
    }
  }

  public void Trigger(GameCubeCoordinates coords) {
    GameCoordinate coordinate = coords.GetCoordinateFromContainer(_cube, "all");
    if (coordinate) {
      switch (_attackPowerInfo.Type) {
        case AttackType.Attack1:
          coordinate.ClearRadius(_cube, coords, 1);
          attackPower -= attack1Cost;
          OnAttack.Invoke();
          break;
        case AttackType.Attack2:
          coordinate.ClearRadius(_cube, coords, 2);
          attackPower -= attack2Cost;
          OnAttack.Invoke();
          break;
        case AttackType.Attack3:
          coordinate.ClearRadius(_cube, coords, 3);
          attackPower -= attack3Cost;
          OnAttack.Invoke();
          break;
        case AttackType.Attack4:
          coordinate.ClearRadius(_cube, coords, 4);
          attackPower -= attack4Cost;
          OnAttack.Invoke();
          break;
        default:
          OnNotEnoughAttackPower.Invoke();
          break;
      }
    }
  }

  void FixedUpdate() {
    attackPower = Mathf.Clamp(attackPower + attackPowerRegenPerSecond * Time.fixedDeltaTime, 0.0f, 1.0f);
    OnAttackPowerUpdate.Invoke(_attackPowerInfo);
    _amountMoved = Mathf.Clamp(_amountMoved - playerAmountMovedDecay * Time.fixedDeltaTime, 0.0f, playerMaxAmountMoved);
  }
}
