using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static string[] Levels = new string[0];
    public static string MenuScene = "MainMenu";

    public float fadeTime = 1f;
    public float messageTime = 2f;
    public string message = "LEVEL CLEARED";

    [Header("Sounds")]
    public AudioClip buttonHover;
    public AudioClip buttonPress;
    public AudioClip spawn;
    public AudioClip respawnCountdown;
    public AudioClip gameOver;
    public AudioClip shot;
    public AudioClip hit;
    public AudioClip playerHurt;
    public AudioClip playerDeath;
    public AudioClip zombieHurt;

    static LevelManager instance;

    Texture2D black;
    AudioSource sfx;
    string text = "";
    float alpha;
    float timer;
    bool sawEnemy;
    bool loading;
    bool ourLoad;
    
    public static void PlayButtonHover() => Play(instance != null ? instance.buttonHover : null);
    public static void PlayButtonPress() => Play(instance != null ? instance.buttonPress : null);
    public static void PlaySpawn() => Play(instance != null ? instance.spawn : null);
    public static void PlayRespawnCountdown() => Play(instance != null ? instance.respawnCountdown : null);
    public static void PlayGameOver() => Play(instance != null ? instance.gameOver : null);

    public static void PlayShot() => Play(instance != null ? instance.shot : null);
    public static void PlayHit() => Play(instance != null ? instance.hit : null);
    public static void PlayPlayerHurt() => Play(instance != null ? instance.playerHurt : null);
    public static void PlayPlayerDeath() => Play(instance != null ? instance.playerDeath : null);
    public static void PlayZombieHurt() => Play(instance != null ? instance.zombieHurt : null);

    static void Play(AudioClip clip)
    {
        if (instance == null || clip == null) return;
        instance.sfx.PlayOneShot(clip);
    }

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;

        sfx = gameObject.AddComponent<AudioSource>();
        sfx.playOnAwake = false;

        black = new Texture2D(1, 1);
        black.SetPixel(0, 0, Color.black);
        black.Apply();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        sawEnemy = false;
        timer = 0.5f;

        if (System.Array.IndexOf(Levels, scene.name) >= 0)
            PlaySpawn();

        if (ourLoad)
        {
            ourLoad = false;
            return;
        }

        StopAllCoroutines();
        alpha = 0f;
        text = "";
        loading = false;
    }

    void Update()
    {
        if (loading) return;
        if (System.Array.IndexOf(Levels, SceneManager.GetActiveScene().name) < 0) return;

        timer -= Time.deltaTime;
        if (timer > 0f) return;
        timer = 0.5f;

        if (GameObject.FindWithTag("Enemy") != null)
            sawEnemy = true;
        else if (sawEnemy)
            StartCoroutine(NextLevel());
    }

    IEnumerator NextLevel()
    {
        loading = true;
        text = message;
        yield return Fade(1f);
        yield return new WaitForSeconds(messageTime);

        int i = System.Array.IndexOf(Levels, SceneManager.GetActiveScene().name);
        string next = i + 1 < Levels.Length ? Levels[i + 1] : MenuScene;

        ourLoad = true;
        yield return SceneManager.LoadSceneAsync(next);

        text = "";
        yield return Fade(0f);
        loading = false;
    }

    IEnumerator Fade(float to)
    {
        while (!Mathf.Approximately(alpha, to))
        {
            alpha = Mathf.MoveTowards(alpha, to, Time.deltaTime / fadeTime);
            yield return null;
        }
    }

    void OnGUI()
    {
        if (alpha <= 0f) return;

        GUI.color = new Color(1f, 1f, 1f, alpha);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), black);

        if (text != "")
        {
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            GUI.skin.label.fontSize = 48;
            GUI.Label(new Rect(0, 0, Screen.width, Screen.height), text);
        }
    }
}
