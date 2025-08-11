using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class GPUinst_cellline_dataloader : MonoBehaviour
{
    public string inputCSV;
    public int maxPoints = 20000;

    [HideInInspector]
    public List<EVData> evList = new();
    [HideInInspector]
    public List<string> biomarkerNames = new();

    void Awake()
    {
        LoadCSVdata();
    }

    void LoadCSVdata()
    {
        var pointList = CSVReader.Read(inputCSV);
        List<string> columnList = new(pointList[1].Keys.Select(k => k.Trim().Trim('"')));

        //string xCol = columnList[1]; // tsne_x
        //string yCol = columnList[2]; // tsne_y
        //string zCol = columnList[3]; // tsne_z
        string xCol = "1";
        string yCol = "2";
        string zCol = "3";

        //// 모든 biomarker 이름 (PanEV 포함)
        //biomarkerNames = columnList
        //    .Where(c => c != "" && c != xCol && c != yCol && c != zCol)
        //    .ToList();
        biomarkerNames = columnList
                .Where(c => c != "" && c != "1" && c != "2" && c != "3")
                .ToList();

        //Debug.Log($"[tSNE Loader] Using x: {xCol}, y: {yCol}, z: {zCol}");
        //Debug.Log($"[tSNE Loader] Biomarkers (including PanEV): {string.Join(", ", biomarkerNames)}");


        float xMin = Min(pointList, xCol), xMax = Max(pointList, xCol);
        float yMin = Min(pointList, yCol), yMax = Max(pointList, yCol);
        float zMin = Min(pointList, zCol), zMax = Max(pointList, zCol);

        foreach (var row in pointList)
        {
            Vector3 pos = new(
                (Convert.ToSingle(row[xCol]) - xMin) / (xMax - xMin) - 0.5f,
                (Convert.ToSingle(row[yCol]) - yMin) / (yMax - yMin) - 0.5f,
                (Convert.ToSingle(row[zCol]) - zMin) / (zMax - zMin) - 0.5f
            );
            Vector3 scaledPos = pos * 1f;

            List<string> activeMarkers = new();
            foreach (var m in biomarkerNames)
            {
                if (Convert.ToInt32(row[m]) == 1)
                    activeMarkers.Add(m);
            }

            evList.Add(new EVData { position = scaledPos, activeMarkers = activeMarkers });
        }

        if (evList.Count > maxPoints)
        {
            System.Random rand = new();
            evList = evList.OrderBy(_ => rand.Next()).Take(maxPoints).ToList();
        }

    }

    float Min(List<Dictionary<string, object>> data, string col) =>
        data.Min(p => Convert.ToSingle(p[col]));

    float Max(List<Dictionary<string, object>> data, string col) =>
        data.Max(p => Convert.ToSingle(p[col]));

    [System.Serializable]
    public class EVData
    {
        public Vector3 position;
        public List<string> activeMarkers;
    }

}
