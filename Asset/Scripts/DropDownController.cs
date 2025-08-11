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
        OnDropdownChanged(dropdown.value);  // 초기값 적용
    }

    void OnDropdownChanged(int index)
    {
        // 패널 활성화/비활성화
        humanPanel.SetActive(index == 0);
        mousePanel.SetActive(index == 1);
        kpCellPanel.SetActive(index == 2);

        // 데이터 오브젝트 활성화/비활성화
        humanData.SetActive(index == 0);
        mouseData.SetActive(index == 1);
        kpCellData.SetActive(index == 2);
    }
}
