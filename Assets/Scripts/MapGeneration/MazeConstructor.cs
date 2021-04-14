using Photon.Pun;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class MazeConstructor : MonoBehaviour
{
    public float placementThreshold = 0.1f;

    public Material mazeMat1;
    public Material mazeMat2;
    public int sizeRows = 15;
    public int sizeCols = 15;
    public float hallWidth = 7.5f;
    public float hallHeight = 3.5f;

    private MazeMeshGenerator meshGenerator;

    public PhotonView photonView;

    public int[,] data;

    private void Awake()
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
            sizeRows = GameSettings.MapRows + (GameSettings.MapRows + 1) % 2; //Must be odd
            sizeCols = GameSettings.MapCols + (GameSettings.MapCols + 1) % 2; //Must be odd
            GenerateNewMaze();
            photonView.RPC("DisplayMaze", RpcTarget.All, Serialize(data));
        }
    }

    private int[,] FromDimensions(int sizeRows, int sizeCols)
    {
        // Use maze gereration algorithm to create randomised maze
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
                    if (UnityEngine.Random.value > placementThreshold)
                    {
                        maze[i, j] = 1;

                        int a = UnityEngine.Random.value < .5 ? 0 : (UnityEngine.Random.value < .5 ? -1 : 1);
                        int b = a != 0 ? 0 : (UnityEngine.Random.value < .5 ? -1 : 1);
                        maze[i + a, j + b] = 1;
                    }
                }
            }
        }
        return maze;
    }

    /// <summary>
    /// To be called by MasterClient after the maze data has been generated.
    /// </summary>
    /// <param name="mazeDataBytes">Maze data converted to binary from int array.</param>
    [PunRPC]
    private void DisplayMaze(byte[] mazeDataBytes)
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

        // Add box filters to prevent the player clipping through the walls at high speeds
        for (int i = 0; i < sizeRows; i++)
        {
            for (int j = 0; j < sizeCols; j++)
            {
                if (data[i, j] == 1)
                {
                    BoxCollider bc = gameObject.AddComponent<BoxCollider>();
                    bc.center = Position(i, j) + new Vector3(0, hallHeight / 2, 0);
                    bc.size = new Vector3(hallWidth, hallHeight, hallWidth);
                }
            }
        }

        MeshRenderer mr = gameObject.AddComponent<MeshRenderer>();
        mr.materials = new Material[2] { mazeMat1, mazeMat2 };
    }

    /// <summary>
    /// Convert object to binary format to be sent over Photon Network.
    /// </summary>
    /// <param name="toSerialize">Object to be serialised.</param>
    /// <returns>Returns object in form of byte array.</returns>
    public static byte[] Serialize(object toSerialize)
    {
        BinaryFormatter bf = new BinaryFormatter();
        MemoryStream ms = new MemoryStream();
        bf.Serialize(ms, toSerialize);
        return ms.ToArray();
    }

    /// <summary>
    /// Return binary object to original form.
    /// </summary>
    /// <typeparam name="T">Desired type of object.</typeparam>
    /// <param name="toDeserialize">Binary object to be deserialised.</param>
    /// <returns>Returns object.</returns>
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

    /// <summary> Length of the whole map,from outer wall to outer wall parallel to the spawn platforms </summary>
    public float MapLength()
    {
        return (sizeRows + 2) * hallWidth;
    }

    /// <summary> Width of the whole map,from outer wall to outer wall perpendicular to the spawn platforms </summary>
    public float MapWidth()
    {
        return (sizeCols + 2) * hallWidth;
    }

    /// <summary> Length of the maze,from inner wall to inner wall parallel to the spawn platforms </summary>
    public float MazeInnerLength()
    {
        return sizeRows * hallWidth;
    }

    /// <summary> Width of the maze,from inner wall to inner wall perpendicular to the spawn platforms </summary>
    public float MazeInnerWidth()
    {
        return sizeCols * hallWidth;
    }

    /// <summary> Area of maze from inner wall to inner wall </summary>
    /// <returns></returns>
    public float MazeInnerArea()
    {
        float l = MazeInnerLength();
        float w = MazeInnerWidth();
        float a = MazeInnerWidth() * MazeInnerLength();
        return MazeInnerLength() * MazeInnerWidth();
    }

    /// <summary> Get the world position of a given row/col in the maze </summary>
    public Vector3 Position(int row, int col)
    {
        float x = (col - (sizeCols - 1) / 2) * hallWidth;
        float y = 0f;
        float z = (row - (sizeRows - 1) / 2) * hallWidth;
        return new Vector3(x, y, z);
    }

    public Vector3 RandomEmptySpace()
    {
        System.Random r = new System.Random();

        List<Vector2Int> freeSpaces = new List<Vector2Int>();

        for (int i = 0; i < sizeRows; i++)
        {
            for (int j = 0; j < sizeCols; j++)
            {
                if (data[i, j] == 0)
                {
                    freeSpaces.Add(new Vector2Int(i, j));
                }
            }
        }

        Vector2Int space = freeSpaces[r.Next(freeSpaces.Count - 1)];

        return Position(space.x, space.y);
    }
}