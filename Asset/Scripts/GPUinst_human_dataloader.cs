using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GPUinst_human_dataloader : MonoBehaviour
{
    // csv string, data point lists, index 
    public string inputCSV;
    private List<Dictionary<string, object>> pointList;
    public Dictionary<string, List<Vector3>> stageData = new Dictionary<string, List<Vector3>>();
    public int num_dataCategory = 2000;

    // 0,1,2,5
    private string data_x_name;
    private string data_y_name;
    private string data_z_name;
    private string stage_name;
    private string stage_category_name;

    // stage_category와 stage 정보를 매핑
    public Dictionary<string, List<string>> stageCategoryMapping = new Dictionary<string, List<string>>()
    {
        { "Healthy", new List<string> { "F1", "F2", "M1" } },
        { "T1", new List<string> { "T1-1", "T1-2", "T1-3" } },
        { "T2", new List<string> { "T2-1", "T2-2", "T2-3" } },
        { "T3 and T4", new List<string> { "T3-1", "T3-2", "T4-1" } }
    };


    public void LoadCSVdata()
    {
        pointList = CSVReader.Read(inputCSV);
        List<string> columnList = new List<string>(pointList[1].Keys);  // columnList[0] = x_coord

        data_x_name = columnList[0];   // x_coord
        data_y_name = columnList[1];   // y_coord
        data_z_name = columnList[2];   // z_coord

        stage_name = columnList[4];  // F1, F2, M1 ... stages 
        stage_category_name = columnList[5];    // stage categroy  : Healthy, T1. T2. T3 and T4

        // Min-Max norm.
        float xMax = FindMaxValue(data_x_name);
        float xMin = FindMinValue(data_x_name);

        float yMax = FindMaxValue(data_y_name);
        float yMin = FindMinValue(data_y_name);

        float zMax = FindMaxValue(data_z_name);
        float zMin = FindMinValue(data_z_name);


        // stage_category와 stage별 데이터를 저장할 임시 딕셔너리 생성
        Dictionary<string, Dictionary<string, List<Vector3>>> allStageData = new Dictionary<string, Dictionary<string, List<Vector3>>>();
        

        for (int i = 0; i < pointList.Count; i++)
        {

            float x = ((Convert.ToSingle(pointList[i][data_x_name]) - xMin) / (xMax - xMin)) - 0.5f;
            float y = ((Convert.ToSingle(pointList[i][data_y_name]) - yMin) / (yMax - yMin)) - 0.5f;
            float z = ((Convert.ToSingle(pointList[i][data_z_name]) - zMin) / (zMax - zMin)) - 0.5f;

            Vector3 normalizedPoint = new Vector3(x, y, z);
            string category = pointList[i][stage_category_name].ToString();
            string stage = pointList[i][stage_name].ToString();

            // 각 stage별 데이터 그룹화
            if (!allStageData.ContainsKey(category)) //  healthy, T1...
            {
                allStageData[category] = new Dictionary<string, List<Vector3>>();
            }
            if (!allStageData[category].ContainsKey(stage))  // F1, F2,, 
            {
                allStageData[category][stage] = new List<Vector3>();
            }
            allStageData[category][stage].Add(normalizedPoint);

        }


        // 각 클래스별 num_dataCategory 개씩  무작위 샘플링
        System.Random random = new System.Random();

        foreach (var categoryKvp in allStageData)  /// stage_category , (stage, norm_pos)
        {
            string category = categoryKvp.Key;
            var stageGroups = categoryKvp.Value;  
            int numStages = stageCategoryMapping[category].Count;
            int samplesPerStage = num_dataCategory / numStages;

            stageData[category] = new List<Vector3>();

            foreach (var stageKvp in stageGroups)
            {
                string stage = stageKvp.Key;


                if (stage != "F1")
                {
                    List<Vector3> dataPoints = stageKvp.Value;

                    List<Vector3> sampledData = dataPoints.Count > samplesPerStage
                        ? dataPoints.OrderBy(x => random.Next()).Take(samplesPerStage).ToList()
                        : new List<Vector3>(dataPoints);

                    stageData[category].AddRange(sampledData);
                  //  Debug.Log($"Category: {category}, Stage: {stage}, Sampled Count: {sampledData.Count}");
                }

            }

        }
    }

    private float FindMaxValue(string columnName)
    {
        //set initial value to first value
        float maxValue = Convert.ToSingle(pointList[0][columnName]);

        //Loop through Dictionary, overwrite existing maxValue if new value is larger
        for (var i = 0; i < pointList.Count; i++)
        {
            if (maxValue < Convert.ToSingle(pointList[i][columnName]))
                maxValue = Convert.ToSingle(pointList[i][columnName]);
        }

        //Spit out the max value
        return maxValue;
    }


    private float FindMinValue(string columnName)
    {

        float minValue = Convert.ToSingle(pointList[0][columnName]);

        //Loop through Dictionary, overwrite existing minValue if new value is smaller
        for (var i = 0; i < pointList.Count; i++)
        {
            if (Convert.ToSingle(pointList[i][columnName]) < minValue)
                minValue = Convert.ToSingle(pointList[i][columnName]);
        }

        return minValue;
    }

}
