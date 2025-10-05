using UnityEngine;

public class SpriteShaker : MonoBehaviour
{
    public float amplitude = 0.1f;
    public float frequency = 20f;
    public bool continuous = false;  // enable for nonstop shake

    private Vector3 originalPos;
    private bool shaking = false;
    private float elapsed = 0f;

    void Start()
    {
        originalPos = transform.localPosition;
        if(continuous){ StartContinuousShake(); }
        
    }

    void Update()
    {
        if (shaking)
        {
            elapsed += Time.unscaledDeltaTime;
            float offsetX = Mathf.Sin(elapsed * frequency) * amplitude;
            float offsetY = Mathf.Cos(elapsed * frequency * 1.3f) * amplitude;
            transform.localPosition = originalPos + new Vector3(offsetX, offsetY, 0);

            if (!continuous && elapsed >= 0.3f) // default short shake
                StopShake();
        }
    }

    public void Shake(float duration = 0.3f)
    {
        elapsed = 0f;
        shaking = true;
        continuous = false;
    }

    public void StartContinuousShake()
    {
        elapsed = 0f;
        shaking = true;
        continuous = true;
    }

    public void StopShake()
    {
        shaking = false;
        transform.localPosition = originalPos;
    }
}
