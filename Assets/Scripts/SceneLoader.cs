using UnityEngine.SceneManagement;

public static class SceneLoader
{
    public const string Registration = "RegistrationScene";
    public const string Main = "MainModel";
    public static void LoadRegistration() => SceneManager.LoadScene(Registration, LoadSceneMode.Single);
    public static void LoadMain() => SceneManager.LoadScene(Main, LoadSceneMode.Single);
}
