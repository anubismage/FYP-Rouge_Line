using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(TimeActive());
    }


    IEnumerator TimeActive()
    {
        

        //yield on a new YieldInstruction that waits for 5 seconds.
        yield return new WaitForSeconds(1);
      
        this.gameObject.SetActive(false);
        Debug.Log(this.gameObject.name);


    }
}
