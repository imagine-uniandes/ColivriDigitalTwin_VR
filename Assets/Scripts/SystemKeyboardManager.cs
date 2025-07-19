using UnityEngine;

public class KeyboardExample : MonoBehaviour
{
    private TouchScreenKeyboard keyboard;

    public void OpenKeyboard()
    {
        keyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default, false, false, false, false);
    }

    public void HideKeyboard()
    {
        if (keyboard != null)
        {
            keyboard.active = false;
        }
    }

    void Update()
    {
        if (keyboard != null && keyboard.status == TouchScreenKeyboard.Status.Done)
        {
            string inputText = keyboard.text;
            Debug.Log("Input Text: " + inputText);
            keyboard = null; // Reset keyboard
        }
    }
}