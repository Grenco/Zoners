using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon.Pun;

public class TeamController : MonoBehaviour
{
    public GameObject[] players;
    public GameObject dangerZone;
    public GameObject minimapZone;
    public Color teamZoneColor;

    private MeshFilter zoneMesh;
    private MeshRenderer zoneMeshRenderer;
    private MeshFilter minimapZoneMesh;
    public string team;

    public GameObject enemyTeam;
    private TeamController enemyTeamController;

    private Transform[] transforms;
    private Vector3[] positions;

    private LineRenderer lr;

    public int score;

    private float zoneArea;
    public float damageMultiplier;

    // Start is called before the first frame update
    void Start()
    {
        //transforms = new Transform[4];
        //players = new GameObject[4];
        transforms = players.Select(x => x.transform).ToArray();

        lr = gameObject.GetComponent<LineRenderer>();

        zoneMesh = dangerZone.GetComponent<MeshFilter>();
        minimapZoneMesh = minimapZone.GetComponent<MeshFilter>();
        zoneMeshRenderer = dangerZone.GetComponent<MeshRenderer>();

        enemyTeamController = enemyTeam.GetComponent<TeamController>();
    }

    // Update is called once per frame
    void Update()
    {
        lr.positionCount = transforms.Count(i => i != null);
        if (lr.positionCount > 0)
        {
            positions = transforms.Where(x => x != null).Select(x => x.position).ToArray();
        }
        else
        {
            positions = new Vector3[0];
        }

        lr.SetPositions(positions);

        UpdateZone();
    }

    /// <summary>
    /// Add a player gameobject to the team to be used in the zone.
    /// Can be used at the beginning of hte game to replace AI players and after respawn.
    /// </summary>
    /// <param name="player"> GameeOject of the player ot be added. </param>
    public void AddPlayer(GameObject player)
    {
        int playerNumber = player.GetComponent<MultiplayerControls>().playerNumber;

        if (players[playerNumber] != null)
        {
            players[playerNumber].SetActive(false);
        }

        transforms[playerNumber] = player.transform;
        players[playerNumber] = player;
    }

    /// <summary>
    /// Remove a player from the teame to be left out of zone calculations while dead.
    /// </summary>
    /// <param name="playerNumber"> Index of the player within the team. </param>
    public void RemovePlayer(int playerNumber)
    {
        transforms[playerNumber] = null;
        players[playerNumber] = null;
    }

    /// <summary>
    /// Check if a ray from p intersects the line between p1 and p2.
    /// The ray is projected from p, parallel to the z-axis.
    /// Calculations are completed in 2D on the x-z plane.
    /// </summary>
    /// <param name="p">Emission point for the ray.</param>
    /// <param name="p1">First point on the intersection line.</param>
    /// <param name="p2">Second point on the intersection line.</param>
    /// <returns>Returns 0 for no intersection, 1 if p1 or p2 lie on the ray, 
    /// or 2 if the ray intersects the line. </returns>
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

    /// <summary>
    /// Check if the team's zone lies around a given point in the x-z plane.
    /// </summary>
    /// <param name="point">Input value.</param>
    /// <returns>Returns true if point lies inside the zone. False if not.</returns>
    public bool IsAround(Vector3 point)
    {
        // If a line projected from the point intersects the zone an even number of times,
        // the point lies outside the zone. Odd and it lies inside.
        if (players.Count(x => x != null) < 3)
        {
            return false;
        }
        int vertCount = positions.Length;
        int intersections = 0;
        Vector2 v0 = new Vector2(point.x, point.z);

        for (int i = 0; i < vertCount; i++)
        {
            Vector2 v1 = new Vector2(positions[i].x, positions[i].z);
            Vector2 v2 = new Vector2(positions[(i + 1) % vertCount].x, positions[(i + 1) % vertCount].z);

            intersections += LineCrossCheck(v0, v1, v2);
        }

        // Checks are done to mod 4 as the line intersection checker returns 2 for a single intersection
        // or 1 for a corner intersection, which happens twice on the corner.
        if (intersections % 4 == 0)
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// Update the mesh of the zone to accomodate for player movement.
    /// </summary>
    private void UpdateZone()
    {
        Mesh mesh = new Mesh();
        if (positions.Length == 4)
        {
            Quadilateral quad = new Quadilateral(positions);

            for (int i = 0; i < quad.points.Count; i++)
            {
                quad.points[i] -= zoneMesh.transform.position;
            }

            zoneArea = quad.area;

            mesh.SetVertices(quad.points);
            mesh.SetTriangles(quad.trirefs, 0);
            mesh.SetColors(new List<Color>(quad.triangles.Count));
            mesh.RecalculateNormals();
        }
        else if (positions.Length == 3)
        {
            Triangle tri = new Triangle(positions, 0, 1, 2);

            for (int i = 0; i < tri.points.Count; i++)
            {
                tri.points[i] -= zoneMesh.transform.position;
            }

            zoneArea = tri.area;

            mesh.SetVertices(tri.points);
            mesh.SetTriangles(tri.refs, 0);
            mesh.SetColors(new List<Color>(tri.refs.Count));
            mesh.RecalculateNormals();
        }
        else
        {
            mesh.SetVertices(new List<Vector3>());
            mesh.SetTriangles(new List<int>(), 0);
            mesh.SetColors(new List<Color>());
            mesh.RecalculateNormals();

            zoneArea = 0;
        }
        zoneMesh.mesh = mesh;
        minimapZoneMesh.mesh = mesh;

        Debug.Log("Zone Area: " + zoneArea.ToString());

        damageMultiplier = 1 - Mathf.Max(Mathf.Min((zoneArea - 56.25f) / 8000, 1), 0);

        zoneMeshRenderer.material.color = Color.Lerp(Color.clear, teamZoneColor, Mathf.Max(damageMultiplier - 0.05f, 0f));
        zoneMeshRenderer.material.SetColor("_EmissionColor", teamZoneColor * damageMultiplier * 2);
    }

}
