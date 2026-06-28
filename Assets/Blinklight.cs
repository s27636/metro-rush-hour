using UnityEngine;


[RequireComponent(typeof(Light))]
public class BlinkingLight : MonoBehaviour
{
    [Tooltip("Czas jednego stanu (s). Mniejszy = szybsze miganie.")]
    public float interval = 0.5f;

    private Light lt;
    private bool on = false;
    private float timer = 0f;

    void Start()
    {
        lt = GetComponent<Light>();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= interval)
        {
            timer = 0f;
            on = !on;
            lt.enabled = on;
        }
    }
}