using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("Audio Clips")]
    public List<Sound> sounds = new List<Sound>();

    private Dictionary<string, AudioClip> soundDict = new Dictionary<string, AudioClip>();

    float lastFailTime = 0f;
    float failCooldown = 0.2f;

    Dictionary<string, float> lastPlayTime = new Dictionary<string, float>();
    public float globalCooldown = 0.1f; // Global cooldown để chống spam âm thanh

    void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Load vào Dictionary
        foreach (var s in sounds)
        {
            soundDict[s.name] = s.clip;
        }
    }

    // 🎵 Play BGM
    public void PlayBGM(string name)
    {
        if (!soundDict.ContainsKey(name)) return;

        bgmSource.clip = soundDict[name];
        bgmSource.loop = true;
        bgmSource.Play();
    }

    // 🔊 Play SFX
    public void PlaySFX(string name)
    {
        if (!soundDict.ContainsKey(name)) return;

        if (name == "fail")
        {
            if (Time.time - lastFailTime < failCooldown) return;
            lastFailTime = Time.time;
        }

        sfxSource.pitch = Random.Range(0.95f, 1.05f);

        sfxSource.PlayOneShot(soundDict[name]);

        if (name == "coin")
        {
            sfxSource.pitch = Random.Range(1.1f, 1.3f);
        }
        else
        {
            sfxSource.pitch = Random.Range(0.95f, 1.05f);
        }
    }

    public void PlayZombieSFX(string name, float distance)
    {
        if (!soundDict.ContainsKey(name)) return;

        // ===== 1. Cooldown riêng =====
        float cd = globalCooldown;

        if (name == "zombie_attack")
            cd = 0.3f; // attack thưa hơn

        if (lastPlayTime.ContainsKey(name))
        {
            if (Time.time - lastPlayTime[name] < cd)
                return;
        }

        lastPlayTime[name] = Time.time;

        // ===== 2. Distance =====
        if (distance > 15f) return;

        // ===== 3. Random (tránh đồng thanh) =====
        if (name == "zombie_attack" && Random.value > 0.7f)
            return;

        // ===== 4. Volume =====
        float volume = Mathf.Clamp01(1f - (distance / 15f));

        if (name == "zombie_walk")
            volume *= 0.3f; // nhỏ
        else if (name == "zombie_attack")
            volume *= 0.6f; // vừa

        // ===== 5. Pitch =====
        if (name == "zombie_attack")
            sfxSource.pitch = Random.Range(0.9f, 1.1f);
        else
            sfxSource.pitch = Random.Range(0.95f, 1.05f);

        // ===== 6. Play =====
        sfxSource.PlayOneShot(soundDict[name], volume);
    }

   
    public void FadeOutBGM(float duration)
    {
        StartCoroutine(FadeOutCoroutine(duration));
    }

    private IEnumerator FadeOutCoroutine(float duration)
    {
        float startVolume = bgmSource.volume;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(startVolume, 0f, t / duration);
            yield return null;
        }

        bgmSource.volume = 0f;
        bgmSource.Stop();
    }

    // 🎚️ Volume
    public void SetBGMVolume(float v)
    {
        bgmSource.volume = v;
    }

    public void SetSFXVolume(float v)
    {
        sfxSource.volume = v;
    }

    

}

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
}