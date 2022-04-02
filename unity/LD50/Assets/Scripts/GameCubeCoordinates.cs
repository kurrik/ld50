using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCubeCoordinates : CubeCoordinates<GameCoordinate> {
  public GameCoordinate coordinatePrefab;

  public override GameCoordinate CreateCoordinate(Vector3 cube) {
    if (coordinatePrefab == null) {
      throw new System.NotSupportedException();
    }
    GameCoordinate coordinate = Instantiate(coordinatePrefab, new Vector3(0, 0, 0), coordinatePrefab.transform.rotation);
    coordinate.Init(
        cube,
        ConvertCubeToWorldPosition(cube),
        this
    );
    return coordinate;
  }
}
