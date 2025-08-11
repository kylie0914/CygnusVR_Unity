
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GPUInstancingBatchforHuman : MonoBehaviour
{
    public GPUinst_human_dataloader dataLoader; // CSV ������ �δ�
    public Mesh objMesh; 
    public Material HD_mat; 
    public Material T1_mat; // T1 ī�װ��� Material
    public Material T2_mat; // T2 ī�װ��� Material
    public Material T34_mat; // T3 and T4 ī�װ��� Material
    public Vector3 databall_scale = new Vector3(0.003f, 0.003f, 0.003f); // ������ ����Ʈ �⺻ ������
    
    public GameObject Dataholder_object;
    public GameObject HD_holder_object;  
    public GameObject T1_holder_object;
    public GameObject T2_holder_object;
    public GameObject T34_holder_object;

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
            { "Healthy", HD_mat },
            { "T1", T1_mat },
            { "T2", T2_mat },
            { "T3 and T4", T34_mat }
        };

        // ī�װ��� �θ� ������Ʈ�� Dictionary�� �߰�
        categoryParents = new Dictionary<string, GameObject>
        {
            { "Healthy", HD_holder_object },
            { "T1", T1_holder_object },
            { "T2", T2_holder_object },
            { "T3 and T4", T34_holder_object }
        };

        // �� ī�װ��� �� �θ� GameObject ���� �� ������ ����Ʈ ��ġ
        foreach (KeyValuePair<string, List<Vector3>> kvp in dataLoader.stageData)
        {
            string category = kvp.Key;
            Material mat = materialMapping.ContainsKey(category) ? materialMapping[category] : HD_mat;

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


                //Quaternion parentRotation = parentTransform.rotation;
                //Vector3 parentScale = parentTransform.localScale;
                //Matrix4x4 transformMatrix = Matrix4x4.TRS(Vector3.zero, parentRotation, parentScale);

                for (int i = 0; i < totalInstances; i += batchSize)
                {
                    reusableBatch.Clear(); // ���� ������ batch ����Ʈ �ʱ�ȭ

                    // �θ��� ȸ���� �������� �ݿ��� ����� �߰�
                    reusableBatch.AddRange(matrices.Skip(i).Take(batchSize).Select(matrix => transformMatrix * matrix));

                    Graphics.DrawMeshInstanced(objMesh, 0, mat, reusableBatch);
                    //// �� ��ġ�� ���� �θ� ������Ʈ�� ȸ�� �� �������� �ݿ��Ͽ� ����� ��ȯ
                    //List<Matrix4x4> batch = matrices
                    //    .Skip(i)
                    //    .Take(batchSize)
                    //    .Select(matrix => transformMatrix * matrix) // �θ��� ȸ�� �� �������� ���� ����
                    //    .ToList();

                    //Graphics.DrawMeshInstanced(objMesh, 0, mat, batch);
                }
            }
        }
    }

}


