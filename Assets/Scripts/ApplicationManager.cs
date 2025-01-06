using UnityEngine;
using UnityEngine.UI;

public class ApplicationManager : MonoBehaviour
{
    public Button QuitApplicationButton;

    private void Awake()
    {
        QuitApplicationButton.onClick.AddListener(OnQuitApplicationButtonPressed);
    }

    public void OnQuitApplicationButtonPressed()
    {
        Application.Quit();
    }
}
