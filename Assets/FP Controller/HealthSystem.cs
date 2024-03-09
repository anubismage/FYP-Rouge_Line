using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthSystem 
{
    int CurrentMaxHealth;
    int CurrentHealth;

    public int Health
    {
        get
        {
            return CurrentHealth;
        }
        set
        {
            CurrentHealth = value;
        }
    }

    public int MaxHealth
    {
        get
        {
            return CurrentMaxHealth;
        }
        set
        {
            CurrentMaxHealth = value;
        }
    }

    public HealthSystem(int health, int maxHealth)
    {
        CurrentHealth = health;
        CurrentMaxHealth = maxHealth;
    }

    public void Damage(int dmgAmount)
    {
        if (CurrentHealth > 0)
        {
            CurrentHealth -= dmgAmount;
        }
    }

    public void Heal(int healAmount)
    {
        if (CurrentHealth < CurrentMaxHealth)
        {
            CurrentHealth += healAmount;
        }
        if (CurrentHealth > CurrentMaxHealth)
        {
            CurrentHealth = CurrentMaxHealth;
        }


    }

}
