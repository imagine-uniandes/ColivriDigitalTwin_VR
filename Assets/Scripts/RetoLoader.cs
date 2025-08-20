using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class TripleDigits
{
    public TextMeshProUGUI d1;
    public TextMeshProUGUI d2;
    public TextMeshProUGUI d3;

    public void SetDigits(string threeDigits)
    {
        if (string.IsNullOrEmpty(threeDigits) || threeDigits.Length < 3)
            return;

        if (d1) d1.text = threeDigits[0].ToString();
        if (d2) d2.text = threeDigits[1].ToString();
        if (d3) d3.text = threeDigits[2].ToString();
    }

    public void Clear()
    {
        if (d1) d1.text = "";
        if (d2) d2.text = "";
        if (d3) d3.text = "";
    }
}

public class RetoLoader : MonoBehaviour
{
    [Header("UI Pistas (texto completo)")]
    [SerializeField] private TextMeshProUGUI pista1UI;
    [SerializeField] private TextMeshProUGUI pista2UI;
    [SerializeField] private TextMeshProUGUI pista3UI;
    [SerializeField] private TextMeshProUGUI pista4UI;
    [SerializeField] private TextMeshProUGUI pista5UI;

    [Header("UI Dígitos por pista (3 TMP_Text por pista)")]
    [SerializeField] private TripleDigits pista1Digits;
    [SerializeField] private TripleDigits pista2Digits;
    [SerializeField] private TripleDigits pista3Digits;
    [SerializeField] private TripleDigits pista4Digits;
    [SerializeField] private TripleDigits pista5Digits;

    public RetosList retosList;
    private int currentIndex = 0;

    public int TotalRetos => (retosList?.retos?.Count) ?? 0;
    public int CurrentIndex => currentIndex;

    private void Awake()
    {
        TextAsset ta = Resources.Load<TextAsset>("Retos");
        if (ta == null)
        {
            Debug.LogError("RetoLoader: No se encontró Resources/Retos.json (usa nombre 'Retos').");
            retosList = new RetosList { retos = new List<Reto>() };
            return;
        }

        // Envuelve el array para que JsonUtility lo parsee
        string wrapped = "{\"retos\":" + ta.text + "}";
        retosList = JsonUtility.FromJson<RetosList>(wrapped);

        if (retosList == null || retosList.retos == null || retosList.retos.Count == 0)
        {
            Debug.LogError("RetoLoader: No se pudieron cargar retos desde el JSON.");
            retosList = new RetosList { retos = new List<Reto>() };
        }
    }

    public void ResetSequence(bool shuffle = false)
    {
        currentIndex = 0;
        if (shuffle && TotalRetos > 1) Shuffle(retosList.retos);
        UpdatePistasUI();
    }

    public Reto GetCurrentReto()
    {
        if (retosList == null || retosList.retos == null || retosList.retos.Count == 0) return null;
        currentIndex = Mathf.Clamp(currentIndex, 0, retosList.retos.Count - 1);
        return retosList.retos[currentIndex];
    }

    public bool LoadNextReto()
    {
        if (retosList == null || retosList.retos == null) return false;
        if (currentIndex + 1 < retosList.retos.Count)
        {
            currentIndex++;
            UpdatePistasUI();
            return true;
        }
        return false;
    }

    public void UpdatePistasUI()
    {
        var r = GetCurrentReto();
        if (r == null) return;

        // 1) Actualiza textos completos (si los usas)
        if (pista1UI) pista1UI.text = r.pista1;
        if (pista2UI) pista2UI.text = r.pista2;
        if (pista3UI) pista3UI.text = r.pista3;
        if (pista4UI) pista4UI.text = r.pista4;
        if (pista5UI) pista5UI.text = r.pista5;

        // 2) Extrae y coloca los tres dígitos de cada pista
        if (pista1Digits != null) pista1Digits.SetDigits(ExtractThreeDigits(r.pista1));
        if (pista2Digits != null) pista2Digits.SetDigits(ExtractThreeDigits(r.pista2));
        if (pista3Digits != null) pista3Digits.SetDigits(ExtractThreeDigits(r.pista3));
        if (pista4Digits != null) pista4Digits.SetDigits(ExtractThreeDigits(r.pista4));
        if (pista5Digits != null) pista5Digits.SetDigits(ExtractThreeDigits(r.pista5));
    }

    /// <summary>
    /// Toma la subcadena de los primeros 3 dígitos de la pista (antes del primer espacio).
    /// Ej: "738 ningun..." -> "738"
    /// </summary>
    private string ExtractThreeDigits(string pista)
    {
        if (string.IsNullOrEmpty(pista)) return "";
        // Split por espacio y toma el primer token
        int space = pista.IndexOf(' ');
        string token = (space > 0) ? pista.Substring(0, space) : pista;

        // Sanitiza y asegura largo 3 (por si viniera con algo raro)
        token = token.Trim();
        if (token.Length >= 3) token = token.Substring(0, 3);
        return token;
    }

    private void Shuffle(List<Reto> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = Random.Range(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
