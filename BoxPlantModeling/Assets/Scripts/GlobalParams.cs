using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// ��־�Ƶ� "L-ϵͳ" �����ֲ�
/// 
/// + - : ��Z�� ��/����ת 
/// \ / : ��Y�� ��/����ת
/// & ^ : ��X�� ��/����ת
///
/// F : ������֦-�ƶ�Turtle������: len��
/// J : ������Ҷ������: w,h,horAng,VerAng��
/// 
/// ! ? : ����/���� ����Turtle�ƶ��Ĳ��� ����p_branch_len_factor��Ӧ��
/// ; @ : ����/���� ����֦����ת�Ƕȣ���p_angX_factorXYZ��Ӧ����ע�� ";"�Ƿֺ�
/// 


// 



public class GlobalParams
{
    //public static string p_default_flower = "flower2";
    public static string p_default_flower = "flower2";
    public static string p_default_leaf = "default.png";
    
    // [1] Plant�ࣺ
    // �������
    public static float p_base_radius = 0.0015f;  // ֦����ĩ�˰뾶 0.0015, 1.043
    public static float p_radius_factor = 1.043f; // ֦�ɰ뾶�Ŵ�ϵ��
                                                 //     ---Ĭ�ϲ�����(0.008f, 1.02f)
                                                 //     ---03��ľ (��ɫ) 0.01f, 1.04f;
                                                 //     ---Ҭ����:0.023f 1.04f
                                                 //     ---01��ľ (��ɫ) 0.01f, 1.035f
    // ��Ҷ���
    public static float p_leaf_witdth = 0.2f;  // ��Ҷ���
    public static float p_leaf_height = 0.2f;  // ��Ҷ�߶�
    public static float p_leaf_horBend_Ang = 0f;  // ��Ҷˮƽ�������
    public static float p_leaf_verBend_Ang = 0f;  // ��Ҷ��ֱ�������
    // �������
    public static float p_flower_scale = 0.3f; // ���������ϵ��

    //[2] GrammarParser�ࣺ
    // Ĭ����ת���� (+-\/&^)
    public static float p_angX = 25.0f;   // x������ת�Ƕ�
    public static float p_angY = 25.0f;   // y������ת�Ƕ�
    public static float p_angZ = 25.0f;   // z������ת�Ƕ�
    public static float p_angX_factor = 1.0f;  // ��������ϵ��-x������ת�Ƕ�
    public static float p_angY_factor = 1.0f;  // ��������ϵ��-y������ת�Ƕ�
    public static float p_angZ_factor = 1.0f;  // ��������ϵ��-z������ת�Ƕ�


    // Ĭ��֦�ɲ��� (F)
    public static float p_branch_len = 0.20f;  // Ĭ��֦�ɳ��� 0.2f
                                               //     ---���ݼ�չʾ��: 0.24
    public static float p_branch_len_factor = 0.9f;  // ��������ϵ��-֦�ɳ���

    // ����Կ���
    public static float p_prob_branch = 0.9f;  // ����ʽ�����ɸ���
    public static float p_prob_leaf = 0.8f;    // ҶƬ����������   (δʹ��)


}

public class GlobalPath
{
    static public string p_leaf_path = "Assets/Materials/LeafTextures/";
    static public string p_flower_path = "Assets/Resources/Flowers/";
    static public string p_seg_leaf_color_file = "Assets/Scripts/PythonScripts/leaf_color.txt";  // leaf����ɫ�ļ�

    static public string p_catch_path = "C:/Users/liuzh/Desktop/cache/";   // Ĭ�ϵ�ͼ�ṹ�Ĵ洢·��
    static public string p_GAE_graph_path = "C:/Users/liuzh/Desktop/dataset_plant_graph/"; // ����ѵ��GAE��ֲ��·��

    // ����ѵ��StructureNet��json�ļ���·��
    static public string p_json_dataset_path = "D:/Projects/PlantAssembly/StruBoxNet/data/partnetdata/plant_hier/";

    // Python.exe��·��(UnityPlant���⻷��)
    static public string p_python_exe = "D:/IDE/Anaconda/envs/UnityPlant/python.exe";
    static public string p_python_server_script = "C:/Users/liuzh/PycharmProjects/PlantAssemblyPythonLib/Plant_Server.py";
}

public class Interactive_OP
{
    static public bool p_isUseSubTree = false;
    static public string p_current_leaf_tex_name;
    static public string p_current_flower_name;

    static public bool p_isMoveSubTree;  // �Ƿ��ƶ�����

    static public float p_node_move_offset;
    static public float p_node_rotate_angle;

    static public float p_leaf_rotate_angle;
    static public float p_leaf_delta_size;
    static public float p_leaf_delta_angle;

    
}

public class Global_Interactive_Mode
{
    static public bool m_mode;  // ����״̬

    static public float p_rotate_angle = 0.0f;
    static public bool p_is_rotate = false;
    static public int p_framedID = 1;
    static public float p_rotate_delta = 0.1f;
    static public bool p_is_only_1_round = true;
}


public class Global_Materials
{
    // Preset Hierarchical boxes.
    static public bool m_new_box_material_mode = false;  // False:(�ɵ�bbox�Ļ�ͼģʽ), True: (�µ�bbox�Ļ�ͼģʽ-��Ƶ) 
    static public Material m_mat_box_ActiveNode = null;
    static public Material m_mat_box_Branch = null;
    static public Material m_mat_box_Flower = null;
    static public Material m_mat_box_Leaf = null;

    // Ĭ����ɫ
    static float p_box_transparent_alpha = 0.2f;
    static Color m_color_box_Flower = new Color(0.9f, 0.0f, 0.0f, p_box_transparent_alpha);  // ��
    static Color m_color_box_Leaf = new Color(0.71f, 1.0f, 0.56f, p_box_transparent_alpha); // Ҷ��
    static Color m_color_box_Branch = new Color(1.0f, 0.5f, 0.0f, 0.5f);  // ĩ-֦��
    static Color m_color_box_ActiveNode = new Color(0.2f, 0.4f, 0.4f, p_box_transparent_alpha);  // �м�-box(����չ)


    static public void InitializeMats()
    {
        m_mat_box_ActiveNode = Resources.Load<Material>("BoxMaterials/MatBox_ActiveNode");
        m_mat_box_Branch = Resources.Load<Material>("BoxMaterials/MatBox_Branch");
        m_mat_box_Flower = Resources.Load<Material>("BoxMaterials/MatBox_Flower");
        m_mat_box_Leaf = Resources.Load<Material>("BoxMaterials/MatBox_Leaf");

        //m_mat_box_ActiveNode.SetColor("_Color", m_color_box_ActiveNode);

        //m_mat_box_Branch.SetColor("_Color", m_color_box_Branch);
        //m_mat_box_Flower.SetColor("_Color", m_color_box_Flower);
        //m_mat_box_Leaf.SetColor("_Color", m_color_box_Leaf);
        TCP_Client.SendMessage("A mat");
    }

    static public Material GetMaterialFromBoxType(MinBoundingBox.BoxType type)
    {
        if (type == MinBoundingBox.BoxType.endBranch)
            return m_mat_box_Branch;
        if (type == MinBoundingBox.BoxType.flower)
            return m_mat_box_Flower;
        if (type == MinBoundingBox.BoxType.leaf)
            return m_mat_box_Leaf;
        return m_mat_box_ActiveNode;
    }
}


public class GlobalSketchSetting
{
    // �û��˴����Ĳ�������
    // �������
    public static float p_stem_len_ratio = 1.4f;  // C++�˻��Ƶĳ��ȱ任��Unity����ϵ������ϵ��


    // ��Ҷ���
    public static string m_default_leaf_textures = "default.png";   // ��Ҷ����
    public static string m_default_bark_textures = "default.png";   // ��������

    // ������� (base len
    public static float p_twig_base_len = 0.12f;  // �����ɵ�twig�Ļ�������
    public static int p_twig_density = 8;    // �����ɵ�twig���ܶ�90%
    public static float p_twig_gravity_factor = 0.0f;
    public static float p_twig_base_dir_factor = 0.2f;
    //public static float p_twig_base_len = 0.14f;  // �����ɵ�twig�Ļ�������
    //public static int p_twig_density = 12;    // �����ɵ�twig���ܶ�90%
    //public static float p_twig_gravity_factor = -0.2f;
    //public static float p_twig_base_dir_factor = 0.0f;


    public static float p_leaf_density = 0.35f;    // ҶƬ�ܶ�
    public static float p_leaf_size = 0.13f;       // ҶƬ�ߴ�
    public static bool p_leaf_is_crossed = false;   // ��Ҷ�Ƿ��ǽ���ģʽ

    public static float p_leaf_gravity_factor = 0f;
    public static float p_leaf_base_dir_factor = 0.1f;
    //public static float p_leaf_gravity_factor = -0.3f;
    //public static float p_leaf_base_dir_factor = 0.0f;
}


//public class GlobalSketchSetting
//{
//    // �û��˴����Ĳ�������
//    // �������
//    public static float p_stem_len_ratio = 1.4f;  // C++�˻��Ƶĳ��ȱ任��Unity����ϵ������ϵ��


//    // ��Ҷ���
//    public static string m_default_leaf_textures = "default.png";   // ��Ҷ����
//    public static string m_default_bark_textures = "default.png";   // ��������

//    // ������� (base len
//    public static float p_twig_base_len = 0.12f;  // �����ɵ�twig�Ļ�������
//    public static int p_twig_density = 8;    // �����ɵ�twig���ܶ�90%
//    public static float p_twig_gravity_factor = 0.0f;
//    public static float p_twig_base_dir_factor = 0.2f;
//    //public static float p_twig_base_len = 0.14f;  // �����ɵ�twig�Ļ�������
//    //public static int p_twig_density = 12;    // �����ɵ�twig���ܶ�90%
//    //public static float p_twig_gravity_factor = -0.2f;
//    //public static float p_twig_base_dir_factor = 0.0f;


//    public static float p_leaf_density = 0.35f;    // ҶƬ�ܶ�
//    public static float p_leaf_size = 0.05f;       // ҶƬ�ߴ�
//    public static bool p_leaf_is_crossed = false;   // ��Ҷ�Ƿ��ǽ���ģʽ

//    public static float p_leaf_gravity_factor = 0f;
//    public static float p_leaf_base_dir_factor = 0.1f;
//    //public static float p_leaf_gravity_factor = -0.3f;
//    //public static float p_leaf_base_dir_factor = 0.0f;
//}


