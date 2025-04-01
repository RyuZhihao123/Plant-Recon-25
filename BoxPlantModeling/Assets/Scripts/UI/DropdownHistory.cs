using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;


public class DropdownHistory : MonoBehaviour
{
    // 用来获取Plant对象（Main.Plant)
    public GameObject m_main_obj;  // main那个脚本对应的物体
    Main m_main;                   // main脚本对象
    public GameObject m_camera;    // 相机对象（用来重置所有选项）
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
        Debug.Log("重新加载树木: " + GlobalPath.p_catch_path + filename);

        if (filename == "---")
        {
            return;
        }

        // 首先检查文件是否存在
        if (File.Exists(GlobalPath.p_catch_path+filename))
        {
            m_camera.GetComponent<CameraCtrl>().ResetState();

            UTIL.LoadPlant(m_main.plant, GlobalPath.p_catch_path + filename);

            m_main.ConstructPlantGameObjects();
        }
        else
        {
            Debug.Log("不存在: "+filename);
            UpdateItems();
            return;
        }
    }

}
