using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
  private Vector3 _cube;
  private Vector3 _realPosition;
  public float playerSpeed = 0.5f;

  void Start() {

  }

  void Update() {

  }

  public void Init(Vector3 cube, GameCubeCoordinates coords) {
    _cube = cube;
    transform.position = coords.ConvertCubeToWorldPosition(cube);
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
      coordinate.ClearRadius(_cube, coords, 2);
    }
  }
}
