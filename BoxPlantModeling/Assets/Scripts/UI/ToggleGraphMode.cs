using UnityEngine;
using UnityEngine.UI;

// UI�ؼ�- checkbox(Toggle) �����л�Graph���ģʽ��

public class ToggleGraphMode : MonoBehaviour
{
    public GameObject m_main_obj;     // ���ǵ�Plantֲ��������

    public GameObject m_plane_ground; // ��������
    public Material m_matPureBackground; // ��ɫ�ı�����ɫ
    Material m_matDefaultPlaneGround;
    public GameObject m_origin_obj; // ԭ��Ľ�������
    Material m_matDefaultSkyBox;  // Ĭ����պеĲ���

    GameObject m_graphObject = null;
    void Start()
    {
        // ��ʼ������ȡmaterial
        m_matDefaultSkyBox = RenderSettings.skybox;
        m_matDefaultPlaneGround = m_plane_ground.GetComponent<Renderer>().material;

        var toggle = this.GetComponent<Toggle>();

        toggle.interactable = true;

        //����ģʽ
        toggle.transition = Selectable.Transition.ColorTint;
        toggle.group = GetComponent<ToggleGroup>();
        toggle.toggleTransition = Toggle.ToggleTransition.Fade; //ToggleTransition.None
        toggle.isOn = false;

        //��״̬���ı�ʱ����
        toggle.onValueChanged.AddListener((bool v) => { OnChanged(v); });
    }

    void OnChanged(bool v)
    {
        //��ǰToggle����������ˣ�todo sth...
        var m_main = m_main_obj.GetComponent<Main>();

        if (v == true)  // ѡ�У�����graphģʽ
        {
            m_graphObject = Geometry.GetHierarchicalGraph(m_main.plant);

            m_main.PlantObj.SetActive(false);
            m_origin_obj.SetActive(false);
            RenderSettings.skybox = m_matPureBackground;
            m_plane_ground.GetComponent<Renderer>().material = m_matPureBackground;
        }
        else
        {
            if (m_graphObject != null)
                Destroy(m_graphObject);
            m_graphObject = null;
            m_origin_obj.SetActive(true);
            m_main.PlantObj.SetActive(true);
            RenderSettings.skybox = m_matDefaultSkyBox;
            m_plane_ground.GetComponent<Renderer>().material = m_matDefaultPlaneGround;
        }
    }
}
