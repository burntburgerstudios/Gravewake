using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] TMP_Dropdown levelDropdown;
    [SerializeField] string[] levelScenes;
    [SerializeField] string mainMenuScene = "MainMenu";

    void Start()
    {
        LevelManager.Levels = levelScenes;
        LevelManager.MenuScene = mainMenuScene;

        foreach (Button b in FindObjectsByType<Button>(FindObjectsInactive.Include))
            AddButtonSounds(b);
    }

    static void AddButtonSounds(Button b)
    {
        b.onClick.AddListener(LevelManager.PlayButtonPress);

        EventTrigger trigger = b.GetComponent<EventTrigger>();
        if (trigger == null) trigger = b.gameObject.AddComponent<EventTrigger>();

        var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        entry.callback.AddListener(_ => { if (b.interactable) LevelManager.PlayButtonHover(); });
        trigger.triggers.Add(entry);
    }

    public void Play() => SceneManager.LoadScene(levelScenes[levelDropdown.value]);

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}