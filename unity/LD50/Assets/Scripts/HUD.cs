using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUD : MonoBehaviour {
  public TMPro.TextMeshProUGUI scoreText;
  private int _realScore;
  private float _displayScore;
  private float _animationDuration = 0.3f;
  private float _velocity;

  public void Start() {
    SetScore(0);
  }

  public void SetScore(int amount) {
    _realScore = amount;
  }

  public void FixedUpdate() {
    if (_realScore == 0) {
      scoreText.text = "";
    } else {
      _displayScore = Mathf.SmoothDamp(_displayScore, _realScore, ref _velocity, _animationDuration);
      scoreText.text = string.Format("{0}", Mathf.RoundToInt(_displayScore));
    }
  }
}
