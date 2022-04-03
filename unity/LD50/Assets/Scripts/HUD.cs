using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour {
  public TMPro.TextMeshProUGUI scoreText;
  public TMPro.TextMeshProUGUI remainingText;
  public Image warningIcon;
  public float warningIconDuration = 1.0f;

  private int _realScore;
  private float _displayScore;
  private float _animationDuration = 0.3f;
  private float _velocity;

  public void Start() {
    SetScore(0);
    warningIcon.gameObject.SetActive(false);
    SetRemaining("");
  }

  public void SetScore(int amount) {
    _realScore = amount;
  }

  public void SetRemaining(string text) {
    remainingText.text = text;
  }

  public void ShowWarningIcon() {
    StartCoroutine(HandleShowWarningIcon());
  }

  private IEnumerator HandleShowWarningIcon() {
    warningIcon.gameObject.SetActive(true);
    yield return new WaitForSeconds(warningIconDuration);
    warningIcon.gameObject.SetActive(false);
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
