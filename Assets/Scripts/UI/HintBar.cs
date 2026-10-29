using TMPro;
using UnityEngine;

public class HintBar : MonoBehaviour
{
    [SerializeField] private TMP_Text _hintText;

    private void Awake() { HideHint(); }
    public void ShowHint(string message) { _hintText.text = message; }
    public void HideHint() { _hintText.text = string.Empty; }
}