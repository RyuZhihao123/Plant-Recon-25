using UnityEngine;
using UnityEngine.UI;

// UI�ؼ�- checkbox(Toggle) �����л���Ⱦģʽ���Ƿ���Ⱦsegmentation������ģʽ��

public class ToggleSegMode : MonoBehaviour
{
    public GameObject m_main_obj;     // ���ǵ�Plantֲ��������

    public GameObject m_plane_ground; // ��������
    public Material m_matPureBackground; // ��ɫ�ı�����ɫ
    public GameObject m_origin_obj; // ԭ��Ľ�������
    Material m_matDefaultPlaneGround;

    Material m_matDefaultSkyBox;
    

    void Start()
    {
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

        if (v == true)
        {
            m_main.ConstructPlantGameObjects_SEGMENT(isHouseplantMode: false);
            m_origin_obj.SetActive(false);
            RenderSettings.skybox = m_matPureBackground;
            m_plane_ground.GetComponent<Renderer>().material = m_matPureBackground;
        }
        else
        {
            m_main.ConstructPlantGameObjects();
            m_origin_obj.SetActive(true);
            RenderSettings.skybox = m_matDefaultSkyBox;
            m_plane_ground.GetComponent<Renderer>().material = m_matDefaultPlaneGround;
        }
    }
}
