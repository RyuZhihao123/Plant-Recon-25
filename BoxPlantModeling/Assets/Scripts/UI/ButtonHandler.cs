using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Data;
using System.Text;
using System;

// 点击保存(Save)按钮的事件

public class ButtonHandler : MonoBehaviour
{
	// 用来获取Plant对象（Main.Plant)
	public GameObject m_main_obj;  // main那个脚本对应的物体
	Main m_main;                   // main脚本对象

	public GameObject m_dropdown_history;
	DropdownHistory m_dropdwon; // 脚本对象

	void Start()
	{
		m_main = m_main_obj.GetComponent<Main>();
		m_dropdwon = m_dropdown_history.GetComponent<DropdownHistory>();

		Button btn = this.GetComponent<Button>();
	
		btn.onClick.AddListener(OnClickSave);


	}

	private void OnClickSave()
	{
		DateTime dt = DateTime.Now;
		string filename = string.Format("{0}_{1}_{2}_{3}_{4}_{5}_{6}.txt",
			dt.Year, dt.Month.ToString("D2"), dt.Day.ToString("D2"),
			dt.Hour.ToString("D2"), dt.Minute.ToString("D2"), dt.Second.ToString("D2"), dt.Millisecond.ToString("D3"));
		string filename2 = string.Format("{0}_{1}_{2}_{3}_{4}_{5}_{6}_.txt",
			dt.Year, dt.Month.ToString("D2"), dt.Day.ToString("D2"),
			dt.Hour.ToString("D2"), dt.Minute.ToString("D2"), dt.Second.ToString("D2"), dt.Millisecond.ToString("D3"));
		UTIL.SavePlantStructure(m_main.plant, filename);
		UTIL.SavePlantStructure(m_main.plant_with_subtree, filename2);
		m_dropdwon.UpdateItems();
	}

	
}
