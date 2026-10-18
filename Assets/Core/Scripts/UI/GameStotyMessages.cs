
using UnityEngine;

public class GameStoryMessages : MonoBehaviour
{
    [TextArea(2, 4)]
    public string introMessage =
        "You must find the invisible key... in the place zombies fear most.";

    [TextArea(2, 4)]
    public string endingMessage =
        "You did it! Now enjoy being eaten alive by zombies.";

    private bool endingShown = false;

    void Start()
    {
        if (TextDisplayManager.Instance != null)
        {
            TextDisplayManager.Instance.ShowMessage(introMessage);
        }
        else
        {
            Debug.LogWarning("TextDisplayManager was not found!");
        }
    }

    public void ShowEndingMessage()
    {
        if (endingShown) return;

        endingShown = true;

        if (TextDisplayManager.Instance != null)
        {
            TextDisplayManager.Instance.ShowMessage(endingMessage);
        }
    }
}
