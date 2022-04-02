using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class AttackBar : MonoBehaviour {
  public Slider slider;

  void Start() {
    slider = GetComponent<Slider>();
  }

  public void SetValue(float value) {
    slider.value = value;
  }
}
