using System.Collections;
using UnityEngine;


[RequireComponent(typeof(Light))]
public class FlickerLight : MonoBehaviour
{
    [Header("Jasność")]
    [Tooltip("Pełna jasność (gdy świeci normalnie). 0 = weź obecną z Light.")]
    public float onIntensity = 0f;
    [Tooltip("Jasność w trakcie przygasania.")]
    public float dimIntensity = 0.2f;

    [Header("Rytm (sekundy)")]
    [Tooltip("Jak długo świeci spokojnie, zanim zacznie mrugać.")]
    public Vector2 stableTime = new Vector2(2f, 6f);
    [Tooltip("Jak długo trwa seria mrugań.")]
    public Vector2 flickerBurst = new Vector2(0.2f, 0.8f);
    [Tooltip("Tempo pojedynczych mrugnięć.")]
    public Vector2 flickerSpeed = new Vector2(0.03f, 0.12f);

    [Header("Dźwięk (opcjonalnie)")]
    public AudioSource hum; // buczenie/trzask, gra tylko gdy światło włączone

    private Light lt;

    void Start()
    {
        lt = GetComponent<Light>();
        if (onIntensity <= 0f) onIntensity = lt.intensity;
        StartCoroutine(FlickerRoutine());
    }

    IEnumerator FlickerRoutine()
    {
        while (true)
        {
            // Faza spokojna - świeci normalnie.
            SetLight(onIntensity);
            yield return new WaitForSeconds(Random.Range(stableTime.x, stableTime.y));

            // Faza mrugania - seria szybkich przełączeń.
            float burst = Random.Range(flickerBurst.x, flickerBurst.y);
            float t = 0f;
            while (t < burst)
            {
                // Losowo: pełna jasność, przygaszone albo całkiem zgaszone.
                float r = Random.value;
                if (r < 0.5f) SetLight(0f);
                else if (r < 0.8f) SetLight(dimIntensity);
                else SetLight(onIntensity);

                float step = Random.Range(flickerSpeed.x, flickerSpeed.y);
                t += step;
                yield return new WaitForSeconds(step);
            }
        }
    }

    void SetLight(float intensity)
    {
        lt.intensity = intensity;
        if (hum != null)
        {
            if (intensity <= 0f && hum.isPlaying) hum.Pause();
            else if (intensity > 0f && !hum.isPlaying) hum.UnPause();
        }
    }
}