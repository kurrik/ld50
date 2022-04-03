using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class AttackBar : MonoBehaviour {
  private Slider slider;
  public Image fillImage;

  void Awake() {
    slider = GetComponent<Slider>();
  }

  public void SetValue(float value, Color color) {
    slider.value = value;
    fillImage.color = color;
  }
}
