using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private GameObject panelMenuPrincipal;
    [SerializeField] private GameObject panelInstrucciones;
    [SerializeField] private GameObject panelTopMejores;
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private TMP_Dropdown opcionDropdown;

    public static string playerName { get; private set; } = "Player";
    public static int opcionSeleccionada { get; private set; } = 0;

    void Start()
    {
        MostrarMenuPrincipal();
    }
    public void StartGame()
    {
        string nombre = nameInputField.text.Trim();
        if (string.IsNullOrEmpty(nombre))
        {
            Debug.LogWarning("Por favor ingresa un nombre antes de comenzar.");
            return;
        }
        playerName = nombre;
        opcionSeleccionada = opcionDropdown.value;
        SceneManager.LoadScene("MainModel");
    }

    public void MostrarMenuPrincipal()
    {
        ActivarSolo(panelMenuPrincipal);
    }

    public void MostrarInstrucciones()
    {
        ActivarSolo(panelInstrucciones);
    }

    public void MostrarTopMejores()
    {
        ActivarSolo(panelTopMejores);
    }
    public void MostrarPanel(GameObject panel)
    {
        ActivarSolo(panel);
    }
    private void ActivarSolo(GameObject panelActivo)
    {
        panelMenuPrincipal.SetActive(panelActivo == panelMenuPrincipal);
        panelInstrucciones.SetActive(panelActivo == panelInstrucciones);
        panelTopMejores.SetActive(panelActivo == panelTopMejores);
    }
}
