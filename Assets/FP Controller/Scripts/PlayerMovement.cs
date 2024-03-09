using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class PlayerMovement : MonoBehaviour {

    [Header("Layer Mask")]
    [Tooltip("Which Layers can be walked on?")]
    public LayerMask walkableMask;

    [Header("Movement")]
    public float speed = 12f;
    public float jumpHeight = 2f;
    CharacterController controller;
    Transform groundCheck;
    Vector3 velocity;
    float gravity;
    bool isGrounded;
    public bool Shoot;


    void Start() {
        
        Physics.gravity = Vector3.down * 20;
        controller = GetComponent<CharacterController>();
        groundCheck = transform.Find("GroundCheck");
        gravity = Physics.gravity.y;
        Cursor.visible = false;
    }

    void Update() {
        movement();
        if (Input.GetButtonDown("Fire1"))
        {
            
            if (GameManager.gameManager.playerHealth.Health > 5)
            {
                TakeDamage(5);
                
                
            }
            Shoot = true;
           
        }

    }


    void movement()
    {


        isGrounded = Physics.CheckSphere(groundCheck.position, 0.2f, walkableMask);

        float horz = Input.GetAxis("Horizontal");
        float vert = Input.GetAxis("Vertical");

        Vector3 motion = transform.right * horz + transform.forward * vert;

        controller.Move(motion * speed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
    } 



    public void TakeDamage(int dmg)
    {
        GameManager.gameManager.TakeDamage(dmg);
        
    }

    public void GetHealed(int healing)
    {
        GameManager.gameManager.RestoreHealth(healing);
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Attack")
        {
           // collision.transform.parent.GetComponent<Enemy>().GetBottomIndex()
            switch (collision.transform.parent.GetComponent<Enemy>().GetBottomIndex())
            {
                case 0:
                    TakeDamage(5);
                    break;
                case 1:
                    TakeDamage(5);
                    break;
                case 2:
                    TakeDamage(10);
                    break;
                case 3:
                    TakeDamage(15);
                    break;

            }

        }
    
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Attack")
        {
            // collision.transform.parent.GetComponent<Enemy>().GetBottomIndex()
            switch (other.transform.parent.GetComponent<Enemy>().GetBottomIndex())
            {
                case 0:
                    TakeDamage(5);
                    break;
                case 1:
                    TakeDamage(5);
                    break;
                case 2:
                    TakeDamage(10);
                    break;
                case 3:
                    TakeDamage(15);
                    break;

            }

        }
    }
}
