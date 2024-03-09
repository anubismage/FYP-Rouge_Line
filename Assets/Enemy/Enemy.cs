using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Globalization;
public class Enemy : MonoBehaviour
{
    int starthealth = 5;
    public HealthSystem enemyHealth = new HealthSystem(5, 5);
    public float AttackSpeed = 120f;
    public GameObject[] Top;
    public GameObject[] Bottom;
    public GameObject[] Attacks;
    public NavMeshAgent enemy;
    public GameObject DropPrefab;
    public Transform DropPoint;


    public GameObject Player;
    private Bounds bndfloor;
    private Vector3 moveto;

    private Vector3 Destination;
    public int TopIndex, BottomIndex;

    public float RoamRange = 30f;
    public bool chasing = false;
    private float timer = 0f;
    public float fireRate = 1f;
    int x = 0;
    bool preDeterminded = false;

    int dif1, dif2, dif3;
    // Start is called before the first frame update
    void Start()
    {
        enemy = this.GetComponent<NavMeshAgent>();
        TopIndex = Random.Range(0, Top.Length);
        GenerateParts(); //Calling function for Iniatate parts of the Enemy
        preDeterminded = GameManager.gameManager.getPDiff();
        //taking Difficulty parameters from gamemanger 
        dif1 = GameManager.gameManager.Difficulty1;
        dif2 = GameManager.gameManager.Difficulty2;
        dif3 = GameManager.gameManager.Difficulty3;
    }

    void Update()
    {
        Death();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        
        if (x == 0) 
        { 
            Player = GameObject.FindWithTag("Player");
            ++x;

        }
        //int.Parse(name[1], CultureInfo.InvariantCulture.NumberFormat)
        if (GameManager.gameManager.Points >= dif1 && GameManager.gameManager.Points < dif2 && x == 1)
        {
            ++x; 
            BuffStats();
        }
        else if(GameManager.gameManager.Points >= dif2 && GameManager.gameManager.Points < dif3 && x==2)
        {
            ++x;
            BuffStats();
        }
        else if (GameManager.gameManager.Points > dif3 && x==3)
        {
            ++x;
            BuffStats();
        }


    }

    void GenerateParts()
    {
        GameObject tempgo;
        if (preDeterminded)
        {
            string[] Tname = transform.parent.parent.name.Split(",");
            int i = int.Parse(Tname[1], CultureInfo.InvariantCulture.NumberFormat);
            Debug.Log(BottomIndex);
            if (i > 50 && i <= 100)
            {
                enemyHealth.MaxHealth = 10;
                enemyHealth.Health = 10;
                BottomIndex = 2;
            }
            else if (i > 100)
            {
                enemyHealth.MaxHealth = 10;
                enemyHealth.Health = 10;
                BottomIndex = 2;
            }
        }
        tempgo = (GameObject)Top.GetValue(TopIndex);
        tempgo.SetActive(true);
        tempgo = (GameObject)Bottom.GetValue(BottomIndex);
        tempgo.SetActive(true);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "bullet")
        {
            enemyHealth.Damage(5);
            collision.gameObject.SetActive(false);
            
        }
    }

    public void moveToPlayer()
    {
        chasing = true;
        enemy.SetDestination(Player.transform.position);
        
       
    }

    public void BuffStats()
    {
        if (BottomIndex<3)
        { 
            GameObject go;
            go = (GameObject)Bottom.GetValue(BottomIndex);
            go.SetActive(false);
            BottomIndex = BottomIndex + 1;
            go = (GameObject)Bottom.GetValue(BottomIndex);
            go.SetActive(true);
            starthealth = starthealth + 5;
            enemyHealth.MaxHealth = starthealth;
            enemyHealth.Health = starthealth;
        }
        

    }

    public void Roam()
    {
        enemy.SetDestination(GetRandomPoint());
    }

   public Vector3 GetRandomPoint()
    {
       // Vector3 result;
        for (int i = 0; i < 30; i++)
        {
            Vector3 randomPoint = gameObject.transform.position + Random.insideUnitSphere * RoamRange;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
            {
                Debug.Log(hit.position);
                return hit.position;
                
            }
         
        }
        return Vector3.zero;
       
    }

    public void SpinAttack()
    {
        GameObject spinMelee = Instantiate(Attacks[0], transform.position, Quaternion.identity, transform);
    }

    public void SpawnMinons()
    {   float offest = 1.2f;
        Vector3[] poslist = { new Vector3(transform.position.x + offest, transform.position.y, transform.position.z), new Vector3(transform.position.x, transform.position.y, transform.position.z + offest), new Vector3(transform.position.x - offest, transform.position.y, transform.position.z), new Vector3(transform.position.x, transform.position.y, transform.position.z - offest) };

        foreach (Vector3 pos in poslist)
        {
            Instantiate(Attacks[1], pos , Quaternion.identity, transform);
          
        }
        
    }

    public int GetBottomIndex()
    {
        return BottomIndex;
    }

    public void RangedAttack()
    {
        timer += Time.deltaTime;
        if (timer >= fireRate)
        {
            GameObject orb = Instantiate(Attacks[2], new Vector3(transform.position.x, transform.position.y + 1.2f, transform.position.z + 2f), Quaternion.identity, transform);
            orb.GetComponent<Rigidbody>().AddForce((Player.transform.position - transform.position) * 30f);
            Destroy(orb, 2.0f);
            timer = 0f;
        }

    }

    void Death()
    {
        if (enemyHealth.Health < 1)
        {
            GameObject drop = Instantiate(DropPrefab, DropPoint.position, transform.rotation);
            string[] name = transform.parent.parent.name.Split(",");
            GameManager.gameManager.AddPoints(int.Parse(name[1], CultureInfo.InvariantCulture.NumberFormat));
            Destroy(this.gameObject);
        }
    }


}
