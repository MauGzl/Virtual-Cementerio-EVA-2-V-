using UnityEngine;

public enum InteractKind { Teleport, Lantern, Gate, Bell, Portal, Restart }

/// <summary>
/// Objeto que reacciona a la mirada. Necesita un Collider.
/// GazePointer llama GazeEnter / GazeExit / Activate; Activate avisa al GameManager qué hacer según 'kind'.
/// </summary>
public class GazeInteractable : MonoBehaviour
{
    public InteractKind kind;
    public float gazeTime = 1.5f;       // segundos de mirada para el autoclick
    public Transform destination;       // para Gate (destino); en Teleport se usa el propio objeto

    Vector3 baseScale;

    void Awake() { baseScale = transform.localScale; }
    void OnDisable() { transform.localScale = baseScale; }

    public Transform Target { get { return destination != null ? destination : transform; } }

    public void GazeEnter() { transform.localScale = baseScale * 1.15f; }
    public void GazeExit() { transform.localScale = baseScale; }

    public void Activate()
    {
        if (GameManager.Instance != null) GameManager.Instance.HandleAction(this);
    }
}
