using UnityEngine;

// Lives with the scene's hunter. The second beat grows audible as the chase
// starts; the slower pulse leaves room to hear directional footsteps in search.
public class FearDirector : MonoBehaviour
{
    private EnemyAI hunter;
    private AudioSource pulse;
    private AudioClip heartbeat;
    private float tension;

    private void Start()
    {
        hunter = GetComponent<EnemyAI>();
        if (hunter == null) { enabled = false; return; }
        const int sampleRate = 22050;
        float[] samples = new float[sampleRate];
        for (int i = 0; i < samples.Length; i++)
        {
            float t = i / (float)sampleRate;
            samples[i] = Beat(t - 0.04f, 70f) + 0.58f * Beat(t - 0.22f, 84f);
        }
        heartbeat = AudioClip.Create("The hunt", samples.Length, 1, sampleRate, false);
        heartbeat.SetData(samples, 0);
        pulse = gameObject.AddComponent<AudioSource>();
        pulse.clip = heartbeat;
        pulse.spatialBlend = 0f;
        pulse.loop = true;
        pulse.volume = 0f;
        pulse.Play();
    }

    private static float Beat(float seconds, float frequency)
    {
        if (seconds < 0f || seconds > 0.15f) return 0f;
        return Mathf.Sin(seconds * Mathf.PI * 2f * frequency) * Mathf.Exp(-seconds * 46f);
    }

    private void Update()
    {
        float target = 0f;
        if (hunter.currentState == EnemyAI.EnemyState.Chasing) target = 1f;
        else if (hunter.currentState == EnemyAI.EnemyState.Searching) target = 0.28f;
        tension = Mathf.MoveTowards(tension, target, Time.deltaTime * (target > tension ? 1.8f : 0.55f));
        pulse.pitch = Mathf.Lerp(0.85f, 1.4f, tension);
        pulse.volume = 0.34f * tension;
    }

    private void OnDestroy()
    {
        if (heartbeat != null) Destroy(heartbeat);
    }
}
