using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Quadilateral
{

    public List<Vector3> points;
    List<LineSegment> lines;
    public List<int> trirefs;
    public List<Triangle> triangles;
    public List<Vector3> normals;

    public float area
    {
        get
        {
            float area = triangles.Sum(x => x.area);
            return area;
        }
    }


    void Triangulate()
    {
        trirefs = new List<int>();
        triangles = new List<Triangle>();
        if (points.Count == 4)
        {
            Triangle tri1 = new Triangle(points[0], points[1], points[2], 0, 1, 2);
            Triangle tri2 = new Triangle(points[2], points[3], points[0], 2, 3, 0);

            if (tri1.cw != tri2.cw)
            {
                tri1 = new Triangle(points[0], points[1], points[3], 0, 1, 3);
                tri2 = new Triangle(points[1], points[2], points[3], 1, 2, 3);
            }

            triangles.Add(tri1);
            triangles.Add(tri2);

            trirefs.AddRange(triangles[0].refs);
            trirefs.AddRange(triangles[1].refs);
        }
        else if(points.Count == 5)
        {
            Triangle tri;

            for (int i = 0; i < 4; i++)
            {
                int i2 = (i + 1) % 4;
                tri = new Triangle(points[i], points[i2], points[4], i, i2, 4);
                triangles.Add(tri);

                if (tri.colinear) { }
                else
                {
                    trirefs.AddRange(tri.refs);
                }
            }

        }
        else
        {
            Debug.Log("Quadrilateral Error");
        }
    }

    void IntersectionCheck()
    {
        foreach (LineSegment l1 in lines)
        {
            foreach (LineSegment l2 in lines)
            {
                if (l1.SharesPoint(l2)) { }
                else
                {
                    if (l1.Intersects(l2, out Vector3 intersect))
                    {
                        points.Add(intersect);
                        return;
                    }
                }
            }
        }
    }

    void CreateLines()
    {
        lines = new List<LineSegment>();
        for (int i = 0; i < points.Count; i++)
        {
            Vector3 p1 = points[i];
            Vector3 p2 = points[(i + 1) % points.Count];

            lines.Add(new LineSegment(p1, p2));
        }

        IntersectionCheck();
    }



    public Quadilateral(Vector3[] p)
    {
        points = new List<Vector3>();
        for (int i = 0; i < p.Length; i++)
        {
            points.Add(p[i]);
        }

        CreateLines();

        Triangulate();

    }
}
