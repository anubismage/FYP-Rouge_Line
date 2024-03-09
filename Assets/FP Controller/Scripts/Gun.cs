using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    PlayerMovement _pm;
   // [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject ExitPoint;

    public float bulletspeed= 600;
    // Start is called before the first frame update
    void Start()
    {
        _pm = transform.root.GetComponent<PlayerMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_pm.Shoot)
        {
            OnShoot();
            _pm.Shoot = false;
        }
    }

    void OnShoot()
    {
        Debug.Log("shot");
        //GameObject bullet = Instantiate(bulletPrefab, ExitPoint.transform.position, transform.rotation);
        GameObject bullet = ObjectPool.instance.GetPooledObject();
        if (bullet == null)
        {
            return;
        }
        
        bullet.transform.position = ExitPoint.transform.position;
        bullet.transform.rotation = ExitPoint.transform.rotation;
        bullet.SetActive(true);
        bullet.GetComponent<Rigidbody>().velocity = ExitPoint.transform.forward * bulletspeed;
       // bullet.GetComponent<Rigidbody>().AddForce(ExitPoint.transform. * bulletspeed);
        
    }

    private void OnTriggerExit(Collider other)
    {
        //Debug.Log(other.name);
        if (other.tag == "bullet")
        {
            
            other.gameObject.SetActive(false);
        }
    }

}
