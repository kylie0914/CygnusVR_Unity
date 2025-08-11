
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GPUInstancingBatchforMouse : MonoBehaviour
{
    public GPUinst_mouse_dataloader dataLoader; // CSV ������ �δ�
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

    private int batchSize = 512; // GPU Instancing�� ���� �ִ� �ν��Ͻ� �� ����

    private Dictionary<string, List<Matrix4x4>> categorizedMatrices = new Dictionary<string, List<Matrix4x4>>();
    private Dictionary<string, Material> materialMapping;
    private Dictionary<string, GameObject> categoryParents = new Dictionary<string, GameObject>();

    private List<Matrix4x4> reusableBatch = new List<Matrix4x4>(512); // ���� ������ ��ġ ����Ʈ


    void Start()
    {
        // ������ �δ��� ����� �������� �ʾҴٸ� ���� ��� �� ����
        if (dataLoader == null)
        {
            Debug.LogError("dataLoader is not assigned.");
            return;
        }

        // ������ �ε�
        dataLoader.LoadCSVdata();

        // ī�װ��� �ش��ϴ� Material ����
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

        // ī�װ��� �θ� ������Ʈ�� Dictionary�� �߰�
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

        // �� ī�װ��� �� �θ� GameObject ���� �� ������ ����Ʈ ��ġ
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
                    // �� ����Ʈ�� ��ġ, ȸ��, ������ ������ Matrix4x4�� ����
                    Matrix4x4 matrix = Matrix4x4.TRS(pos, Quaternion.identity, databall_scale);
                    matrices.Add(matrix);
                }

                // �� ī�װ��� Matrix ����Ʈ ����
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

            // �ش� ī�װ��� �θ� GameObject�� Ȱ��ȭ�� ��쿡�� ������
            if (categoryParents[categoryName].activeSelf)
            {
                List<Matrix4x4> matrices = category.Value;
                int totalInstances = matrices.Count;

                // �θ� ������Ʈ�� ȸ���� �������� ����
                Transform parentTransform = Dataholder_object.transform; //categoryParents[categoryName].transform;
                Vector3 parentPosition = parentTransform.position; //
                                                                   // added
                Quaternion parentRotation = parentTransform.rotation;
                Vector3 parentScale = parentTransform.localScale;
                Matrix4x4 transformMatrix = Matrix4x4.TRS(parentPosition, parentRotation, parentScale);


                for (int i = 0; i < totalInstances; i += batchSize)
                {
                    reusableBatch.Clear(); // ���� ������ batch ����Ʈ �ʱ�ȭ

                    // �θ��� ȸ���� �������� �ݿ��� ����� �߰�
                    reusableBatch.AddRange(matrices.Skip(i).Take(batchSize).Select(matrix => transformMatrix * matrix));

                    Graphics.DrawMeshInstanced(objMesh, 0, mat, reusableBatch);
                }
            }
        }
    }

}


