using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHint : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private HintBar _hintBar;
    [SerializeField] private string _message = "";

    public void OnPointerEnter(PointerEventData eventData) { _hintBar.ShowHint(_message); }
    public void OnPointerExit(PointerEventData eventData) { _hintBar.HideHint(); }

    private void OnDisable()
    {
        if (_hintBar != null)
        {
            _hintBar.HideHint();
        }
    }
}