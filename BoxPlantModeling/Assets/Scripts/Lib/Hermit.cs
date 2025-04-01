using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class Hermite
{
    public static List<Internode> GetHermiteFromOneInternode(Internode cur, Vector3 t1, Vector3 t2,int num=5)
    {
        float interval = 1.0f / num;

        if(num<2)
            throw new System.Exception(string.Format("异常: 插值数必须>=2，当前为{0}", num));
        float len = (cur.b - cur.a).magnitude;

        t1 *= len;
        t2 *= len;

        List<Vector3> outputs = new List<Vector3>();
        for (float step = 0; step <= 1; step += interval) 
        {
            outputs.Add(Hermite.GetVector3AtStep(cur.a, cur.b, t1, t2, step));
        }

        List<Internode> list = new List<Internode>();

        for (int i = 0; i < outputs.Count - 1; ++i) 
        {
            Internode t = cur.copy(); // 注意这个copy不会copy，只会copy固有属性childs

            t.a = outputs[i];
            t.b = outputs[i + 1];

            list.Add(t);

            if (i != 0)
            {
                list[i - 1].childs.Add(list[i]);
            }
        }


        return list;
    }
    public static List<Vector3> GetHermiteList(List<Vector3> input)
    {

        float interval = 1.0f / 4.0f;
        List<Vector3> outputs = new List<Vector3>();
        List<Vector3> tangents = new List<Vector3>();

        if (input.Count == 0)
            return outputs; 
        for (int i = 0; i < input.Count; ++i)
        {
            if(i!=input.Count-1)
            {
                tangents.Add((input[i + 1] - input[i]).normalized);
            }
            else
            {
                tangents.Add(tangents[tangents.Count - 1]);
            }
        }

        for (int i = 0; i < input.Count-1; ++i)  // 0-1-2-3
        {
            Vector3 p1 = input[i];
            Vector3 p2 = input[i + 1];

            Vector3 t1 = tangents[i];
            Vector3 t2 = t1;

            if (i != 0)
            {
                t1 = (t1 + tangents[i - 1]).normalized;
            }
            if (i != input.Count - 2)
            {
                t2 = (t2 + tangents[i + 1]).normalized;
            }
            float len = (p2 - p1).magnitude;

            //t1 *= 1e-1f;
            //t2 *= 1e-1f;

            for (float step = 0.0f; step <= 1; step+=interval)
            {

                outputs.Add(Hermite.GetVector3AtStep(p1, p2,t1, t2, step));
            }
        }
        return outputs;

    }


    //===================
    // GetVector2AtStep
    //-------------------
    // Returns a two dimensional vector at a point along a curve.  The 'step' is where you are along the curve, zero
    // being the starting position and one being the end point. [0...1]
    // You must also provide two tangent vectors.
    //-------------------
    //      p1:  The starting point of the curve
    //      t1:  The tangent (e.g. direction and speed) to how the curve leaves the starting point
    //      p2:  The endpoint of the curve
    //      t2:  The tangent (e.g. direction and speed) to how the curve meets the endpoint
    //    step:  A position along the curve from 0 to 1 inclusive (e.g. halfway would be 0.5f)
    //===================
    public static Vector2 GetVector2AtStep(Vector2 p1, Vector2 p2, Vector2 t1, Vector2 t2, float step)
    {
        float h1 = 2 * (float)Math.Pow(step, 3) - 3 * (float)Math.Pow(step, 2) + 1;
        float h2 = -2 * (float)Math.Pow(step, 3) + 3 * (float)Math.Pow(step, 2);
        float h3 = (float)Math.Pow(step, 3) - 2 * (float)Math.Pow(step, 2) + step;
        float h4 = (float)Math.Pow(step, 3) - (float)Math.Pow(step, 2);
        // multiply and sum all functions together to build the interpolated point along the curve.
        return (h1 * p1) + (h2 * p2) + (h3 * t1) + (h4 * t2);
    }

    //===================
    // GetVector3AtStep
    //-------------------
    // Returns a three dimensional vector at a point along a curve.  The 'step' is where you are along the curve, zero
    // being the starting position and one being the end point. [0...1]
    // You must also provide two tangent vectors.
    // This builds the 3D curve by combining two 2D Hermite curves together, one for the XY plane and one for XZ.
    //-------------------
    //      p1:  The starting point of the curve
    //      t1:  The tangent (e.g. direction and speed) to how the curve leaves the starting point
    //      p2:  The endpoint of the curve
    //      t2:  The tangent (e.g. direction and speed) to how the curve meets the endpoint
    //    step:  A position along the curve from 0 to 1 inclusive (e.g. halfway would be 0.5f)
    //===================
    public static Vector3 GetVector3AtStep(Vector3 p1, Vector3 p2, Vector3 t1, Vector3 t2, float step)
    {
        float t = step;
        Vector3 position = (2.0f * t * t * t - 3.0f * t * t + 1.0f) * p1
        + (t * t * t - 2.0f * t * t + t) * t1
        + (-2.0f * t * t * t + 3.0f * t * t) * p2
        + (t * t * t - t * t) * t2;
        return position;
        //// XY Plane
        //Vector2 xyPlane = GetVector2AtStep(new Vector2(p1.x, p1.y), new Vector2(p2.x, p2.y), new Vector2(t1.x, t1.y),
        //    new Vector2(t2.x, t2.y), step);
        //// XZ Plane
        //Vector2 xzPlane = GetVector2AtStep(new Vector2(p1.x, p1.z), new Vector2(p2.x, p2.z), new Vector2(t1.x, t1.z),
        //    new Vector2(t2.x, t2.z), step);
        //// Output combined Vector3
        // return new Vector3(xyPlane.x, xyPlane.y, xzPlane.y);
    }

    //==============
    // DrawVector2
    //--------------
    // Using Debug.DrawLine(), this method uses a for loop to build the segments of a Hermite curve.  It can only be
    // seen in the Editor, in the 3D scene view, when paused.  This does NOT build a visible curve in the live game.
    //==============
    public static void DrawVector2(Vector2 p1, Vector2 p2, Vector2 t1, Vector2 t2, int segments)
    {
        Vector2 prevPoint;                                        // Start point of segment
        Vector2 nextPoint;                                        // End point of segment
        Color[] colors = new Color[2] { Color.white, Color.red };   // Colors array for display purposes
        int colorCounter = 0;                                     // Counter for looping through Colors array
        float stepLength;
        float step;

        for (int i = 0; i < segments; i++)
        {
            stepLength = 1.0f / segments;
            step = i * stepLength;
            prevPoint = GetVector2AtStep(p1, p2, t1, t2, step);
            nextPoint = GetVector2AtStep(p1, p2, t1, t2, step + stepLength);
            Debug.DrawLine(new Vector3(prevPoint.x, prevPoint.y, 0), new Vector3(nextPoint.x, nextPoint.y, 0), colors[colorCounter % colors.Length]);
            colorCounter++;
        }
    }

    //==============
    // DrawVector3
    //--------------
    // Just like the three dimensional version of the "vector at step" method above, this combines two 2D Hermite curves
    // to build up the 3D curve to display.
    //==============
    public static void DrawVector3(Vector3 p1, Vector3 p2, Vector3 t1, Vector3 t2, int segments)
    {
        Vector3 prevPoint;                                        // Start point of segment
        Vector3 nextPoint;                                        // End point of segment
        Color[] colors = new Color[2] { Color.white, Color.red };   // Colors array for display purposes
        int colorCounter = 0;                                     // Counter for looping through Colors array
        float stepLength;
        float step;

        for (int i = 0; i < segments; i++)
        {
            stepLength = 1.0f / segments;
            step = i * stepLength;
            prevPoint = GetVector3AtStep(p1, p2, t1, t2, step);
            nextPoint = GetVector3AtStep(p1, p2, t1, t2, step + stepLength);
            Debug.DrawLine(prevPoint, nextPoint, colors[colorCounter % colors.Length]);
            colorCounter++;
        }
    }
}