using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Goes on each car (Car1..Car4). Shows one color, reports color clicks, knows its price.
public class CarView : MonoBehaviour
{
    [Header("Colors")]
    [SerializeField] private GameObject[] _colorImages;   // RedCar, BlueCar, ...
    [SerializeField] private Button[] _colorButtons;      // the color squares, same order

    [Header("Price")]
    [SerializeField] private int _price = 0;              // 0 = free, always owned
    [SerializeField] private TMP_Text _priceText;         // optional

    private UnityAction[] _handlers;

    public event Action<int> ColorClicked;

    public int ColorCount => _colorImages.Length;
    public int Price => _price;

    private void Awake()
    {
        _handlers = new UnityAction[_colorButtons.Length];
        for (int i = 0; i < _colorButtons.Length; i++)
        {
            int index = i;   // copy i: avoids the loop variable capture bug
            _handlers[i] = () => OnColorButtonClicked(index);
        }
    }

    private void OnEnable()
    {
        for (int i = 0; i < _colorButtons.Length; i++)
        {
            _colorButtons[i].onClick.AddListener(_handlers[i]);
        }

        if (_priceText != null)
        {
            _priceText.text = _price.ToString("N0");
        }
    }

    private void OnDisable()
    {
        for (int i = 0; i < _colorButtons.Length; i++)
        {
            _colorButtons[i].onClick.RemoveListener(_handlers[i]);
        }
    }

    public void ShowColor(int index)
    {
        for (int i = 0; i < _colorImages.Length; i++)
        {
            _colorImages[i].SetActive(i == index);
        }
    }

    private void OnColorButtonClicked(int index)
    {
        ColorClicked?.Invoke(index);
    }
}