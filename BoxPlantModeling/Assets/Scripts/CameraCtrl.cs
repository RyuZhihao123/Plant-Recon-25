using UnityEngine;
using System.Collections.Generic;


 public partial class CameraCtrl : MonoBehaviour
{
    // 相机移动相关
    public float moveVerticalSpeed = 10.0f;//水平移动速度
    public float moveHorizontalSpeed = 10.0f;//垂直移动速度
    public float moveJumpSpeed = 10.0f;//垂直移动速度
    private float angleX = 0.0f;
    private float angleY = 0.0f;
    private float movement_dist = 0.1f;

    // 是否显示Graph模式
    public bool isGraphRenderMode = false;

    Vector3 last_pos;
    bool m_left_mouseBtn_down = false;
    bool m_right_mouseBtn_down = false;
    Internode m_curBranch = null;
    int m_curLeafID = -1;
    int m_curFlowerID = -1;

    Camera m_camera; // 相机
    GameObject m_cursorObject;  // 有时候用来当作cursor显示的Object

    // 用来获取Plant对象（Main.Plant)
    public GameObject m_main_obj;  // main那个脚本对应的物体
    Main m_main;                   // main脚本对象

    public void ResetState()
    {
        if (m_curBranch != null && m_curLeafID != -1)
        {
            Leaf lf = (Leaf)m_curBranch.kits[m_curLeafID];
            lf.isSelected = false;
            m_main.ConstructPlantGameObjects();
        }

        if (m_curBranch != null && m_curFlowerID != -1)
        {
            Flower fl = (Flower)m_curBranch.kits[m_curFlowerID];
            fl.isSelected = false;
            m_main.ConstructPlantGameObjects();
        }
        m_curLeafID = -1;
        m_curFlowerID = -1;
        m_curBranch = null;
        m_cursorObject.transform.rotation = Quaternion.identity;

        m_cursorObject.SetActive(false);
        m_left_mouseBtn_down = false;
        m_right_mouseBtn_down = false;
    }


    void Start()
    {
        m_camera = GetComponent<Camera>();
        m_main = m_main_obj.GetComponent<Main>();

        m_cursorObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        m_cursorObject.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);

        
        m_cursorObject.SetActive(false);
    }


    void Update()
    {
        DoMouseEvent();  // 鼠标交互事件

        DoKey_MoveEvent();
    }

    void DoMouseEvent()
    {
       
    }



    // 键盘事件
    bool m_camera_move_flag = false;  // 相机是否发生移动

    void DoKey_MoveEvent()
    {
        // ZX-CV-BN: （在选定小球的时候）沿着XYZ轴移动
        // 绕XYZ轴旋转。
        // 在选择一个节点的基础上 -> （如果按下alt）可以拖动一个线条


        ///////////////// 移动相机 WASD /////////////////
        if (Input.GetKeyDown(KeyCode.LeftControl))  // control键用来开启/关闭
        {
            m_camera_move_flag = !m_camera_move_flag;
        }


        if (m_camera_move_flag)
        {
            float Ztranslation = Input.GetAxis("Vertical") * moveVerticalSpeed * Time.deltaTime;
            float Xtranslation = Input.GetAxis("Horizontal") * moveHorizontalSpeed * Time.deltaTime;
            float Ytranslation = Input.GetAxis("Jump") * moveJumpSpeed * Time.deltaTime;

            //重新计算旋转角度
            angleX += Input.GetAxis("Mouse X");  // 水平方向转过的角度 [-width, width]
            angleY += Input.GetAxis("Mouse Y");  // 竖直方向转过的角度 [-height, height]

            if (angleX < 0)
                angleX += 360;
            if (angleY > 360)
                angleX -= 360;
            angleY = Mathf.Clamp(angleY, -89, 89);

            //----------------------旋转视角---------------------------
            transform.forward = new Vector3(
                1.0f * Mathf.Cos(3.141592653f * angleY / 180.0f) * Mathf.Sin(3.141592653f * angleX / 180.0f),
                1.0f * Mathf.Sin(3.141592653f * angleY / 180.0f),
                1.0f * Mathf.Cos(3.141592653f * angleY / 180.0f) * Mathf.Cos(3.141592653f * angleX / 180.0f)
                ).normalized;  // 正向默认都是(0,0,1)

            // --------------WASD移动 & space上移------------------
            transform.Translate(0, 0, Ztranslation);
            transform.Translate(Xtranslation, 0, 0);
            transform.Translate(0, Ytranslation, 0);
        }
    }



}
