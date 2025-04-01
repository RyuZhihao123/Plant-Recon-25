using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;


public class DropdownHistory : MonoBehaviour
{
    // ������ȡPlant����Main.Plant)
    public GameObject m_main_obj;  // main�Ǹ��ű���Ӧ������
    Main m_main;                   // main�ű�����
    public GameObject m_camera;    // �������������������ѡ�
    TMPro.TMP_Dropdown dropdown;


    void Start()
    {
        m_main = m_main_obj.GetComponent<Main>();
        dropdown = this.GetComponent<TMPro.TMP_Dropdown>();
        
        UpdateItems();

        dropdown.onValueChanged.AddListener(delegate { DropdownItemSelected(dropdown); });
        
    }

    public void UpdateItems()
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(GlobalPath.p_catch_path);

        FileInfo[] files = directoryInfo.GetFiles("*.txt");
        
        dropdown.options.Clear();
        dropdown.options.Add(new TMPro.TMP_Dropdown.OptionData() { text = "---" });

        for (int i=0; i<files.Length; ++i)
        {
            dropdown.options.Add(new TMPro.TMP_Dropdown.OptionData()
            { text = files[files.Length - 1 - i].Name });
        }
        dropdown.value = 0;
    }


    void DropdownItemSelected(TMPro.TMP_Dropdown dropdown)
    {
        string filename = dropdown.options[dropdown.value].text;
        Debug.Log("���¼�����ľ: " + GlobalPath.p_catch_path + filename);

        if (filename == "---")
        {
            return;
        }

        // ���ȼ���ļ��Ƿ����
        if (File.Exists(GlobalPath.p_catch_path+filename))
        {
            m_camera.GetComponent<CameraCtrl>().ResetState();

            UTIL.LoadPlant(m_main.plant, GlobalPath.p_catch_path + filename);

            m_main.ConstructPlantGameObjects();
        }
        else
        {
            Debug.Log("������: "+filename);
            UpdateItems();
            return;
        }
    }

}
