using System.Collections;
using UnityEngine;

/// <summary>
/// ScreenFade: un quad negro pegado a la cámara (funciona en VR estéreo, a diferencia de un Canvas Overlay).
/// Secuencia de teletransporte: FadeOut -> mover al jugador -> FadeIn.
/// </summary>
public class FadeController : MonoBehaviour
{
    public static FadeController Instance { get; private set; }
    public float duration = 0.6f;
    public Material unlitMaterial;

    Material mat;
    Renderer rend;
    float alpha = 1f;

    void Awake()
    {
        Instance = this;
        Camera cam = Camera.main;
        cam.nearClipPlane = 0.05f;

        GameObject q = GameObject.CreatePrimitive(PrimitiveType.Quad);
        q.name = "FadeOverlay";
        Destroy(q.GetComponent<Collider>());
        q.transform.SetParent(cam.transform, false);
        q.transform.localPosition = new Vector3(0f, 0f, 0.2f);
        q.transform.localScale = new Vector3(4f, 4f, 1f);

        mat = unlitMaterial != null ? new Material(unlitMaterial) : new Material(Shader.Find("Sprites/Default"));
        rend = q.GetComponent<Renderer>();
        rend.material = mat;
        rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        SetAlpha(1f); // arranca en negro; el GameManager hace el primer FadeIn
    }

    void SetAlpha(float a)
    {
        alpha = a;
        mat.color = new Color(0f, 0f, 0f, a);
        rend.enabled = a > 0.001f;   // nunca queda una pantalla negra "invisible" tapando
    }

    public IEnumerator FadeOut() { return Fade(1f); }
    public IEnumerator FadeIn() { return Fade(0f); }

    IEnumerator Fade(float target)
    {
        float start = alpha, t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            SetAlpha(Mathf.Lerp(start, target, t / duration));
            yield return null;
        }
        SetAlpha(target);
    }
}
