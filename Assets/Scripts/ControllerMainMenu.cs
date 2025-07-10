using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private GameObject panelInstrucciones;
    [SerializeField] private GameObject panelTopMejores;
    [SerializeField] private GameObject panelMenuPrincipal;

    public void StartGame()
    {
        SceneManager.LoadScene("MainModel"); 
    }
    public void IngresaNombre()
    {
        panelMenuPrincipal.SetActive(true);
        panelInstrucciones.SetActive(false);
        panelTopMejores.SetActive(false);
    }
    public void Instructions()
    {
    
        panelMenuPrincipal.SetActive(false);
        panelInstrucciones.SetActive(true);
        panelTopMejores.SetActive(false);
    }
    public void TopMejores()
    {
        panelMenuPrincipal.SetActive(false);
        panelInstrucciones.SetActive(false);
        panelTopMejores.SetActive(true);
    }
}
