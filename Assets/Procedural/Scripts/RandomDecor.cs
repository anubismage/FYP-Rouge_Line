using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using NavmeshBuilder = UnityEngine.AI.NavMeshBuilder;


public class RandomDecor : MonoBehaviour
{
    [SerializeField] GameObject[] decorPrefabs;

     
    DungeonGenerator myGenerator;
    bool isCompleted;
    GameObject go;
    // Start is called before the first frame update
    void Start()
    {
        go = GameObject.FindGameObjectWithTag("gen");
        myGenerator = go.GetComponent<DungeonGenerator>();
        Random.InitState(myGenerator.seedNumber);
    }

    // Update is called once per frame
    void Update()
    {
        
        if (!isCompleted && myGenerator.dungeonState == DungeonState.completed)
        {
            int s = Mathf.FloorToInt(Vector3.Distance(transform.position, go.transform.GetChild(0).position));         
            isCompleted = true;
            int dIndex = Random.Range(0, decorPrefabs.Length);
            GameObject goDecor = Instantiate(decorPrefabs[dIndex], transform.position, transform.rotation, transform) as GameObject;
            //decorPrefabs[dIndex]
            transform.name = transform.name+","+s.ToString();
        }
    }

}
