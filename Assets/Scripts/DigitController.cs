using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DigitController : MonoBehaviour
{
    [SerializeField] private TMP_Text digitText;
    private int valor = 0;
    private void Awake()
    {
        UpdateText();
    }
    public void Increase()
    {
        valor = (valor + 1)%10;
        UpdateText();
    }
    public void Decrease()
    {
        valor = (valor - 1)%10;
        UpdateText();
    }
    private void UpdateText()
    {
        digitText.text = valor.ToString();
    }
    public int GetValue() => valor;
}
