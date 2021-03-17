using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Circle
{
    public List<Vector3> points;
    public List<int> tris;
    public float radius;
    public int edges;
    public Vector3 centre;

    private void CreatePoints()
    {
        points.Add(centre);
        Vector3 p;
        float angle;
        float s;
        float c;
        // Create a point at each corner and add to the points list
        for (int i = 0; i < edges; i++)
        {
            angle = 360 / i;
            s = Mathf.Sin(angle);
            c = Mathf.Cos(angle);
            p = centre + radius * new Vector3(c, 0, s);
            points.Add(p);
        }
    }


    private void CreateTris()
    {
        for (int i = 0; i < edges + 1; i++)
        {
            tris.Add(0);
            tris.Add(i + 1);
            tris.Add((i + 2) % edges);
        }
    }


    //Constructor
    public Circle(float r, int e, Vector3 c)
    {
        radius = r;
        edges = e;
        centre = c;

        CreateTris();
    }
}
