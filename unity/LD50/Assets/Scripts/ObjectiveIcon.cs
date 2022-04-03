using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveIcon : MonoBehaviour {
  public static ObjectiveIcon GetUnusedInstance() {
    GameObject pool = Gameboard.instance.PooledObjects;
    foreach (ObjectiveIcon poolIcon in pool.GetComponentsInChildren<ObjectiveIcon>(true)) {
      if (!poolIcon.gameObject.activeSelf) {
        return poolIcon;
      }
    }
    ObjectiveIcon prefab = Gameboard.instance.objectiveIconPrefab;
    ObjectiveIcon icon = Instantiate(prefab, new Vector3(0, 0, 0), prefab.transform.rotation);
    icon.transform.parent = pool.transform;
    icon.gameObject.SetActive(false);
    return icon;
  }

  public static void DisableAllIcons() {
    GameObject pool = Gameboard.instance.PooledObjects;
    foreach (ObjectiveIcon poolIcon in pool.GetComponentsInChildren<ObjectiveIcon>(true)) {
      poolIcon.gameObject.SetActive(false);
    }
  }

  public Vector3 targetOffset = new Vector3(0.0f, 50.0f, 0.0f);
  public Vector3 initialOffset = new Vector3(0.0f, 2.0f, 0.0f);
  public float duration = 2.0f;
  public float durationVariance = 0.5f;
  public float rotationHz = 1.0f;
  public float rotationVariance = 0.5f;
  public float startDelayVariance = 1.0f;
  public AnimationCurve positionCurve;
  private Vector3 startPosition;
  private Vector3 targetPosition;
  private float rotationOffset;
  private float durationOffset;
  private float elapsed;

  public void Init(Vector3 position) {
    elapsed = 0.0f - Random.Range(0.0f, startDelayVariance);
    transform.position = position;
    startPosition = position;
    targetPosition = position + targetOffset;
    rotationOffset = Random.Range(0.0f, rotationVariance);
    durationOffset = Random.Range(0.0f, durationVariance);
    gameObject.SetActive(true);
  }

  void Update() {
    elapsed += Time.deltaTime;
    // Implements a delay.
    if (elapsed < 0) {
      return;
    }
    float pct = elapsed / (duration + durationOffset);
    transform.position = Vector3.Lerp(startPosition, targetPosition, positionCurve.Evaluate(pct));
    var rotationAngle = 360.0f * (rotationHz + rotationOffset) * Time.deltaTime;
    transform.Rotate(Vector3.up, rotationAngle, Space.World);

    if (elapsed >= duration) {
      gameObject.SetActive(false);
    }
  }
}
