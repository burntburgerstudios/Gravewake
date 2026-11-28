using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class EscToMenu : MonoBehaviour
{
    [SerializeField] string mainMenuScene = "MainMenu";
    [SerializeField] InputAction menu = new InputAction("Menu", InputActionType.Button, "<Keyboard>/escape");

    void OnEnable() => menu.Enable();
    void OnDisable() => menu.Disable();

    void Update()
    {
        if (menu.WasPressedThisFrame())
            SceneManager.LoadScene(mainMenuScene);
    }
}