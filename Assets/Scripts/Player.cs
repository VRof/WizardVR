using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100;
    [SerializeField] private float maxMana = 100;
    private float currentHealth;
    private float currentMana;
    //private float addedHealth;
    [SerializeField] private int ManaRegenerationAmount = 5;
    [SerializeField] private Bar healthBar;
    [SerializeField] private Bar manaBar;



    void Start()
    {
        currentHealth = maxHealth;
        currentMana = 10;
        healthBar.UpdateHealthBar(maxHealth, currentHealth, 0);
        manaBar.UpdateManaBar(maxMana, currentMana, ManaRegenerationAmount);

        StartCoroutine(RegenerateMana());
    }

    void Update()
    {

    }

    private IEnumerator RegenerateMana() {
        while (true)
        {
            yield return new WaitForSeconds(1);
            PlayerUpdateMana(ManaRegenerationAmount);
        }
    }

    public void PlayerUpdateHealth(float amount)
    {

        currentHealth += amount;
        if (currentHealth <= 0)
        {
            healthBar.UpdateHealthBar(maxHealth, 0, 0);
            PlayerDie();
        }
        else
        {
            currentHealth = currentHealth > maxHealth ? maxHealth : currentHealth;
            currentHealth = currentHealth < 0 ? 0 : currentHealth;
            healthBar.UpdateHealthBar(maxHealth, currentHealth, 0);
        }
    }
    public void PlayerUpdateMana(float manaAmount)
    {
        currentMana += manaAmount;
        currentMana = currentMana > maxMana ? maxMana : currentMana;
        currentMana = currentMana < 0 ? 0 : currentMana;
        manaBar.UpdateManaBar(maxMana, currentMana, ManaRegenerationAmount);
    }

    
    
    private void PlayerDie()
    {

    }

}
