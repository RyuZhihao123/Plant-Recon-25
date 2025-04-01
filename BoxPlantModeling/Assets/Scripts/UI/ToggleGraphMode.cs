using UnityEngine;
using UnityEngine.UI;

// UI控件- checkbox(Toggle) 用于切换Graph结点模式。

public class ToggleGraphMode : MonoBehaviour
{
    public GameObject m_main_obj;     // 我们的Plant植物主物体

    public GameObject m_plane_ground; // 地面物体
    public Material m_matPureBackground; // 纯色的背景颜色
    Material m_matDefaultPlaneGround;
    public GameObject m_origin_obj; // 原点的胶囊物体
    Material m_matDefaultSkyBox;  // 默认天空盒的材质

    GameObject m_graphObject = null;
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

    void OnChanged(bool v)
    {
        //当前Toggle组件被触发了，todo sth...
        var m_main = m_main_obj.GetComponent<Main>();

        if (v == true)  // 选中，进入graph模式
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
