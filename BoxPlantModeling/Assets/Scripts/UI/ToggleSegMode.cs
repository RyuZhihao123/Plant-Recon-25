using UnityEngine;
using UnityEngine.UI;

// UI控件- checkbox(Toggle) 用于切换渲染模式，是否渲染segmentation的样本模式。

public class ToggleSegMode : MonoBehaviour
{
    public GameObject m_main_obj;     // 我们的Plant植物主物体

    public GameObject m_plane_ground; // 地面物体
    public Material m_matPureBackground; // 纯色的背景颜色
    public GameObject m_origin_obj; // 原点的胶囊物体
    Material m_matDefaultPlaneGround;

    Material m_matDefaultSkyBox;
    

    void Start()
    {
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
