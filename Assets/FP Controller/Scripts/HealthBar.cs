using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    private int health;
    private float lerpTimer;
    private int maxHealth;
    public float chipSpeed = 2f;
    public Image frontHealthBar;
    public Image backHealthBar;
    // Start is called before the first frame update
    void Start()
    {
        health = GameManager.gameManager.playerHealth.Health;
        maxHealth = GameManager.gameManager.playerHealth.MaxHealth;
        
    }

    // Update is called once per frame
    void Update()
    {
        
       
        UpdateHealthUI();


    }
    public void UpdateHealthUI()
    {
        
        float fillF = frontHealthBar.fillAmount;
        float fillB = backHealthBar.fillAmount;
        float hFraction = health / maxHealth;
        if(fillB > hFraction)
        {
            frontHealthBar.fillAmount = hFraction;
            backHealthBar.color = Color.red;
            lerpTimer += Time.deltaTime;
            float percentComplete = lerpTimer / chipSpeed;
            backHealthBar.fillAmount = Mathf.Lerp(fillB, hFraction, percentComplete);
        }
    }
    
    public void TakeDamage(int dmg)
    {
        health -= dmg;
        lerpTimer = 0f;
    }
}
