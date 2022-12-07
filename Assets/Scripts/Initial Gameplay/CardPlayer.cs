using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using UnityEngine.UI;
using TMPro;

public class CardPlayer : MonoBehaviour
{
    [SerializeField] Card chosenCard;
    public Transform attackPosReference;
    public TMP_Text nameText;
    public TMP_Text healthText;
    public HealthBar healthBar;
    public float health;
    private Tweener animationTweener;
    public TMP_Text NickName {get => nameText;}
    public bool IsReady = false;

    public PlayerStats stats = new PlayerStats
    {
        MaxHealth = 100,
        RestoreValue = 5,
        DamageValue = 10
    };

    private void Start()
    {
        health = stats.MaxHealth;
    }

    public void SetStats(PlayerStats newStats, bool restoreFullHealth = false)
    {
        Debug.Log("restoreFullHealth : " + restoreFullHealth);
        Debug.Log("Before Max Health : " + stats.MaxHealth);
        this.stats = newStats;
        Debug.Log("Next Max Health : " + stats.MaxHealth);
        if(restoreFullHealth)
        {
            health = stats.MaxHealth;
            Debug.Log("Max Health : " + stats.MaxHealth);
            Debug.Log("Unique Health : " + health);
        }
        
        UpdateHealthBar();
    }

    public Attack? AttackValue 
    {
        get
        {
            if(chosenCard==null)
                return null;
            else
                return chosenCard.attackValue;
        }
    }
    public void Reset()
    {
        if(chosenCard!=null)
        {
            chosenCard.Reset();
        }

        chosenCard = null;
    }
    public void SetChosenCard(Card newCard)
    {
        if(chosenCard != null)
        {
            chosenCard.transform.DOKill();
            chosenCard.Reset();
        }

        chosenCard = newCard;
        chosenCard.transform.DOScale(chosenCard.transform.localScale*1.2f,0.2f);
    }

    public void ChangingHealth(float amount)
    {
        health += amount;
        health = Mathf.Clamp(health, 0, stats.MaxHealth);
        Debug.Log("Debug 1");
        UpdateHealthBar();
    }

    public void UpdateHealthBar()
    {
        
        Debug.Log("Update Max Health : " + stats.MaxHealth);
        Debug.Log("Update Unique Health : " + health);
        // HealthBar
        healthBar.UpdateBar(health / stats.MaxHealth);
        // Text
        healthText.text = health + "/" + stats.MaxHealth;
    }

    public void AnimateAttack()
    {
        animationTweener = chosenCard.transform.DOMove(attackPosReference.position,1);
    }

    public bool InAnimation()
    {
        return animationTweener.IsActive();
    }

    internal void DamageAnimation()
    {
        var image = chosenCard.GetComponent<Image>();
        animationTweener =  image
            .DOColor(Color.red,0.1f)
            .SetLoops(2,LoopType.Yoyo)
            .SetDelay(0.5f);
    }

    internal void DrawAnimation()
    {
        var image = chosenCard.GetComponent<Image>();
        animationTweener =  image
            .DOColor(Color.blue,0.1f)
            .SetLoops(2,LoopType.Yoyo)
            .SetDelay(0.5f);
        animationTweener = chosenCard.transform
            .DOMove(chosenCard.firstCardPosition,1)
            .SetEase(Ease.InBack)
            .SetDelay(0.2f);
    }

    public void isClickable(bool value)
    {
        Card[] cards = GetComponentsInChildren<Card>();
        foreach (var card in cards)
        {
            card.SetClickable(value);
        }
    }
}
