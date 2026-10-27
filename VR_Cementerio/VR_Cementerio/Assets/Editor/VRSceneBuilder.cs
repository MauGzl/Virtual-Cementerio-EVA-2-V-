using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Menu "VR Cementerio":
///  1) Configurar Android  -> plataforma, IL2CPP, ARM64, OpenGLES3, Landscape.
///  2) Construir experiencia -> crea en la escena abierta las dos zonas, pads, farol, campana, portal, dialogos, fade y GameManager.
/// </summary>
public static class VRSceneBuilder
{
    // ------------------------------------------------------------------ 1) ANDROID
    [MenuItem("VR Cementerio/1) Configurar Android")]
    public static void ConfigureAndroid()
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(NamedBuildTarget.Android, BuildTarget.Android);
        PlayerSettings.companyName = "Estudiante";
        PlayerSettings.productName = "LaUltimaLuzCementerio";
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.estudiante.cementeriovr");
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3 });
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
        Debug.Log("[VR] Android configurado. Falta marcar Cardboard en Project Settings > XR Plug-in Management > Android.");
    }

    // ------------------------------------------------------------------ 2) ESCENA
    static Vector3 origin;
    const string MatFolder = "Assets/VR_Generated";

    [MenuItem("VR Cementerio/2) Construir experiencia")]
    public static void Build()
    {
        Scene scene = SceneManager.GetActiveScene();
        DestroyIfExists("VR_Experiencia");

        origin = FindOrigin();

        // ---- materiales
        Material unlitBase = MakeMat("Unlit_Base", Color.white, true);
        Material mPad = MakeMat("Pad", new Color(0.2f, 0.9f, 1f), true);
        Material mDoor = MakeMat("Puerta", new Color(0.45f, 0.28f, 0.15f), true);
        Material mLamp = MakeMat("Farol", new Color(0.35f, 0.35f, 0.35f), true);
        Material mPost = MakeMat("Poste", new Color(0.12f, 0.12f, 0.14f), false);
        Material mBell = MakeMat("Campana", new Color(0.55f, 0.5f, 0.4f), true);
        Material mPortal = MakeMat("Portal", new Color(0.25f, 0.25f, 0.35f), true);
        Material mGreen = MakeMat("BotonReiniciar", new Color(0.2f, 0.9f, 0.3f), true);
        Material mPumpkin = MakeMat("Calabaza", new Color(1f, 0.5f, 0.05f), true);
        Material mCandle = MakeMat("Vela", new Color(1f, 0.95f, 0.75f), true);
        Material mStone = MakeMat("Piedra", new Color(0.35f, 0.38f, 0.4f), false);
        Material mWall = MakeMat("MuroCripta", new Color(0.22f, 0.18f, 0.2f), false);
        Material mFloor = MakeMat("SueloCripta", new Color(0.3f, 0.15f, 0.15f), false);

        GameObject root = new GameObject("VR_Experiencia");

        Transform z1 = new GameObject("Zona1_Cementerio_Entrada").transform;
        z1.SetParent(root.transform);
        z1.position = origin;
        Transform z2 = new GameObject("Zona2_Cripta").transform;
        z2.SetParent(root.transform);
        z2.position = origin + new Vector3(0f, 60f, 0f);

        // ============================== ZONA 1 ==============================
        Transform start1 = Empty("Inicio_Zona1", z1, L1(0f, 0f, 0f));

        // Farol (ACCION 1)
        GameObject post = Prim(PrimitiveType.Cylinder, "Farol_Poste", z1, L1(0f, 1f, 7f), new Vector3(0.15f, 1f, 0.15f), mPost);
        GameObject lamp = Prim(PrimitiveType.Sphere, "Farol_Lampara", z1, L1(0f, 2.2f, 7f), Vector3.one * 0.6f, mLamp);
        MakeInteractable(lamp, InteractKind.Lantern, 1.5f, null);
        Light lanternLight = AddLight(lamp.transform, new Color(1f, 0.8f, 0.4f), 4f, 14f);

        // Puerta de la cripta
        GameObject gate = Prim(PrimitiveType.Cube, "Puerta_Cripta", z1, L1(-8f, 1.8f, 6f), new Vector3(3f, 3.6f, 0.4f), mDoor);
        FaceFlat(gate.transform, start1.position);

        // Pads de teletransporte
        GameObject padA = Pad("Zona1_Pad_A", z1, L1(-3f, 0.03f, 3f), mPad, lamp.transform.position);
        GameObject padB = Pad("Zona1_Pad_B", z1, L1(-5.5f, 0.03f, 6f), mPad, gate.transform.position);

        // Elementos tematicos de la zona 1
        Prim(PrimitiveType.Sphere, "Calabaza_1", z1, L1(2f, 0.25f, 3f), new Vector3(0.5f, 0.42f, 0.5f), mPumpkin, false);
        Prim(PrimitiveType.Sphere, "Calabaza_2", z1, L1(3.5f, 0.25f, 8f), new Vector3(0.5f, 0.42f, 0.5f), mPumpkin, false);
        Prim(PrimitiveType.Sphere, "Calabaza_3", z1, L1(-2f, 0.25f, 9f), new Vector3(0.5f, 0.42f, 0.5f), mPumpkin, false);
        Prim(PrimitiveType.Cube, "Lapida_1", z1, L1(5f, 0.5f, 6f), new Vector3(0.6f, 1f, 0.2f), mStone, false);
        Prim(PrimitiveType.Cube, "Lapida_2", z1, L1(-5f, 0.5f, 10f), new Vector3(0.6f, 1f, 0.2f), mStone, false);
        Prim(PrimitiveType.Cube, "Lapida_3", z1, L1(6f, 0.5f, 10f), new Vector3(0.6f, 1f, 0.2f), mStone, false);
        Prim(PrimitiveType.Cylinder, "Vela_1", z1, L1(0.8f, 0.1f, 6.2f), new Vector3(0.08f, 0.1f, 0.08f), mCandle, false);
        Prim(PrimitiveType.Cylinder, "Vela_2", z1, L1(-0.8f, 0.1f, 6.6f), new Vector3(0.08f, 0.1f, 0.08f), mCandle, false);

        GameObject d1 = new GameObject("Dialogo_Zona1");
        d1.transform.SetParent(z1);
        d1.transform.position = z1.position + new Vector3(0f, 3.0f + Gy(0f, 5f), 5f);
        DialogBubble dialog1 = d1.AddComponent<DialogBubble>();

        // ============================== ZONA 2 ==============================
        Prim(PrimitiveType.Cube, "Cripta_Suelo", z2, new Vector3(0f, -0.1f, 0f), new Vector3(14f, 0.2f, 14f), mFloor);
        Prim(PrimitiveType.Cube, "Cripta_Techo", z2, new Vector3(0f, 4.1f, 0f), new Vector3(14f, 0.2f, 14f), mWall);
        Prim(PrimitiveType.Cube, "Cripta_Muro_N", z2, new Vector3(0f, 2f, 7f), new Vector3(14f, 4f, 0.2f), mWall);
        Prim(PrimitiveType.Cube, "Cripta_Muro_S", z2, new Vector3(0f, 2f, -7f), new Vector3(14f, 4f, 0.2f), mWall);
        Prim(PrimitiveType.Cube, "Cripta_Muro_E", z2, new Vector3(7f, 2f, 0f), new Vector3(0.2f, 4f, 14f), mWall);
        Prim(PrimitiveType.Cube, "Cripta_Muro_O", z2, new Vector3(-7f, 2f, 0f), new Vector3(0.2f, 4f, 14f), mWall);
        AddLightAt(z2, "Luz_Cripta_Roja", new Vector3(0f, 3.2f, 0f), new Color(1f, 0.35f, 0.25f), 3f, 16f).enabled = true;
        AddLightAt(z2, "Luz_Campana_Azul", new Vector3(-4f, 3f, 2f), new Color(0.4f, 0.5f, 1f), 2f, 8f).enabled = true;

        Transform start2 = Empty("Inicio_Zona2", z2, new Vector3(0f, 0f, -5.5f));

        // Campana (ACCION 2)
        Prim(PrimitiveType.Cylinder, "Marco_Poste_1", z2, new Vector3(-5.2f, 1.4f, 2f), new Vector3(0.1f, 1.4f, 0.1f), mPost);
        Prim(PrimitiveType.Cylinder, "Marco_Poste_2", z2, new Vector3(-2.8f, 1.4f, 2f), new Vector3(0.1f, 1.4f, 0.1f), mPost);
        Prim(PrimitiveType.Cube, "Marco_Viga", z2, new Vector3(-4f, 2.8f, 2f), new Vector3(2.6f, 0.15f, 0.15f), mPost);
        Transform bellPivot = Empty("Campana_Pivote", z2, new Vector3(-4f, 2.75f, 2f));
        GameObject bell = Prim(PrimitiveType.Sphere, "Campana", bellPivot, Vector3.zero, new Vector3(0.7f, 0.8f, 0.7f), mBell);
        bell.transform.localPosition = new Vector3(0f, -0.9f, 0f);
        MakeInteractable(bell, InteractKind.Bell, 1.5f, null);

        // Portal
        GameObject portal = Prim(PrimitiveType.Cube, "Portal", z2, new Vector3(0f, 1.7f, 6.4f), new Vector3(3f, 3.4f, 0.3f), mPortal);
        MakeInteractable(portal, InteractKind.Portal, 1.5f, null);
        Light portalLight = AddLightAt(z2, "Luz_Portal", new Vector3(0f, 1.7f, 5.4f), new Color(0.2f, 1f, 0.9f), 4f, 10f);

        // Pads zona 2
        Pad("Zona2_Pad_A", z2, new Vector3(2f, 0.03f, -2f), mPad, portal.transform.position);
        Pad("Zona2_Pad_B", z2, new Vector3(-1.5f, 0.03f, 2f), mPad, bell.transform.position);

        // Boton reiniciar
        GameObject restart = Prim(PrimitiveType.Cube, "Boton_Reiniciar", z2, new Vector3(4.5f, 1.2f, 5.5f), new Vector3(1.4f, 0.5f, 0.25f), mGreen);
        MakeInteractable(restart, InteractKind.Restart, 1.5f, null);

        // Elementos tematicos zona 2
        Prim(PrimitiveType.Cylinder, "Vela_Z2_1", z2, new Vector3(-6f, 0.15f, -4f), new Vector3(0.15f, 0.15f, 0.15f), mCandle, false);
        Prim(PrimitiveType.Cylinder, "Vela_Z2_2", z2, new Vector3(6f, 0.15f, -4f), new Vector3(0.15f, 0.15f, 0.15f), mCandle, false);
        Prim(PrimitiveType.Cylinder, "Vela_Z2_3", z2, new Vector3(-6f, 0.15f, 5f), new Vector3(0.15f, 0.15f, 0.15f), mCandle, false);
        Prim(PrimitiveType.Cylinder, "Vela_Z2_4", z2, new Vector3(5.8f, 0.15f, 2f), new Vector3(0.15f, 0.15f, 0.15f), mCandle, false);
        Prim(PrimitiveType.Cylinder, "Pilar_1", z2, new Vector3(5.5f, 2f, -3f), new Vector3(0.5f, 2f, 0.5f), mStone);
        Prim(PrimitiveType.Cylinder, "Pilar_2", z2, new Vector3(-5.5f, 2f, -3f), new Vector3(0.5f, 2f, 0.5f), mStone);
        Prim(PrimitiveType.Cube, "Cajon_1", z2, new Vector3(5f, 0.4f, -5.5f), new Vector3(0.8f, 0.8f, 0.8f), mStone);
        Prim(PrimitiveType.Cube, "Cajon_2", z2, new Vector3(5.9f, 0.3f, -5f), new Vector3(0.6f, 0.6f, 0.6f), mStone);

        GameObject d2 = new GameObject("Dialogo_Zona2");
        d2.transform.SetParent(z2);
        d2.transform.position = z2.position + new Vector3(0f, 3.1f, 3f);
        DialogBubble dialog2 = d2.AddComponent<DialogBubble>();

        // ============================== JUGADOR ==============================
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject cg = new GameObject("Main Camera");
            cg.tag = "MainCamera";
            cam = cg.AddComponent<Camera>();
            cg.AddComponent<AudioListener>();
        }
        foreach (Camera c in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
            if (c != cam) c.enabled = false;
        foreach (AudioListener a in Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None))
            if (a.gameObject != cam.gameObject) a.enabled = false;

        GameObject rig = GameObject.Find("PlayerRig");
        if (rig == null) rig = new GameObject("PlayerRig");
        cam.transform.SetParent(rig.transform, false);
        cam.transform.localPosition = new Vector3(0f, 1.6f, 0f);
        cam.transform.localRotation = Quaternion.identity;
        rig.transform.position = start1.position;
        cam.nearClipPlane = 0.05f;

        if (cam.GetComponent<HeadPoseDriver>() == null) cam.gameObject.AddComponent<HeadPoseDriver>();
        GazePointer gp = cam.GetComponent<GazePointer>();
        if (gp == null) gp = cam.gameObject.AddComponent<GazePointer>();
        gp.unlitMaterial = unlitBase;

        // ============================== GAME MANAGER ==============================
        GameObject gmo = new GameObject("GameManager");
        gmo.transform.SetParent(root.transform);
        FadeController fc = gmo.AddComponent<FadeController>();
        fc.unlitMaterial = unlitBase;
        GameManager gm = gmo.AddComponent<GameManager>();
        gm.playerRig = rig.transform;
        gm.startZone1 = start1;
        gm.startZone2 = start2;
        gm.dialogZone1 = dialog1;
        gm.dialogZone2 = dialog2;
        gm.lanternLight = lanternLight;
        gm.lanternLamp = lamp.GetComponent<Renderer>();
        gm.bellPivot = bellPivot;
        gm.bellRenderer = bell.GetComponent<Renderer>();
        gm.portalRenderer = portal.GetComponent<Renderer>();
        gm.portalLight = portalLight;
        gm.restartButton = restart;

        // Puerta -> destino zona 2
        gate.AddComponent<GazeInteractable>();
        GazeInteractable gi = gate.GetComponent<GazeInteractable>();
        gi.kind = InteractKind.Gate;
        gi.gazeTime = 1.5f;
        gi.destination = start2;

        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(scene);

        if (!string.IsNullOrEmpty(scene.path))
        {
            bool found = false;
            foreach (var s in EditorBuildSettings.scenes) if (s.path == scene.path) found = true;
            if (!found)
            {
                var list = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
                list.Add(new EditorBuildSettingsScene(scene.path, true));
                EditorBuildSettings.scenes = list.ToArray();
            }
        }

        Selection.activeGameObject = z1.gameObject;
        Debug.Log("[VR] Experiencia construida. Guarda con Ctrl+S. Zona1 en " + origin + ", Zona2 60 m mas arriba.");
    }

    // ------------------------------------------------------------------ helpers
    static void DestroyIfExists(string name)
    {
        GameObject g = GameObject.Find(name);
        if (g != null) Object.DestroyImmediate(g);
    }

    static Vector3 FindOrigin()
    {
        Vector3 pivot = SceneView.lastActiveSceneView != null ? SceneView.lastActiveSceneView.pivot : Vector3.zero;
        if (Physics.Raycast(pivot + Vector3.up * 100f, Vector3.down, out RaycastHit hit, 400f)) return hit.point;
        return pivot;
    }

    // Altura del suelo en la posicion local (x,z) de la zona 1, relativa al origen
    static float Gy(float x, float z)
    {
        Vector3 w = origin + new Vector3(x, 0f, z);
        Terrain t = Terrain.activeTerrain;
        if (t != null) return t.SampleHeight(w) + t.transform.position.y - origin.y;
        if (Physics.Raycast(w + Vector3.up * 50f, Vector3.down, out RaycastHit h, 200f)) return h.point.y - origin.y;
        return 0f;
    }

    static Vector3 L1(float x, float y, float z) { return new Vector3(x, y + Gy(x, z), z); }

    static Transform Empty(string name, Transform parent, Vector3 localPos)
    {
        GameObject g = new GameObject(name);
        g.transform.SetParent(parent, false);
        g.transform.localPosition = localPos;
        return g.transform;
    }

    static GameObject Prim(PrimitiveType t, string name, Transform parent, Vector3 lp, Vector3 ls, Material m, bool collider = true)
    {
        GameObject g = GameObject.CreatePrimitive(t);
        g.name = name;
        g.transform.SetParent(parent, false);
        g.transform.localPosition = lp;
        g.transform.localScale = ls;
        g.GetComponent<Renderer>().sharedMaterial = m;
        if (!collider) Object.DestroyImmediate(g.GetComponent<Collider>());
        return g;
    }

    static GameObject Pad(string name, Transform parent, Vector3 lp, Material m, Vector3 lookAtWorld)
    {
        GameObject g = Prim(PrimitiveType.Cylinder, name, parent, lp, new Vector3(1.8f, 0.03f, 1.8f), m);
        FaceFlat(g.transform, lookAtWorld);
        MakeInteractable(g, InteractKind.Teleport, 1.5f, null);
        return g;
    }

    static void FaceFlat(Transform t, Vector3 worldTarget)
    {
        Vector3 d = worldTarget - t.position;
        d.y = 0f;
        if (d.sqrMagnitude > 0.001f) t.rotation = Quaternion.LookRotation(d);
    }

    static GazeInteractable MakeInteractable(GameObject g, InteractKind k, float time, Transform dest)
    {
        GazeInteractable gi = g.AddComponent<GazeInteractable>();
        gi.kind = k;
        gi.gazeTime = time;
        gi.destination = dest;
        return gi;
    }

    static Light AddLight(Transform parent, Color c, float intensity, float range)
    {
        GameObject g = new GameObject("Luz");
        g.transform.SetParent(parent, false);
        Light l = g.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = c;
        l.intensity = intensity;
        l.range = range;
        l.shadows = LightShadows.None;
        l.enabled = false;
        return l;
    }

    static Light AddLightAt(Transform parent, string name, Vector3 lp, Color c, float intensity, float range)
    {
        GameObject g = new GameObject(name);
        g.transform.SetParent(parent, false);
        g.transform.localPosition = lp;
        Light l = g.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = c;
        l.intensity = intensity;
        l.range = range;
        l.shadows = LightShadows.None;
        l.enabled = false;
        return l;
    }

    static Material MakeMat(string name, Color c, bool unlit)
    {
        if (!AssetDatabase.IsValidFolder(MatFolder)) AssetDatabase.CreateFolder("Assets", "VR_Generated");
        Material m;
        if (unlit)
        {
            m = new Material(Shader.Find("Sprites/Default"));
        }
        else
        {
            GameObject tmp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            m = new Material(tmp.GetComponent<Renderer>().sharedMaterial);   // copia del material por defecto (sirve en Built-in o URP)
            Object.DestroyImmediate(tmp);
        }
        m.color = c;
        string path = MatFolder + "/" + name + ".mat";
        AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(m, path);
        return AssetDatabase.LoadAssetAtPath<Material>(path);
    }
}
