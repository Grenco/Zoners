using System.Collections;
using System.Collections.Generic;
using System.Windows;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using Photon.Pun;

public class TeamController : MonoBehaviour
{
    GameObject[] players;
    public GameObject dangerZone;
    public GameObject minimapZone;
    public string team;

    public GameObject enemyTeam;
    TeamController enemyTeamController;

    Transform[] transforms;
    Vector3[] positions;

    LineRenderer lr;

    MeshFilter zoneMesh;
    MeshFilter minimapZoneMesh;

    public int score;

    // Start is called before the first frame update
    void Start()
    {
        transforms = new Transform[4];
        players = new GameObject[4];

        lr = gameObject.GetComponent<LineRenderer>();

        zoneMesh = dangerZone.GetComponent<MeshFilter>();
        minimapZoneMesh = minimapZone.GetComponent<MeshFilter>();

        enemyTeamController = enemyTeam.GetComponent<TeamController>();
    }

    // Update is called once per frame
    void Update()
    {
        //transforms.Remove(null);
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

    public void AddPlayer(GameObject player)
    {
        int[] teamList = (int[])PhotonNetwork.PlayerList[0].CustomProperties[team];
        PhotonView playerView = player.GetPhotonView();
        int teamNumber = System.Array.IndexOf(teamList, playerView.CreatorActorNr);

        transforms[teamNumber] = player.transform;
        players[teamNumber] = player;
    }

    public void RemovePlayer(GameObject player)
    {
        int[] teamList = (int[])PhotonNetwork.PlayerList[0].CustomProperties[team];
        PhotonView playerView = player.GetPhotonView();
        int teamNumber = System.Array.IndexOf(teamList, playerView.CreatorActorNr);

        //transforms.Remove(player.transform);
        //players.Remove(player);

        transforms[teamNumber] = null;
        players[teamNumber] = null;
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

    public bool IsAround(Vector3 point)
    {
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

        if (intersections % 4 == 0)
        {
            return false;
        }
        return true;

    }

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
        }
        zoneMesh.mesh = mesh;
        minimapZoneMesh.mesh = mesh;
    }

}
