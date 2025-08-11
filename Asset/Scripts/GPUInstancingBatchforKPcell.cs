
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;


public class GPUInstancingBatchforKPcell : MonoBehaviour
{
    public GPUinst_cellline_dataloader dataHolder; // csv reader 참조
    public Mesh mesh;
    public Material instancingMaterial;
    public Vector3 databallScale = new Vector3(0.0005f, 0.0005f, 0.0005f); // 기존 0.02보다 큼

    [Header("Marker Holder Objects")]
    public GameObject PanEV_holder;
    public GameObject CD63_holder;
    public GameObject EpCAM_holder;
    public GameObject CD9_holder;
    public GameObject SDC1_holder;
    public GameObject EGFR_holder;
    public GameObject PDL1_holder;
    public GameObject HER2_holder;
    public GameObject ADAM10_holder;
    public GameObject CTSH_holder;
    public GameObject MET_holder;
    public GameObject CD81_holder;

    private Dictionary<string, GameObject> markerHolders;
    private Dictionary<string, Color> markerColors;

    private Dictionary<string, List<Matrix4x4>> markerMatrices = new();
    private Dictionary<string, List<Color>> markerColorsPerInstance = new();


    private Dictionary<string, bool> markerToggleStatus = new();


    private MaterialPropertyBlock mpb;
    private int batchSize = 1;

    void Start()
    {
        InitToggleStatus();

        InitColorMap();
        InitMarkerHolders();
        PrepareAllMarkerInstances();
    }

    void Update()
    {
        RenderVisibleMarkers();
    }

    void InitColorMap()
    {
        markerColors = new Dictionary<string, Color>
        {
            {"PanEV", HexToColor("#AAAAAA") },
            { "CD63", HexToColor("#FF4C4C") },
            { "EpCAM", HexToColor("#FF9900") },
            { "CD9", HexToColor("#FFD700") },
            { "SDC1", HexToColor("#A2FF00") },
            { "EGFR", HexToColor("#00FFFF") },
            { "PDL1", HexToColor("#4C9BFF") },
            { "HER2", HexToColor("#B266FF") },
            { "ADAM10", HexToColor("#FF66B2") },
            { "CTSH", HexToColor("#00FF99") },
            { "MET", HexToColor("#FFFFFF") },
            { "CD81", HexToColor("#90CAF9") }
        };

        //markerColors = new Dictionary<string, Color>
        //{
        //    {"PanEV", HexToColor("#AAAAAA") },
        //    { "CD63", HexToColor("#1f77b4") },
        //    { "EpCAM", HexToColor("#bcbd22") },
        //    { "CD9", HexToColor("#2ca02c") },
        //    { "SDC1", HexToColor("#ff9896") },
        //    { "EGFR", HexToColor("#d62728") },
        //    { "PDL1", HexToColor("#ffbb78") },
        //    { "HER2", HexToColor("#9467bd") },
        //    { "ADAM10", HexToColor("#ffda44") },
        //    { "CTSH", HexToColor("#8c564b") },
        //    { "MET", HexToColor("#ff7f0e") },
        //    { "CD81", HexToColor("#c5b0d5") }
        //};
    }
    void InitMarkerHolders()
    {
        markerHolders = new Dictionary<string, GameObject>
        {
            {"PanEV",PanEV_holder },
            { "CD63", CD63_holder },
            { "EpCAM", EpCAM_holder },
            { "CD9", CD9_holder },
            { "SDC1", SDC1_holder },
            { "EGFR", EGFR_holder },
            { "PDL1", PDL1_holder },
            { "HER2", HER2_holder },
            { "ADAM10", ADAM10_holder },
            { "CTSH", CTSH_holder },
            { "MET", MET_holder },
            { "CD81", CD81_holder }
        };
    }

    void PrepareAllMarkerInstances()
    {
        mpb = new MaterialPropertyBlock();
        markerMatrices.Clear();
        markerColorsPerInstance.Clear();

        markerMatrices["PanEV"] = new List<Matrix4x4>();
        markerColorsPerInstance["PanEV"] = new List<Color>();

        if (!markerToggleStatus.ContainsKey("PanEV"))
            markerToggleStatus["PanEV"] = false;

        foreach (var ev in dataHolder.evList)
        {
            Vector3 jitter = UnityEngine.Random.insideUnitSphere * 0.01f;

            var (color, scale) = MixColorAndScale(ev.activeMarkers); // 새 함수 사용

            Matrix4x4 matrix = Matrix4x4.TRS(ev.position + jitter, Quaternion.identity, scale);

            markerMatrices["PanEV"].Add(matrix);
            markerColorsPerInstance["PanEV"].Add(color);
        }


        UpdateColorsFromToggles();  // 처음 한번 색상 설정
    }


    void RenderVisibleMarkers()
    {
        GameObject parent = PanEV_holder;
        var matrices = markerMatrices["PanEV"];
        var colors = markerColorsPerInstance["PanEV"];

        var transformMatrix = parent.transform.localToWorldMatrix;

        int totalDrawCount = 0;

        for (int i = 0; i < matrices.Count; i += batchSize)
        {
            int count = Mathf.Min(batchSize, matrices.Count - i);

            // extract matrix and color
            var batch = matrices.GetRange(i, count).Select(m => transformMatrix * m).ToList();
            var batchColors = colors.GetRange(i, count).Select(c => (Vector4)c).ToList();

            mpb.Clear();
            mpb.SetVectorArray("_Color", batchColors);
            Graphics.DrawMeshInstanced(mesh, 0, instancingMaterial, batch, mpb);

            totalDrawCount += batch.Count;
        }
    }


    (Color, Vector3) MixColorAndScale(List<string> markers)
    {
        var active = markers
            .Where(m => m != "PanEV" && markerToggleStatus.ContainsKey(m) && markerToggleStatus[m])
            .ToList();

        if (active.Count == 0)
            return (markerColors["PanEV"], databallScale);  // 기본

        // 색상 혼합
        Color mixed = markerColors[active[0]];
        for (int i = 1; i < active.Count; i++)
            mixed = Color.Lerp(mixed, markerColors[active[i]], 0.25f);

        mixed *= 1.2f;  // 약간 밝기 증가

        // 활성 마커 수에 따라 스케일 조절
        float scaleMultiplier = 1f + 0.5f * active.Count;  // ex) 1개: 1.5, 2개: 2.0, 3개: 2.5 ...
        Vector3 scale = databallScale * scaleMultiplier;

        return (mixed, scale);
    }

    Color HexToColor(string hex)
    {
        Color c;
        ColorUtility.TryParseHtmlString(hex, out c);
        return c;
    }


    void UpdateColorsFromToggles()
    {
        var newColors = new List<Color>();
        var newMatrices = new List<Matrix4x4>();

        foreach (var ev in dataHolder.evList)
        {
            var (color, scale) = MixColorAndScale(ev.activeMarkers);

            Vector3 jitter = Vector3.zero;  // 또는 기존 위치 유지
            Matrix4x4 matrix = Matrix4x4.TRS(ev.position + jitter, Quaternion.identity, scale);

            newColors.Add(color);
            newMatrices.Add(matrix);
        }

        markerColorsPerInstance["PanEV"] = newColors;
        markerMatrices["PanEV"] = newMatrices;

        //Debug.Log("[UpdateColorsFromToggles] Updated matrices & colors.");
    }

    void InitToggleStatus()
    {
        markerToggleStatus = new Dictionary<string, bool>
        {
            { "PanEV", false }, // 체크박스 없지만, 에러 방지를 위해 등록 필요
            { "CD63", false },
            { "EpCAM", false },
            { "CD9", false },
            { "SDC1", false },
            { "EGFR", false },
            { "PDL1", false },
            { "HER2", false },
            { "ADAM10", false },
            { "CTSH", false },
            { "MET", false },
            { "CD81", false }
        };
    }
    public void OnMarkerToggleChanged(string markerName, bool isOn)
    {
        //Debug.Log($"[MARKER TOGGLE] {markerName} → {isOn}");

        if (markerToggleStatus.ContainsKey(markerName))
        {
            markerToggleStatus[markerName] = isOn;
            UpdateColorsFromToggles();
        }
    }
}


