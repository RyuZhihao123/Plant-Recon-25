using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 所有的scroll bar（滚动条）事件。

public class ScrollBarScript : MonoBehaviour
{
    float min_node_move_offset = 0.01f;
    float max_node_move_offset = 0.1f;
    float def_node_move_offset = 0.05f;

    float min_node_rotate_angle = 1f;
    float max_node_rotate_angle = 10f;
    float def_node_rotate_angle = 5f;

    float min_leaf_rotate_angle = 1f;
    float max_leaf_rotate_angle = 10f;
    float def_leaf_rotate_angle = 5f;

    float min_leaf_delta_size = 0.01f;
    float max_leaf_delta_size = 0.2f;
    float def_leaf_delta_size = 0.1f;

    float min_leaf_delta_angle = 1f;  // hor和ver的角度
    float max_leaf_delta_angle = 6f;
    float def_leaf_delta_angle = 3f;

    void Start()
    {
        Debug.Log("Hello," + gameObject.name);
        Scrollbar bar = this.GetComponent<Scrollbar>();
        Interactive_OP.p_node_move_offset = def_node_move_offset;
        Interactive_OP.p_node_rotate_angle = def_node_rotate_angle;
        Interactive_OP.p_leaf_rotate_angle = def_leaf_rotate_angle;
        Interactive_OP.p_leaf_delta_size = def_leaf_delta_size;
        Interactive_OP.p_leaf_delta_angle = def_leaf_delta_angle;

        if (gameObject.name == "Scrollbar_node_move_off")
        {
            bar.value = GetSBarValue(min_node_move_offset, max_node_move_offset,
                Interactive_OP.p_node_move_offset);
        }
        if (gameObject.name == "Scrollbar_node_angle")
        {
            bar.value = GetSBarValue(min_node_rotate_angle, max_node_rotate_angle,
                Interactive_OP.p_node_rotate_angle);
        }
        if (gameObject.name == "Scrollbar_leaf_angle")
        {
            bar.value = GetSBarValue(min_leaf_rotate_angle, max_leaf_rotate_angle,
                Interactive_OP.p_leaf_rotate_angle);
        }
        if (gameObject.name == "Scrollbar_leaf_size")
        {
            bar.value = GetSBarValue(min_leaf_delta_size, max_leaf_delta_size,
                Interactive_OP.p_leaf_delta_size);
        }
        if (gameObject.name == "Scrollbar_leaf_horAng")
        {
            bar.value = GetSBarValue(min_leaf_delta_angle, max_leaf_delta_angle,
                Interactive_OP.p_leaf_delta_angle);
        }
        bar.onValueChanged.AddListener(GetDown);
    }
    void Update()
    {
    }
    void GetDown(float value)
    {
        if (gameObject.name == "Scrollbar_node_move_off")
            Interactive_OP.p_node_move_offset = GetRealValue(min_node_move_offset, max_node_move_offset, value);

        if (gameObject.name == "Scrollbar_node_angle")
            Interactive_OP.p_node_rotate_angle = GetRealValue(min_node_rotate_angle, max_node_rotate_angle, value);

        if (gameObject.name == "Scrollbar_leaf_angle")
            Interactive_OP.p_leaf_rotate_angle = GetRealValue(min_leaf_rotate_angle, max_leaf_rotate_angle, value);

        if (gameObject.name == "Scrollbar_leaf_size")
            Interactive_OP.p_leaf_delta_size = GetRealValue(min_leaf_delta_size, max_leaf_delta_size, value);

        if (gameObject.name == "Scrollbar_leaf_horAng")
            Interactive_OP.p_leaf_delta_angle = GetRealValue(min_leaf_delta_angle, max_leaf_delta_angle, value);
    }

    float GetSBarValue(float min, float max, float cur)
    {
        float res = (cur - min) / (max - min);

        if (res < 0) res = 0;
        if (res > 1) res = 1;

        return res;
    }

    float GetRealValue(float min, float max, float cur)
    {
        float res = cur * (max - min) + min;
        return res;
    }
}