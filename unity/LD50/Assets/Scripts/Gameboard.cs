using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gameboard : MonoBehaviour {
  private GameCubeCoordinates coords;
  public int gridSize = 30;

  private void BuildMap() {
    coords.Construct(gridSize);

    // Remove 25% of Coordinates except 0,0,0
    foreach (Vector3 cube in coords.GetCubesFromContainer("all")) {
      if (cube == Vector3.zero)
        continue;

      if (Random.Range(0.0f, 100.0f) < 25.0f)
        coords.RemoveCube(cube);
    }

    // Remove Coordinates not reachable from 0,0,0
    coords.RemoveCubes(
        coords.BooleanDifferenceCubes(
            coords.GetCubesFromContainer("all"),
            coords.GetReachableCubes(Vector3.zero, gridSize)
        )
    );

    // Display Coordinates
    coords.ShowCoordinatesInContainer("all");

    // Construct Examples
    ConstructExamples();
  }

  private void ConstructExamples() {
    List<Vector3> allCubes = coords.GetCubesFromContainer("all");

    // Line between the first and last cube coordinate
    coords.AddCubesToContainer(coords.GetLineBetweenTwoCubes(allCubes[0], allCubes[allCubes.Count - 1]), "line");

    // Reachable, 3 coordinates away from 0.0.0
    coords.AddCubesToContainer(coords.GetReachableCubes(Vector3.zero, 3), "reachable");

    // Spiral, 3 coordinates away from 0.0.0
    coords.AddCubesToContainer(coords.GetSpiralCubes(Vector3.zero, 10), "spiral");

    // Path between the first and last cube coordinate
    coords.AddCubesToContainer(coords.GetPathBetweenTwoCubes(allCubes[0], allCubes[allCubes.Count - 1]), "path");
  }

  private void Tick() {
    foreach (Vector3 cube in coords.GetCubesFromContainer("all")) {
      GameCoordinate coord = coords.GetCoordinateFromContainer(cube, "all");
      coords.GetNeighborCubes(cube);
    }

  }

  private void Awake() {
    coords = gameObject.GetComponent<GameCubeCoordinates>();
  }

  private void ShowExample(string key) {
    coords.HideCoordinatesInContainer("all");
    coords.ShowCoordinatesInContainer(key);
  }

  void Start() {
    BuildMap();
  }

  void Update() {
    if (Input.GetKeyDown(KeyCode.Return)) {
      BuildMap();
      return;
    }
    if (coords.GetCoordinatesFromContainer("all").Count == 0)
      return;

    if (Input.GetKeyDown(KeyCode.Backspace)) {
      coords.ShowCoordinatesInContainer("all");
    }

    if (Input.GetKeyDown(KeyCode.L))
      ShowExample("line");

    if (Input.GetKeyDown(KeyCode.R))
      ShowExample("reachable");

    if (Input.GetKeyDown(KeyCode.S))
      ShowExample("spiral");

    if (Input.GetKeyDown(KeyCode.P))
      ShowExample("path");
  }
}
