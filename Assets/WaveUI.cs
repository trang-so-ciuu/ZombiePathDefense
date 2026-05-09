using UnityEngine;
using TMPro;
using System.Collections;

public class WaveUI : MonoBehaviour
{
    public TextMeshProUGUI waveCompleteText;
    public GameObject startButton;
    public TextMeshProUGUI countdownText; 

    public TextMeshProUGUI waveText;

    public void ShowWaveComplete(int wave)
    {
        waveCompleteText.gameObject.SetActive(true);
        waveCompleteText.text = " WAVE " + wave + " COMPLETE!";
    }

    public void HideWaveComplete()
    {
        waveCompleteText.gameObject.SetActive(false);
    }

    public void ShowStartButton()
    {
        startButton.SetActive(true);
        countdownText.gameObject.SetActive(false);
    }

    public void OnClickStart()
    {
        startButton.SetActive(false);
        AudioManager.Instance.PlaySFX("click");
        StartCoroutine(Countdown());
    }

    IEnumerator Countdown()
    {
        countdownText.gameObject.SetActive(true);

        for (int i = 3; i > 0; i--)
        {
            countdownText.text = i.ToString();
            yield return new WaitForSeconds(1f);
        }

        countdownText.text = "GO!";
        yield return new WaitForSeconds(0.5f);
        countdownText.gameObject.SetActive(false);

        // CHỈ GỌI StartWave, không bật isPlaying ở đây. 
        // Hãy để WaveManager lo việc bật isPlaying khi mọi thứ đã sẵn sàng.
        WaveManager.Instance.StartWave();

    }
}