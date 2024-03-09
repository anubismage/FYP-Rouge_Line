using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using System.Linq;

public enum DungeonState {inactive, generatingMain, generatingBranches, cleanup, completed }

public class DungeonGenerator : MonoBehaviour
{
    
    public GameObject[] tilePrefabs;
    public GameObject[] startPrefabs;
    public GameObject[] exitPrefabs;
    public GameObject[] blockedPrefabs;
    public GameObject[] RoomPrefabs;
    public GameObject[] HallPrefabs;
    [Header("Debugging Options")]
    public bool useBoxColliders;

    [Header("Keybinds")]
    public KeyCode reloadKey = KeyCode.Space;
    public KeyCode toggleMapKey = KeyCode.M;

    [Header("Generation Limits")]
    [Range(0, 1f)] public float cDelay;
    [Range(2, 100)]public int mainLenght = 10;
    [Range(0, 50)] public int branchLenght = 5;
    [Range(0, 25)] public int numBranches = 10;
    //[Range(0, 100)] public int doorPercent = 25;
    
    //runtimelist
    public DungeonState dungeonState = DungeonState.inactive;
    public List<Tile> generatedTiles = new List<Tile>();
    

    GameObject OverheadCam, goPlayer;
    List<Connector> availableConnectors = new List<Connector>();

    Transform tileFrom;
    Transform tileTo;
    Transform tileRoot;
    Transform container;
    int attempts;
    int maxAttempts = 50;
    public int seedNumber;
    public bool randomizeSeed;

    bool isTileRoom= true;
    void Start()
    {
        if (randomizeSeed)
        {
            seedNumber = Random.Range(0, 99999);
        }
        OverheadCam = GameObject.Find("OverheadCamera");
        
        goPlayer = GameObject.FindWithTag("Player");
        Random.InitState(seedNumber);
        StartCoroutine(DungeonBuild());
    }

    private void Update()
    {
        if (Input.GetKeyDown(reloadKey))
        {
            SceneManager.LoadScene("Game");
        }

        if (Input.GetKeyDown(toggleMapKey))
        {
            OverheadCam.SetActive(!OverheadCam.activeInHierarchy);
            goPlayer.SetActive(!goPlayer.activeInHierarchy);
        }
    }

    IEnumerator DungeonBuild()
    {
        OverheadCam.SetActive(true);
        goPlayer.SetActive(false);
        GameObject gContainer = new GameObject("Main Path");
        container = gContainer.transform;
        container.SetParent(transform);
        tileRoot = CreateStartTile();
        tileTo = tileRoot;
        dungeonState = DungeonState.generatingMain;
        //main tiles HERE
       while(generatedTiles.Count < mainLenght)
        {
            yield return new WaitForSeconds(cDelay);
            tileFrom = tileTo;
           
            if (generatedTiles.Count == mainLenght - 1)
            {
                //exit
                tileTo = CreateExitTile();
            }
            else
            {
                tileTo = CreateTile(isTileRoom);

            }
            ConnectTiles();
            CollisionCheck();
            isTileRoom = !isTileRoom;
        }
        //get all non connected connectors in the container
        foreach (Connector connector in container.GetComponentsInChildren<Connector>())
        {
            if(!connector.isConnected)
            {
                if (!availableConnectors.Contains(connector))
                {
                    availableConnectors.Add(connector);
                }
            }
        }
        //branches
        dungeonState = DungeonState.generatingBranches;
        for (int br = 0; br < numBranches; br++)
        {
            if (availableConnectors.Count > 0)
            {
                gContainer = new GameObject("Branch" + (br + 1));
                container = gContainer.transform;
                container.SetParent(transform);
                int availIndex = Random.Range(0, availableConnectors.Count);
                tileRoot = availableConnectors[availIndex].transform.parent.parent;
                availableConnectors.RemoveAt(availIndex);
                tileTo = tileRoot;
                for (int i = 0; i < branchLenght - 1; i++)
                {
                    yield return new WaitForSeconds(cDelay);
                    tileFrom = tileTo;
                    //garenteed room generation at end of branch
                    if (i == branchLenght - 2)
                    {
                        tileTo = CreateTile(true);
                        ConnectTiles();
                        CollisionCheck();
                    }
                    else// random tile
                    {
                        bool b = Random.value > 0.5f;
                        tileTo = CreateTile(b);
                        ConnectTiles();
                        CollisionCheck();
                    }
                    if (attempts >= maxAttempts) { break; }
                }
            }else { break; }
        }
        dungeonState = DungeonState.cleanup;
        CleanupBoxes();
        BlockedPaths();
        dungeonState = DungeonState.completed;
        yield return null;
        OverheadCam.SetActive(false);
        goPlayer.SetActive(true);
        GameManager.gameManager.playerloaded = true;
    }

    void BlockedPaths()
    {
        foreach(Connector connector in transform.GetComponentsInChildren<Connector>()){
            if (!connector.isConnected)
            {
                Vector3 pos = connector.transform.position;
                int wallIndex = Random.Range(0, blockedPrefabs.Length);
                GameObject goWall = Instantiate(blockedPrefabs[wallIndex], pos, connector.transform.rotation, connector.transform) as GameObject;
                goWall.name = blockedPrefabs[wallIndex].name;
            }
        }
    }

    void CollisionCheck()
    {
        BoxCollider box = tileTo.GetComponent<BoxCollider>();
        if(box == null)
        {
            box = tileTo.gameObject.AddComponent<BoxCollider>();
            box.isTrigger = true;
        }
        Vector3 offset = (tileTo.right * box.center.x) + (tileTo.up * box.center.y) + (tileTo.forward * box.center.z);
        Vector3 halfExtents = box.bounds.extents;
        List<Collider> hits = Physics.OverlapBox(tileTo.position + offset, halfExtents, Quaternion.identity, LayerMask.GetMask("Tiles")).ToList();
        if(hits.Count > 0)
        {
            if(hits.Exists(x => x.transform != tileFrom && x.transform != tileTo))
            {
                //hits something
                attempts++;
                int toIndex = generatedTiles.FindIndex(x => x.tile == tileTo);
                if(generatedTiles[toIndex].connector != null)  //check weather its the Start tile
                {
                    generatedTiles[toIndex].connector.isConnected = false;
                }
                generatedTiles.RemoveAt(toIndex);
                DestroyImmediate(tileTo.gameObject);
                //BACKTRACK
                if(attempts >= maxAttempts)
                {
                    int fromIndex = generatedTiles.FindIndex(x => x.tile == tileFrom);
                    Tile myTileFrom = generatedTiles[fromIndex];
                    if(tileFrom != tileRoot)
                    {
                        if(myTileFrom.connector != null)
                        {
                            myTileFrom.connector.isConnected = false;
                        }
                        availableConnectors.RemoveAll(x => x.transform.parent.parent == tileFrom);
                        generatedTiles.RemoveAt(fromIndex);
                        DestroyImmediate(tileFrom.gameObject);

                        if (myTileFrom.origin != tileRoot)
                        {
                            tileFrom = myTileFrom.origin;
                        }
                        else if (container.name.Contains("Main"))
                        {
                            if(myTileFrom.origin != null)
                            {
                                tileRoot = myTileFrom.origin;
                                tileFrom = tileRoot;
                            }
                        }
                        else if (availableConnectors.Count > 0) //check for branch
                        {
                            int availIndex = Random.Range(0, availableConnectors.Count);
                            tileRoot = availableConnectors[availIndex].transform.parent.parent;
                            availableConnectors.RemoveAt(availIndex);
                            tileFrom = tileRoot;
                        }
                        else { return; }

                    }
                    else if (container.name.Contains("Main"))
                    {
                        if(myTileFrom.origin != null)
                        {
                            tileRoot = myTileFrom.origin;
                            tileFrom = tileRoot;
                        }
                    }
                    else if (availableConnectors.Count > 0)
                    {
                        int availIndex = Random.Range(0, availableConnectors.Count);
                        tileRoot = availableConnectors[availIndex].transform.parent.parent;
                        availableConnectors.RemoveAt(availIndex);
                        tileFrom = tileRoot;

                    }
                    else { return;}
                }
                //retry
                if (tileFrom != null)
                {
                    if (generatedTiles.Count == mainLenght - 1)
                    {
                        //exit
                        tileTo = CreateExitTile();
                    }
                    else
                    {
                        tileTo = CreateTile(!isTileRoom);

                    }
                    ConnectTiles();
                    CollisionCheck();

                }
            }
            else
            {
                attempts = 0;
            }//no tiles other than tileto or from was hit 

        }
    }
    void CleanupBoxes()
    {
        if (!useBoxColliders)
        {
            foreach(Tile thisTile in generatedTiles)
            {
                BoxCollider box = thisTile.tile.GetComponent<BoxCollider>();
                if(box != null) { Destroy(box); }
            }
        }
    }
    void ConnectTiles()
    {
        Transform connectFrom = GetRandomConnector(tileFrom);
        if(connectFrom == null){return;}
        Transform connectTo = GetRandomConnector(tileTo);
        if (connectTo == null) { return; }

        //Rotational Parenting Connection method
        connectTo.SetParent(connectFrom);
        tileTo.SetParent(connectTo);
        connectTo.localPosition= Vector3.zero;
        connectTo.localRotation = Quaternion.identity;
        connectTo.Rotate(0, 180f, 0);
        tileTo.SetParent(container);
        connectTo.SetParent(tileTo.Find("Connectors"));
        generatedTiles.Last().connector = connectFrom.GetComponent<Connector>();
    }

    Transform GetRandomConnector(Transform tile)
    {
        if(tile == null) { return null; }
        List<Connector> connectorList =  tile.GetComponentsInChildren<Connector>().ToList().FindAll(x => x.isConnected == false);
        if(connectorList.Count > 0)
        {
            int connectorIndex = Random.Range(0, connectorList.Count);
            connectorList[connectorIndex].isConnected = true;
            if(tile == tileFrom)
            {
                BoxCollider box = tile.GetComponent<BoxCollider>();
                if(box == null)
                {
                    box = tile.gameObject.AddComponent<BoxCollider>();
                    box.isTrigger = true;
                }
            }
            return connectorList[connectorIndex].transform;
        }
        return null;
    }

    Transform CreateTile(bool isRoom)
    {
        if (isRoom)
        {
            int indexR = Random.Range(0, RoomPrefabs.Length);
            GameObject gTile = Instantiate(RoomPrefabs[indexR], Vector3.zero, Quaternion.identity, container) as GameObject;
            gTile.name = RoomPrefabs[indexR].name; //indexd
            Transform origin = generatedTiles[generatedTiles.FindIndex(x => x.tile == tileFrom)].tile; //sets origin tile to tile from(prev tile)
            generatedTiles.Add(new Tile(gTile.transform, origin));
            return gTile.transform;
        }
        else
        {
            int indexH = Random.Range(0, HallPrefabs.Length);
            GameObject gTile = Instantiate(HallPrefabs[indexH], Vector3.zero, Quaternion.identity, container) as GameObject;
            gTile.name = HallPrefabs[indexH].name;
            Transform origin = generatedTiles[generatedTiles.FindIndex(x => x.tile == tileFrom)].tile; //sets origin tile to tile from(prev tile)
            generatedTiles.Add(new Tile(gTile.transform, origin));
            return gTile.transform;
        }

    }
    Transform CreateExitTile()
    {
        int indexR = Random.Range(0, exitPrefabs.Length);
        GameObject gTile = Instantiate(exitPrefabs[indexR], Vector3.zero, Quaternion.identity, container) as GameObject;
        gTile.name = "Exit Room";
        Transform origin = generatedTiles[generatedTiles.FindIndex(x => x.tile == tileFrom)].tile; //sets origin tile to tile from(prev tile)
        generatedTiles.Add(new Tile(gTile.transform, origin));
        return gTile.transform;
    }

    Transform CreateStartTile()
    {
        int index = Random.Range(0, startPrefabs.Length);
        GameObject gTile = Instantiate(startPrefabs[index], Vector3.zero, Quaternion.identity, container) as GameObject;
        gTile.name = "Start Room";
        float yRot = Random.Range(0, 4) * 90f;
        gTile.transform.Rotate(0, yRot, 0);
        //SET PLAYER START LOOK ROTATION HERE
        generatedTiles.Add(new Tile(gTile.transform, null)); 
        return gTile.transform;
    }

}
