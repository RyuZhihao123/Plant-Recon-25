using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;

public class Main : MonoBehaviour
{
    // public ��������Ĳ���
    public Material mat_leaf_template = null;
    public Material mat_Segmentation_Sample = null;
    public Material mat_branch = null;
    public Material mat_arrow = null;

    // ��Ҷ����-�ֵ�
    private Dictionary<string, Material> m_leaf_textures = new Dictionary<string, Material>();
    private Dictionary<string, Material> m_leaf_Seg_Textures = new Dictionary<string, Material>();

    // Start is called before the first frame update
    GrammarParser parser = new GrammarParser();
    public Plant plant = new Plant();
    public Plant plant_with_subtree = new Plant();

    List<Color> colors = new List<Color>();
    List<Vector3> outputs = new List<Vector3>();

    public GameObject PlantObj = null;  // ��ߵ�hierarchy
    GameObject LeafObj = null, FlowerObj = null;
    public GameObject existingPlantModel = null;
    List<Color> seg_Leaf_Colors = new List<Color>();
    void Start()
    {
        // ����������
        PythonHandler.RunPythonScript(GlobalPath.p_python_exe, GlobalPath.p_python_server_script, "");

        TCP_Client.ConnectToServer();

        // ��ʼ��һЩ��Դ
        this.InitFlowerBBox();
        this.InitLeafTextures();
        this.LoadSegmentLeafColor();
        Global_Materials.InitializeMats();

        // [1] ����+����L-system�ļ� 
        parser.ParseFile("C:/Users/liuzh/Desktop/l_systems/������.txt");  //(l4)

        //JsonDatasetGenerator.GenerateJsonDataset(dataNum: 250, iter: 4, parser, isConsiderLeaf: false);  // ����StructureNet��ѵ�����ݼ�

         //[2] ����plant structure
        {
            // parser.ConstructePlantStructure(4, plant.root); // ����grammar����plants Ĭ��4��
        }

        //JsonDatasetGenerator.SaveJson(plant, "C:/Users/liuzh/Desktop/aaaa.json");

        this.ConstructPlantGameObjects();  // ��������gameobject

        for (int i = 0; i < 100; ++i)
        {
            colors.Add(new Color(Random.Range(0.5f, 1.0f), Random.Range(0.5f, 1.0f), Random.Range(0.5f, 1.0f)));
        }

        Debug.Log("����:");
    }


    //////////////////////////////////////////////////////
    //// ��ת������
    bool is_rotated_mode = false;
    int duation = 100;
    int framedID = 0;
    int maxNodeID = -1;

    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.F5))  // �����µ���
        {
            plant.ClearTree(plant.root);
            plant.ClearTree(plant.root_interpolated);
            parser.ConstructePlantStructure(8, plant.root); // ����grammar����plants
            this.ConstructPlantGameObjects();
            return;
        }
        if (Input.GetKeyDown(KeyCode.F2))  // ��΢�Ŷ�����
        {
            plant.randomly_disturb_plant(plant.root);

            this.ConstructPlantGameObjects();

        }
        //if (PlantObj != null && PlantObj.activeInHierarchy == true  && Global_Interactive_Mode.p_is_rotate)
        //{
        //    PlantObj.transform.rotation = Quaternion.AngleAxis(-Global_Interactive_Mode.p_rotate_angle, Vector3.up);

        //    Global_Interactive_Mode.p_rotate_angle += Global_Interactive_Mode.p_rotate_delta;
        //    if (Global_Interactive_Mode.p_rotate_angle >= 360.0f && Global_Interactive_Mode.p_is_only_1_round == true)
        //    {
        //        Global_Interactive_Mode.p_rotate_angle = 0.0f;
        //        Global_Interactive_Mode.p_is_rotate = false;
        //    }
        //}
        //if (Input.GetKeyDown(KeyCode.F9))
        //{
        //    Global_Interactive_Mode.p_is_rotate = !Global_Interactive_Mode.p_is_rotate;
        //    Global_Interactive_Mode.p_rotate_angle = 0.0f;

        //}

        if(Input.GetKeyDown(KeyCode.P))
        {
            Interactive_OP.p_isUseSubTree = !Interactive_OP.p_isUseSubTree;
            Debug.Log("����P:"+Interactive_OP.p_isUseSubTree.ToString());
            this.ConstructPlantGameObjects();
        }


        if (Input.GetKeyDown(KeyCode.F9))
        {
            Global_Interactive_Mode.p_is_rotate = !Global_Interactive_Mode.p_is_rotate;
            //PlantObj.transform.rotation = Quaternion.identity;
            Global_Interactive_Mode.p_framedID = 1;
            Global_Interactive_Mode.p_rotate_angle = 0;
        }

        if (PlantObj.activeInHierarchy == true && Global_Interactive_Mode.p_is_rotate && PlantObj != null)
        {
            float angleDelta = 0.8f;
            //PlantObj.transform.RotateAround(Vector3.zero, Vector3.up, angle: -angleDelta);
            existingPlantModel.transform.RotateAround(Vector3.zero, Vector3.up, angle: -angleDelta);
            Global_Interactive_Mode.p_rotate_angle += angleDelta;

            if (Global_Interactive_Mode.p_rotate_angle >= 360)
                Global_Interactive_Mode.p_is_rotate = false;

            string name = string.Format("E:/Workspace/TreeProjects/HKUST-HierarchicalPlant-SingleImage/ֲ�����/����figures/video_frame_cache/shot{0}.png", Global_Interactive_Mode.p_framedID);
            Global_Interactive_Mode.p_framedID++;
            ScreenCapture.CaptureScreenshot(name);
        }

    }
    //////////////////////////////////////////////////////


    void OnDrawGizmos()
    {
        for (int i = 0; i < plant.originalpoints.Count; ++i)
        {
            Gizmos.color = colors[i % colors.Count];

            for (int k = 0; k < plant.originalpoints[i].Count - 1; k++)
            {
                Vector3 a = plant.originalpoints[i][k];
                Vector3 b = plant.originalpoints[i][k + 1];
                Gizmos.DrawSphere(a, 0.05f);
                Gizmos.DrawSphere(b, 0.05f);
                Gizmos.DrawLine(a, b);
                Handles.DrawBezier(a, b, a, b, Gizmos.color, null, 3);
            }
        }
    }


    // ����plant��hierarchy����gameobjects
    public void ConstructPlantGameObjects()
    {
        Destroy(PlantObj); // ����ԭ��plant


        PlantObj = new GameObject("Plant");
        PlantObj.transform.position = Vector3.zero;

        // [3]����structure����meshes
        // - ����skeleton��mesh


        if(plant_with_subtree != plant)
        {
            plant_with_subtree.ClearTree(plant_with_subtree.root);
            plant_with_subtree.ClearTree(plant_with_subtree.root_interpolated);
        }
        
        plant_with_subtree = Interactive_OP.p_isUseSubTree?
            plant.GetNewPlantStructure(): plant;
        //var meshes = plant.GetSkeletonMesh(isUseInterp: true);
        var meshes = plant_with_subtree.GetSkeletonMesh(isUseInterp:true);

        int v_count = 0;
        for (int i = 0; i < meshes.Count; ++i)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);


            obj.transform.position = new Vector3(0.0f, 0.0f, 0.0f);
            obj.GetComponent<Renderer>().material = mat_branch;
            obj.GetComponent<MeshFilter>().mesh = meshes[i];
            v_count += obj.GetComponent<MeshFilter>().mesh.vertexCount;
            obj.transform.parent = PlantObj.transform;
        }
        //Debug.Log("Skel������Ŀ: " + meshes.Count.ToString());

        v_count += RefreshLeafMesh(plant_with_subtree);
        v_count += RefreshFlowerMesh(plant_with_subtree);

        Debug.Log("������Ŀ: " + (v_count/3).ToString());
        plant.construct_progressive_bbox(plant.root);
        //Debug.Log(plant.root.kits.Count);
        //Debug.Log("Kit������Ŀ: " + leaf_meshes.Count.ToString());
    }


    // ��ʼ�� - ҶƬ��Ӧ�Ĳ���Material���Լ���Ӧ��������
    void InitLeafTextures()
    {
        // ����Ŀ��µĵ����И��~�y��
        DirectoryInfo folder = new DirectoryInfo(GlobalPath.p_leaf_path);
        var files = folder.GetFiles("*.*");
        List<string> items = new List<string>();

        MinBoundingBox.init_leaf_ratio.Clear();

        for (int i = 0; i < files.Length; i++)
        {
            string fileName = files[i].Name.ToLower();

            if (fileName.EndsWith(".png") || fileName.EndsWith(".jpg") || fileName.EndsWith(".bmp"))
            {
                items.Add(files[i].Name);

                // ������ʾ��materials
                {
                    Material mat = new Material(mat_leaf_template);
                    Texture2D texture = AssetDatabase.LoadAssetAtPath(GlobalPath.p_leaf_path + files[i].Name,
                        typeof(Texture2D)) as Texture2D;
                    mat.mainTexture = texture;

                    m_leaf_textures.Add(files[i].Name, mat);  // �Ѳ��ʼ���һ��


                    // ˳�����һ��bounding box�õ�ratioX
                    float maxX, maxY, minX, minY;
                    maxX = maxY = float.MinValue;
                    minX = minY = float.MaxValue;

                    for (int x = 0; x < mat.mainTexture.width; x+=5)
                    {
                        for (int y = 0; y < mat.mainTexture.height; y+=5)
                        {
                            var Color = texture.GetPixel(x, y);

                            if (Color.a < 0.5f)
                                continue;
                            maxX = Mathf.Max(x, maxX);
                            maxY = Mathf.Max(y, maxY);
                            minX = Mathf.Min(x, minX);
                            minY = Mathf.Min(y, minY);
                        }
                    }
                    float half_width = mat.mainTexture.width / 2.0f;

                    minX = (minX < half_width) ? (half_width - minX) / half_width : (minX - half_width) / half_width;
                    maxX = (maxX < half_width) ? (half_width - maxX) / half_width : (maxX - half_width) / half_width;

                    float ratioX = Mathf.Max(minX, maxX);

                    MinBoundingBox.init_leaf_ratio.Add(files[i].Name, (ratioX, 1.0f));
                    Debug.Log("Leaf: " + files[i].Name + " :" + ratioX.ToString());
                }
                // �ָ���ʾ��materials
                {
                    Material mat = new Material(mat_Segmentation_Sample);
                    mat.mainTexture = AssetDatabase.LoadAssetAtPath(GlobalPath.p_leaf_path + files[i].Name,
                        typeof(Texture2D)) as Texture2D;

                    m_leaf_Seg_Textures.Add(files[i].Name, mat);
                }
            }
        }
    }

    // ��ʼ�� - �����bounding box
    void InitFlowerBBox()
    {
        // ����Ŀ��µĵ����л���
        DirectoryInfo folder = new DirectoryInfo(GlobalPath.p_flower_path);
        var files = folder.GetFiles("*.*");

        MinBoundingBox.init_flower_bbox.Clear();

        for (int i = 0; i < files.Length; i++)
        {
            string fileName = files[i].Name.ToLower();

            if (fileName.EndsWith(".fbx"))
            {
                string flowerFileName = fileName.Remove(fileName.Length - files[i].Extension.Length);  //ȥ����׺

                // ����һ����������ʵ�壨�ڳ�ʼλ�ã�
                GameObject f = (GameObject)GameObject.Instantiate(
                    Resources.Load("Flowers/" + flowerFileName), Vector3.zero, Quaternion.identity);
                // Debug.Log("Flower Model: " + fileName.Remove(fileName.Length-files[i].Extension.Length));

                // ����������ж���
                List<Vector3> globalVertices = MinBoundingBox.GetVertexList_FromGameObject(f);

                // �����ʼboundingbox
                MinBoundingBox bbox = MinBoundingBox.Initilize_Flower_BBOX_in_YXZaxis(globalVertices.ToArray());

                MinBoundingBox.init_flower_bbox.Add(flowerFileName, bbox);
                Debug.Log(bbox.ToString());
                Destroy(f);
            }
        }
    }

    // �ع���Ҷ������
    int RefreshLeafMesh(Plant __plant)
    {
        Destroy(LeafObj);
        List<Mesh> leaf_meshes;
        List<string> leaf_texIDs;
        int selectedID = -1;
        (leaf_meshes, leaf_texIDs, selectedID) = __plant.GetLeafMesh();

        LeafObj = new GameObject("Leafs");
        LeafObj.transform.position = Vector3.zero;

        int v_count = 0;
        for (int i = 0; i < leaf_meshes.Count; ++i) // ������Ҷ
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.transform.position = new Vector3(0.0f, 0.0f, 0.0f);
            obj.transform.parent = LeafObj.transform;
            obj.name = "Leaf-" + i.ToString();
            
            if (selectedID == i)
                obj.GetComponent<Renderer>().material = mat_arrow;
            else

                obj.GetComponent<Renderer>().material = m_leaf_textures[leaf_texIDs[i]];

            obj.GetComponent<MeshFilter>().mesh = leaf_meshes[i];

            v_count += obj.GetComponent<MeshFilter>().mesh.vertexCount;
        }
        LeafObj.transform.parent = PlantObj.transform;

        return v_count;
    }

    // �ع����������
    int RefreshFlowerMesh(Plant __plant)
    {
        Destroy(FlowerObj);

        // - ����Flowers���������
        List<GameObject> flower_meshes;

        flower_meshes= __plant.GetFlowerMesh();
        FlowerObj = new GameObject("Flowers");
        FlowerObj.transform.position = Vector3.zero;

        int v_count = 0;
        for (int i = 0; i < flower_meshes.Count; ++i) // �ѻ���������������
        {
            flower_meshes[i].transform.parent = FlowerObj.transform;
            v_count += flower_meshes[i].GetComponent<MeshFilter>().mesh.vertexCount;
        }
        FlowerObj.transform.parent = PlantObj.transform;

        return v_count;
    }


    //////////////////////////////// ͼ��ָ��ѵ�������������ã�������ɫ��GameObject
    public void ConstructPlantGameObjects_SEGMENT(bool isHouseplantMode=false)
    {
        Destroy(PlantObj); // ����ԭ��plant


        PlantObj = new GameObject("Plant");
        PlantObj.transform.position = Vector3.zero;

        // [3]����structure����meshes
        // - ����skeleton��mesh
        var meshes = plant.GetSkeletonMesh(isUseInterp: true);

        for (int i = 0; i < meshes.Count; ++i)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);

            obj.transform.position = new Vector3(0.0f, 0.0f, 0.0f);
            obj.GetComponent<Renderer>().material = new Material(mat_Segmentation_Sample);
            obj.GetComponent<Renderer>().material.SetColor("_Color", new Color(1.0f, 0.0f, 0.0f));
            obj.GetComponent<Renderer>().material.SetTexture("_MainTex", mat_branch.mainTexture);
            obj.GetComponent<MeshFilter>().mesh = meshes[i];

            obj.transform.parent = PlantObj.transform;
        }
  

        Refresh_Leaf_and_Flower_Mesh_SEGMENT(isHouseplantMode);

    }

    void Refresh_Leaf_and_Flower_Mesh_SEGMENT(bool isHouseplantMode = false)
    {

        int leafCount = 0;

        {
            Destroy(LeafObj);
            List<Mesh> leaf_meshes;
            List<string> leaf_texIDs;
            int selectedID = -1;
            (leaf_meshes, leaf_texIDs, selectedID) = plant.GetLeafMesh();

            LeafObj = new GameObject("Leafs");
            LeafObj.transform.position = Vector3.zero;


            for (int i = 0; i < leaf_meshes.Count; ++i) // ������Ҷ
            {
                GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                obj.transform.position = new Vector3(0.0f, 0.0f, 0.0f);
                obj.transform.parent = LeafObj.transform;
                obj.name = "Leaf-" + i.ToString();

                if (selectedID == i)
                    obj.GetComponent<Renderer>().material = mat_arrow;
                else
                {
                    obj.GetComponent<Renderer>().material = m_leaf_Seg_Textures[leaf_texIDs[i]];
                    if(isHouseplantMode == true)
                        obj.GetComponent<Renderer>().material.SetColor("_Color", seg_Leaf_Colors[leafCount]);
                    else
                        obj.GetComponent<Renderer>().material.SetColor("_Color", new Color(0.0f, 1.0f, 0.0f));
                    leafCount = (leafCount + 1) % seg_Leaf_Colors.Count;
                }

                obj.GetComponent<MeshFilter>().mesh = leaf_meshes[i];
            }
            LeafObj.transform.parent = PlantObj.transform;
        }

        {
            Destroy(FlowerObj);
            // - ����Flowers���������
            List<GameObject> flower_meshes;

            flower_meshes = plant.GetFlowerMesh();
            FlowerObj = new GameObject("Flowers");
            FlowerObj.transform.position = Vector3.zero;
            for (int i = 0; i < flower_meshes.Count; ++i) // �ѻ���������������
            {
                GameObject obj = flower_meshes[i];
                obj.transform.parent = FlowerObj.transform;

                if(obj.GetComponent<Renderer>() != null)
                {
                    Material mat = new Material(mat_Segmentation_Sample);
                    mat.mainTexture = Texture2D.whiteTexture;
                    obj.GetComponent<Renderer>().material = mat;
                    obj.GetComponent<Renderer>().material.SetColor("_Color", seg_Leaf_Colors[leafCount]);
                }

                var childs = obj.GetComponentsInChildren<Transform>();


                foreach (var child in childs)
                {
                    Debug.Log("����!!!!");
                    if (child.gameObject.GetComponent<Renderer>() != null)
                    {
                        Material mat = new Material(mat_Segmentation_Sample);
                        mat.mainTexture = Texture2D.whiteTexture;
                        child.GetComponent<Renderer>().material = mat;
                        child.GetComponent<Renderer>().material.SetColor("_Color", seg_Leaf_Colors[leafCount]);
                    }
                }
                leafCount = (leafCount + 1) % seg_Leaf_Colors.Count;
            }
            FlowerObj.transform.parent = PlantObj.transform;
        }
    }



    void LoadSegmentLeafColor()
    {
        string[] allLines = File.ReadAllLines(GlobalPath.p_seg_leaf_color_file, Encoding.UTF8);

        seg_Leaf_Colors.Clear();

        for (int m = 0; m < allLines.Length; m++)
        {
            string[] line = allLines[m].Split(" ");

            if (line.Length == 3)
            {
                seg_Leaf_Colors.Add(new Color(float.Parse(line[0]), float.Parse(line[1]), float.Parse(line[2])));
            }
        }
    }

    // ����Graphģʽ������
   
}
