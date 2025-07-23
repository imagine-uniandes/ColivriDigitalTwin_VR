using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance;

    [Header("Referencias")]
    public Transform playerStart;
    public CanvasGroup fadeCanvas;
    public GameObject menuPanel;
    public GameObject panelIngresoDatos;
    public GameObject gameOverPanel;
    public TMP_Text gameOverText;
    public GameObject top10Panel;
    public Transform cameraTransform;
    public Vector3 cameraTopPos;
    public Vector3 cameraTopRot;

    float delayOnSuccess = 3f;
    string playerName;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void StartGameSession()
    {
        menuPanel.SetActive(false);
        top10Panel.SetActive(false);
        gameOverPanel.SetActive(false);
        fadeCanvas.alpha = 0f;
        panelIngresoDatos.SetActive(true);
        playerName = PlayerPrefs.GetString("PlayerName", "Jugador");

        var cm = FindObjectOfType<CodeManager>();
        if (cm != null)
        {
            cm.ResetSession();
        }

        var ct = FindObjectOfType<CountdownTimer>();
        if (ct != null)
        {
            ct.ResetTimer();
        }

        bool isEasy = PlayerPrefs.GetInt("DifficultyLevel", 1) == 1;
        TeleportManager.Instance.ShowHelperTeleports(isEasy);
    }

    public void OnCodeSuccess(float elapsed)
    {
        StartCoroutine(EndSequence(elapsed));
    }

    IEnumerator EndSequence(float elapsed)
    {
        yield return new WaitForSeconds(delayOnSuccess);
        yield return StartCoroutine(FadeOutCoroutine());
        LeaderboardManager.Instance.AddEntry(playerName, elapsed);
        panelIngresoDatos.SetActive(false);

        var player = GameObject.FindGameObjectWithTag("Player");
        if(player != null)
        {
            player.transform.position = playerStart.position;
        }
        
        cameraTransform.position = cameraTopPos;
        cameraTransform.rotation = Quaternion.Euler(cameraTopRot);
        fadeCanvas.alpha = 0f;

        top10Panel.SetActive(true);
        LeaderboardManager.Instance.DisplayLeaderboard();

    }

    IEnumerator FadeOutCoroutine()
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime;
            fadeCanvas.alpha = t;
            yield return null;
        }
    }

    public void HandleTimeUp()
    {
        gameOverText.text = "¡Se acabó el tiempo! Vuelve a intentarlo";
        gameOverPanel.SetActive(true);
    }

    public void Retry()
    {
        gameOverPanel.SetActive(false);
        var cm = FindObjectOfType<CodeManager>();
        if(cm != null )
        {
            cm.ResetSession();
        }
        var ct = FindObjectOfType<CountdownTimer>();
        if( ct != null)
        {
            ct.ResetTimer();
        }


        gameOverPanel.SetActive(false);
        top10Panel.SetActive(false);
        menuPanel.SetActive(true);
        panelIngresoDatos.SetActive(false);
    }

}