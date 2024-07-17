using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100;
    [SerializeField] private float maxMana = 100;
    private float currentHealth;
    private float currentMana;
    private float addedHealth;
    private float addedMana;
    [SerializeField] private Bar healthBar;
    [SerializeField] private Bar manaBar;
    void Start()
    {
        addedHealth = 0;
        addedMana = 1;
        currentHealth = maxHealth;
        currentMana = maxMana;

        healthBar.UpdateHealthBar(maxHealth, currentHealth, addedHealth);
        manaBar.UpdateManaBar(maxMana, currentMana, addedMana);
    }

    void Update()
    {
        if (currentHealth > 0)
        {
            PlayerTakeDamage(2);
            PlayerUseMana(1);

            AddHealth(addedHealth);
            currentHealth = currentHealth > maxHealth ? maxHealth : currentHealth;
            currentHealth = currentHealth < 0 ? 0 : currentHealth;
            healthBar.UpdateHealthBar(maxHealth, currentHealth, addedHealth);

            AddMana(addedMana);
            currentMana = currentMana > maxMana ? maxMana : currentMana;
            currentMana = currentMana < 0 ? 0 : currentMana;
            manaBar.UpdateManaBar(maxMana, currentMana, addedMana);
        }
        else
        {
            healthBar.UpdateHealthBar(maxHealth, currentHealth, addedHealth);
            PlayerDie();
        }
    }

    public void PlayerTakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
    }
    public void PlayerUseMana(float manaAmount)
    {
        currentMana -= manaAmount;
    }

    public void AddHealth(float health)
    {
        currentHealth += health;
    }

    public void AddMana(float mana)
    {
        currentMana += mana;
    }

    private void PlayerDie()
    {

    }
}
