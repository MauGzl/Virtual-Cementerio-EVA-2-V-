using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>Clic manual (mouse o toque en pantalla). Es solo una alternativa al autoclick.</summary>
public static class VRInput
{
    public static bool ClickDown()
    {
#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetMouseButtonDown(0)) return true;
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) return true;
#endif
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) return true;
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame) return true;
#endif
        return false;
    }
}

/// <summary>
/// Retícula + autoclick. Va en la Main Camera.
/// Cada frame lanza un rayo desde el centro de la cámara:
///  - Si cambia el objeto mirado: llama GazeExit al anterior, GazeEnter al nuevo y REINICIA el temporizador (interrumpir la mirada cancela la espera).
///  - Mientras se mira un objeto, el temporizador sube; al llegar a gazeTime se ejecuta Activate() UNA vez.
///  - 'fired' impide repetir la acción hasta que se aparte la mirada (evita ejecuciones continuas).
/// </summary>
public class GazePointer : MonoBehaviour
{
    public float maxDistance = 60f;
    public Material unlitMaterial;

    Transform reticle;
    Material mat;
    GazeInteractable current;
    float timer;
    bool fired;

    static readonly Color idleColor = new Color(1f, 1f, 1f, 0.85f);
    static readonly Color gazeColor = new Color(1f, 0.85f, 0.1f, 1f);

    void Awake()
    {
        var s = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        s.name = "Reticula";
        Destroy(s.GetComponent<Collider>());
        reticle = s.transform;
        reticle.SetParent(transform, false);
        reticle.localPosition = new Vector3(0f, 0f, 1.5f);
        reticle.localScale = Vector3.one * 0.03f;
        mat = unlitMaterial != null ? new Material(unlitMaterial) : new Material(Shader.Find("Sprites/Default"));
        var r = s.GetComponent<Renderer>();
        r.material = mat;
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mat.color = idleColor;
    }

    void LateUpdate()
    {
        GazeInteractable hit = null;
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit h, maxDistance, ~0, QueryTriggerInteraction.Collide))
            hit = h.collider.GetComponentInParent<GazeInteractable>();

        if (hit != current)
        {
            if (current != null) current.GazeExit();   // salida: restablece el estado
            current = hit;
            timer = 0f;                                 // interrumpir la mirada reinicia la espera
            fired = false;
            if (current != null) current.GazeEnter();   // entrada: respuesta visual
        }

        float progress = 0f;
        bool busy = GameManager.Instance != null && GameManager.Instance.IsBusy;

        if (current != null && !fired && !busy)
        {
            timer += Time.deltaTime;
            progress = Mathf.Clamp01(timer / current.gazeTime);
            if (timer >= current.gazeTime || VRInput.ClickDown())
            {
                fired = true;           // no se repite hasta apartar la mirada
                timer = 0f;
                current.Activate();
            }
        }
        else if (current != null && fired)
        {
            progress = 1f;
        }

        // Respuesta visual de la retícula
        mat.color = current != null ? Color.Lerp(idleColor, gazeColor, progress) : idleColor;
        reticle.localScale = Vector3.one * Mathf.Lerp(0.03f, 0.07f, progress);
    }
}
