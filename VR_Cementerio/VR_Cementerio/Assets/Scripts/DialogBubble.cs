using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Burbuja de diálogo en el espacio 3D (Canvas World Space), creada por código.
/// Se orienta hacia el jugador (solo en el eje Y) y se muestra/oculta de forma controlada.
/// </summary>
public class DialogBubble : MonoBehaviour
{
    public float width = 3.2f;     // ancho en metros
    public int fontSize = 44;

    GameObject canvasGO;
    Text label;
    Coroutine hideRoutine;
    Transform cam;

    void Awake()
    {
        canvasGO = new GameObject("Canvas");
        canvasGO.transform.SetParent(transform, false);
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        RectTransform rt = canvasGO.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(800f, 300f);
        rt.localScale = Vector3.one * (width / 800f);

        GameObject panel = new GameObject("Fondo");
        panel.transform.SetParent(canvasGO.transform, false);
        Image img = panel.AddComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0.78f);
        Stretch(img.rectTransform, 0f);

        GameObject tgo = new GameObject("Texto");
        tgo.transform.SetParent(canvasGO.transform, false);
        label = tgo.AddComponent<Text>();
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = fontSize;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.white;
        label.horizontalOverflow = HorizontalWrapMode.Wrap;
        label.verticalOverflow = VerticalWrapMode.Truncate;
        label.resizeTextForBestFit = true;
        label.resizeTextMinSize = 18;
        label.resizeTextMaxSize = fontSize;
        Stretch(label.rectTransform, 25f);

        canvasGO.SetActive(false);
    }

    static void Stretch(RectTransform r, float margin)
    {
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.offsetMin = new Vector2(margin, margin);
        r.offsetMax = new Vector2(-margin, -margin);
    }

    public void Show(string message, float seconds = 0f)
    {
        label.text = message;
        canvasGO.SetActive(true);
        if (hideRoutine != null) StopCoroutine(hideRoutine);
        if (seconds > 0f) hideRoutine = StartCoroutine(HideAfter(seconds));
    }

    public void Hide()
    {
        if (hideRoutine != null) StopCoroutine(hideRoutine);
        canvasGO.SetActive(false);
    }

    IEnumerator HideAfter(float s)
    {
        yield return new WaitForSeconds(s);
        canvasGO.SetActive(false);
    }

    void LateUpdate()
    {
        if (!canvasGO.activeSelf) return;
        if (cam == null && Camera.main != null) cam = Camera.main.transform;
        if (cam == null) return;
        Vector3 dir = transform.position - cam.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f) transform.rotation = Quaternion.LookRotation(dir);
    }
}
