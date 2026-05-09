using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    public GameObject settingsPanel;

    // ▶ PLAY
    public void PlayGame()
    {
        SceneManager.LoadScene("SampleScene"); // tên scene game của bạn
        AudioManager.Instance.PlaySFX("click");
    }

    // ⚙ SETTINGS
    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
        AudioManager.Instance.PlaySFX("click");
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        AudioManager.Instance.PlaySFX("click");
    }

    // ❌ EXIT
    public void ExitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Debug.Log("Thoát game"); // chỉ thấy khi chạy trong Editor
    }
}