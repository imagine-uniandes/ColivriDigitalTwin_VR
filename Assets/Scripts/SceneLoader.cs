using UnityEngine.SceneManagement;
using UnityEngine;

public static class SceneLoader
{
    public const string Registration = "RegistrationScene";
    public const string Main = "MainModel";
    public static void LoadRegistration()
    {
        if (PersistentSceneLoader.Instance != null)
        {
            PersistentSceneLoader.Instance.LoadSceneAsync(Registration);
        }
        else
        {
            Debug.LogError("PersistentSceneLoader no encontrado. Cargando de forma síncrona.");
            SceneManager.LoadScene(Registration, LoadSceneMode.Single);
        }
    }

    public static void LoadMain()
    {
        if (PersistentSceneLoader.Instance != null)
        {
            PersistentSceneLoader.Instance.LoadSceneAsync(Main);
        }
        else
        {
            Debug.LogError("PersistentSceneLoader no encontrado. Cargando de forma síncrona.");
            SceneManager.LoadScene(Main, LoadSceneMode.Single);
        }
    }
}
