using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DropDownController : MonoBehaviour
{

    public TMP_Dropdown dropdown;

    public GameObject humanPanel;
    public GameObject mousePanel;
    public GameObject kpCellPanel;

    public GameObject humanData;
    public GameObject mouseData;
    public GameObject kpCellData;


    // Start is called before the first frame update
    void Start()
    {
        dropdown.onValueChanged.AddListener(OnDropdownChanged);
        OnDropdownChanged(dropdown.value);  // �ʱⰪ ����
    }

    void OnDropdownChanged(int index)
    {
        // �г� Ȱ��ȭ/��Ȱ��ȭ
        humanPanel.SetActive(index == 0);
        mousePanel.SetActive(index == 1);
        kpCellPanel.SetActive(index == 2);

        // ������ ������Ʈ Ȱ��ȭ/��Ȱ��ȭ
        humanData.SetActive(index == 0);
        mouseData.SetActive(index == 1);
        kpCellData.SetActive(index == 2);
    }
}
