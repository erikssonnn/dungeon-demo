using System;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using UnityEditor;
using Logger = erikssonn.Logger;
using Random = UnityEngine.Random;

[CustomEditor(typeof(RoomGenerationController))]
public class RoomGenerationControllerEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        RoomGenerationController gc = (RoomGenerationController)target;

        if (GUILayout.Button("GENERATE")) {
            gc.Generate();
        }

        if (GUILayout.Button("CLEAR")) {
            gc.Clear();
        }

        EditorUtility.SetDirty(gc);
    }
}

[System.Serializable]
public class Tile {
    public enum TileType {
        NONE,
        CORRIDOR,
        ROOM,
        DOORWAY
    };

    public int taken;
    public GameObject obj;
    public TileType tileType;
    public bool filled = false;

    public Tile(int taken, GameObject obj, TileType tileType, bool filled) {
        this.taken = taken;
        this.obj = obj;
        this.tileType = tileType;
        this.filled = filled;
    }
}

public class RoomGenerationController : MonoBehaviour {
    [Header("ASSIGNABLE: ")]
    [Tooltip("Tile which the generator uses for its mesh generation. This can be any prefab, it doesn't have to be a basic plane.")]
    [SerializeField] private GameObject tile = null;

    [Tooltip("Material that the generator assigns to the created mesh")]
    [SerializeField] private Material mat = null;

    [Tooltip("Player object")]
    [SerializeField] private GameObject player = null;

    [Header("SETTINGS: ")]
    [Tooltip("Size of the generated dungeon in tiles for each axis. a map size of 16 creates a dungeon of 16x16")]
    [SerializeField] private int mapSize = 16;

    [Tooltip("Amount of rooms the generator aims at generating. A high amount means more open areas and rooms being generated")]
    [SerializeField] private int roomGoal = 8;

    [Tooltip("Smooths the generation, a higher value is slower but removes unwanted parts of the dungeon")]
    [SerializeField] private int smoothness = 2;

    [Tooltip("Max size of the generated rooms. It uses the following algorithm: 'roomSize = 1 + (value * 2)', where value is random between 0 and maxRoomRadius")]
    [SerializeField] private int maxRoomRadius = 2;

    [Tooltip("Scales the whole mesh of the dungeon")]
    [SerializeField] private float meshScaling = 3;

    [Tooltip("Scales the height of the walls in the dungeon")]
    [SerializeField] private float dungeonHeight = 1;

    [Header("DEBUG: ")]
    [Tooltip("Generate the dungeon on game start")]
    [SerializeField] private bool generateOnStart = false;

    [Tooltip("Draw gizmos of the generated dungeon in the scene-view, where each piece of the dungeon is shown as a color")]
    [SerializeField] private bool drawGizmos = false;

    private Tile[,] grid = null;
    private int retries = 0;
    [SerializeField] private List<Vector3> eligibleSpawnPositions = new List<Vector3>();

    public int GetMapSize() => mapSize;
    public List<Vector3> GetEligiblePositions() {
        Logger.Print("eligibleSpawnPositions: " + eligibleSpawnPositions.Count);
        return eligibleSpawnPositions;
    }

    private void OnDrawGizmos() {
        if (drawGizmos) {
            Gizmos.DrawCube(transform.position + new Vector3(Mathf.RoundToInt(mapSize / 2) - 0.5f,
                -1, Mathf.RoundToInt(mapSize / 2) - 0.5f) * meshScaling, new Vector3(mapSize, 0, mapSize) * meshScaling);

            if (grid != null && grid.GetLength(0) > 0) {
                for (int x = 0; x < grid.GetLength(0); x++) {
                    for (int y = 0; y < grid.GetLength(1); y++) {
                        if (grid[x, y].tileType == Tile.TileType.NONE) continue;
                        if (grid[x, y].tileType == Tile.TileType.ROOM) Gizmos.color = Color.red;
                        if (grid[x, y].tileType == Tile.TileType.CORRIDOR) Gizmos.color = Color.blue;
                        if (grid[x, y].tileType == Tile.TileType.DOORWAY) Gizmos.color = Color.green;
                        Gizmos.DrawCube(new Vector3(x * meshScaling, 1, y * meshScaling), Vector3.one * meshScaling);
                    }
                }
            }

            // if (eligibleSpawnPositions.Count <= 0) {
            //     return;
            // }
            // Gizmos.color = Color.magenta;
            // for (int i = 0; i < eligibleSpawnPositions.Count; i++) {
            //     Gizmos.DrawSphere(eligibleSpawnPositions[i], 0.1f);
            // }
        }
    }

    private void Start() {
        if (generateOnStart) Generate();
    }

    public void Generate() {
        ScaleMap(1f);
        Clear();
        CreateGrid();

        //limit room goal if higher than mapsize
        if (roomGoal > grid.GetLength(0) * grid.GetLength(1)) {
            roomGoal = (grid.GetLength(0) * grid.GetLength(1));
        }

        GenerateRooms();
        GenerateCorridors();
        GenerateDoorways();
        Cleanup();
        FloodFill();
        GenerateFloorMesh();
        GenerateWallMesh();
        MergeMesh();
        ScaleMap(meshScaling);
        SpawnPlayer();
    }

    public void Clear() {
        eligibleSpawnPositions.Clear();
        highestSection = 0;
        if (transform.childCount > 0) {
            for (int i = transform.childCount - 1; i >= 0; i--) {
                Transform child = transform.GetChild(i);
                DestroyImmediate(child.gameObject);
            }
        }

        ClearLog();
    }

    private static void ClearLog() {
        Assembly assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
        Type type = assembly.GetType("UnityEditor.LogEntries");
        MethodInfo method = type.GetMethod("Clear");
        method?.Invoke(new object(), null);
    }

    private void CreateGrid() {
        grid = new Tile[mapSize, mapSize];

        for (int x = 0; x < grid.GetLength(0); x++) {
            for (int y = 0; y < grid.GetLength(1); y++) {
                grid[x, y] = new Tile(0, null, Tile.TileType.NONE, false);
            }
        }
    }

    private void SpawnPlayer() {
        Logger.Print("Spawn player; " + eligibleSpawnPositions.Count);
        player.transform.position = eligibleSpawnPositions[Random.Range(0, eligibleSpawnPositions.Count - 1)] + new Vector3(0, 1, 0);
        FindObjectOfType<MonsterSpawnerController>().SetSpawnPositions();
    }

    private void MergeMesh() {
        List<MeshRenderer> allMeshes = new List<MeshRenderer>();
        if (transform.childCount > 0) {
            for (int i = transform.childCount - 1; i >= 0; i--) {
                allMeshes.Add(transform.GetChild(i).GetComponent<MeshRenderer>());
            }
        }

        Mesh finalMesh = new Mesh();
        CombineInstance[] combineInstance = new CombineInstance[allMeshes.Count];
        for (int i = 0; i < allMeshes.Count; i++) {
            combineInstance[i].mesh = allMeshes[i].GetComponent<MeshFilter>().sharedMesh;
            combineInstance[i].transform = allMeshes[i].transform.localToWorldMatrix;
        }

        finalMesh.CombineMeshes(combineInstance);
        GameObject newObject = new GameObject("mesh");
        MeshFilter filter = newObject.AddComponent<MeshFilter>();
        filter.sharedMesh = finalMesh;
        MeshRenderer ren = newObject.AddComponent<MeshRenderer>();
        ren.sharedMaterial = mat;
        newObject.AddComponent<MeshCollider>();

        for (int i = transform.childCount - 1; i >= 0; i--) {
            Transform child = transform.GetChild(i);
            DestroyImmediate(child.gameObject);
        }

        newObject.transform.SetParent(transform, false);
    }

    private void GenerateWallMesh() {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector3> normals = new List<Vector3>();

        for (int x = 0; x < grid.GetLength(0); x++) {
            for (int y = 0; y < grid.GetLength(1); y++) {
                if (grid[x, y].taken != 1) continue;
                //top neighbor
                if (y < grid.GetLength(1) - 1 && grid[x, y + 1].taken == 0) {
                    Vector3 topLeft = new Vector3(x, dungeonHeight, y + 1);
                    Vector3 topRight = new Vector3(x + 1, dungeonHeight, y + 1);
                    Vector3 bottomLeft = new Vector3(x, 0, y + 1);
                    Vector3 bottomRight = new Vector3(x + 1, 0, y + 1);
                    int startIndex = vertices.Count;
                    vertices.Add(topLeft);
                    vertices.Add(topRight);
                    vertices.Add(bottomLeft);
                    vertices.Add(bottomRight);
                    triangles.Add(startIndex);
                    triangles.Add(startIndex + 1);
                    triangles.Add(startIndex + 2);
                    triangles.Add(startIndex + 1);
                    triangles.Add(startIndex + 3);
                    triangles.Add(startIndex + 2);
                }

                //right neighbor
                if (x < grid.GetLength(0) - 1 && grid[x + 1, y].taken == 0) {
                    Vector3 topLeft = new Vector3(x + 1, dungeonHeight, y + 1);
                    Vector3 topRight = new Vector3(x + 1, dungeonHeight, y);
                    Vector3 bottomLeft = new Vector3(x + 1, 0, y + 1);
                    Vector3 bottomRight = new Vector3(x + 1, 0, y);
                    int startIndex = vertices.Count;
                    vertices.Add(topLeft);
                    vertices.Add(topRight);
                    vertices.Add(bottomLeft);
                    vertices.Add(bottomRight);
                    triangles.Add(startIndex);
                    triangles.Add(startIndex + 1);
                    triangles.Add(startIndex + 2);
                    triangles.Add(startIndex + 1);
                    triangles.Add(startIndex + 3);
                    triangles.Add(startIndex + 2);
                }

                //bottom neighbor
                if (y > 0 && grid[x, y - 1].taken == 0) {
                    Vector3 topLeft = new Vector3(x + 1, dungeonHeight, y);
                    Vector3 topRight = new Vector3(x, dungeonHeight, y);
                    Vector3 bottomLeft = new Vector3(x + 1, 0, y);
                    Vector3 bottomRight = new Vector3(x, 0, y);
                    int startIndex = vertices.Count;
                    vertices.Add(topLeft);
                    vertices.Add(topRight);
                    vertices.Add(bottomLeft);
                    vertices.Add(bottomRight);
                    triangles.Add(startIndex);
                    triangles.Add(startIndex + 1);
                    triangles.Add(startIndex + 2);
                    triangles.Add(startIndex + 1);
                    triangles.Add(startIndex + 3);
                    triangles.Add(startIndex + 2);
                }

                //left neighbor
                if (x > 0 && grid[x - 1, y].taken == 0) {
                    Vector3 topLeft = new Vector3(x, dungeonHeight, y);
                    Vector3 topRight = new Vector3(x, dungeonHeight, y + 1);
                    Vector3 bottomLeft = new Vector3(x, 0, y);
                    Vector3 bottomRight = new Vector3(x, 0, y + 1);
                    int startIndex = vertices.Count;
                    vertices.Add(topLeft);
                    vertices.Add(topRight);
                    vertices.Add(bottomLeft);
                    vertices.Add(bottomRight);
                    triangles.Add(startIndex);
                    triangles.Add(startIndex + 1);
                    triangles.Add(startIndex + 2);
                    triangles.Add(startIndex + 1);
                    triangles.Add(startIndex + 3);
                    triangles.Add(startIndex + 2);
                }

                //check end of grid too
                if (y + 1 >= grid.GetLength(1)) {
                    Vector3 topLeft = new Vector3(x, dungeonHeight, y + 1);
                    Vector3 topRight = new Vector3(x + 1, dungeonHeight, y + 1);
                    Vector3 bottomLeft = new Vector3(x, 0, y + 1);
                    Vector3 bottomRight = new Vector3(x + 1, 0, y + 1);
                    int startIndex = vertices.Count;
                    vertices.Add(topLeft);
                    vertices.Add(topRight);
                    vertices.Add(bottomLeft);
                    vertices.Add(bottomRight);
                    triangles.Add(startIndex);
                    triangles.Add(startIndex + 1);
                    triangles.Add(startIndex + 2);
                    triangles.Add(startIndex + 1);
                    triangles.Add(startIndex + 3);
                    triangles.Add(startIndex + 2);
                }

                if (x + 1 >= grid.GetLength(0)) {
                    Vector3 topLeft = new Vector3(x + 1, dungeonHeight, y + 1);
                    Vector3 topRight = new Vector3(x + 1, dungeonHeight, y);
                    Vector3 bottomLeft = new Vector3(x + 1, 0, y + 1);
                    Vector3 bottomRight = new Vector3(x + 1, 0, y);
                    int startIndex = vertices.Count;
                    vertices.Add(topLeft);
                    vertices.Add(topRight);
                    vertices.Add(bottomLeft);
                    vertices.Add(bottomRight);
                    triangles.Add(startIndex);
                    triangles.Add(startIndex + 1);
                    triangles.Add(startIndex + 2);
                    triangles.Add(startIndex + 1);
                    triangles.Add(startIndex + 3);
                    triangles.Add(startIndex + 2);
                }

                if (y - 1 < 0) {
                    Vector3 topLeft = new Vector3(x + 1, dungeonHeight, y);
                    Vector3 topRight = new Vector3(x, dungeonHeight, y);
                    Vector3 bottomLeft = new Vector3(x + 1, 0, y);
                    Vector3 bottomRight = new Vector3(x, 0, y);
                    int startIndex = vertices.Count;
                    vertices.Add(topLeft);
                    vertices.Add(topRight);
                    vertices.Add(bottomLeft);
                    vertices.Add(bottomRight);
                    triangles.Add(startIndex);
                    triangles.Add(startIndex + 1);
                    triangles.Add(startIndex + 2);
                    triangles.Add(startIndex + 1);
                    triangles.Add(startIndex + 3);
                    triangles.Add(startIndex + 2);
                }

                if (x - 1 < 0) {
                    Vector3 topLeft = new Vector3(x, dungeonHeight, y);
                    Vector3 topRight = new Vector3(x, dungeonHeight, y + 1);
                    Vector3 bottomLeft = new Vector3(x, 0, y);
                    Vector3 bottomRight = new Vector3(x, 0, y + 1);
                    int startIndex = vertices.Count;
                    vertices.Add(topLeft);
                    vertices.Add(topRight);
                    vertices.Add(bottomLeft);
                    vertices.Add(bottomRight);
                    triangles.Add(startIndex);
                    triangles.Add(startIndex + 1);
                    triangles.Add(startIndex + 2);
                    triangles.Add(startIndex + 1);
                    triangles.Add(startIndex + 3);
                    triangles.Add(startIndex + 2);
                }
            }
        }

        Vector2[] uvs = new Vector2[vertices.Count];

        for (int i = 0; i < triangles.Count; i += 3) {
            Vector3 v1 = vertices[triangles[i]];
            Vector3 v2 = vertices[triangles[i + 1]];
            Vector3 v3 = vertices[triangles[i + 2]];

            Vector3 normal = Vector3.Cross(v2 - v1, v3 - v1).normalized;
            normals.Add(normal);

            if (normal == Vector3.forward || normal == -Vector3.forward) {
                uvs[triangles[i]] = new Vector2(v1.x, v1.y);
                uvs[triangles[i + 1]] = new Vector2(v2.x, v2.y);
                uvs[triangles[i + 2]] = new Vector2(v3.x, v3.y);
            } else if (normal == Vector3.right || normal == -Vector3.right) {
                uvs[triangles[i]] = new Vector2(v1.z, v1.y);
                uvs[triangles[i + 1]] = new Vector2(v2.z, v2.y);
                uvs[triangles[i + 2]] = new Vector2(v3.z, v3.y);
            } else if (normal == Vector3.up || normal == -Vector3.up) {
                uvs[triangles[i]] = new Vector2(v1.x, v1.z);
                uvs[triangles[i + 1]] = new Vector2(v2.x, v2.z);
                uvs[triangles[i + 2]] = new Vector2(v3.x, v3.z);
            }
        }

        if (normals.Count < vertices.Count) {
            for (int i = normals.Count; i < vertices.Count; i++) {
                normals.Add(Vector3.zero);
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.normals = normals.ToArray();
        mesh.uv = uvs;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        GameObject newObject = new GameObject("WallMesh");
        MeshFilter meshFilter = newObject.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;
        MeshRenderer meshRenderer = newObject.AddComponent<MeshRenderer>();
        meshRenderer.material = mat;
        newObject.transform.SetParent(transform, false);
        newObject.transform.position += new Vector3(-0.5f, 0.0f, -0.5f);
    }

    private void GenerateFloorMesh() {
        for (int x = 0; x < grid.GetLength(0); x++) {
            for (int y = 0; y < grid.GetLength(1); y++) {
                if (grid[x, y].taken != 1) continue;
                GameObject newTile = Instantiate(tile, transform, false) as GameObject;
                newTile.transform.position = new Vector3(x, 0, y);
                eligibleSpawnPositions.Add(new Vector3(x, 0, y));
                grid[x, y].obj = newTile;
            }
        }
    }

    private void GenerateRooms() {
        for (int i = 0; i < roomGoal; i++) {
            GenerateRoom();
        }
    }

    private void GenerateRoom() {
        int roomSize = Random.Range(1, maxRoomRadius + 1);
        Vector2Int pos = new Vector2Int(Random.Range(roomSize, mapSize - roomSize), Random.Range(roomSize, mapSize - roomSize));
        bool canPlace = true;

        for (int x = pos.x - roomSize; x <= pos.x + roomSize; x++) {
            for (int y = pos.y - roomSize; y <= pos.y + roomSize; y++) {
                if (CanPlaceTile(new Vector2Int(x, y))) continue;
                canPlace = false;
                break;
            }

            if (!canPlace) {
                break;
            }
        }

        if (canPlace) {
            for (int x = pos.x - roomSize; x <= pos.x + roomSize; x++) {
                for (int y = pos.y - roomSize; y <= pos.y + roomSize; y++) {
                    grid[x, y].taken = 1;
                    grid[x, y].tileType = Tile.TileType.ROOM;
                }
            }
        }

        retries++;
        if (retries < 10) {
            GenerateRoom();
            return;
        }

        retries = 0;
    }

    private bool CanPlaceTile(Vector2Int position) {
        return grid[position.x, position.y].taken == 0;
    }

    private int NeighbourCount(Vector2Int pos) {
        int amount = 0;

        if (pos.x - 1 < 0 || pos.x + 1 >= grid.GetLength(0) ||
            pos.y - 1 < 0 || pos.y + 1 >= grid.GetLength(1)) {
            return 4;
        }

        if (grid[pos.x - 1, pos.y].taken == 1) {
            amount++;
        }

        if (grid[pos.x + 1, pos.y].taken == 1) {
            amount++;
        }

        if (grid[pos.x, pos.y - 1].taken == 1) {
            amount++;
        }

        if (grid[pos.x, pos.y + 1].taken == 1) {
            amount++;
        }

        return amount;
    }

    private static int NeighbourCountVisited(Vector2Int pos, bool[,] visited) {
        int amount = 0;

        if (pos.x - 1 < 0 || pos.x + 1 >= visited.GetLength(0) ||
            pos.y - 1 < 0 || pos.y + 1 >= visited.GetLength(1)) {
            return 4;
        }

        if (visited[pos.x - 1, pos.y] == true) {
            amount++;
        }

        if (visited[pos.x + 1, pos.y] == true) {
            amount++;
        }

        if (visited[pos.x, pos.y - 1] == true) {
            amount++;
        }

        if (visited[pos.x, pos.y + 1] == true) {
            amount++;
        }

        return amount;
    }

    private void GenerateCorridors() {
        for (int x = 1; x < mapSize - 1; x++) {
            for (int y = 1; y < mapSize - 1; y++) {
                if (grid[x, y].taken == 0 && NeighbourCount(new Vector2Int(x, y)) == 0) {
                    GenerateCorridor(new Vector2Int(x, y));
                }
            }
        }
    }

    private void GenerateCorridor(Vector2Int startPos) {
        bool[,] visited = new bool[mapSize, mapSize];
        int[,] path = new int[mapSize, mapSize];

        for (int x = 0; x < path.GetLength(0); x++) {
            for (int y = 0; y < path.GetLength(1); y++) {
                path[x, y] = 0;
            }
        }

        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        stack.Push(startPos);

        while (stack.Count > 0) {
            Vector2Int pos = stack.Pop();
            if (visited[pos.x, pos.y]) {
                continue;
            }

            if (NeighbourCountVisited(new Vector2Int(pos.x, pos.y), visited) > 1) {
                continue;
            }

            visited[pos.x, pos.y] = true;
            path[pos.x, pos.y] = 1;
            if (pos.x == 0 || pos.x == mapSize - 1 || pos.y == 0 || pos.y == mapSize - 1) {
                break;
            }

            if (grid[pos.x - 1, pos.y].taken == 0 && NeighbourCount(new Vector2Int(pos.x - 1, pos.y)) == 0 &&
                NeighbourCountVisited(new Vector2Int(pos.x - 1, pos.y), visited) <= 1) {
                stack.Push(new Vector2Int(pos.x - 1, pos.y));
            }

            if (grid[pos.x + 1, pos.y].taken == 0 && NeighbourCount(new Vector2Int(pos.x + 1, pos.y)) == 0 &&
                NeighbourCountVisited(new Vector2Int(pos.x + 1, pos.y), visited) <= 1) {
                stack.Push(new Vector2Int(pos.x + 1, pos.y));
            }

            if (grid[pos.x, pos.y - 1].taken == 0 && NeighbourCount(new Vector2Int(pos.x, pos.y - 1)) == 0 &&
                NeighbourCountVisited(new Vector2Int(pos.x, pos.y - 1), visited) <= 1) {
                stack.Push(new Vector2Int(pos.x, pos.y - 1));
            }

            if (grid[pos.x, pos.y + 1].taken == 0 && NeighbourCount(new Vector2Int(pos.x, pos.y + 1)) == 0 &&
                NeighbourCountVisited(new Vector2Int(pos.x, pos.y + 1), visited) <= 1) {
                stack.Push(new Vector2Int(pos.x, pos.y + 1));
            }
        }

        if (path.GetLength(0) > 0) {
            for (int x = 0; x < path.GetLength(0); x++) {
                for (int y = 0; y < path.GetLength(1); y++) {
                    if (path[x, y] != 1) continue;
                    grid[x, y].taken = 1;
                    grid[x, y].tileType = Tile.TileType.CORRIDOR;
                }
            }
        }
    }

    private void GenerateDoorways() {
        for (int x = 0; x < grid.GetLength(0); x++) {
            for (int y = 0; y < grid.GetLength(1); y++) {
                if (x - 1 < 0 || x + 1 >= grid.GetLength(0) ||
                    y - 1 < 0 || y + 1 >= grid.GetLength(1)) {
                    continue;
                }

                if (grid[x, y].taken == 0 && (((grid[x - 1, y].taken == 1 && grid[x - 1, y].tileType != Tile.TileType.DOORWAY) &&
                                               (grid[x + 1, y].taken == 1 && grid[x + 1, y].tileType != Tile.TileType.DOORWAY)) ||
                                              ((grid[x, y - 1].taken == 1 && grid[x, y - 1].tileType != Tile.TileType.DOORWAY) &&
                                               (grid[x, y + 1].taken == 1 && grid[x, y + 1].tileType != Tile.TileType.DOORWAY))) && NeighbourCount(new Vector2Int(x, y)) == 2) {
                    grid[x, y].taken = 1;
                    grid[x, y].tileType = Tile.TileType.DOORWAY;
                }
            }
        }
    }

    private void Cleanup() {
        //make a copy of grid
        Tile[,] newGrid = new Tile[grid.GetLength(0), grid.GetLength(1)];
        for (int i = 0; i < grid.GetLength(0); i++) {
            for (int k = 0; k < grid.GetLength(1); k++) {
                newGrid[i, k] = new Tile(grid[i, k].taken, grid[i, k].obj, grid[i, k].tileType, false);
            }
        }

        //checks and replace newGrids tiles
        for (int i = 0; i < smoothness; i++) {
            for (int x = 0; x < grid.GetLength(0); x++) {
                for (int y = 0; y < grid.GetLength(1); y++) {
                    if (x - 1 < 0 || x + 1 >= grid.GetLength(0) ||
                        y - 1 < 0 || y + 1 >= grid.GetLength(1)) {
                        continue;
                    }

                    if (grid[x, y].taken == 0) {
                        continue;
                    }

                    //Remove floating tiles and dead ends
                    if (NeighbourCount(new Vector2Int(x, y)) <= 1 &&
                        (grid[x, y].tileType == Tile.TileType.DOORWAY || grid[x, y].tileType == Tile.TileType.CORRIDOR)) {
                        RemoveTile(new Vector2Int(x, y), newGrid);
                    }

                    if (x - 2 < 0 || x + 2 >= grid.GetLength(0) ||
                        y - 2 < 0 || y + 2 >= grid.GetLength(1)) {
                        continue;
                    }

                    //remove doorways too close to eachother
                    if (grid[x, y].taken == 1 && grid[x, y].tileType == Tile.TileType.DOORWAY) {
                        if ((grid[x - 2, y].taken == 1 && grid[x - 2, y].tileType == Tile.TileType.DOORWAY)) {
                            RemoveTile(new Vector2Int(x - 2, y), newGrid);
                        }

                        if ((grid[x + 2, y].taken == 1 && grid[x + 2, y].tileType == Tile.TileType.DOORWAY)) {
                            RemoveTile(new Vector2Int(x + 2, y), newGrid);
                        }

                        if ((grid[x, y - 2].taken == 1 && grid[x, y - 2].tileType == Tile.TileType.DOORWAY)) {
                            RemoveTile(new Vector2Int(x, y - 2), newGrid);
                        }

                        if ((grid[x, y + 2].taken == 1 && grid[x, y + 2].tileType == Tile.TileType.DOORWAY)) {
                            RemoveTile(new Vector2Int(x, y + 2), newGrid);
                        }
                    }

                    //remove random
                    if (grid[x, y].taken == 1 && grid[x, y].tileType == Tile.TileType.DOORWAY) {
                        if (Random.value < 0.45f) {
                            RemoveTile(new Vector2Int(x, y), newGrid);
                        }
                    }
                }
            }
        }

        //overwrite grid with newGrid
        for (int x = 0; x < grid.GetLength(0); x++) {
            for (int y = 0; y < grid.GetLength(1); y++) {
                grid[x, y] = new Tile(newGrid[x, y].taken, newGrid[x, y].obj, newGrid[x, y].tileType, false);
            }
        }
    }

    private void RemoveTile(Vector2Int pos, Tile[,] newGrid) {
        newGrid[pos.x, pos.y].taken = 0;
        newGrid[pos.x, pos.y].tileType = Tile.TileType.NONE;
        GameObject g = newGrid[pos.x, pos.y].obj;
        newGrid[pos.x, pos.y].obj = null;
        if (g != null) DestroyImmediate(g);
    }

    private void ScaleMap(float factor) {
        transform.transform.localScale = new Vector3(factor, factor, factor);

        //since we scale up the map, we also have to scale up the spawnPositions with the same factor
        for (int i = 0; i < eligibleSpawnPositions.Count; i++) {
            eligibleSpawnPositions[i] *= factor;
        }
    }

    #region FLOOD_FILL

    private Tile[,] highestGrid = null;
    private int highestSection = 0;

    private void FloodFill() {
        //create highest grid
        highestGrid = new Tile[grid.GetLength(0), grid.GetLength(1)];
        for (int x = 0; x < grid.GetLength(0); x++) {
            for (int y = 0; y < grid.GetLength(1); y++) {
                highestGrid[x, y] = new Tile(0, null, Tile.TileType.NONE, false);
            }
        }

        //flood fill
        for (int x = 0; x < grid.GetLength(0); x++) {
            for (int y = 0; y < grid.GetLength(1); y++) {
                if (grid[x, y].taken == 1 && !grid[x, y].filled)
                    FloodNode(x, y);
            }
        }

        //replace current grid with highest grid from floodfill
        for (int x = 0; x < grid.GetLength(0); x++) {
            for (int y = 0; y < grid.GetLength(1); y++) {
                grid[x, y] = new Tile(highestGrid[x, y].taken, highestGrid[x, y].obj, highestGrid[x, y].tileType, highestGrid[x, y].filled);
            }
        }
    }

    private void FloodNode(int x, int y) {
        Stack<Vector2Int> q = new Stack<Vector2Int>();
        q.Push(new Vector2Int(x, y));

        //create current grid
        Tile[,] currentGrid = new Tile[grid.GetLength(0), grid.GetLength(1)];
        ;
        int currentSection = 0;
        for (int i = 0; i < grid.GetLength(0); i++) {
            for (int k = 0; k < grid.GetLength(1); k++) {
                currentGrid[i, k] = new Tile(0, null, Tile.TileType.NONE, false);
            }
        }

        while (q.Count > 0) {
            Vector2Int n = q.Pop();
            if (grid[n.x, n.y].taken != 1 || grid[n.x, n.y].filled) continue;
            grid[n.x, n.y].filled = true;
            currentSection++;
            currentGrid[n.x, n.y] = new Tile(grid[n.x, n.y].taken, null, grid[n.x, n.y].tileType, grid[n.x, n.y].filled);
            if (n.x + 1 < grid.GetLength(0)) q.Push(new Vector2Int(n.x + 1, n.y));
            if (n.x - 1 > 0) q.Push(new Vector2Int(n.x - 1, n.y));
            if (n.y + 1 < grid.GetLength(1)) q.Push(new Vector2Int(n.x, n.y + 1));
            if (n.y - 1 > 0) q.Push(new Vector2Int(n.x, n.y - 1));
        }

        //replace the highestSection with the new highest
        if (currentSection > highestSection) {
            highestSection = currentSection;
            for (int i = 0; i < currentGrid.GetLength(0); i++) {
                for (int k = 0; k < currentGrid.GetLength(1); k++) {
                    highestGrid[i, k] = new Tile(currentGrid[i, k].taken, null, currentGrid[i, k].tileType, currentGrid[i, k].filled);
                }
            }
        }

        return;
    }

    #endregion
}