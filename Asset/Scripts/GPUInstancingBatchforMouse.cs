
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GPUInstancingBatchforMouse : MonoBehaviour
{
    public GPUinst_mouse_dataloader dataLoader; // CSV 데이터 로더
    public Mesh objMesh; 
    public Material Week_0_mat; // Week material
    public Material Week_1_mat; 
    public Material Week_2_mat; 
    public Material Week_3_mat; 
    public Material Week_4_mat;
    public Material Week_5_mat;
    public Material Week_6_mat; 
    public Material Week_7_mat;
    //public Vector3 databall_scale = new Vector3(0.003f, 0.003f, 0.003f); // default size of databall
    public Vector3 databall_scale = new Vector3(0.009f, 0.009f, 0.009f); 


    public GameObject Dataholder_object;
    public GameObject Week_0_holder_object;  
    public GameObject Week_1_holder_object;
    public GameObject Week_2_holder_object;
    public GameObject Week_3_holder_object;
    public GameObject Week_4_holder_object;
    public GameObject Week_5_holder_object;
    public GameObject Week_6_holder_object;
    public GameObject Week_7_holder_object;

    private int batchSize = 512; // GPU Instancing을 위한 최대 인스턴스 수 제한

    private Dictionary<string, List<Matrix4x4>> categorizedMatrices = new Dictionary<string, List<Matrix4x4>>();
    private Dictionary<string, Material> materialMapping;
    private Dictionary<string, GameObject> categoryParents = new Dictionary<string, GameObject>();

    private List<Matrix4x4> reusableBatch = new List<Matrix4x4>(512); // 재사용 가능한 배치 리스트


    void Start()
    {
        // 데이터 로더가 제대로 설정되지 않았다면 오류 출력 후 종료
        if (dataLoader == null)
        {
            Debug.LogError("dataLoader is not assigned.");
            return;
        }

        // 데이터 로드
        dataLoader.LoadCSVdata();

        // 카테고리에 해당하는 Material 매핑
        materialMapping = new Dictionary<string, Material>
        {
            { "0", Week_0_mat },
            { "1", Week_1_mat },
            { "2", Week_2_mat },
            { "3", Week_3_mat },
            { "4", Week_4_mat },
            { "5", Week_5_mat },
            { "6", Week_6_mat },
            { "7", Week_7_mat }
        };

        // 카테고리별 부모 오브젝트를 Dictionary에 추가
        categoryParents = new Dictionary<string, GameObject>
        {
            { "0", Week_0_holder_object },
            { "1", Week_1_holder_object },
            { "2", Week_2_holder_object },
            { "3", Week_3_holder_object },
            { "4", Week_4_holder_object },
            { "5", Week_5_holder_object },
            { "6", Week_6_holder_object },
            { "7", Week_7_holder_object }
        };

        // 각 카테고리의 빈 부모 GameObject 생성 및 데이터 포인트 배치
        foreach (KeyValuePair<string, List<Vector3>> kvp in dataLoader.stageData)
        {
            string category = kvp.Key;
            Material mat = materialMapping.ContainsKey(category) ? materialMapping[category] : Week_0_mat;

            if (categoryParents.ContainsKey(category))
            {
                GameObject categoryParent = categoryParents[category];
                List<Matrix4x4> matrices = new List<Matrix4x4>();

                foreach (Vector3 pos in kvp.Value)
                {
                    // 각 포인트의 위치, 회전, 스케일 정보를 Matrix4x4로 저장
                    Matrix4x4 matrix = Matrix4x4.TRS(pos, Quaternion.identity, databall_scale);
                    matrices.Add(matrix);
                }

                // 각 카테고리의 Matrix 리스트 저장
                categorizedMatrices[category] = matrices;
            }
            else
            {
                Debug.LogWarning($"Category '{category}' does not have a corresponding GameObject.");
            }
        }
    }

    void Update()
    {
        RenderBatches();
    }


    private void RenderBatches()
    {
        foreach (var category in categorizedMatrices)
        {
            string categoryName = category.Key;
            Material mat = materialMapping[categoryName];

            // 해당 카테고리의 부모 GameObject가 활성화된 경우에만 렌더링
            if (categoryParents[categoryName].activeSelf)
            {
                List<Matrix4x4> matrices = category.Value;
                int totalInstances = matrices.Count;

                // 부모 오브젝트의 회전과 스케일을 적용
                Transform parentTransform = Dataholder_object.transform; //categoryParents[categoryName].transform;
                Vector3 parentPosition = parentTransform.position; //
                                                                   // added
                Quaternion parentRotation = parentTransform.rotation;
                Vector3 parentScale = parentTransform.localScale;
                Matrix4x4 transformMatrix = Matrix4x4.TRS(parentPosition, parentRotation, parentScale);


                for (int i = 0; i < totalInstances; i += batchSize)
                {
                    reusableBatch.Clear(); // 재사용 가능한 batch 리스트 초기화

                    // 부모의 회전과 스케일을 반영한 행렬을 추가
                    reusableBatch.AddRange(matrices.Skip(i).Take(batchSize).Select(matrix => transformMatrix * matrix));

                    Graphics.DrawMeshInstanced(objMesh, 0, mat, reusableBatch);
                }
            }
        }
    }

}


