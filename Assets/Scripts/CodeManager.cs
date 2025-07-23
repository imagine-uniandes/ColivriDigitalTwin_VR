using System.Linq;
using UnityEngine;
using TMPro;
using System;

public class CodeManager : MonoBehaviour
{
    [Header("Carga de la pista")]
    [SerializeField] private RetoLoader retoLoader;
    private string respuesta;
    [Header("UI de digitos")]
    [SerializeField] private TMP_Text[] digitTexts;
    private int[] digitValues = new int[3];
    [Header("Feedback")]
    [SerializeField] private GameObject panelCorrecto;
    [SerializeField] private GameObject panelVolverMenu;
    [SerializeField] private GameObject panelCerca;
    [SerializeField] private GameObject panelClave;
    [SerializeField] private TextMeshProUGUI txtPosiciones;
    [SerializeField] private TextMeshProUGUI txtWrongPos;

    private float startTime;
    public static event Action<float> OnCodeSuccessEvent;
    

    void Start()
    {
        InitializeAnswer();
        ResetSession();
        
    }
  
    public void ResetSession()
    {
        InitializeAnswer();
        for (int i = 0; i < digitValues.Length; i++)
            digitValues[i] = 0;
        UpdateDisplay();
        panelCorrecto.SetActive(false);
        panelCerca.SetActive(false);
        panelClave.SetActive(true);
        panelVolverMenu.SetActive(false);
        startTime = Time.time;

    }
    



    private void InitializeAnswer()
    {
        if (retoLoader != null && retoLoader.reto != null)
        {
            respuesta = retoLoader.reto.respuesta;
        }
    }
    /*
    private void MostrarPistas()
    {
        if (retoLoader.reto != null && pistasUI.Length >= 5)
        {
            pistasUI[0].text = retoLoader.reto.pista1;
            pistasUI[1].text = retoLoader.reto.pista2;
            pistasUI[2].text = retoLoader.reto.pista3;
            pistasUI[3].text = retoLoader.reto.pista4;
            pistasUI[4].text = retoLoader.reto.pista5;
        }
    }
    */
    public void IncreaseDigit(int index)
    {
        panelCorrecto.SetActive(false);
        panelCerca.SetActive(false);
        panelClave.SetActive(true);

        digitValues[index] = (digitValues[index] +1) %10;
        UpdateDisplay();
    }

    public void DecreaseDigit(int index)
    {
        panelCorrecto.SetActive(false);
        panelCerca.SetActive(false);
        panelClave.SetActive(true);
        digitValues[index] = (digitValues[index] +9) %10;
        UpdateDisplay();
    }

    public void OnClear()
    {
        panelCorrecto.SetActive(false);
        panelCerca.SetActive(false);
        panelClave.SetActive(true);
        for (int i = 0; i < digitValues.Length; i++)
            digitValues[i] = 0;
        UpdateDisplay();
    }

    public void OnValidate()
    {
        if (respuesta == null)
        {
            InitializeAnswer();
        }
        if (string.IsNullOrEmpty(respuesta))
        {
            Debug.LogWarning("No se puede validar: respuesta no est� inicializada");
            return;
        }
        if (panelCorrecto == null || panelCerca == null || panelClave == null)
        {
            Debug.LogWarning("Paneles no est�n asignados en el inspector");
            return;
        }
        panelCorrecto.SetActive(false);
        panelCerca.SetActive(false);

        string currentInput = string.Concat(digitValues.Select(d => d.ToString()));

        if (currentInput == respuesta)
        {
            panelCorrecto.SetActive(true);
            panelClave.SetActive(false);
            panelVolverMenu.SetActive(true);
            float elapsed = Time.time - startTime;
            string playerName = PlayerPrefs.GetString("PlayerName", "Jugador");
            LeaderboardManager.Instance.AddEntry(playerName, elapsed);
            GameFlowManager.Instance.OnCodeSuccess(elapsed);
            OnCodeSuccessEvent?.Invoke(elapsed);
            Debug.Log($"Código correcto ingresado por {playerName} en {elapsed} segundos.");
            PlayerDataManager.Instance.RecordSessionTime(elapsed);

        }
        else
        {
            int good = 0, wrong = 0;
            for (int i = 0; i < 3; i++)
            {
                char c = currentInput[i];
                if (c == respuesta[i])
                    good++;
                else if (respuesta.Contains(c.ToString()))
                    wrong++;
            }
            if (txtPosiciones != null)
                txtPosiciones.SetText("{0}", good);
            if (txtWrongPos != null)
                txtWrongPos.SetText("{0}", wrong);

            panelCerca.SetActive(true);
            panelClave.SetActive(false);
        }
    }

    private void UpdateDisplay()
    {
        if (digitTexts != null)
        {
            for (int i = 0; i < digitTexts.Length; i++)
            {
                if (digitTexts[i] != null)
                    digitTexts[i].text = digitValues[i].ToString();
            }
        }
    }
}