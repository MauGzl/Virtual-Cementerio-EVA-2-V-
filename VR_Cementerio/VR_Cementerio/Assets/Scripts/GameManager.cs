using System.Collections;
using UnityEngine;

/// <summary>
/// Cerebro de la experiencia "La última luz del cementerio".
/// Zona 1 (entrada): Acción 1 = encender el farol -> habilita la puerta de la cripta.
/// Zona 2 (cripta):  Acción 2 = despertar la campana -> habilita el portal -> final y reinicio.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Jugador")]
    public Transform playerRig;
    public Transform startZone1;
    public Transform startZone2;

    [Header("Dialogos")]
    public DialogBubble dialogZone1;
    public DialogBubble dialogZone2;

    [Header("Accion 1: farol")]
    public Light lanternLight;
    public Renderer lanternLamp;

    [Header("Accion 2: campana y portal")]
    public Transform bellPivot;
    public Renderer bellRenderer;
    public Renderer portalRenderer;
    public Light portalLight;

    [Header("Final")]
    public GameObject restartButton;

    [Header("Estado (solo lectura)")]
    public bool lanternOn;
    public bool bellRung;
    public bool finished;

    public bool IsBusy { get; private set; }

    FadeController fade;
    AudioSource audioSource;

    const string TXT_INTRO = "Son las 23:57. El farol del cementerio se apagó. Mantén la mirada sobre él para encenderlo y luego cruza la puerta de la cripta.";
    const string TXT_LANTERN_ON = "El farol brilla otra vez. El sello se abrió: mira la puerta de la cripta.";
    const string TXT_GATE_BLOCKED = "El sello todavía está apagado. Enciende el farol.";
    const string TXT_ZONE2 = "El guardián susurra: «La luz ha vuelto, pero mi campana sigue dormida. Despiértala y abriré el camino».";
    const string TXT_PORTAL_BLOCKED = "El portal duerme. Despierta primero la campana.";
    const string TXT_BELL_DONE = "La campana despertó. El camino está abierto: mira el portal.";
    const string TXT_END = "¡Escapaste antes de medianoche! Mira el cubo verde para volver a comenzar.";

    static readonly Color lanternOffColor = new Color(0.35f, 0.35f, 0.35f);
    static readonly Color lanternOnColor = new Color(1f, 0.85f, 0.3f);
    static readonly Color bellOffColor = new Color(0.55f, 0.5f, 0.4f);
    static readonly Color bellOnColor = new Color(1f, 0.8f, 0.1f);
    static readonly Color portalOffColor = new Color(0.25f, 0.25f, 0.35f);
    static readonly Color portalOnColor = new Color(0.2f, 1f, 0.9f);

    void Awake()
    {
        Instance = this;
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    IEnumerator Start()
    {
        fade = FadeController.Instance;
        IsBusy = true;
        ApplyInitialState();
        MovePlayerTo(startZone1);
        yield return fade.FadeIn();
        IsBusy = false;
        dialogZone1.Show(TXT_INTRO);
    }

    // ---------- Punto de entrada desde GazeInteractable ----------
    public void HandleAction(GazeInteractable it)
    {
        if (IsBusy) return;

        switch (it.kind)
        {
            case InteractKind.Teleport:
                StartCoroutine(TeleportRoutine(it.Target, null));
                break;

            case InteractKind.Lantern:                      // ACCION 1
                if (!lanternOn) ActivateLantern();
                break;

            case InteractKind.Gate:                         // condicion: farol encendido
                if (!lanternOn)
                {
                    dialogZone1.Show(TXT_GATE_BLOCKED, 4f);
                    Beep(200f, 0.3f);
                }
                else
                {
                    StartCoroutine(TeleportRoutine(startZone2, () =>
                    {
                        dialogZone1.Hide();
                        dialogZone2.Show(TXT_ZONE2);
                    }));
                }
                break;

            case InteractKind.Bell:                         // ACCION 2
                if (!bellRung) StartCoroutine(RingBell());
                break;

            case InteractKind.Portal:                       // condicion: campana activada
                if (!bellRung)
                {
                    dialogZone2.Show(TXT_PORTAL_BLOCKED, 4f);
                    Beep(200f, 0.3f);
                }
                else if (!finished) Finish();
                break;

            case InteractKind.Restart:
                StartCoroutine(ResetGame());
                break;
        }
    }

    // ---------- Teletransporte: FadeOut -> mover -> FadeIn ----------
    IEnumerator TeleportRoutine(Transform dest, System.Action onArrive)
    {
        IsBusy = true;
        yield return fade.FadeOut();          // 1) oscurecer
        MovePlayerTo(dest);                   // 2) trasladar
        if (onArrive != null) onArrive();
        yield return null;
        yield return fade.FadeIn();           // 3) revelar el destino
        IsBusy = false;
    }

    void MovePlayerTo(Transform dest)
    {
        playerRig.position = dest.position;
        playerRig.rotation = Quaternion.Euler(0f, dest.eulerAngles.y, 0f);
    }

    // ---------- Acciones ----------
    void ActivateLantern()
    {
        lanternOn = true;
        lanternLight.enabled = true;
        lanternLamp.material.color = lanternOnColor;
        Beep(660f, 0.4f);
        dialogZone1.Show(TXT_LANTERN_ON, 6f);
    }

    IEnumerator RingBell()
    {
        bellRung = true;
        bellRenderer.material.color = bellOnColor;
        Beep(440f, 0.7f);
        float t = 0f, total = 2.5f;
        while (t < total)
        {
            t += Time.deltaTime;
            float ang = Mathf.Sin(t * 8f) * 30f * (1f - t / total);
            bellPivot.localRotation = Quaternion.Euler(ang, 0f, 0f);
            yield return null;
        }
        bellPivot.localRotation = Quaternion.identity;
        portalRenderer.material.color = portalOnColor;
        portalLight.enabled = true;
        dialogZone2.Show(TXT_BELL_DONE);
    }

    void Finish()
    {
        finished = true;
        Beep(880f, 0.9f);
        dialogZone2.Show(TXT_END);
        restartButton.SetActive(true);
    }

    // ---------- Reinicio (sin recargar la escena) ----------
    IEnumerator ResetGame()
    {
        IsBusy = true;
        yield return fade.FadeOut();
        ApplyInitialState();
        MovePlayerTo(startZone1);
        dialogZone1.Show(TXT_INTRO);
        yield return null;
        yield return fade.FadeIn();
        IsBusy = false;
    }

    void ApplyInitialState()
    {
        lanternOn = false;
        bellRung = false;
        finished = false;
        lanternLight.enabled = false;
        lanternLamp.material.color = lanternOffColor;
        bellRenderer.material.color = bellOffColor;
        bellPivot.localRotation = Quaternion.identity;
        portalRenderer.material.color = portalOffColor;
        portalLight.enabled = false;
        restartButton.SetActive(false);
        dialogZone1.Hide();
        dialogZone2.Hide();
    }

    // ---------- Sonido generado por código (sin assets) ----------
    void Beep(float freq, float dur)
    {
        int sr = 44100, n = (int)(sr * dur);
        float[] d = new float[n];
        for (int i = 0; i < n; i++)
            d[i] = Mathf.Sin(2f * Mathf.PI * freq * i / sr) * 0.3f * (1f - (float)i / n);
        AudioClip clip = AudioClip.Create("beep", n, 1, sr, false);
        clip.SetData(d, 0);
        audioSource.PlayOneShot(clip);
    }
}
