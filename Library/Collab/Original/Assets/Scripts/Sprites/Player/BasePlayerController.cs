using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BasePlayerController : MonoBehaviour
{
    //strings unique to the player
    public string attack, backstab, revive, otherplayer, player;

    #region movement_variables
    public float speed; //The speed the player can move
    protected Vector2 moveVelocity; //The velocity of the player's movement
    protected BasePlayerController playerMovement; //Allows us to refer to the player's movement
    protected float someScale;
    protected int direction;
    protected float _posX;

    //Sets the controller inputs
    public string horizontalCtrl; 
    public string verticalCtrl;
    #endregion

    #region physics_variables
    protected Rigidbody2D rb;
    protected SpriteRenderer sr;
    #endregion

    #region attack_variables
    //Variables for how often player can attack
    protected bool isAttacking;
    public float startTimeBtwAttack;

    //Position and range of AOE
    public Transform attackPos;
    public float attackRange;
    public LayerMask whatIsEnemy; //Only enemies affected by AOE
    public Enemy currEnemy;

    //Damage variable
    public int damage;

    //Attack animation
    public Animator playerAnim;
    #endregion

    #region health_variables
    public float startingHealth; //How much health the player starts the game with
    public float currentHealth;  //How much health the player currently has
    
    public bool isDowned; //True when the player's health reaches zero

    public float maxInvincibilityTime;
    protected bool canBeAttacked = true;
    public Image visualHealth;
    #endregion

    #region backstab_variables
    //Variables for how often player can backstab
    protected bool isBackstabbing = false;
    public float startTimeBtwBackstab;

    //Position and range of backstab
    public Transform backstabPos;
    public float backstabRangeX;
    public float backstabRangeY;

    //Who the backstab can affect
    public LayerMask whatIsPlayer;

    //Attack animation
    public Animator b_playerAnim;

    //Respawn
    public Vector2 spawnPoint;
    //public GameObject newPlayer;
    public Renderer rend;
    #endregion

    #region revive_variables
    protected BasePlayerController currDownedPlayer = null;
    //public PlayerController1 P1;
    //public PlayerController2 P2;

    #endregion

    #region shard_variables
    public Shard shard;
    public Color color;
    #endregion

    //Start is called before the first frame update.
    protected void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        playerMovement = GetComponent<BasePlayerController>();
        currEnemy = GetComponent<Enemy>();

        // Sets the player's health to max when the game starts
        currentHealth = startingHealth;
        //invincibilityTime = maxInvincibilityTime;

        //spawnPoint = new Vector2(0, 0);
        rend = GetComponent<Renderer>();
        //rend.enabled = true;
        someScale = transform.localScale.x;
        direction = 1;
        _posX = transform.position.x;
        StartCoroutine(UpdateHealthBar());
    }

    //Update is called once per frame
    protected void Update()
    {
        if (!isDowned)
        {
            Vector2 moveInput = new Vector2(Input.GetAxisRaw(horizontalCtrl), Input.GetAxisRaw(verticalCtrl));
            moveVelocity = moveInput.normalized * speed;
            //Debug.Log(moveVelocity);

            //Moves the player
            //transform.Translate(moveVelocity * Time.deltaTime);
            //rb.velocity = moveVelocity;
            rb.MovePosition((Vector2)transform.position + moveVelocity * Time.deltaTime);
        }
        else
        {
            rb.velocity = Vector2.zero;
        }


        //if (transform.position.x >= _posX)
        //{
        //    //Debug.Log("Moving left - " + transform.position.x);
        //    if (direction == 1)
        //    {
        //        transform.localScale = new Vector2(someScale, transform.localScale.y);
        //        direction = -1;
        //    }
        //}
        //else
        //{
        //    //Debug.Log("Moving right - " + transform.position.x);
        //    if (direction == -1)
        //    {
        //        transform.localScale = new Vector2(-someScale, transform.localScale.y);
        //        direction = 1;
        //    }
        //}

        //_posX = transform.position.x;

        //Checks if the time between player attacks is less than or equal to zero
        if (Input.GetButtonDown(attack) && !isAttacking)
        {
            StartCoroutine(AttackCoroutine());
        }

        //Checks if the time between player backstab is less than or equal to zero
        if (!isBackstabbing && Input.GetButtonDown(backstab) && !isDowned) {
            StartCoroutine(BackstabCoroutine());
        }
        //Checks to see if the player presses the revive button
        if (Input.GetButtonDown(revive) && currDownedPlayer)
        {
            Revive();
        }
    }

    #region attack_functions
    //Allows us to see how big the AOE is
    protected void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPos.position, attackRange);
        Gizmos.DrawWireSphere(backstabPos.position, backstabRangeX);
    }

    protected IEnumerator AttackCoroutine()
    {
        //then player can attack

        //playerAnim.SetTrigger("attack");
        //Deals damage to all enemies with in a certain AOE
        Collider2D[] enemiesToDamage = Physics2D.OverlapCircleAll(attackPos.position, attackRange, whatIsEnemy);
        //Loops through all enemies in the circle and deals each one damage
        for (int i = 0; i < enemiesToDamage.Length; i++)
        {
            Debug.Log("Damage");
            enemiesToDamage[i].GetComponent<Enemy>().TakeDamage(damage);
        }
        isAttacking = true;
        yield return new WaitForSeconds(startTimeBtwAttack);
        isAttacking = false;
    }
    #endregion

    #region backstab_functions
    public void GetBackstabbed()
    {
        shardSystem.shardValue += 1;
        StartCoroutine("Respawn", 3f);
    }

    protected IEnumerator Respawn(float spawnDelay)
    {
        rend.enabled = false;
        playerMovement.enabled = false;
        isDowned = true;
        canBeAttacked = false;
        yield return new WaitForSeconds(spawnDelay);
        //need to fix this, it might be exploitable, but for the time being it's okay
        canBeAttacked = true;
        isDowned = false;
        playerMovement.enabled = true;
        transform.position = spawnPoint;
        rend.enabled = true;
        
    }
    protected IEnumerator BackstabCoroutine()
    {
        //playerAnim.SetTrigger("backstab");
        //we seriously need to redo this, lol.
        Collider2D[] playersToBackstab = Physics2D.OverlapBoxAll(backstabPos.position, new Vector2(backstabRangeX, backstabRangeY), 0, whatIsPlayer);

        //Loops through all enemies in the circle and deals each one damage
        playersToBackstab[1].GetComponent<BasePlayerController>().GetBackstabbed();
        isBackstabbing = true;
        affectShards.loseShards(); //calls the lose shards function inside shard system
        yield return new WaitForSeconds(startTimeBtwBackstab);
        isBackstabbing = false;
    }
    #endregion

    #region health_functions
    protected IEnumerator UpdateHealthBar()
    {
        while (true)
        {
            float fillRatio = MapValues(currentHealth, 0, startingHealth, 0, 1);

            visualHealth.fillAmount = Mathf.Lerp(visualHealth.fillAmount, fillRatio, Time.deltaTime * 10);
            yield return null;
        }
    }

    public void UpdateHealthValue(float newHealth)
    {
        currentHealth = Mathf.Min(Mathf.Max(newHealth, 0), startingHealth);
    }

    private float MapValues(float x, float inMin, float inMax, float outMin, float outMax)
    {
        return (x - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;
    }

    //Calls this function if the player has taken damage
    public void TakeDamage (int amount)
    {
        //Reduces the players health by specified amount
        if (canBeAttacked && currentHealth > 0)
        {
            Debug.Log("Health is now " + currentHealth);
            //currentHealth -= amount;
            UpdateHealthValue(currentHealth - amount);
            StartCoroutine(Invincible());
        }

        //Changes the health bar's UI to match current health
        //healthSlider.value = currentHealth;

        //Checks to see if player has lost all their health and is not already downed
        if(currentHealth <= 0 && !isDowned)
        {
            //affectShards.loseShards(); //calls the lose shards function inside shard system script
            //Player is downed
            Down();
        }
    }

    //Calls this function if the player is downed
    protected void Down()
    {
        //Indicates that the player is already downed, so they cannot be downed again
        Debug.Log("Downed!");
        isDowned = true;
        //Changes the sprite's color to indicate a player has been downed, until we have an animation to replace it
        sr.color = Color.red;

        //Prevents the player from being able to move
        playerMovement.enabled = false;

        moveVelocity = Vector2.zero;
    }

    IEnumerator Invincible()
    {
        canBeAttacked = false;
        sr.color = Color.magenta;
        yield return new WaitForSeconds(maxInvincibilityTime);
        if (!isDowned)
        {
            sr.color = Color.white;
        }
        canBeAttacked = true;
    }
    #endregion

    #region revive_functions

    void Revive()
    {
        Debug.Log(currDownedPlayer);
        currDownedPlayer.currentHealth = currDownedPlayer.startingHealth;
        currDownedPlayer.isDowned = false;
        currDownedPlayer.sr.color = new Color(1, 1, 1);
        currDownedPlayer.playerMovement.enabled = true;
        return;
    }

    protected void OnTriggerEnter2D(Collider2D downedPlayer)
    {
        if (downedPlayer.CompareTag(otherplayer))
        {
            //Debug.Log(downedPlayer.tag);
            currDownedPlayer = downedPlayer.GetComponent<BasePlayerController>();
            if (!currDownedPlayer.isDowned)
            {
                currDownedPlayer = null;
            }
        }
    }
    protected void OnTriggerExit2D(Collider2D downedPlayer)
    {
        if (downedPlayer.CompareTag(otherplayer))
        {
            if (downedPlayer.GetComponent<BasePlayerController>() == currDownedPlayer)
            {
                currDownedPlayer = null;
            }
        }
    }

    #endregion

    #region shard_functions
    protected void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Shard"))
        {
            other.gameObject.SetActive(false);
            shard.StartFollowing(); //calls the gain shard function inside the shard system script
        }
    }
    #endregion
}
