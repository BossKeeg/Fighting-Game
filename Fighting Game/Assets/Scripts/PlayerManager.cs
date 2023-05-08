using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public GameObject otherPlayer;

    public KeyCode playerDodge;
    public KeyCode playerAttack;

    public float forceMultiplierWait = 2f;
    public float knockBackWait = 2f;

    bool canInteract;

    public bool isFlipped;

    public Vector2 originalForce;
    public Vector2 originalForceFlipped;
    public Vector2 attackForce;

    public Animator animator;

    public string shieldName;
    public string stopShieldName;

    public float maxHP = 100f;
    public float currentHP;
    public float damage = 10f;
    public Transform healthBar;

    public float dodgeCooldown = 2f;
    public List<GameObject> blowsIcon;
    public int maxBlowCount;
    public int currentBlowCount;

    public bool isAttack;
    public bool isShiled;

    private void Awake()
    {
        originalForce = this.GetComponent<ConstantForce2D>().force;
        originalForceFlipped = this.GetComponent<ConstantForce2D>().force * -1;

        currentHP = maxHP;

        Vector3 size = Vector3.one;
        size.y = healthBar.localScale.y;

        healthBar.localScale = size;

        currentBlowCount = maxBlowCount;
    }
     
    private void Update()
    {
        if (!isAttack)
        {
            if (isFlipped)
            {
                this.GetComponent<ConstantForce2D>().force = originalForceFlipped;
            }
            else
            {
                this.GetComponent<ConstantForce2D>().force = originalForce;
            }
        }

        if (isAttack)
        {
            this.GetComponent<ConstantForce2D>().force = attackForce * otherPlayer.transform.forward.z /** (otherPlayer.GetComponent<PlayerManager>().isFlipped ? 1 : -1)*/;
        }

        //if (Input.GetKeyUp(playerDodge))
        //{
        //    Dodge();
        //}

        if (Input.GetKeyDown(playerDodge))
        {
            Dodge(true);
        }
        else if (Input.GetKeyUp(playerDodge))
        {
            Dodge(false);
        }

        if (Input.GetKeyDown(playerAttack))
        {
            Attack();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            canInteract = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            canInteract = false;
        }

    }

    public void Damage(float damage)
    {
        if (isShiled)
        {
            currentBlowCount--;

            if (blowsIcon.Count > 0)
            {
                blowsIcon[currentBlowCount].SetActive(false);
            }

            if (currentBlowCount <= 0)
            {
                StartCoroutine(CoolDown());
                Dodge(false);
            }
        }
        else
        {
            StartCoroutine(TakeDamage());
            currentHP -= damage;
            
            Vector3 size = Vector3.one;
            size.x = currentHP / maxHP;
            size.y = healthBar.localScale.y;

            healthBar.localScale = size;
        }
    }

    void Dodge(bool val)
    {
        if (val == isShiled)
        {
            return;
        }

        Debug.Log("Dodging...");
        StartCoroutine(Dodges(val));
    }

    void Dodge() 
    {
        Debug.Log("Dodging...");
        StartCoroutine(Dodges());   
    }

    IEnumerator CoolDown()
    {
        yield return new WaitForSeconds(dodgeCooldown);
        currentBlowCount = maxBlowCount;

        if (blowsIcon.Count > 0)
        {
            foreach (var item in blowsIcon)
            {
                item.SetActive(true);
            }
        }
    }

    IEnumerator Dodges(bool val)
    {
        if (currentBlowCount <= 0 && !isShiled) 
        {
            yield break;
        } 

        isShiled = val;

        animator.SetTrigger(isShiled ? shieldName : stopShieldName);

        if (!isShiled)
        {
            yield break;
        }

        yield return null;

        //this.GetComponent<ConstantForce2D>().enabled = true;
        //yield return new WaitForSeconds(forceMultiplierWait);
        //this.GetComponent<ConstantForce2D>().enabled = false;
    }

    IEnumerator Dodges()
    {
        if (currentBlowCount <= 0 && !isShiled)
        {
            yield break;
        }

        isShiled = !isShiled;

        animator.SetTrigger(isShiled ? shieldName : stopShieldName);

        if (!isShiled)
        {
            yield break;
        }

        yield return null;

        //this.GetComponent<ConstantForce2D>().enabled = true;
        //yield return new WaitForSeconds(forceMultiplierWait);
        //this.GetComponent<ConstantForce2D>().enabled = false;
    }

    IEnumerator AttackN()
    {
        otherPlayer.GetComponent<PlayerManager>().isAttack = true;
        otherPlayer.GetComponent<ConstantForce2D>().enabled = true;
        yield return new WaitForSeconds(knockBackWait);
        otherPlayer.GetComponent<ConstantForce2D>().enabled = false;
        otherPlayer.GetComponent<PlayerManager>().isAttack = false;
    }

    IEnumerator KnockBackThis()
    {
        isAttack = true;
        this.GetComponent<ConstantForce2D>().enabled = true;
        yield return new WaitForSeconds(knockBackWait);
        this.GetComponent<ConstantForce2D>().enabled = false;
        isAttack = false;
    }

    void Attack()
    {
        if (canInteract)
        {
            StartCoroutine(AttackN());
            otherPlayer.GetComponent<PlayerManager>().Damage(damage);

            if (otherPlayer.GetComponent<PlayerManager>().isShiled)
            {
                //Knockback this
                StartCoroutine(KnockBackThis());
            }
        }
    }

    IEnumerator TakeDamage()
    {
        animator.SetBool("takenDamage", true);
        yield return new WaitForSeconds(0.15f);
        animator.SetBool("takenDamage", false);

    }
}
