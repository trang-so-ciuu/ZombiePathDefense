using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Data")]
    public int humanCount;
    public int zombieCount;
    public event Action OnDataChanged;

    [Header("UI")]
    public GameObject resultPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI coinText;
    public Color winColor = Color.yellow;
    public Color loseColor = Color.red;

    //private bool isGameStarted = false;  //  chặn check lúc khởi tạo
    private bool isGameOver = false;     //  chặn gọi ShowResult nhiều lần

    public bool isPlaying = false; // 🔥 chỉ bắt đầu check win/lose khi zombie được set >

    private bool hasSpawnedZombie = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        resultPanel.SetActive(false);
        AudioManager.Instance.PlayBGM("main");
    }

    // ================= DATA =================
    public void SetHuman(int value)
    {
        humanCount = value;
        OnDataChanged?.Invoke();
        //TryStartGame();
    }


    public void SetZombie(int value)
    {
        zombieCount = value;

        if (value > 0)
            hasSpawnedZombie = true;

        OnDataChanged?.Invoke();

    }

    // Chỉ bắt đầu check khi cả 2 đã được set > 0
    //void TryStartGame()
    //{
       // if (!isGameStarted && humanCount > 0 && zombieCount > 0)
        //    isGameStarted = true;
    //}


    public void ChangeHuman(int value)
    {
        humanCount += value;

        if (humanCount < 0) humanCount = 0;


        OnDataChanged?.Invoke();

        if (isPlaying && !isGameOver)
        {
            CheckGameState();
        }
    }

    public void ChangeZombie(int value)
    {
        zombieCount += value;
        
        if (zombieCount < 0) zombieCount = 0;

        // Chỉ check khi đang chơi, chưa xong game và CÓ ít nhất 1 zombie từng tồn tại
        if (isPlaying && !isGameOver)
        {
            CheckGameState();
        }
    }

    // ================= WIN / LOSE =================
    void CheckGameState()
    {
        if (!isPlaying || isGameOver) return;
        if (!hasSpawnedZombie) return;

        if (humanCount <= 0)
        {
            Debug.Log("LOSE");
            isGameOver = true;
            StartCoroutine(EndGameSequence(false));
            return;
        }

        if (zombieCount <= 0)
        {
            Debug.Log("WIN");
            isPlaying = false;

            RewardWave();// 🎁 Thưởng sau khi thắng wave

            if (WaveManager.Instance.currentWave >= WaveManager.Instance.maxWave)
            {
                isGameOver = true;
                StartCoroutine(EndGameSequence(true));
            }
            else
            {
                StartCoroutine(NextWaveWithUI());
            }
        }
    }

    void ShowResult(bool isWin)
    {
        Time.timeScale = 0f;
        resultPanel.SetActive(true);

        titleText.text = isWin ? "VICTORY" : "GAME OVER";
        titleText.color = isWin ? winColor : loseColor;

        // ✅ CHỈ dùng CoinManager
        coinText.text = " " + CoinManager.Instance.currentCoin;
    }

    // ================= BUTTON =================
    public void NewGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ExitGame()
    {
        Debug.Log("Exit Game");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }

    IEnumerator NextWaveWithUI()
    {
        AudioManager.Instance.PlaySFX("wave_complete");

        WaveUI ui = FindFirstObjectByType<WaveUI>();

        // 1. Hiện text
        ui.ShowWaveComplete(WaveManager.Instance.currentWave);

        // 2. Đợi 2s
        yield return new WaitForSeconds(2f);

        // 3. Ẩn text
        ui.HideWaveComplete();

        // 4. Update text wave tiếp theo
        ui.waveText.text = "Wave " + (WaveManager.Instance.currentWave + 1);

        // 5. Hiện nút Start
        ui.ShowStartButton();
        
    }

    IEnumerator EndGameSequence(bool isWin)
    {
        // 1. Fade nhạc nền
        AudioManager.Instance.FadeOutBGM(1.2f);

        // 2. Giảm âm zombie
        AudioManager.Instance.SetSFXVolume(0.3f);

        // 3. CHỜ (REALTIME)
        yield return new WaitForSecondsRealtime(1.0f);

        // 4. Phát âm thanh
        if (isWin)
            AudioManager.Instance.PlaySFX("win");
        else
            AudioManager.Instance.PlaySFX("lose");

        // 5. Hiện UI (lúc này mới pause game)
        ShowResult(isWin);
    }

    void RewardWave()
    {
        int wave = WaveManager.Instance.currentWave;

        int reward = 50 + (wave * 10);

        CoinManager.Instance.AddCoin(reward);

        Debug.Log($"🎁 Wave Reward: {reward} (Wave {wave})");
    }
}