using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gameboard : MonoBehaviour {
  private float _elapsed;
  private GameCubeCoordinates _coords;
  public int gridSize = 30;
  public float tickTimeStep = 0.5f;

  private void BuildMap() {
    _coords.Construct(gridSize);


    /*
    // Remove 25% of Coordinates except 0,0,0
    foreach (Vector3 cube in coords.GetCubesFromContainer("all")) {
      if (cube == Vector3.zero)
        continue;

      if (Random.Range(0.0f, 100.0f) < 5.0f)
        coords.RemoveCube(cube);
    }

    // Remove Coordinates not reachable from 0,0,0
    coords.RemoveCubes(
        coords.BooleanDifferenceCubes(
            coords.GetCubesFromContainer("all"),
            coords.GetReachableCubes(Vector3.zero, gridSize)
        )
    );
    */

    // Display Coordinates
    //coords.ShowCoordinatesInContainer("all");

    // Construct Examples
    ConstructExamples();
  }

  private void ConstructExamples() {
    List<Vector3> allCubes = _coords.GetCubesFromContainer("all");

    // Line between the first and last cube coordinate
    _coords.AddCubesToContainer(_coords.GetLineBetweenTwoCubes(allCubes[0], allCubes[allCubes.Count - 1]), "line");

    // Reachable, 3 coordinates away from 0.0.0
    _coords.AddCubesToContainer(_coords.GetReachableCubes(Vector3.zero, 3), "reachable");

    // Spiral, 3 coordinates away from 0.0.0
    _coords.AddCubesToContainer(_coords.GetSpiralCubes(Vector3.zero, 10), "spiral");

    // Path between the first and last cube coordinate
    _coords.AddCubesToContainer(_coords.GetPathBetweenTwoCubes(allCubes[0], allCubes[allCubes.Count - 1]), "path");
  }

  private void Tick() {
    foreach (Vector3 cube in _coords.GetCubesFromContainer("all")) {
      GameCoordinate coord = _coords.GetCoordinateFromContainer(cube, "all");
      coord.Tick(cube, _coords);
    }
    foreach (Vector3 cube in _coords.GetCubesFromContainer("all")) {
      GameCoordinate coord = _coords.GetCoordinateFromContainer(cube, "all");
      coord.ApplyTick();
    }
  }

  private void Awake() {
    _coords = gameObject.GetComponent<GameCubeCoordinates>();
  }

  private void ShowExample(string key) {
    _coords.HideCoordinatesInContainer("all");
    _coords.ShowCoordinatesInContainer(key);
  }

  void Start() {
    BuildMap();
    _elapsed = 0.0f;
  }

  void FixedUpdate() {
    _elapsed += Time.fixedDeltaTime;
    if (_elapsed > tickTimeStep) {
      _elapsed -= tickTimeStep;
      Tick();
    }
  }

  void Update() {
    if (Input.GetKeyDown(KeyCode.Return)) {
      BuildMap();
      return;
    }
    if (_coords.GetCoordinatesFromContainer("all").Count == 0)
      return;

    if (Input.GetKeyDown(KeyCode.Backspace)) {
      _coords.ShowCoordinatesInContainer("all");
    }

    if (Input.GetKeyDown(KeyCode.L))
      ShowExample("line");

    if (Input.GetKeyDown(KeyCode.R))
      ShowExample("reachable");

    if (Input.GetKeyDown(KeyCode.S))
      ShowExample("spiral");

    if (Input.GetKeyDown(KeyCode.P))
      ShowExample("path");

    if (Input.GetKeyDown(KeyCode.T))
      Tick();

    if (Input.GetMouseButtonDown(0)) {
      Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
      Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
      RaycastHit hit;
      if (Physics.Raycast(ray, out hit, 200f)) {
        Vector3 cube = _coords.ConvertWorldPositionToCube(hit.point);
        GameCoordinate coordinate = _coords.GetCoordinateFromContainer(cube, "all");
        if (coordinate) {
          coordinate.ClearRadius(cube, _coords, 2);
        }
      }
    }
  }
}
