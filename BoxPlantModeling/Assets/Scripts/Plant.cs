using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Internode
{
    // a->b
    public Vector3 a = new Vector3();
    public Vector3 b = new Vector3();

    public float ra = 0.5f;
    public float rb = 0.5f;

    public List<Internode> childs = new List<Internode>();
    public Internode parent = null;

    public List<Kit> kits = new List<Kit>();

    //  ���level����������ͬ��branch����Skeleton����ʱ��level��Ҳ����mesh���ɣ�
    //  level=0��ʾ��root��������ʲôҲû���õ��� ��Ҫע�⣬ֻ�и��ڵ��level=0�������Ĵ�1��ʼ����
    public int level = 0;
    public static int max_level = 0;

    public int depth = 0; // ��Ҷ�ӽڵ㿪ʼ�����depth��Ҷ�ӽڵ���1��

    public Internode() { }
    public Internode(Vector3 a, Vector3 b, float ra = 0.001f, float rb = 0.001f) { this.a = a; this.b = b; this.ra = ra; this.rb = rb; }

    public Internode copy() { return new Internode(a, b, ra, rb); }
}

public class Kit
{
    public Vector3 a = new Vector3();
    public Vector3 dir = Vector3.up;

    public Kit() { }
    public Kit(Vector3 a, Vector3 dir) { this.a = a;this.dir = dir; }
    public Kit copy() { return new Kit(a, dir); }
}


[System.Serializable]
public class Leaf: Kit
{
    public float w = GlobalParams.p_leaf_witdth;
    public float h = GlobalParams.p_leaf_height;
    public float horAng = GlobalParams.p_leaf_horBend_Ang;
    public float verAng = GlobalParams.p_leaf_verBend_Ang;

    public Quaternion rotation = Quaternion.identity;  // ��dir�Ļ����Ͽ��ƣ�ҶƬ��ת

    public bool isCrossShape = false; // �Ƿ���ʮ����ҶƬ 

    public string textureName = GlobalParams.p_default_leaf; 
    public bool isSelected = false;

    public Leaf() { }
    public Leaf(Vector3 a, Vector3 dir, float w, float h, float hAng, float vAng)
    { 
        this.a = a; this.dir = dir; this.w = w; this.h = h; this.horAng = hAng; this.verAng = vAng;
    }
    public Leaf copy() { return new Leaf(a, dir, w, h, horAng, verAng); }

    public Leaf deepCopy()
    {
        Leaf lf = new Leaf(a, dir, w, h, horAng, verAng);

        lf.rotation = rotation;
        lf.isCrossShape = isCrossShape;
        lf.textureName = textureName;
        lf.isSelected = false;
        return lf;
    }
}

[System.Serializable]
public class Flower: Kit
{
    public Quaternion rotation = Quaternion.identity;
    public Vector3 local_scale 
        = new Vector3(GlobalParams.p_flower_scale, GlobalParams.p_flower_scale, GlobalParams.p_flower_scale);

    public string flowerName = GlobalParams.p_default_flower;
    public bool isSelected = false;

    public Flower() { }
    public Flower(Vector3 a, Quaternion rt, float sc) 
    { this.a = a; this.rotation = rt; this.local_scale = new Vector3(sc,sc,sc); }

    public Flower copy() 
    {
        Flower t = new Flower();
        t.a = a;
        t.rotation = rotation;
        t.local_scale = local_scale;
        return t; 
    }

    public Flower deepCopy()
    {
        Flower t = this.copy();
        t.dir = dir;
        t.flowerName = flowerName;
        return t;
    }
}


public class Plant
{
    // ʱ��ע���׸��ڵ�Ϊ�գ���ΪL-system�����ж����
    public Internode root = new Internode();  // ԭʼ��root��L-system��
    public Internode root_interpolated = new Internode();  // ��ֵ���root

    public List<Mesh> m_meshes = new List<Mesh>();


    // ��Χ�����BoundingBox (bbox)
    public Dictionary<Kit, MinBoundingBox> m_boxes_of_kits = new Dictionary<Kit, MinBoundingBox>();
    public Dictionary<MinBoundingBox, (int, bool)> m_hierarchy_boxes = new Dictionary<MinBoundingBox, (int, bool)>();
    public int max_box_level = -1; // -1��ʾɶҲû�У�level��һ���0��ʼ����
    // �����rtΪ���ڵ������
    public void ClearTree(Internode rt)
    {
        if (rt == null)
            return;

        Queue<Internode> queue = new Queue<Internode>();
        queue.Enqueue(rt);
        List<Internode> internodes = new List<Internode>();

        while(queue.Count!=0)
        {
            Internode cur = queue.Dequeue();
            internodes.Add(cur);
            foreach(Internode child in cur.childs)
            {
                queue.Enqueue(child);
            }
        }

        for (int i = internodes.Count - 1; i >= 0; --i)
        {
            internodes[i].childs.Clear();
            internodes[i].kits.Clear();
        }

        rt = null;

        m_boxes_of_kits.Clear();
        m_hierarchy_boxes.Clear();
        max_box_level = -1;
    }


    private void UpdateInternodeDepth(Internode cur)
    {
        // depth: Ҷ�ӽڵ�=1���������+1������rootҲ����ֵ
        if (cur.childs.Count == 0)
        {
            cur.depth = 1;
            return;
        }

        int max_child_depth = -1;
        for (int i = 0; i < cur.childs.Count; ++i)
        {
            UpdateInternodeDepth(cur.childs[i]);

            if (max_child_depth < cur.childs[i].depth)
                max_child_depth = cur.childs[i].depth;
        }
        cur.depth = max_child_depth + 1;
    }


    private void UpdateInternodeLevel(Internode rt)
    {
        if (rt.childs.Count == 0)
        {
            Internode.max_level = 0;
            return;
        }
        // ͬ����һ��level����ζ������ͬһ��branch
        // root�ڵ��level=0����ֻ��root��level=0;
        // ����root�ڵ��ÿ��child��level��ֵ(1,2,3,4....)
        Queue<Internode> queue = new Queue<Internode>(); // ���ڱ���

        for (int i = 0; i < rt.childs.Count; ++i)
        {
            rt.childs[i].level = i + 1;
            queue.Enqueue(rt.childs[i]);
        }
        Internode.max_level = rt.childs.Count; // ����max_level;
        rt.level = 0;

        while (queue.Count != 0)
        {
            Internode cur = queue.Dequeue();

            int min_delta = int.MaxValue;
            int min_delta_id = -1;
            for (int i = 0; i < cur.childs.Count; ++i)
            {
                int cur_delta = cur.depth - cur.childs[i].depth;

                if (cur_delta < min_delta)
                {
                    min_delta = cur_delta;
                    min_delta_id = i;
                }
                queue.Enqueue(cur.childs[i]);
            }

            for (int i = 0; i < cur.childs.Count; ++i)
            {
                if (i == min_delta_id) // ����������ӽ��ģ���ô����ͬһ��level
                    cur.childs[i].level = cur.level;
                else  // ��������������µ�level
                {
                    cur.childs[i].level = Internode.max_level + 1;
                    Internode.max_level += 1;
                }

            }
        }
    }

    /// <summary>
    /// ����branch�İ뾶��
    /// </summary>
    /// <param name="cur"> ����һ�����ڵ㣨root��root_interp��</param>
    void UpdateInternodeRadius(Internode cur)
    {

        // depth: Ҷ�ӽڵ�=1���������+1������rootҲ����ֵ
        if (cur.childs.Count == 0)
        {
            cur.rb = cur.ra = GlobalParams.p_base_radius;
            return;
        }

        float max_child_radius = -1;
        for (int i = 0; i < cur.childs.Count; ++i)
        {
            UpdateInternodeRadius(cur.childs[i]);

            if (max_child_radius < cur.childs[i].ra)
                max_child_radius = cur.childs[i].ra;
        }
        cur.rb = cur.ra = max_child_radius * GlobalParams.p_radius_factor;

    }

    // ���branch��Ӧ��list������internode.level���л���
    public List<List<Internode>> GetBranchLists(Internode rt)
    {
        List<List<Internode>> levellist = new List<List<Internode>>();
        for (int i = 0; i < Internode.max_level + 1; ++i)
            levellist.Add(new List<Internode>());
        //Debug.Log("LevelList��Ŀ: " + levellist.Count.ToString());

        Queue<Internode> queue = new Queue<Internode>();
        queue.Enqueue(rt);

        while (queue.Count != 0)
        {
            Internode cur = queue.Dequeue();
            levellist[cur.level].Add(cur);

            for (int i = 0; i < cur.childs.Count; ++i)
                queue.Enqueue(cur.childs[i]);
        }
        levellist.RemoveAt(0);  // �����root��Ӧ��levellist����Ϊ�ò��ϵģ�

        return levellist;
    }

    public void UpdateParentHierarchy(Internode rt)
    {
        if (rt.childs.Count == 0) // ����
            return;

        Queue<Internode> queue = new Queue<Internode>();
        for (int i = 0; i < rt.childs.Count; ++i)
        {
            rt.childs[i].parent = rt;
            queue.Enqueue(rt.childs[i]);
        }

        while (queue.Count != 0)
        {
            Internode cur = queue.Dequeue();

            for (int i = 0; i < cur.childs.Count; ++i)
            {
                cur.childs[i].parent = cur;
                queue.Enqueue(cur.childs[i]);
            }
        }
        rt.parent = null;
    }
    public List<List<Vector3>> originalpoints = new List<List<Vector3>>();
    public List<List<Vector3>> newpoints = new List<List<Vector3>>();

    public void UpdateSupportNodeList()
    {
        originalpoints.Clear();
        newpoints.Clear();

        var levellist = GetBranchLists(this.root);

        for (int i = 0; i < levellist.Count; ++i)
        {
            var list = levellist[i];

            originalpoints.Add(new List<Vector3>());

            for (int k = 0; k < list.Count; k++)
            {
                var cur = list[k];
                if (k == 0)
                    originalpoints[i].Add(cur.a);
                originalpoints[i].Add(cur.b);
            }
        }
        levellist = GetBranchLists(this.root_interpolated);
        for (int i = 0; i < levellist.Count; ++i)
        {
            var list = levellist[i];

            newpoints.Add(new List<Vector3>());

            for (int k = 0; k < list.Count; k++)
            {
                var cur = list[k];
                if (k == 0)
                    newpoints[i].Add(cur.a);
                newpoints[i].Add(cur.b);
            }
        }
        //Debug.Log("levellist������:" + levellist.Count.ToString());
    }

    public void GetInterpolatedTree()
    {
        if (this.root.childs.Count == 0) // ����
            return;

        ClearTree(this.root_interpolated);  // ����ԭ���Ĳ�ֵ��


        //
        // ׼����д
        // 1. ����levellistɾ����Щ����ܽӽ���Ƭ��
        // 2. ���в�ֵ����ֵ��ͬʱ
        //


        // [1] ����һ����ȫcopy�ĸ����� -> this.root_interpolated
        Queue<Internode> queue = new Queue<Internode>();
        Queue<Internode> queue_new = new Queue<Internode>();

        for (int i = 0; i < this.root.childs.Count; ++i)
        {
            queue.Enqueue(this.root.childs[i]);

            // �����µ�Node��copy���µ���
            Internode node_new = this.root.childs[i].copy();
            this.root_interpolated.childs.Add(node_new);
            queue_new.Enqueue(node_new);
        }
        int count = 0;
        while (queue.Count != 0)
        {
            Internode cur = queue.Dequeue();
            Internode cur_new = queue_new.Dequeue();
            count++;
            for (int i = 0; i < cur.childs.Count; ++i)
            {
                queue.Enqueue(cur.childs[i]);

                // ����copy���Node
                Internode node_new = cur.childs[i].copy();
                cur_new.childs.Add(node_new);
                queue_new.Enqueue(node_new);

            }
        }

        // [2] ����levellist��ɾ����Щ�����ֱ�߽��
        UpdateInternodeDepth(this.root_interpolated);
        UpdateInternodeLevel(this.root_interpolated);

        var levellist = GetBranchLists(this.root_interpolated);

        for (int i = 0; i < levellist.Count; i++)
        {
            var list = levellist[i];

            if (list.Count <= 1)
                continue;

            for (int k = 0; k < list.Count - 1; ++k)
            {
                var cur = list[k];

                if (cur.childs.Count != 1)  // ������ӽ�����Ŀ����1������Ҫ����һ��
                    continue;

                var next = list[k + 1];

                Vector3 dir_cur = (cur.b - cur.a).normalized;
                Vector3 dir_next = (next.b - next.a).normalized;

                float angle = Vector3.Angle(dir_cur, dir_next);
                //Debug.Log("�Ƕ�:" + angle.ToString() + ": " + dir_cur.ToString() + dir_next.ToString());

                if (angle < 0.5f)
                {
                    //Debug.Log("�Ƴ�");
                    cur.childs.Clear();
                    cur.childs = next.childs;
                    cur.b = next.b;
                    list.RemoveAt(k + 1);
                    k--;

                }
            }
        }


        UpdateInternodeDepth(this.root_interpolated);
        UpdateInternodeLevel(this.root_interpolated);
        

        // ֮���ٽ��в�ֵ
        for (int i = 0; i < levellist.Count; i++)
        {
            var list = levellist[i];

            if (list.Count <= 1) // ����Ҫ��ֵ
                continue;

            for (int k = 0; k < list.Count;)  // ����ÿһ�Σ�������β��
            {
                var cur = list[k];

                Vector3 p1 = cur.a;
                Vector3 p2 = cur.b;
                Vector3 t1 = (cur.b - cur.a).normalized;
                Vector3 t2 = t1;

                if (k != 0) // ������λ
                {
                    var parent = list[k - 1];
                    t1 = ((parent.b - parent.a).normalized + t1).normalized;
                }
                if (k != list.Count - 1) // ����ĩλ
                {
                    var next = list[k + 1];
                    t2 = ((next.b - next.a).normalized + t2).normalized;
                }
                // cur->..->..->..->end
                List<Internode> new_nodes = Hermite.GetHermiteFromOneInternode(cur, t1, t2, 5);

                new_nodes[new_nodes.Count - 1].b = p2;  // ���һ�����Ĵ���
                new_nodes[new_nodes.Count - 1].childs = cur.childs;

                cur.a = p1;  // ��һ�����Ĵ���
                cur.b = new_nodes[0].b;
                cur.childs = new_nodes[0].childs;


                new_nodes.RemoveAt(0);
                k += 1;
                foreach (var nn in new_nodes)
                {

                    list.Insert(k, nn);
                    k += 1;

                }
            }
        }



    }


    public void RefreshTree()
    {
        UpdateInternodeDepth(this.root); // depth�Ĵ���һ��Ҫ��levelǰ����Ϊlevel����depth
        UpdateInternodeLevel(this.root);
        UpdateInternodeRadius(this.root);
        UpdateParentHierarchy(this.root);

        this.GetInterpolatedTree(); // ע�⣺��ֵǰ��һ��Ҫ��������Щ����

        UpdateInternodeDepth(this.root_interpolated);
        UpdateInternodeLevel(this.root_interpolated);
        UpdateInternodeRadius(this.root_interpolated);
        //UpdateParentHierarchy(this.root_interpolated);

        UpdateSupportNodeList();
    }


    public List<Mesh> GetSkeletonMesh(bool isUseInterp = true)
    {
        //[1] ���ȸ�������Internodes��level/depth/radius����Ϣ��ͬʱ���в�ֵ
        this.RefreshTree();


        // [2] ����ɵ�������Ϣ:
        foreach (Mesh m in m_meshes)
            m.Clear();
        m_meshes.Clear();

        // [3] ����level����levellist
        List<List<Internode>> levellist;

        if (!isUseInterp)
            levellist = GetBranchLists(this.root);
        else
            levellist = GetBranchLists(this.root_interpolated);

        if (levellist.Count == 0)
            return new List<Mesh>();

        // ��������Զ�ÿ��levelist����ֵ
        int mId = 0;
        int count = 0;
        List<List<Vector3>> vecs = new List<List<Vector3>>();
        List<List<Vector2>> uvs = new List<List<Vector2>>();
        List<List<Vector3>> norms = new List<List<Vector3>>();
        List<List<int>> indices = new List<List<int>>();
        vecs.Add(new List<Vector3>());
        uvs.Add(new List<Vector2>());
        norms.Add(new List<Vector3>());
        indices.Add(new List<int>());
        m_meshes.Add(new Mesh());


        // [5] ��ÿ��levellist����mesh
        int FACE_COUNT = 5; // n����
        foreach (List<Internode> parts in levellist)
        {
            bool concatPrevious = false; // ��һ��(i+1)��bot�Ƿ��뵱ǰ��i��top����

            var topPts_pre = new Vector3[FACE_COUNT];
            var botPts_pre = new Vector3[FACE_COUNT];
            var fNorms_pre = new Vector3[FACE_COUNT];

            for (int i = 0; i < parts.Count; ++i)
            {
                var topPts = new Vector3[FACE_COUNT];
                var botPts = new Vector3[FACE_COUNT];
                var fNorms = new Vector3[FACE_COUNT];

                Vector3 dira = (parts[i].b - parts[i].a).normalized;
                Vector3 dirb = (parts[i].b - parts[i].a).normalized;

                // ���㵱ǰinternode��botPts���ײ��һȦ�㣩
                // ����ǵ�һ�����߲�����������botPts���Լ��ķ����������¼���ͺ�

                Vector3 normA = GetOneNormalVectorFrom(dira); // internode�����ⷨ��


                bool isCreateNewBotPts = false;
                if (i == 0)
                    isCreateNewBotPts = true;
                else
                {
                    float dot = Vector3.Dot((parts[i - 1].b - parts[i - 1].a).normalized, dira);
                    if (dot < 0.8)
                        isCreateNewBotPts = true;
                }
                if (isCreateNewBotPts)
                {
                    for (int k = 0; k < FACE_COUNT; ++k)
                    {
                        Vector3 t_normA = Quaternion.AngleAxis(k / 6.0f * 360.0f, dira) * normA;
                        botPts[k] = parts[i].a + parts[i].ra * (t_normA).normalized;
                        //Debug.Log(string.Format("{0}-{1} -{2}", i, k, botPts[k]));
                    }
                }
                else
                {
                    for (int k = 0; k < FACE_COUNT; ++k)
                        botPts[k] = topPts_pre[k];
                }

                // ���㵱ǰinternode��topPts�������һȦ�㣩

                for (int k = 0; k < FACE_COUNT; ++k)
                {
                    Vector3 pt = RayToPlane(-dirb, parts[i].b, botPts[k], dira);  // ����
                    fNorms[k] = (pt - parts[i].b).normalized;  // �õ��淨��
                    topPts[k] = parts[i].b + parts[i].rb * fNorms[k];  // �õ�topPts
                }

                if (count > 55000)  // �����ǰ��mesh�Ѿ���Ա��
                {
                    m_meshes[mId].vertices = vecs[mId].ToArray();
                    m_meshes[mId].triangles = indices[mId].ToArray();
                    m_meshes[mId].normals = norms[mId].ToArray();
                    m_meshes[mId].uv = uvs[mId].ToArray();

                    vecs.Add(new List<Vector3>());
                    uvs.Add(new List<Vector2>());
                    norms.Add(new List<Vector3>());
                    indices.Add(new List<int>());
                    m_meshes.Add(new Mesh());

                    mId++;
                    count = 0;
                }
                //Debug.Log("----");

                // ����Щ���Ž���
                for (int k = 0; k < FACE_COUNT; k++)
                {
                    int id1 = k;
                    int id2 = (k + 1) % FACE_COUNT;

                    // ����������Ƭ
                    vecs[mId].Add(botPts[id2]); vecs[mId].Add(topPts[id2]); vecs[mId].Add(topPts[id1]);
                    vecs[mId].Add(topPts[id1]); vecs[mId].Add(botPts[id1]); vecs[mId].Add(botPts[id2]);

                    //Debug.Log(string.Format( "xyz={0} {1} {2}",botPts[id2], topPts[id2], topPts[id1]));

                    norms[mId].Add(fNorms[id2]); norms[mId].Add(fNorms[id2]); norms[mId].Add(fNorms[id1]);
                    norms[mId].Add(fNorms[id1]); norms[mId].Add(fNorms[id1]); norms[mId].Add(fNorms[id2]);

                    uvs[mId].Add(new Vector2(id2 % FACE_COUNT, 0.0f));
                    uvs[mId].Add(new Vector2(id2 % FACE_COUNT, 1.0f));
                    uvs[mId].Add(new Vector2(id1 % FACE_COUNT, 1.0f));
                    uvs[mId].Add(new Vector2(id1 % FACE_COUNT, 1.0f));
                    uvs[mId].Add(new Vector2(id1 % FACE_COUNT, 0.0f));
                    uvs[mId].Add(new Vector2(id2 % FACE_COUNT, 0.0f));

                    indices[mId].Add(count + 0); indices[mId].Add(count + 1); indices[mId].Add(count + 2);
                    indices[mId].Add(count + 3); indices[mId].Add(count + 4); indices[mId].Add(count + 5);

                    //Debug.Log(string.Format("xyz={0} {1} {2}", botPts[id2], topPts[id2], topPts[id1]));
                    count += 6;
                }

                for (int k = 0; k < FACE_COUNT; ++k)
                {
                    topPts_pre[k] = topPts[k];
                    botPts_pre[k] = botPts[k];
                    fNorms_pre[k] = fNorms[k];
                }

            }
        }
        //Debug.Log("������Ŀ:" + count.ToString());
        if (count > 0)  // ���mesh�Ƚϴ�
        {
            m_meshes[mId].vertices = vecs[mId].ToArray();
            m_meshes[mId].normals = norms[mId].ToArray();
            m_meshes[mId].uv = uvs[mId].ToArray();
            m_meshes[mId].triangles = indices[mId].ToArray();
            //Debug.Log("mesh update:" + m_meshes[mId].vertices.Length.ToString());
            //Debug.Log("mesh update:" + m_meshes[mId].triangles.Length.ToString());
        }

        return m_meshes;

    }

    /////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////
    

    public Vector3 GetOneNormalVectorFrom(Vector3 dir)
    {
        if (dir.x == 0)
            return new Vector3(0, 0, -1);
        else
            return new Vector3(-dir.z / dir.x, 0, 1).normalized;
    }

    public Vector3 RayToPlane(Vector3 m_n, Vector3 m_a0, Vector3 p0, Vector3 u)
    {
        // ƽ�棺����m_n��ƽ����һ��m_a0
        // ���ߣ����p0����������u
        float t = (Vector3.Dot(m_n, m_a0) - Vector3.Dot(m_n, p0)) / Vector3.Dot(m_n, u);

        return p0 + t * u;
    }

    public Vector3 GetGradientHeatColor(float value, float min, float max)
    {
        Vector3 rgb = new Vector3();

        return rgb;
    }

    public void DrawNodeOnGizmos(Internode rt, List<Color> colors, Vector3 offset=new Vector3())
    {

        Queue<Internode> queue = new Queue<Internode>();
        queue.Enqueue(rt);

        while (queue.Count != 0)
        {
            Internode cur = queue.Dequeue();
            Gizmos.color = colors[cur.level % Internode.max_level];

            Gizmos.DrawSphere(cur.a+offset, 0.01f);
            Gizmos.DrawSphere(cur.b+offset, 0.01f);
            Gizmos.DrawLine(cur.a+offset, cur.b+offset);
            for (int i = 0; i < cur.childs.Count; ++i)
            {
                queue.Enqueue(cur.childs[i]);
            }
        }
    }

    ////////////////// ���������/////////////////////////////////////
    public (List<Mesh>, List<string>, int) GetLeafMesh()
    {
        List<Mesh> leaf_meshes = new List<Mesh>();
        List<string> leaf_texIDs = new List<string>();
        
        if (root == null)
            return (leaf_meshes, leaf_texIDs, - 1);

        int selectedID = -1;

        Queue<Internode> queue = new Queue<Internode>();
        queue.Enqueue(root);

        while (queue.Count != 0)
        {
            Internode cur = queue.Dequeue();

            for (int t = 0; t < cur.kits.Count; ++t)
            {
                if (cur.kits[t].GetType() == typeof(Leaf))
                {
                    Leaf lf = (Leaf)cur.kits[t];

                    //var leafmesh = Geometry.CreateLeaf(lf.a, lf.dir,
                    //    lf.w, lf.h, lf.horAng, lf.verAng);

                    var leafmesh = Geometry.CreateLeaf(lf);

                    leaf_meshes.Add(leafmesh);
                    leaf_texIDs.Add(lf.textureName);

                    if (lf.isSelected == true) // ����Ǳ�ѡ���ҶƬ
                    {
                        selectedID = leaf_meshes.Count - 1;
                    }

                    // ����BBox
                    m_boxes_of_kits.Add(lf, MinBoundingBox.GetMinBBoxFromLeaf(lf));
                }
            }

            foreach (Internode child in cur.childs)
            {
                queue.Enqueue(child);
            }
        }

        return (leaf_meshes, leaf_texIDs, selectedID);
    }

    public List<GameObject> GetFlowerMesh()
    {
        List<GameObject> flower_meshes = new List<GameObject>();
        if (root == null)
            return flower_meshes;

        Queue<Internode> queue = new Queue<Internode>();
        queue.Enqueue(root);

        while (queue.Count != 0)
        {
            Internode cur = queue.Dequeue();

            for (int t = 0; t < cur.kits.Count; ++t)
            {
                if (cur.kits[t].GetType() == typeof(Flower))
                {
                    Flower lf = (Flower)cur.kits[t];
                    //Debug.Log(GlobalPath.p_flower_path + lf.flowerName);

                    var flowermesh = Geometry.CreatePrefab(
                        "Flowers/"+lf.flowerName,
                        lf.a, lf.rotation, lf.local_scale);
                    flower_meshes.Add(flowermesh);

                    // ����BBox
                    m_boxes_of_kits.Add(lf, MinBoundingBox.GetMinBBoxFromFlower(lf));

                }
            }

            foreach (Internode child in cur.childs)
            {
                queue.Enqueue(child);
            }
        }

        return flower_meshes;
    }
    /////////////////// Interactive���////////////////////////////////////

    // �����ʱ��ѡ������internode������internode.b�˵���Ϊ�ж�
    // ע��root�������Ǳ��������ڵġ�
    public Internode GetHitBranch(Ray ray)
    {
        Queue<Internode> queue = new Queue<Internode>();
        queue.Enqueue(root);

        float distToRayOrigin = float.MaxValue;  // �㵽����ԭ��ľ���
        Internode hitInternode = null;

        while(queue.Count!=0)
        {
            Internode cur = queue.Dequeue();

            // ��ǰcur.b��ray�Ĵ�ֱ����
            float distToRay = Vector3.Cross(ray.direction, cur.b - ray.origin).magnitude;

            if(distToRay < 0.1f)
            {
                float len = Vector3.Dot(ray.direction.normalized,
                    (cur.b-ray.origin));

                if(len < distToRayOrigin)
                {
                    distToRayOrigin = len;
                    hitInternode = cur;
                }
            }

            foreach (Internode child in cur.childs)
            {
                queue.Enqueue(child);
            }
        }

        return hitInternode;
    }

    public Internode GetParentNode(Internode query_node)
    {
        Queue<Internode> queue = new Queue<Internode>();

        queue.Enqueue(this.root);

        while(queue.Count!=0)
        {
            Internode cur = queue.Dequeue();

            foreach(var child in cur.childs)
            {
                queue.Enqueue(child);

                if (child == query_node)
                    return cur;
            }
        }

        return null;
    }

    public void GenerateNewBranch(Internode curBranch, List<Vector3> list)
    {
        if (list.Count < 2) 
            return;

        List<Internode> internodes = new List<Internode>();

        for(int i=0; i<list.Count-1; ++i)
        {
            Internode tmp = new Internode(list[i], list[i+1]);
            internodes.Add(tmp);

            if (i != 0)
            {
                internodes[i - 1].childs.Add(tmp);
            }
        }
        curBranch.childs.Add(internodes[0]);
    }

    public void GenerateNewLeaf(Internode curBranch, bool isCrossedLeaf = false)
    {
        if (curBranch == null)
            return;

        Leaf lf = new Leaf();
        lf.a = curBranch.b;
        lf.textureName = Interactive_OP.p_current_leaf_tex_name;
        lf.dir = Vector3.up;

        lf.isCrossShape = isCrossedLeaf;

        lf.w = GlobalParams.p_leaf_witdth;
        lf.h = GlobalParams.p_leaf_height;
        lf.horAng = GlobalParams.p_leaf_horBend_Ang;
        lf.verAng = GlobalParams.p_leaf_verBend_Ang;

        curBranch.kits.Add(lf);
    }

    public void GenerateNewFlower(Internode curBranch)
    {
        if (curBranch == null)
            return;

        Flower fl = new Flower();
        fl.a = curBranch.b;
        fl.flowerName = Interactive_OP.p_current_flower_name;

        Vector3 branchDir = (curBranch.b - curBranch.a).normalized;
        fl.rotation = Quaternion.FromToRotation(Vector3.up, branchDir);

        curBranch.kits.Add(fl);
        
    }

    public void RotateSubTree(Internode rt, Quaternion q)
    {

        Queue<Internode> queue = new Queue<Internode>();
        queue.Enqueue(rt);

        var origin = rt.b;
        while (queue.Count != 0)
        {
            Internode cur = queue.Dequeue();

            if(cur != rt)
            {
                cur.a = q * (cur.a - origin) + origin;
                cur.b = q * (cur.b - origin) + origin;
            }

            foreach(var kit in cur.kits)
            {
                var old_kit_a = kit.a;
                kit.a = q * (kit.a - origin) + origin;
                kit.dir = q * (kit.dir + old_kit_a - origin) + origin  -kit.a;
                kit.dir.Normalize();

                if(kit.GetType() == typeof(Flower))
                {
                    Flower f = (Flower)kit;
                    f.rotation = q* f.rotation;
                }
            }

            foreach (var child in cur.childs)
            {
                queue.Enqueue(child);
            }
        }
    }

    public void TranslateSubTree(Internode rt, Vector3 offset, bool isTranslateSubtree)
    {
        //// ���ֻ�ƶ���ǰ���
        //if (isTranslateSubtree==false)
        //{
        //    rt.b += offset;

        //    foreach (var kit in rt.kits)
        //    {
        //        kit.a += offset;
        //    }

        //    foreach (var child in rt.childs)
        //    {
        //        child.a += offset;
        //    }
        //}
        // ������ƶ���������
        if (isTranslateSubtree==true)
        {
            Queue<Internode> queue = new Queue<Internode>();
            queue.Enqueue(rt);

            while (queue.Count != 0)
            {
                Internode cur = queue.Dequeue();

                if (cur == rt)
                    cur.b += offset;
                else
                {
                    cur.a += offset;
                    cur.b += offset;
                }

                foreach (var kit in cur.kits)
                {
                    kit.a += offset;
                }

                foreach (var child in cur.childs)
                {
                    queue.Enqueue(child);
                }
            }
        }

        // ���ֻ�ƶ���ǰinternode��b�˵㣺
        if (isTranslateSubtree == false)
        {
            rt.b += offset;
            foreach (var child in rt.childs)
            {
                child.a = rt.b;
            }
            foreach (var kit in rt.kits)
            {
                kit.a = rt.b;
            }
        }
    }

    // ���������幹��Bounding box
    public void construct_progressive_bbox(Internode rt)
    {
        (m_hierarchy_boxes, max_box_level) = MinBoundingBox.GetMinBBoxFromTreeRoot(this.root);
    }


    // ����Ŷ����Ľṹ
    public void randomly_disturb_plant(Internode rt)
    {
        if (this.root == null)
            return;

        Queue<Internode> queue = new Queue<Internode>();

        queue.Enqueue(rt);

        float angle_kit = 15.0f;
        float offset_branching_nodes = 0.03f;
        while (queue.Count != 0)
        {
            var cur = queue.Dequeue();

            if (cur.childs.Count > 1)
            {
                TranslateSubTree(cur, 
                    new Vector3(Random.Range(-offset_branching_nodes, offset_branching_nodes),
                                Random.Range(-offset_branching_nodes, offset_branching_nodes),
                                Random.Range(-offset_branching_nodes, offset_branching_nodes)), true);
            }


            foreach (var kit in cur.kits)
            {
                
                if(typeof(Leaf) == kit.GetType())
                {
                    Leaf lf = (Leaf)kit;

                    lf.rotation *= Quaternion.AngleAxis(Random.Range(-angle_kit, angle_kit),
                        new Vector3(Random.Range(-1.0f, 1.0f),
                                    Random.Range(0.1f, 1.0f),
                                    Random.Range(-1.0f, 1.0f)).normalized);
                    lf.w += Random.Range(-0.03f, 0.03f);
                    lf.h += Random.Range(-0.03f, 0.03f);
                }

                if (typeof(Flower) == kit.GetType())
                {
                    Flower lf = (Flower)kit;

                    lf.rotation *= Quaternion.AngleAxis(Random.Range(-angle_kit, angle_kit),
                        new Vector3(Random.Range(-1.0f, 1.0f),
                                    Random.Range(0.1f, 1.0f),
                                    Random.Range(-1.0f, 1.0f)).normalized);
                    lf.local_scale *= Random.Range(0.90f, 1.1f);
                }
            }
            foreach(var child in cur.childs)
            {
                queue.Enqueue(child);
            }
        }

    }


    //// �������¹���
    public Plant GetNewPlantStructure()
    {
        Plant plant = new Plant();

        if (this.root == null)
            return plant;

        Queue<Internode> queue = new Queue<Internode>();
        Queue<Internode> queue_new = new Queue<Internode>();
        queue.Enqueue(this.root);
        queue_new.Enqueue(plant.root);


        while(queue.Count != 0)
        {
            Internode cur = queue.Dequeue();
            Internode cur_new = queue_new.Dequeue();

            foreach(var kit in cur.kits)
            {
                if (typeof(Flower) == kit.GetType())
                    cur_new.kits.Add(((Flower)kit).deepCopy());
                if(typeof(Leaf) == kit.GetType())
                {
                    Leaf lf = (Leaf)kit;

                    if(lf.isCrossShape == true)
                    {
                        List<Internode> axis = new List<Internode>();

                        int main_axis_piece_num = GlobalSketchSetting.p_twig_density;  // default 5
                        Vector3 leaf_dir = lf.rotation * lf.dir;
                        for (int i = 0; i < main_axis_piece_num; i++)
                        {
                            Internode node = new Internode();

                            node.a = lf.a + (i * lf.h / (float)main_axis_piece_num)* leaf_dir;
                            node.b = lf.a + ((i+1) * lf.h / (float)main_axis_piece_num) * leaf_dir;

                            axis.Add(node);
                            if (i != 0)
                                axis[i - 1].childs.Add(node);

                            // �������һЩ����
                            if (i != (main_axis_piece_num - 1))
                            {
                                GetOneShoot(node, type: 0, lf.w);
                                GetOneShoot(node, type: 0, lf.w);
                            }


                        }
                        RandomlyGenerateLeaves(axis[0], lf.textureName);
                        cur_new.childs.Add(axis[0]);
                    }
                    else
                    {
                        Leaf lf_new = lf.deepCopy();
                        cur_new.kits.Add(lf_new);
                    }
                }
            }


            for (int i = 0; i < cur.childs.Count; ++i)
            {
                Internode child = cur.childs[i];

                Internode child_new = child.copy();

                queue.Enqueue(child);

                queue_new.Enqueue(child_new);

                cur_new.childs.Add(child_new);
            }
        }


        return plant;
    }


    public void GetOneShoot(Internode root, int type = 0, float leaf_w = -1)
    {
        if (type == 0)
        {
            
            float baselen = GlobalSketchSetting.p_twig_base_len;
            //if (leaf_w > 0) baselen = leaf_w*0.15f;

            // ��һ��
            Vector3 basedir = Geometry.GetOneSideVectorFrom(root.b - root.a, 30.0f, 60.0f).normalized;
            basedir = this.GetWeighedDir(basedir, root.b - root.a);

            Internode twig0 = new Internode(root.b, root.b + basedir * baselen);

            root.childs.Add(twig0);

            // �ڶ���
            Vector3 dir1 = Geometry.GetOneSideVectorFrom(twig0.b - twig0.a, 30.0f, 60.0f).normalized;
            dir1 = this.GetWeighedDir(dir1, twig0.b - twig0.a);

            Internode twig1 = new Internode(twig0.b, twig0.b + dir1 * baselen);

            twig0.childs.Add(twig1);

            // ������
            Vector3 dir2 = Geometry.GetOneSideVectorFrom(twig0.b - twig0.a, 30.0f, 60.0f).normalized;
            dir2 = this.GetWeighedDir(dir2, twig0.b - twig0.a);

            Internode twig2 = new Internode(twig0.b, twig0.b + dir2 * baselen);
            twig0.childs.Add(twig2);
        }
    }

    private Vector3 GetWeighedDir(Vector3 newDir, Vector3 parentDir)
    {
        parentDir = parentDir.normalized;
        Vector3 res = newDir
            + GlobalSketchSetting.p_twig_gravity_factor * Vector3.up
            + 1.1f * GlobalSketchSetting.p_twig_base_dir_factor * parentDir;

        return res.normalized;
    }


    public void RandomlyGenerateLeaves(Internode rt, string textureName)
    {

        Queue<Internode> queue = new Queue<Internode>();


        // ��������ɵ�Ҷ��
        queue.Enqueue(rt);
        while (queue.Count != 0)
        {
            Internode cur = queue.Dequeue();

            cur.kits.Clear();

            foreach (var child in cur.childs)
            {
                queue.Enqueue(child);
            }
        }
        // �������������µ�Ҷ��
        queue.Enqueue(rt);
        while (queue.Count != 0)
        {
            Internode cur = queue.Dequeue();

            foreach (var child in cur.childs)
            {
                queue.Enqueue(child);
            }

            if (cur != root)
            {
                for (int i = 0; i < 18; ++i)
                {
                    if (Random.Range(0.0f, 1.0f) > GlobalSketchSetting.p_leaf_density)
                        continue;
                    Vector3 stemDir = (cur.b - cur.a).normalized;
                    Vector3 stemNorm1 = this.GetOneNormalVectorFrom(stemDir).normalized;
                    Vector3 stemNorm2 = Vector3.Cross(stemDir, stemNorm1).normalized;
                    float w1 = Random.Range(0.0f, 1.0f);
                    float w2 = Random.Range(-1.0f, 1.0f);
                    float w3 = Random.Range(-1.0f, 1.0f);

                    Vector3 leafDir = w1 * stemDir + w2 * stemNorm1 + w3 * stemNorm2;

                    leafDir += GlobalSketchSetting.p_leaf_gravity_factor * Vector3.up;
                    leafDir += GlobalSketchSetting.p_leaf_base_dir_factor * stemDir;
                    leafDir = leafDir.normalized;

                    Leaf lf = new Leaf();
                    lf.a = cur.b;
                    lf.textureName = textureName;
                    lf.dir = leafDir;

                    lf.isCrossShape = GlobalSketchSetting.p_leaf_is_crossed;

                    lf.w = GlobalSketchSetting.p_leaf_size;
                    lf.h = GlobalSketchSetting.p_leaf_size;
                    lf.horAng = 5;
                    lf.verAng = 5;
                    //lf.isCrossShape = false;

                    cur.kits.Add(lf);
                }
            }
        }
    }
}
