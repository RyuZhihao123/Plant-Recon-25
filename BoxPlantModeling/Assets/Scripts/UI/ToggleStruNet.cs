using UnityEngine;
using UnityEngine.UI;

// UI控件- checkbox(Toggle) 用于切换StruNet的graph模式。

public class ToggleStruNet : MonoBehaviour
{
    public GameObject m_main_obj;     // 我们的Plant植物主物体

    public GameObject m_plane_ground; // 地面物体
    public Material m_matPureBackground; // 纯色的背景颜色
    Material m_matDefaultPlaneGround;
    public GameObject m_origin_obj; // 原点的胶囊物体
    Material m_matDefaultSkyBox;  // 默认天空盒的材质

    GameObject m_graphObject = null;

    bool isOn = false;
    int cur_hierarchy_level = 0;

    void Start()
    {
        // 初始化，获取material
        m_matDefaultSkyBox = RenderSettings.skybox;
        m_matDefaultPlaneGround = m_plane_ground.GetComponent<Renderer>().material;

        var toggle = this.GetComponent<Toggle>();

        toggle.interactable = true;

        //过渡模式
        toggle.transition = Selectable.Transition.ColorTint;
        toggle.group = GetComponent<ToggleGroup>();
        toggle.toggleTransition = Toggle.ToggleTransition.Fade; //ToggleTransition.None
        toggle.isOn = false;

        //当状态被改变时触发
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

                string name = string.Format("C:/Users/liuzh/Desktop/sketch_cache/视频制作/Temp-Frames/shot{0}.png", Global_Interactive_Mode.p_framedID);
                Global_Interactive_Mode.p_framedID++;
                ScreenCapture.CaptureScreenshot(name);
            }

        }
    }

    void OnChanged(bool v)
    {
        //当前Toggle组件被触发了，todo sth...
        var m_main = m_main_obj.GetComponent<Main>();

        isOn = v;

        if (v == true)  // 选中，进入graph模式
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
