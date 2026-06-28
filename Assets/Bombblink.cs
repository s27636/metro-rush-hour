using UnityEngine;

public class BombBlink : MonoBehaviour
{
    [Header("Miganie")]
    [Tooltip("Czas między zmianami (s). Mniejszy = szybsze miganie.")]
    public float blinkInterval = 0.5f;
    public Color blinkColor = Color.red;
    [Tooltip("Siła świecenia (HDR). Większa = mocniejsza poświata.")]
    public float emissionIntensity = 5f;

    [Header("Opcjonalne światło")]
    [Tooltip("Punktowe światło, które pulsuje razem z bombą (np. czerwony Point Light jako dziecko).")]
    public Light blinkLight;

    private Renderer[] renderers;
    private bool on = false;
    private float timer = 0f;

    void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();

        // Włącz emisję na wszystkich materiałach bomby.
        foreach (var r in renderers)
            foreach (var m in r.materials)
                m.EnableKeyword("_EMISSION");

        SetEmission(false);
        if (blinkLight != null) blinkLight.enabled = false;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= blinkInterval)
        {
            timer = 0f;
            on = !on;
            SetEmission(on);
            if (blinkLight != null) blinkLight.enabled = on;
        }
    }

    void SetEmission(bool state)
    {
        Color c = state ? blinkColor * emissionIntensity : Color.black;
        foreach (var r in renderers)
            foreach (var m in r.materials)
                m.SetColor("_EmissionColor", c);
    }
}