using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spin : MonoBehaviour
{
    float speed;
    private void Start()
    {

        speed = GetComponentInParent<Enemy>().AttackSpeed;
        
    }
    void Update()
    {
        transform.Rotate(0f, speed * Time.deltaTime, 0f, Space.Self);
    }
}
