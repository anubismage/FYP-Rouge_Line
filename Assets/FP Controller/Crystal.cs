using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crystal : MonoBehaviour
{
    // Start is called before the first frame update
    

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {


        if(other.gameObject.tag == "Player")
        {
            GameManager.gameManager.RestoreHealth(10);
            Destroy(this.gameObject);
        }
    }
}
