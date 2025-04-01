using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class ButtonRefresh : MonoBehaviour
{
	// ������ȡm_dropdown_history����
	public GameObject m_dropdown_history;
	DropdownHistory m_dropdwon; // �ű�����


	void Start()
	{
		m_dropdwon = m_dropdown_history.GetComponent<DropdownHistory>();

		Button btn = this.GetComponent<Button>();

		btn.onClick.AddListener(OnClickSave);


	}

	private void OnClickSave()
	{
		m_dropdwon.UpdateItems();
	}


}
