using UnityEngine;
using UnityEngine.XR;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Orienta la cámara.
/// - En el teléfono con Cardboard: usa la rotación de la cabeza que entrega el plugin XR.
/// - Si el XR no entrega pose: usa el giroscopio (respaldo).
/// - En el editor: mantén CLIC DERECHO y mueve el mouse para mirar (simula la cabeza).
/// Va en la Main Camera.
/// </summary>
public class HeadPoseDriver : MonoBehaviour
{
    public float mouseSensitivity = 2f;
    float yaw, pitch;

    void Start()
    {
#if ENABLE_LEGACY_INPUT_MANAGER
        if (!Application.isEditor) Input.gyro.enabled = true;
#endif
    }

    void Update()
    {
        // 1) Pose del visor (Cardboard XR Plugin)
        InputDevice head = InputDevices.GetDeviceAtXRNode(XRNode.Head);
        if (head.isValid && head.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion q))
        {
            transform.localRotation = q;
            return;
        }

#if UNITY_EDITOR
        MouseLook();
#else
        GyroLook();
#endif
    }

    void MouseLook()
    {
        float dx = 0f, dy = 0f;
        bool hold = false;
#if ENABLE_LEGACY_INPUT_MANAGER
        hold = Input.GetMouseButton(1);
        if (hold) { dx = Input.GetAxis("Mouse X") * mouseSensitivity; dy = Input.GetAxis("Mouse Y") * mouseSensitivity; }
#elif ENABLE_INPUT_SYSTEM
        if (Mouse.current != null && Mouse.current.rightButton.isPressed)
        {
            Vector2 d = Mouse.current.delta.ReadValue() * 0.1f * mouseSensitivity;
            dx = d.x; dy = d.y; hold = true;
        }
#endif
        if (!hold) return;
        yaw += dx;
        pitch = Mathf.Clamp(pitch - dy, -80f, 80f);
        transform.localRotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    void GyroLook()
    {
#if ENABLE_LEGACY_INPUT_MANAGER
        Quaternion g = Input.gyro.attitude;
        transform.localRotation = Quaternion.Euler(90f, 0f, 0f) * new Quaternion(-g.x, -g.y, g.z, -g.w);
#endif
    }
}
