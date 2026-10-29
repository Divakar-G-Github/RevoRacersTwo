using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class BackButton : MonoBehaviour
{
    [SerializeField] private UIManager _uiManager;
    private Button _button;

    private void Awake() { _button = GetComponent<Button>(); }
    private void OnEnable() { _button.onClick.AddListener(OnClicked); }
    private void OnDisable() { _button.onClick.RemoveListener(OnClicked); }
    private void OnClicked() { _uiManager.Back(); }
}