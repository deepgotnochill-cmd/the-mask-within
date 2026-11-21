using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

// Scene-local events use the house's existing character and light fixtures.
public sealed class HouseDread : MonoBehaviour
{
    private Transform player;
    private Camera view;
    private EnemyAI hunter;
    private Crank crank;
    private Light[] fixtures;
    private AudioSource roomSound;
    private AudioSource echo;
    private AudioSource pulse;
    private AudioClip knockClip;
    private AudioClip breathClip;
    private AudioClip heartClip;
    private AudioClip stepClip;
    private GameObject apparition;
    private Renderer[] figureRenderers;
    private Vector3 previousPosition;
    private Vector3 lastFootstep;
    private float stillFor;
    private float nextEvent;
    private float pulseLevel;
    private bool wasMoving;
    private bool busy;
    private bool stairsObserved;
    private bool keyObserved;
    private int sequence;
    private Light dimmedLight;
    private float savedIntensity;
    private bool suspended;

    public static void Attach(PlayerController controller)
    {
        var director = new GameObject("House atmosphere").AddComponent<HouseDread>();
        director.player = controller.transform;
        director.view = controller.GetComponentInChildren<Camera>();
        if (controller.footstepSounds != null && controller.footstepSounds.Length > 0)
            director.stepClip = controller.footstepSounds[0];
    }

    private void Start()
    {
        foreach (var candidate in Resources.FindObjectsOfTypeAll<EnemyAI>())
            if (candidate.gameObject.scene == gameObject.scene) { hunter = candidate; break; }
        foreach (var candidate in Resources.FindObjectsOfTypeAll<Crank>())
            if (candidate.gameObject.scene == gameObject.scene) { crank = candidate; break; }
        fixtures = FindObjectsOfType<Light>();
        ClearImage(view);
        roomSound = MakeSource("Sound inside the walls", 1f);
        echo = MakeSource("The extra footstep", 1f);
        pulse = MakeSource("Breathing", 0f);
        knockClip = MakeClip("Three knocks", 1.4f, 0);
        breathClip = MakeClip("Air in the doorway", 2.8f, 1);
        heartClip = MakeClip("Heartbeat", 1f, 2);
        pulse.clip = heartClip;
        pulse.loop = true;
        pulse.volume = 0f;
        pulse.Play();
        StartCoroutine(PrepareFigure());
        previousPosition = player.position;
        nextEvent = Time.time + 18f;
        if (QualitySettings.vSyncCount == 0) Application.targetFrameRate = 120;
    }

    public static void ClearImage(Camera camera)
    {
        QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
        foreach (var volume in FindObjectsOfType<PostProcessVolume>())
        {
            var profile = volume.profile;
            if (profile.TryGetSettings<MotionBlur>(out var blur)) blur.enabled.Override(false);
            if (profile.TryGetSettings<DepthOfField>(out var focus)) focus.enabled.Override(false);
            if (profile.TryGetSettings<ChromaticAberration>(out var fringe)) fringe.enabled.Override(false);
            if (profile.TryGetSettings<LensDistortion>(out var lens)) lens.enabled.Override(false);
            if (profile.TryGetSettings<Grain>(out var grain)) grain.enabled.Override(false);
            if (profile.TryGetSettings<Bloom>(out var bloom))
            {
                bloom.intensity.Override(0.2f);
                bloom.threshold.Override(1.1f);
            }
            if (profile.TryGetSettings<ColorGrading>(out var grade))
            {
                grade.saturation.Override(-12f);
                grade.temperature.Override(-9f);
                grade.contrast.Override(12f);
                grade.postExposure.Override(1.25f);
            }
        }
        if (camera == null) return;
        var layer = camera.GetComponent<PostProcessLayer>();
        if (layer != null) layer.antialiasingMode = PostProcessLayer.Antialiasing.SubpixelMorphologicalAntialiasing;
    }

    private AudioSource MakeSource(string label, float spatial)
    {
        var sound = new GameObject(label).AddComponent<AudioSource>();
        sound.transform.SetParent(transform);
        sound.playOnAwake = false;
        sound.spatialBlend = spatial;
        sound.rolloffMode = AudioRolloffMode.Logarithmic;
        sound.minDistance = 1.5f;
        sound.maxDistance = 18f;
        sound.dopplerLevel = 0f;
        return sound;
    }

    private static AudioClip MakeClip(string name, float duration, int kind)
    {
        const int rate = 22050;
        var samples = new float[Mathf.CeilToInt(duration * rate)];
        var random = new System.Random(712 + kind);
        float lowNoise = 0f;
        for (int i = 0; i < samples.Length; i++)
        {
            float t = i / (float)rate;
            lowNoise = Mathf.Lerp(lowNoise, (float)random.NextDouble() * 2f - 1f, 0.18f);
            float value;
            if (kind == 0)
            {
                value = Knock(t, lowNoise) + 0.72f * Knock(t - 0.36f, lowNoise)
                    + 0.9f * Knock(t - 0.91f, lowNoise);
            }
            else if (kind == 1)
            {
                float envelope = Mathf.Pow(Mathf.Sin(t / duration * Mathf.PI), 2f);
                value = lowNoise * envelope * 0.45f;
            }
            else value = Beat(t - 0.06f) + Beat(t - 0.25f) * 0.5f;
            samples[i] = Mathf.Clamp(value, -0.85f, 0.85f);
        }
        var clip = AudioClip.Create(name, samples.Length, 1, rate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private static float Knock(float t, float noise)
    {
        if (t < 0f || t > 0.24f) return 0f;
        return (Mathf.Sin(t * 590f) * 0.6f + noise * 0.5f) * Mathf.Exp(-t * 30f);
    }

    private static float Beat(float t)
    {
        if (t < 0f || t > 0.15f) return 0f;
        return Mathf.Sin(t * 440f) * Mathf.Exp(-t * 40f);
    }

    private IEnumerator PrepareFigure()
    {
        // The original player's cloth meshes cannot be read by Unity's cloth solver.
        // Keep their skinned renderers; do not keep reinitialising a failing simulation.
        foreach (var cloth in Resources.FindObjectsOfTypeAll<Cloth>())
            if (cloth.gameObject.scene == gameObject.scene) Destroy(cloth);
        yield return null;
        BuildApparition();
    }

    private void BuildApparition()
    {
        if (hunter == null) return;
        // Keep the original rig and idle pose; strip every gameplay/physics component.
        // Baking the inactive original produced a T-pose and empty attachment meshes.
        apparition = Instantiate(hunter.gameObject);
        apparition.name = "The figure in the doorway";
        apparition.SetActive(false);
        apparition.transform.SetParent(transform, false);
        foreach (var script in apparition.GetComponentsInChildren<MonoBehaviour>(true)) Destroy(script);
        foreach (var shape in apparition.GetComponentsInChildren<Collider>(true)) Destroy(shape);
        foreach (var body in apparition.GetComponentsInChildren<Rigidbody>(true)) Destroy(body);
        foreach (var agent in apparition.GetComponentsInChildren<NavMeshAgent>(true)) Destroy(agent);
        foreach (var sound in apparition.GetComponentsInChildren<AudioSource>(true)) Destroy(sound);
        foreach (var item in apparition.GetComponentsInChildren<Transform>(true)) item.gameObject.layer = 2;
        figureRenderers = apparition.GetComponentsInChildren<Renderer>(true);
        foreach (var renderer in figureRenderers) {
            renderer.shadowCastingMode = ShadowCastingMode.Off;
        }
        apparition.transform.localScale = hunter.transform.lossyScale;
    }

    private bool ChaseActive()
    {
        return hunter != null && hunter.isActiveAndEnabled &&
            (hunter.currentState == EnemyAI.EnemyState.Chasing || hunter.currentState == EnemyAI.EnemyState.Attacking ||
             (hunter.transform.position - player.position).sqrMagnitude < 49f);
    }

    private void Update()
    {
        if (player == null || view == null) return;
        bool paused = Time.timeScale == 0f || !player.gameObject.activeInHierarchy;
        if (paused != suspended)
        {
            suspended = paused;
            if (paused) { roomSound.Pause(); echo.Pause(); pulse.Pause(); }
            else { roomSound.UnPause(); echo.UnPause(); pulse.UnPause(); }
        }
        if (paused) return;

        bool chase = ChaseActive();
        pulseLevel = Mathf.MoveTowards(pulseLevel, chase ? 0.22f : 0f, Time.deltaTime * 0.2f);
        pulse.volume = pulseLevel;
        pulse.pitch = Mathf.Lerp(0.8f, 1.25f, pulseLevel / 0.22f);

        Vector3 movement = player.position - previousPosition;
        movement.y = 0f;
        bool moving = movement.sqrMagnitude > 0.00001f;
        if (moving)
        {
            lastFootstep = previousPosition;
            stillFor = 0f;
            wasMoving = true;
        }
        else stillFor += Time.deltaTime;
        previousPosition = player.position;

        if (crank != null && crank.objectToActivate != null && crank.objectToActivate.activeInHierarchy && !stairsObserved)
        {
            stairsObserved = true;
            nextEvent = Mathf.Min(nextEvent, Time.time + 4f);
        }
        if (crank != null && crank.key != null && crank.key.transform.parent != null &&
            crank.key.transform.parent.name == "keyholder" && !keyObserved)
        {
            keyObserved = true;
            nextEvent = Mathf.Min(nextEvent, Time.time + 3f);
        }
        if (busy || chase || Time.time < nextEvent) return;
        busy = true;
        nextEvent = Time.time + Random.Range(keyObserved ? 20f : 28f, 44f);
        StartCoroutine(Encounter(sequence++ % 3));
    }

    private IEnumerator Encounter(int kind)
    {
        if (kind == 0 && stepClip != null && wasMoving)
        {
            // An extra footstep follows the player after their own feet stop.
            float timeout = Time.time + 6f;
            while (stillFor < 0.35f && Time.time < timeout) yield return null;
            echo.transform.position = lastFootstep;
            echo.pitch = 0.72f;
            echo.volume = 0.42f;
            echo.PlayOneShot(stepClip);
            wasMoving = false;
            yield return new WaitForSeconds(0.85f);
            echo.PlayOneShot(stepClip, 0.65f);
        }
        else if (kind == 2 && ShowFigure())
        {
            roomSound.transform.position = apparition.transform.position + Vector3.up;
            roomSound.volume = 0.36f;
            roomSound.PlayOneShot(breathClip);
            yield return new WaitForSeconds(0.7f);
            float deadline = Time.time + 2.6f;
            while (Time.time < deadline && !ChaseActive())
            {
                Vector3 p = view.WorldToViewportPoint(apparition.transform.position + Vector3.up);
                if (p.z < 0 || p.x < -0.1f || p.x > 1.1f) break;
                yield return null;
            }
            apparition.SetActive(false);
        }
        else
        {
            // The sound stays at a fixed point in the house while the player turns.
            roomSound.transform.position = player.position - player.forward * 3f + player.right * 2f + Vector3.up * 2.3f;
            roomSound.pitch = Random.Range(0.88f, 1.02f);
            roomSound.volume = 0.48f;
            roomSound.PlayOneShot(knockClip);
            yield return DimNearbyLamp();
        }
        busy = false;
    }

    private bool ShowFigure()
    {
        if (apparition == null || figureRenderers.Length == 0 || hunter.waypoints == null) return false;
        foreach (var point in hunter.waypoints)
        {
            if (point == null) continue;
            Vector3 delta = point.position - player.position;
            if (delta.sqrMagnitude < 36f || delta.sqrMagnitude > 196f) continue;
            Vector3 target = point.position + Vector3.up * 1.5f;
            Vector3 screen = view.WorldToViewportPoint(target);
            if (screen.z <= 0 || screen.x < 0.08f || screen.x > 0.92f || screen.y < 0.08f || screen.y > 0.92f) continue;
            int mask = Physics.DefaultRaycastLayers & ~(1 << player.gameObject.layer) & ~(1 << hunter.gameObject.layer);
            if (Physics.Linecast(view.transform.position, target, mask, QueryTriggerInteraction.Ignore)) continue;
            if (!NavMesh.SamplePosition(point.position, out var hit, 1f, NavMesh.AllAreas)) continue;
            apparition.transform.position = hit.position;
            Vector3 facing = player.position - hit.position;
            facing.y = 0;
            apparition.transform.rotation = Quaternion.LookRotation(facing);
            apparition.SetActive(true);
            var animator = apparition.GetComponent<Animator>();
            if (animator != null) { animator.SetFloat("Speed", 0f); animator.Update(0f); }
            return true;
        }
        return false;
    }

    private IEnumerator DimNearbyLamp()
    {
        float distance = 100f;
        foreach (var light in fixtures)
        {
            if (light == null || !light.enabled || light.type == LightType.Directional || light.intensity <= 0.1f ||
                light.transform.IsChildOf(player)) continue;
            float d = (light.transform.position - player.position).sqrMagnitude;
            if (d >= distance) continue;
            dimmedLight = light;
            distance = d;
        }
        if (dimmedLight == null) yield break;
        savedIntensity = dimmedLight.intensity;
        float elapsed = 0f;
        while (elapsed < 3.2f && dimmedLight != null)
        {
            elapsed += Time.deltaTime;
            float dip = Mathf.Sin(Mathf.Clamp01(elapsed / 3.2f) * Mathf.PI);
            dimmedLight.intensity = savedIntensity * (1f - 0.65f * dip * dip);
            yield return null;
        }
        RestoreLamp();
    }

    private void RestoreLamp()
    {
        if (dimmedLight != null) dimmedLight.intensity = savedIntensity;
        dimmedLight = null;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        RestoreLamp();
        if (apparition != null) apparition.SetActive(false);
        if (roomSound != null) roomSound.Stop();
        if (echo != null) echo.Stop();
        if (pulse != null) pulse.Stop();
    }

    private void OnDestroy()
    {
        if (knockClip != null) Destroy(knockClip);
        if (breathClip != null) Destroy(breathClip);
        if (heartClip != null) Destroy(heartClip);
    }
}
