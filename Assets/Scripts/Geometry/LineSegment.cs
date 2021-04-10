using UnityEngine;

public class LineSegment
{
    public Vector3[] points;

    private float Cross(Vector2 p1, Vector2 p2)
    {
        return p1.x * p2.y - p1.y * p2.x;
    }

    public bool Intersects(LineSegment l2, out Vector3 intersection)
    {
        intersection = new Vector3();
        Vector2 p = new Vector2(points[0].x, points[0].z);
        Vector2 r = new Vector2(points[1].x, points[1].z);
        r -= p;

        Vector2 q = new Vector2(l2.points[0].x, l2.points[0].z);
        Vector2 s = new Vector2(l2.points[1].x, l2.points[1].z);
        s -= q;

        float rs = Cross(r, s);
        float qpr = Cross(q - p, r);
        float u = qpr / rs;
        float t = Cross(q - p, s) / rs;

        if (rs == 0 && qpr == 0)
        {
            return false;
        }
        else if (rs == 0)
        {
            return false;
        }
        else if (0 <= t && t <= 1 && 0 <= u && u <= 1)
        {
            Vector2 intersect = p + t * r;
            intersection = new Vector3(intersect.x, points[0].y, intersect.y);
            return true;
        }

        return false;
    }

    public bool SharesPoint(LineSegment line)
    {
        foreach (Vector3 p1 in points)
        {
            foreach (Vector3 p2 in line.points)
            {
                if (p1.Equals(p2))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public LineSegment(Vector3 p1, Vector3 p2)
    {
        points = new Vector3[2];
        points[0] = p1;
        points[1] = p2;
    }
}