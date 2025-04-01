using UnityEngine;
using System.Collections.Generic;


 public partial class CameraCtrl : MonoBehaviour
{
    // ����ƶ����
    public float moveVerticalSpeed = 10.0f;//ˮƽ�ƶ��ٶ�
    public float moveHorizontalSpeed = 10.0f;//��ֱ�ƶ��ٶ�
    public float moveJumpSpeed = 10.0f;//��ֱ�ƶ��ٶ�
    private float angleX = 0.0f;
    private float angleY = 0.0f;
    private float movement_dist = 0.1f;

    // �Ƿ���ʾGraphģʽ
    public bool isGraphRenderMode = false;

    Vector3 last_pos;
    bool m_left_mouseBtn_down = false;
    bool m_right_mouseBtn_down = false;
    Internode m_curBranch = null;
    int m_curLeafID = -1;
    int m_curFlowerID = -1;

    Camera m_camera; // ���
    GameObject m_cursorObject;  // ��ʱ����������cursor��ʾ��Object

    // ������ȡPlant����Main.Plant)
    public GameObject m_main_obj;  // main�Ǹ��ű���Ӧ������
    Main m_main;                   // main�ű�����

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
        DoMouseEvent();  // ��꽻���¼�

        DoKey_MoveEvent();
    }

    void DoMouseEvent()
    {
       
    }



    // �����¼�
    bool m_camera_move_flag = false;  // ����Ƿ����ƶ�

    void DoKey_MoveEvent()
    {
        // ZX-CV-BN: ����ѡ��С���ʱ������XYZ���ƶ�
        // ��XYZ����ת��
        // ��ѡ��һ���ڵ�Ļ����� -> ���������alt�������϶�һ������


        ///////////////// �ƶ���� WASD /////////////////
        if (Input.GetKeyDown(KeyCode.LeftControl))  // control����������/�ر�
        {
            m_camera_move_flag = !m_camera_move_flag;
        }


        if (m_camera_move_flag)
        {
            float Ztranslation = Input.GetAxis("Vertical") * moveVerticalSpeed * Time.deltaTime;
            float Xtranslation = Input.GetAxis("Horizontal") * moveHorizontalSpeed * Time.deltaTime;
            float Ytranslation = Input.GetAxis("Jump") * moveJumpSpeed * Time.deltaTime;

            //���¼�����ת�Ƕ�
            angleX += Input.GetAxis("Mouse X");  // ˮƽ����ת���ĽǶ� [-width, width]
            angleY += Input.GetAxis("Mouse Y");  // ��ֱ����ת���ĽǶ� [-height, height]

            if (angleX < 0)
                angleX += 360;
            if (angleY > 360)
                angleX -= 360;
            angleY = Mathf.Clamp(angleY, -89, 89);

            //----------------------��ת�ӽ�---------------------------
            transform.forward = new Vector3(
                1.0f * Mathf.Cos(3.141592653f * angleY / 180.0f) * Mathf.Sin(3.141592653f * angleX / 180.0f),
                1.0f * Mathf.Sin(3.141592653f * angleY / 180.0f),
                1.0f * Mathf.Cos(3.141592653f * angleY / 180.0f) * Mathf.Cos(3.141592653f * angleX / 180.0f)
                ).normalized;  // ����Ĭ�϶���(0,0,1)

            // --------------WASD�ƶ� & space����------------------
            transform.Translate(0, 0, Ztranslation);
            transform.Translate(Xtranslation, 0, 0);
            transform.Translate(0, Ytranslation, 0);
        }
    }



}
