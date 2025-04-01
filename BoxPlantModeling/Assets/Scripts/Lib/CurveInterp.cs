using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public abstract class BaseCurve
{
    private float[] cacheArcLengths;
    protected bool needsUpdate;

    public abstract Vector3 GetPoint(float t);

    public Vector3 GetPointAt(float u)
    {

        var t = this.GetUtoTmapping(u);
        return this.GetPoint(t);

    }

    // Get sequence of points using getPoint( t )

    public virtual Vector3[] GetPoints(int divisions = 5)
    {

        Vector3[] points = new Vector3[divisions + 1];

        for (int d = 0; d <= divisions; d++)
        {
            points[d] = this.GetPoint((float)d / divisions);
        }

        return points;

    }

    public virtual Vector3[] GetPoints(float from, float to, int divisions = 5)
    {

        Vector3[] points = new Vector3[divisions + 1];
        var delta = (to - from);
        for (int d = 0; d <= divisions; d++)
        {
            float t = (float)d / divisions * delta;


            {
                points[d] = this.GetPoint(t);
            }
        }

        return points;

    }

    // Get sequence of points using getPointAt( u )

    public Vector3[] GetSpacedPoints(int divisions = 5)
    {

        Vector3[] points = new Vector3[divisions + 1];


        for (int d = 0; d <= divisions; d++)
        {

            points[d] = (this.GetPointAt((float)d / divisions));

        }

        return points;

    }

    // Get total curve arc length

    public virtual float GetLength(int divisions = 200)
    {

        var lengths = this.GetLengths(divisions);
        return lengths[lengths.Length - 1];

    }

    // Get list of cumulative segment lengths

    public virtual float[] GetLengths(int divisions = 200)
    {

        if (this.cacheArcLengths != null &&
            (this.cacheArcLengths.Length == divisions + 1) &&
            !this.needsUpdate)
        {

            return this.cacheArcLengths;

        }

        this.needsUpdate = false;

        float[] cache = new float[divisions + 1];
        Vector3 current, last = this.GetPoint(0);
        float sum = 0;

        cache[0] = 0;

        for (int p = 1; p <= divisions; p++)
        {

            current = this.GetPoint((float)p / divisions);
            sum += Vector3.Distance(current, last);
            cache[p] = (sum);
            last = current;

        }

        this.cacheArcLengths = cache;

        return cache; // { sums: cache, sum: sum }; Sum is in the last element.

    }



    public virtual void UpdateArcLengths()
    {

        this.needsUpdate = true;
        this.GetLengths();

    }

    // Given u ( 0 .. 1 ), get a t to find p. This gives you points which are equidistant

    public float GetUtoTmapping(float u, float distance = 0)
    {

        var arcLengths = this.GetLengths();

        int i = 0;
        int il = arcLengths.Length;

        float targetArcLength; // The targeted u distance value to get

        if (distance != 0)
        {
            targetArcLength = distance;
        }
        else
        {
            targetArcLength = u * arcLengths[il - 1];
        }

        // binary search for the index with largest value smaller than target u distance

        int low = 0, high = il - 1;
        float comparison;

        while (low <= high)
        {

            i = Mathf.FloorToInt((float)(low + (high - low)) / 2f);

            comparison = arcLengths[i] - targetArcLength;

            if (comparison < 0)
            {
                low = i + 1;
            }
            else if (comparison > 0)
            {
                high = i - 1;
            }
            else
            {

                high = i;
                break;

                // DONE

            }

        }

        i = high;

        if (arcLengths[i] == targetArcLength)
        {
            return i / (il - 1);

        }

        // we could get finer grain at lengths, or use simple interpolation between two points

        var lengthBefore = arcLengths[i];
        var lengthAfter = arcLengths[i + 1];

        var segmentLength = lengthAfter - lengthBefore;

        // determine where we are between the 'before' and 'after' points

        var segmentFraction = (targetArcLength - lengthBefore) / segmentLength;

        // add that fractional amount to t

        var t = (i + segmentFraction) / (il - 1);

        return t;

    }

    // Returns a unit vector tangent at t
    // In case any sub curve does not implement its tangent derivation,
    // 2 points a small delta apart will be used to find its gradient
    // which seems to give a reasonable approximation

    public Vector3 GetTangent(float t, Vector3 optionalTarget = default)
    {

        var delta = 0.0001f;
        var t1 = t - delta;
        var t2 = t + delta;

        // Capping in case of danger

        if (t1 < 0) t1 = 0;
        if (t2 > 1) t2 = 1;

        var pt1 = this.GetPoint(t1);
        var pt2 = this.GetPoint(t2);

        var tangent = (pt2 - pt1).normalized;

        return tangent;
    }

    public Vector3 GetTangentAt(float u, Vector3 optionalTarget = default)
    {

        var t = this.GetUtoTmapping(u);
        return this.GetTangent(t, optionalTarget);

    }

    public (Vector3[] tangents, Vector3[] normals, Vector3[] binormals) ComputeFrenetFrames(int segments, bool closed)
    {

        // see http://www.cs.indiana.edu/pub/techreports/TR425.pdf

        var normal = new Vector3();

        Vector3[] tangents = new Vector3[segments];
        Vector3[] normals = new Vector3[segments];
        Vector3[] binormals = new Vector3[segments];


        // compute the tangent vectors for each segment on the curve

        for (int i = 0; i <= segments; i++)
        {

            float u = (float)i / segments;

            tangents[i] = this.GetTangentAt(u, new Vector3());
            tangents[i].Normalize();

        }

        // select an initial normal vector perpendicular to the first tangent vector,
        // and in the direction of the minimum tangent xyz component

        normals[0] = new Vector3();
        binormals[0] = new Vector3();
        var min = float.MaxValue;
        var tx = Math.Abs(tangents[0].x);
        var ty = Math.Abs(tangents[0].y);
        var tz = Math.Abs(tangents[0].z);

        if (tx <= min)
        {

            min = tx;
            normal = Vector3.right;

        }

        if (ty <= min)
        {

            min = ty;

            normal = Vector3.up;

        }

        if (tz <= min)
        {

            normal = Vector3.forward;

        }

        var vec = Vector3.Cross(tangents[0], normal).normalized;

        normals[0] = Vector3.Cross(tangents[0], vec);
        binormals[0] = Vector3.Cross(tangents[0], normals[0]);


        // compute the slowly-varying normal and binormal vectors for each segment on the curve

        for (int i = 1; i <= segments; i++)
        {

            normals[i] = normals[i - 1];

            binormals[i] = binormals[i - 1];

            vec = Vector3.Cross(tangents[i - 1], tangents[i]);

            if (vec.magnitude > float.Epsilon)
            {

                vec.Normalize();
                var theta = Mathf.Acos(Mathf.Clamp(Vector3.Dot(tangents[i - 1], tangents[i]), -1, 1)); // clamp for floating pt errors

                normals[i] = Quaternion.AngleAxis(theta, vec) * normals[i];
            }

            binormals[i] = Vector3.Cross(tangents[i], normals[i]);

        }

        // if the curve is closed, postprocess the vectors so the first and last normal vectors are the same

        if (closed == true)
        {

            var theta = Mathf.Acos(Mathf.Clamp(Vector3.Dot(normals[0], normals[segments]), -1, 1));
            theta /= segments;

            if (Vector3.Dot(tangents[0], Vector3.Cross(normals[0], normals[segments])) > 0)
            {
                theta = -theta;
            }

            for (int i = 1; i <= segments; i++)
            {

                // twist a little...
                normals[i] = Quaternion.AngleAxis(theta, tangents[i]) * normals[i];
                binormals[i] = Vector3.Cross(tangents[i], normals[i]);

            }

        }

        return (
            tangents: tangents,
            normals: normals,
            binormals: binormals

        );

    }




}




namespace CurveLib.Curves
{
    public enum SplineType
    {
        Centripetal,
        Chordal,
        Catmullrom
    }

    [Serializable]
    public class SplineCurve : BaseCurve
    {
        private Vector3[] points;
        private bool closed;
        private SplineType curveType;
        private float tension;

        public SplineCurve(Vector3[] points, bool closed = false, SplineType curveType = SplineType.Centripetal, float tension = 0.5f)
        {
            this.points = points;
            this.closed = closed;
            this.curveType = curveType;
            this.tension = tension;
        }

        public override Vector3 GetPoint(float t)
        {
            var l = points.Length;

            var p = (l - (this.closed ? 0 : 1)) * t;
            var intPoint = Mathf.FloorToInt(p);
            var weight = p - intPoint;

            if (this.closed)
            {
                intPoint += intPoint > 0 ? 0 : (Mathf.FloorToInt(Mathf.Abs(intPoint) / l) + 1) * l;
            }
            else if (weight == 0 && intPoint == l - 1)
            {
                intPoint = l - 2;
                weight = 1;
            }

            Vector3 p0, p3; // 4 points (p1 & p2 defined below)

            if (this.closed || intPoint > 0)
            {
                p0 = points[(intPoint - 1) % l];
            }
            else
            {
                // extrapolate first point
                p0 = (points[0] - points[1]) * 2 + points[0];
            }

            var p1 = points[intPoint % l];
            var p2 = points[(intPoint + 1) % l];

            if (this.closed || intPoint + 2 < l)
            {
                p3 = points[(intPoint + 2) % l];
            }
            else
            {
                // extrapolate last point
                p3 = points[l - 1] - points[l - 2] + points[l - 1];
            }

            CubicPoly1D px = new CubicPoly1D(), py = new CubicPoly1D(), pz = new CubicPoly1D();
            if (this.curveType == SplineType.Centripetal || this.curveType == SplineType.Chordal)
            {

                // init Centripetal / Chordal Catmull-Rom
                var pow = this.curveType == SplineType.Chordal ? 0.5f : 0.25f;
                var dt0 = Mathf.Pow((p0 - p1).sqrMagnitude, pow);
                var dt1 = Mathf.Pow((p1 - p2).sqrMagnitude, pow);
                var dt2 = Mathf.Pow((p2 - p3).sqrMagnitude, pow);

                // safety check for repeated points
                var delta = 0.0001f;
                if (dt1 < delta) dt1 = 1.0f;
                if (dt0 < delta) dt0 = dt1;
                if (dt2 < delta) dt2 = dt1;

                px.InitNonuniformCatmullRom(p0.x, p1.x, p2.x, p3.x, dt0, dt1, dt2);
                py.InitNonuniformCatmullRom(p0.y, p1.y, p2.y, p3.y, dt0, dt1, dt2);
                pz.InitNonuniformCatmullRom(p0.z, p1.z, p2.z, p3.z, dt0, dt1, dt2);

            }
            else if (this.curveType == SplineType.Catmullrom)
            {

                px.InitCatmullRom(p0.x, p1.x, p2.x, p3.x, this.tension);
                py.InitCatmullRom(p0.y, p1.y, p2.y, p3.y, this.tension);
                pz.InitCatmullRom(p0.z, p1.z, p2.z, p3.z, this.tension);

            }

            var point = new Vector3(
                px.Calc(weight),
                py.Calc(weight),
                pz.Calc(weight)
            );

            return point;

        }
    }

    public class CubicPoly1D
    {
        float c0;
        float c1;
        float c2;
        float c3;

        /*
         * Compute coefficients for a cubic polynomial
         *   p(s) = c0 + c1*s + c2*s^2 + c3*s^3
         * such that
         *   p(0) = x0, p(1) = x1
         *  and
         *   p'(0) = t0, p'(1) = t1.
         */
        public void Init(float x0, float x1, float t0, float t1)
        {

            c0 = x0;
            c1 = t0;
            c2 = -3 * x0 + 3 * x1 - 2 * t0 - t1;
            c3 = 2 * x0 - 2 * x1 + t0 + t1;

        }

        public void InitCatmullRom(float x0, float x1, float x2, float x3, float tension)
        {

            Init(x1, x2, tension * (x2 - x0), tension * (x3 - x1));

        }

        public void InitNonuniformCatmullRom(float x0, float x1, float x2, float x3, float dt0, float dt1, float dt2)
        {

            // compute tangents when parameterized in [t1,t2]
            var t1 = (x1 - x0) / dt0 - (x2 - x0) / (dt0 + dt1) + (x2 - x1) / dt1;
            var t2 = (x2 - x1) / dt1 - (x3 - x1) / (dt1 + dt2) + (x3 - x2) / dt2;

            // rescale tangents for parametrization in [0,1]
            t1 *= dt1;
            t2 *= dt1;

            Init(x1, x2, t1, t2);

        }

        public float Calc(float t)
        {

            float t2 = t * t;
            float t3 = t2 * t;
            return c0 + c1 * t + c2 * t2 + c3 * t3;

        }

    }

    public class CubicPoly3D
    {
        Vector3 c0;
        Vector3 c1;
        Vector3 c2;
        Vector3 c3;

        /*
         * Compute coefficients for a cubic polynomial
         *   p(s) = c0 + c1*s + c2*s^2 + c3*s^3
         * such that
         *   p(0) = x0, p(1) = x1
         *  and
         *   p'(0) = t0, p'(1) = t1.
         */
        public void Init(Vector3 x0, Vector3 x1, Vector3 t0, Vector3 t1)
        {

            c0 = x0;
            c1 = t0;
            c2 = -3 * x0 + 3 * x1 - 2 * t0 - t1;
            c3 = 2 * x0 - 2 * x1 + t0 + t1;

        }

        public void InitCatmullRom(Vector3 x0, Vector3 x1, Vector3 x2, Vector3 x3, float tension)
        {

            Init(x1, x2, tension * (x2 - x0), tension * (x3 - x1));

        }

        public void InitNonuniformCatmullRom(Vector3 x0, Vector3 x1, Vector3 x2, Vector3 x3, float dt0, float dt1, float dt2)
        {

            // compute tangents when parameterized in [t1,t2]
            var t1 = (x1 - x0) / dt0 - (x2 - x0) / (dt0 + dt1) + (x2 - x1) / dt1;
            var t2 = (x2 - x1) / dt1 - (x3 - x1) / (dt1 + dt2) + (x3 - x2) / dt2;

            // rescale tangents for parametrization in [0,1]
            t1 *= dt1;
            t2 *= dt1;

            Init(x1, x2, t1, t2);

        }

        public Vector3 Calc(float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;
            return c0 + c1 * t + c2 * t2 + c3 * t3;
        }

    }
}