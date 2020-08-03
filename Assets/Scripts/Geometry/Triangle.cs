using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Triangle
{
    public List<Vector3> points;
    public List<int> refs;
    public bool colinear = false;
    public bool cw = true;

    public float area
    {
        get
        {
            List<Vector3> p = points;
            float area = Mathf.Abs(p[0].x * (p[1].z - p[2].z) + p[1].x * (p[2].z - p[0].z) + p[2].x * (p[0].z - p[1].z));
            return area;
        }
    }

    public Vector3 CentrePoint()
    {
        Vector3 centre = new Vector3();
        foreach (Vector3 p in points)
        {
            centre += p;
        }
        return centre / points.Count;
    }

    void CWCheck()
    {
        List<Vector3> p = points;
        float A = p[1].x * p[0].z + p[2].x * p[1].z + p[0].x * p[2].z;
        float B = p[0].x * p[1].z + p[1].x * p[2].z + p[2].x * p[0].z;

        if (Mathf.Abs(A - B) < 0.00001f)
        {
            colinear = true;
        }
        else if (A < B)
        {
            cw = false;
            refs.Reverse();
        }
    }

    public Triangle(List<Vector3> p)
    {
        points = p;
    }

    public Triangle(Vector3[] p, int ref1, int ref2, int ref3)
    {
        points = new List<Vector3>(p);
        refs = new List<int> { ref1, ref2, ref3 };
        CWCheck();
    }

    public Triangle(Vector3 p1, Vector3 p2, Vector3 p3, int ref1, int ref2, int ref3)
    {
        points = new List<Vector3> { p1, p2, p3 };
        refs = new List<int> { ref1, ref2, ref3 };
        CWCheck();
    }
}
