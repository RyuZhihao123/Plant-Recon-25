using UnityEngine;
using UnityEngine.UI;

// UI�ؼ�- checkbox(Toggle) �����л�StruNet��graphģʽ��

public class ToggleStruNet : MonoBehaviour
{
    public GameObject m_main_obj;     // ���ǵ�Plantֲ��������

    public GameObject m_plane_ground; // ��������
    public Material m_matPureBackground; // ��ɫ�ı�����ɫ
    Material m_matDefaultPlaneGround;
    public GameObject m_origin_obj; // ԭ��Ľ�������
    Material m_matDefaultSkyBox;  // Ĭ����պеĲ���

    GameObject m_graphObject = null;

    bool isOn = false;
    int cur_hierarchy_level = 0;

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


    private void Update()
    {
        if(this.isOn)
        {
            if(Input.GetKeyDown(KeyCode.F6))
            {
                var m_main = m_main_obj.GetComponent<Main>();

                cur_hierarchy_level = Mathf.Min(cur_hierarchy_level + 1, m_main.plant.max_box_level);

                if (m_graphObject != null) Destroy(m_graphObject);
                m_graphObject = Geometry.GetMyStructureNetGraph_GameObject(m_main.plant, cur_hierarchy_level);
            }

            if (Input.GetKeyDown(KeyCode.F7))
            {
                var m_main = m_main_obj.GetComponent<Main>();
                cur_hierarchy_level = Mathf.Max(cur_hierarchy_level - 1, 0);

                if (m_graphObject != null) Destroy(m_graphObject);
                m_graphObject = Geometry.GetMyStructureNetGraph_GameObject(m_main.plant, cur_hierarchy_level);
            }

            if (m_graphObject != null && Global_Interactive_Mode.p_is_rotate)
            {
                float angleDelta = 0.8f;
                //m_graphObject.transform.RotateAround(Vector3.zero, Vector3.up, angle: angleDelta);
                m_graphObject.transform.rotation = Quaternion.AngleAxis(-Global_Interactive_Mode.p_rotate_angle, Vector3.up);
                Global_Interactive_Mode.p_rotate_angle += angleDelta;

                if (Global_Interactive_Mode.p_rotate_angle >= 360)
                {
                    Global_Interactive_Mode.p_rotate_angle = 0;
                    Global_Interactive_Mode.p_is_rotate = false;
                }

                string name = string.Format("C:/Users/liuzh/Desktop/sketch_cache/��Ƶ����/Temp-Frames/shot{0}.png", Global_Interactive_Mode.p_framedID);
                Global_Interactive_Mode.p_framedID++;
                ScreenCapture.CaptureScreenshot(name);
            }

        }
    }

    void OnChanged(bool v)
    {
        //��ǰToggle����������ˣ�todo sth...
        var m_main = m_main_obj.GetComponent<Main>();

        isOn = v;

        if (v == true)  // ѡ�У�����graphģʽ
        {

            m_graphObject = Geometry.GetMyStructureNetGraph_GameObject(m_main.plant, 0);

            cur_hierarchy_level = 0;
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
            //m_origin_obj.SetActive(true);
            m_main.PlantObj.SetActive(true);
            RenderSettings.skybox = m_matDefaultSkyBox;
            m_plane_ground.GetComponent<Renderer>().material = m_matDefaultPlaneGround;
        }
    }
}
