using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameFlowManager : MonoBehaviour
{
    public CanvasGroup fadeCanvas;
    public Transform uiTop10Transform;

   
    public void HandleWin()
    {
        StartCoroutine(EndGameSequence());
    }

    private IEnumerator EndGameSequence()
    {
        yield return new WaitForSeconds(3f);
        float t = 0f;
        while (t < 1f)
        {
            fadeCanvas.alpha = Mathf.Lerp(0, 1, t);
            t += Time.deltaTime;
            yield return null;
        }

        PlayerResetter.Instance.ResetPosition();

        t = 0f;
        while (t < 1f)
        {
            fadeCanvas.alpha = Mathf.Lerp(1, 0, t);
            t += Time.deltaTime;
            yield return null;
        }

        uiTop10Transform.gameObject.SetActive(true);
        LeaderboardManager.Instance.HighlightPlayer();
    }
}
