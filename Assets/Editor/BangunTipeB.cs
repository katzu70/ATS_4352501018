using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// Membangun seluruh scene Tipe B (Langkah 1-4 + B5-B6) otomatis.
// Menu: ATS > Bangun Tipe B
public class BangunTipeB : EditorWindow
{
    string nim = "";

    [MenuItem("ATS/Bangun Tipe B")]
    static void Buka()
    {
        GetWindow<BangunTipeB>("ATS Tipe B");
    }

    void OnGUI()
    {
        GUILayout.Label("Isi NIM lalu klik tombol", EditorStyles.boldLabel);
        nim = EditorGUILayout.TextField("NIM", nim);
        if (GUILayout.Button("Bangun Scene Tipe B"))
        {
            Bangun(nim.Trim());
        }
    }

    static void Bangun(string nim)
    {
        if (nim.Length == 0 || !char.IsDigit(nim[nim.Length - 1]))
        {
            EditorUtility.DisplayDialog("NIM salah", "Isi NIM dengan angka.", "OK");
            return;
        }

        int digit = nim[nim.Length - 1] - '0';
        if (digit % 2 == 1)
        {
            bool lanjut = EditorUtility.DisplayDialog(
                "NIM ganjil",
                "NIM ganjil berarti Tipe A. Tetap bangun Tipe B?",
                "Lanjut", "Batal");
            if (!lanjut) return;
        }
        float kecepatan = 3 + digit;

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        Directory.CreateDirectory("Assets/Scenes");
        Directory.CreateDirectory("Assets/Prefabs");
        Directory.CreateDirectory("Assets/Sprites");
        AssetDatabase.Refresh();

        Sprite kotak = BuatSprite();

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.transform.position = new Vector3(0, 0, -10);
            cam.backgroundColor = new Color(0.1f, 0.1f, 0.15f);
        }

        // ---- UI ----
        var canvasGo = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas),
            typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        new GameObject("EventSystem",
            typeof(UnityEngine.EventSystems.EventSystem),
            typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));

        Text identitas = BuatText(canvasGo.transform, "TeksIdentitas",
            "ATS - " + nim + " - Tipe B", 25, Color.white);
        var ri = identitas.rectTransform;
        ri.anchorMin = ri.anchorMax = new Vector2(0, 1);
        ri.pivot = new Vector2(0, 1);
        ri.anchoredPosition = new Vector2(10, -10);
        ri.sizeDelta = new Vector2(500, 40);
        identitas.alignment = TextAnchor.UpperLeft;

        Text pesan = BuatText(canvasGo.transform, "TeksPesan", "", 47, Color.green);
        var rp = pesan.rectTransform;
        rp.anchorMin = rp.anchorMax = new Vector2(0.5f, 0.5f);
        rp.pivot = new Vector2(0.5f, 0.5f);
        rp.anchoredPosition = Vector2.zero;
        rp.sizeDelta = new Vector2(400, 80);
        pesan.alignment = TextAnchor.MiddleCenter;

        // ---- Tembok ----
        Tembok(kotak, "Tembok_Atas", new Vector3(0, 4.5f, 0), new Vector3(18, 1, 1));
        Tembok(kotak, "Tembok_Bawah", new Vector3(0, -4.5f, 0), new Vector3(18, 1, 1));
        Tembok(kotak, "Tembok_Kiri", new Vector3(-8.5f, 0, 0), new Vector3(1, 10, 1));
        Tembok(kotak, "Tembok_Kanan", new Vector3(8.5f, 0, 0), new Vector3(1, 10, 1));

        // ---- Titik awal ----
        var titikAwal = new GameObject("TitikAwal");
        titikAwal.transform.position = new Vector3(-7, -3, 0);

        // ---- Player ----
        GameObject player = Kotak(kotak, "Player", new Vector3(-7, -3, 0), Vector3.one, Color.blue);
        player.tag = "Player";
        player.GetComponent<SpriteRenderer>().sortingOrder = 1;
        var rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        var pc = player.AddComponent<BoxCollider2D>();
        pc.size = Vector2.one;
        var gerak = player.AddComponent<PlayerGerak>();
        gerak.kecepatan = kecepatan;

        // ---- Tujuan ----
        GameObject tujuan = Kotak(kotak, "Tujuan", new Vector3(7, 3, 0), Vector3.one, Color.green);
        var tc = tujuan.AddComponent<BoxCollider2D>();
        tc.size = Vector2.one;
        tc.isTrigger = true;
        tujuan.AddComponent<Tujuan>().teksPesan = pesan;

        // ---- Rintangan (prefab) ----
        GameObject r = Kotak(kotak, "Rintangan", new Vector3(-3, -1, 0), new Vector3(1, 3, 1), Color.red);
        var rc = r.AddComponent<BoxCollider2D>();
        rc.size = Vector2.one;
        rc.isTrigger = true;
        r.AddComponent<Rintangan>();
        GameObject prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(
            r, "Assets/Prefabs/Rintangan.prefab", InteractionMode.AutomatedAction);

        r.GetComponent<Rintangan>().titikAwal = titikAwal.transform;

        Vector3[] posisi = { new Vector3(1, 1, 0), new Vector3(4, -1, 0) };
        foreach (Vector3 p in posisi)
        {
            var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            inst.transform.position = p;
            inst.GetComponent<Rintangan>().titikAwal = titikAwal.transform;
        }

        string pathScene = "Assets/Scenes/Scn_ATS_" + nim + ".unity";
        EditorSceneManager.SaveScene(scene, pathScene);
        AssetDatabase.SaveAssets();
        Selection.activeGameObject = player;

        EditorUtility.DisplayDialog("Selesai",
            "Scene dibuat: " + pathScene + "\nKecepatan Player = " + kecepatan +
            "\n\nKlik Game view lalu tekan Play. Gerak: W A S D.", "OK");
    }

    static Sprite BuatSprite()
    {
        string path = "Assets/Sprites/Kotak.png";
        if (!File.Exists(path))
        {
            var tex = new Texture2D(4, 4);
            var px = new Color[16];
            for (int i = 0; i < px.Length; i++) px[i] = Color.white;
            tex.SetPixels(px);
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);

            var imp = (TextureImporter)AssetImporter.GetAtPath(path);
            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Single;
            imp.spritePixelsPerUnit = 4;
            imp.filterMode = FilterMode.Point;
            imp.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static GameObject Kotak(Sprite sprite, string nama, Vector3 pos, Vector3 skala, Color warna)
    {
        var go = new GameObject(nama);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = warna;
        go.transform.position = pos;
        go.transform.localScale = skala;
        return go;
    }

    static void Tembok(Sprite sprite, string nama, Vector3 pos, Vector3 skala)
    {
        GameObject t = Kotak(sprite, nama, pos, skala, Color.gray);
        t.AddComponent<BoxCollider2D>().size = Vector2.one;
    }

    static Text BuatText(Transform parent, string nama, string isi, int ukuran, Color warna)
    {
        var go = new GameObject(nama, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var t = go.AddComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.text = isi;
        t.fontSize = ukuran;
        t.color = warna;
        return t;
    }
}
