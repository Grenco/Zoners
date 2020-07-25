using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;
using Photon;
using Photon.Pun;

public class MazeConstructor : MonoBehaviour
{

    public float placementThreshold = 0.1f;

    [SerializeField] private Material mazeMat1;
    [SerializeField] private Material mazeMat2;
    public int sizeRows = 15;
    public int sizeCols = 15;
    public float hallWidth = 7.5f;
    public float hallHeight = 3.5f;

    private MazeMeshGenerator meshGenerator;

    public PhotonView photonView;

    public int[,] data;
    //{
    //    get; private set;
    //}


    void Awake()
    {
        photonView = gameObject.GetComponent<PhotonView>();
        // default to walls surrounding a single empty cell
        data = new int[,]
        {
            {1, 1, 1},
            {1, 0, 1},
            {1, 1, 1}
        };

        meshGenerator = new MazeMeshGenerator(hallWidth, hallHeight);

        if (PhotonNetwork.IsMasterClient)//photonView.IsMine)
        {
            GenerateNewMaze();
            //photonView.RPC("Test", RpcTarget.All, new int[,] { { 1, 1, 1 }, { 1, 1, 1 } } as object);
            photonView.RPC("DisplayMaze", RpcTarget.All, Serialize(data));
        }
        //DisplayMaze();

    }

    int[,] FromDimensions(int sizeRows, int sizeCols)
    {
        // Use maze gereration algrithm to create randomised maze
        int[,] maze = new int[sizeRows, sizeCols];
        int rMax = maze.GetUpperBound(0);
        int cMax = maze.GetUpperBound(1);

        for (int i = 0; i <= rMax; i++)
        {
            for (int j = 0; j <= cMax; j++)
            {
                // Add walls to the outer boundaries
                if ((i == 0 || j == 0 || i == rMax || j == cMax) && j != (cMax + 1) / 2)
                {
                    maze[i, j] = 1;
                }

                // Add walls to every other space and a random adjoining space
                else if (i % 2 == 0 && j % 2 == 0)
                {
                    if (Random.value > placementThreshold)
                    {
                        maze[i, j] = 1;

                        int a = Random.value < .5 ? 0 : (Random.value < .5 ? -1 : 1);
                        int b = a != 0 ? 0 : (Random.value < .5 ? -1 : 1);
                        maze[i + a, j + b] = 1;
                    }
                }
            }
        }
        return maze;
    }

    [PunRPC]
    void DisplayMaze(byte[] mazeDataBytes)
    {

        Debug.Log("Generating Maze");

        int[,] mazeData = Deserialize<int[,]>(mazeDataBytes);
        data = mazeData;

        gameObject.transform.position = Vector3.zero;
        gameObject.name = "Procedural Maze";
        gameObject.tag = "Generated";

        MeshFilter mf = gameObject.AddComponent<MeshFilter>();
        mf.mesh = meshGenerator.FromData(mazeData);

        MeshCollider mc = gameObject.AddComponent<MeshCollider>();
        mc.sharedMesh = mf.mesh;

        MeshRenderer mr = gameObject.AddComponent<MeshRenderer>();
        mr.materials = new Material[2] { mazeMat1, mazeMat2 };
    }

    public static byte[] Serialize(object toSerialize)
    {
        BinaryFormatter bf = new BinaryFormatter();
        MemoryStream ms = new MemoryStream();
        bf.Serialize(ms, toSerialize);
        return ms.ToArray();
    }

    public static T Deserialize<T>(byte[] toDeserialize)
    {
        BinaryFormatter bf = new BinaryFormatter();
        MemoryStream ms = new MemoryStream(toDeserialize);
        return (T)bf.Deserialize(ms);
    }

    public void GenerateNewMaze()
    {
        data = FromDimensions(sizeRows, sizeCols);
    }
}
