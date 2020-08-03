using System.Collections;
using System.Collections.Generic;
using System.Windows;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class EnemyArea : MonoBehaviour
{
    GameObject[] enemies;
    GameObject dangerZone;
    GameObject player;
    PlayerControls playerControls;

    HashSet<Transform> transforms;
    Vector3[] positions;

    LineRenderer lr;

    MeshFilter zoneMesh;
    bool zoneActive;

    // Start is called before the first frame update
    void Start()
    {

        transforms = new HashSet<Transform>(GetComponentsInChildren<Transform>());
        transforms.Remove(transform);
        enemies = transforms.Select(x => x.gameObject).ToArray();


        foreach (GameObject enemy in enemies)
        {
            if (enemy.name == "DangerZone")
            {
                dangerZone = enemy;
            }
        }

        transforms.Remove(dangerZone.transform);

        positions = transforms.Select(x => x.position).ToArray();

        lr = gameObject.GetComponent<LineRenderer>();
        lr.loop = true;

        player = GameObject.FindWithTag("Player");
        playerControls = player.GetComponent<PlayerControls>();

        zoneMesh = dangerZone.GetComponent<MeshFilter>();

        zoneActive = true;
    }

    public void RemoveEnemy(GameObject enemy)
    {
        transforms.Remove(enemy.transform);
        if (transforms.Count() == 0)
        {
            GameObject endingTextBox = GameObject.Find("GameEndText");
            Text endingText = endingTextBox.GetComponent<Text>();
            endingText.text = "YOU WIN!";
            playerControls.DisableControls();
        }
    }

    public int LineCrossCheck(Vector2 p, Vector2 p1, Vector2 p2)
    {
        p1 -= p;
        p2 -= p;

        // Check that points are on the correct side of the origin
        if (p1.x < 0 && p2.x < 0)
        {
            return 0;
        }
        else if ((p1.y > 0 && p2.y > 0) || (p1.y < 0 && p2.y < 0))
        {
            return 0;
        }

        //Check if any points are on the ray
        bool p1Check = p1.y == 0 && p1.x > 0;
        bool p2Check = p2.y == 0 && p2.x > 0;
        if (p1Check && p2Check)
        {
            return 0;
        }
        else if (p1Check || p2Check)
        {
            return 1;
        }

        float xCross = (p1.x * p2.y - p1.y * p2.x) / (p2.y - p1.y);
        if (xCross >= 0)
        {
            return 2;
        }

        return 0;
    }

    bool IsAround(Vector3 point)
    {
        int vertCount = positions.Length;
        int intersections = 0;
        Vector2 v0 = new Vector2(point.x, point.z);

        for (int i = 0; i < vertCount; i++)
        {
            Vector2 v1 = new Vector2(positions[i].x, positions[i].z);
            Vector2 v2 = new Vector2(positions[(i + 1) % vertCount].x, positions[(i + 1) % vertCount].z);

            intersections += LineCrossCheck(v0, v1, v2);

        }

        if (intersections % 4 == 0)
        {
            return false;
        }
        return true;

    }


    // Update is called once per frame
    void Update()
    {
        transforms.Remove(null);
        positions = transforms.Select(x => x.position).ToArray();

        lr.positionCount = positions.Length;
        lr.SetPositions(positions);

        if (IsAround(player.transform.position))
        {
            playerControls.TakeDamage();
        }

        //Mesh mesh = zoneMesh.mesh;

        Mesh mesh = new Mesh();


        if (positions.Length == 4)
        {
            Quadrilateral quad = new Quadrilateral(positions);

            for (int i = 0; i < quad.points.Count; i++)
            {
                quad.points[i] -= zoneMesh.transform.position;
            }

            mesh.SetVertices(quad.points);
            mesh.SetTriangles(quad.trirefs, 0);
            mesh.SetColors(new List<Color>(quad.triangles.Count));
            mesh.RecalculateNormals();

            zoneMesh.mesh = mesh;
        }
        else if (positions.Length == 3)
        {
            Triangle tri = new Triangle(positions, 0, 1, 2);

            for (int i = 0; i < tri.points.Count; i++)
            {
                tri.points[i] -= zoneMesh.transform.position;
            }

            mesh.SetVertices(tri.points);
            mesh.SetTriangles(tri.refs, 0);
            mesh.SetColors(new List<Color>(tri.refs.Count));
            mesh.RecalculateNormals();

            zoneMesh.mesh = mesh;
        }
        else if (zoneActive)
        {
            mesh.SetVertices(new List<Vector3>());
            mesh.SetTriangles(new List<int>(), 0);
            mesh.SetColors(new List<Color>());
            mesh.RecalculateNormals();

            zoneMesh.mesh = mesh;
        }
    }
}
