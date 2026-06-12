using System.Collections.Generic;
using UnityEngine;

// Sunete procedurale simple (fara fisiere audio). Genereaza tonuri sinus scurte
// pentru evenimentele cheie - adauga "feel" stil arcade.
public class SoundManager : MonoBehaviour
{
    public enum Sfx { Pickup, LevelUp, GameOver, Hurt, Chest }

    private static SoundManager instance;
    private AudioSource src;
    private readonly Dictionary<Sfx, AudioClip> clips = new();

    public static void Play(Sfx sfx)
    {
        EnsureInstance();
        if (instance == null) return;
        if (!instance.clips.TryGetValue(sfx, out var clip)) return;
        instance.src.PlayOneShot(clip, 0.5f);
    }

    static void EnsureInstance()
    {
        if (instance != null) return;
        GameObject go = new GameObject("SoundManager");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<SoundManager>();
        instance.src = go.AddComponent<AudioSource>();
        instance.GenerateClips();
    }

    void GenerateClips()
    {
        clips[Sfx.Pickup]   = Tone(880f, 0.07f, 0.3f);
        clips[Sfx.LevelUp]  = Sweep(523f, 1046f, 0.25f, 0.3f);   // do -> do octava
        clips[Sfx.GameOver] = Sweep(440f, 110f, 0.6f, 0.35f);    // coboara
        clips[Sfx.Hurt]     = Tone(160f, 0.12f, 0.4f);
        clips[Sfx.Chest]    = Sweep(660f, 1320f, 0.3f, 0.3f);
    }

    // Ton sinus simplu cu fade out
    AudioClip Tone(float freq, float dur, float vol)
    {
        int rate = 44100;
        int n = Mathf.CeilToInt(rate * dur);
        var data = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / rate;
            float env = 1f - (float)i / n;           // fade liniar
            data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * env * vol;
        }
        var clip = AudioClip.Create("tone", n, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }

    // Ton care aluneca intre 2 frecvente
    AudioClip Sweep(float f0, float f1, float dur, float vol)
    {
        int rate = 44100;
        int n = Mathf.CeilToInt(rate * dur);
        var data = new float[n];
        float phase = 0f;
        for (int i = 0; i < n; i++)
        {
            float p = (float)i / n;
            float freq = Mathf.Lerp(f0, f1, p);
            phase += 2f * Mathf.PI * freq / rate;
            float env = Mathf.Sin(Mathf.PI * p);     // fade in+out
            data[i] = Mathf.Sin(phase) * env * vol;
        }
        var clip = AudioClip.Create("sweep", n, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
