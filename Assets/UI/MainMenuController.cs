using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.VFX;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public VisualElement ui;

    public Button playButton;
    public Button loadButton;
    public Button quitButton;

    //ref to our UIDocument
    private void Awake()
    {
        ui = GetComponent<UIDocument>().rootVisualElement;
    }

    //get the 3 buttons by name onEnable
    private void OnEnable()
    {
        playButton = ui.Q<Button>("play-button");
        playButton.clicked += OnPlayButtonClicked;

        loadButton = ui.Q<Button>("load-button");
        loadButton.clicked += OnLoadButtonClicked;

        quitButton = ui.Q<Button>("quit-button");
        quitButton.clicked += OnQuitButtonClicked;
    }

    // button clicked actions
    private void OnPlayButtonClicked()
    {
        Debug.Log("Play Button Clicked");
        SceneManager.LoadScene("Level1");
    }

    private void OnLoadButtonClicked()
    {
        Debug.Log("Load Button Clicked");
    }

    private void OnQuitButtonClicked()
    {
        Debug.Log("Quit Button Clicked");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
