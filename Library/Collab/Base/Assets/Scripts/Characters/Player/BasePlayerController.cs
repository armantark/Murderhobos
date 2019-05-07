using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BasePlayerController : MonoBehaviour
{
    //strings unique to the player
    public string attack, backstab, revive, otherplayer, player, hold;

    #region movement_variables
    [SerializeField]
    private float baseSpeed; // The base speed a player can move
    private float speed; //The speed the player can move
    protected Vector2 moveVelocity; //The velocity of the player's movement
    protected BasePlayerController playerMovement; //Allows us to refer to the player's movement
    protected float someScale;
    protected bool facingLeft;
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
    private protected bool isAttacking;
    [SerializeField]
    private float baseStartTimeBtwAttack;
    private float startTimeBtwAttack;

    //Position and range of AOE
    public Transform attackPos;
    public float attackRange;
    public LayerMask whatIsEnemy; //Only enemies affected by AOE
    public Enemy currEnemy;

    //Damage variable
    [SerializeField]
    private float baseDamage;
    private float damage;

    //Damage reduction variables
    [SerializeField]
    private float baseDamageReduction;
    private float damageReduction;

    //Attack animation
    public Animator playerAnim;
    #endregion

    #region health_variables
    public float startingHealth; //How much health the player starts the game with
    public float currentHealth;  //How much health the player currently has
    
    private bool isDowned; //True when the player's health reaches zero

    public float maxInvincibilityTime;
    protected bool canBeAttacked = true;
    public Image visualHealth;
    private float healthOnRevive;

    //Knockback variables
    public float baseKnockback;
    public float knockback;
    public float knockbackLength;
    public float knockbackCount;
    public bool knockFromRight;
    public bool knockFromTop;
    #endregion

    #region backstab_variables
    //Variables for how often player can backstab
    protected bool isBackstabbing = false;
    [SerializeField]
    private float baseStartTimeBtwBackstab;
    private float startTimeBtwBackstab;

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
    public float timeToRevive;
    private float reviveTimer = 0;

    #endregion

    #region shard_variables
    public Color color;
    public ShardSystem shards;
    #endregion

    #region equipment_variables
    private Equipment equipment;
    private bool equipmentWasUpdated;

    //for the current equipment text
    [SerializeField]
    Text equipText;

    //Equipment change text
    [SerializeField]
    Text changeText;

    #endregion

    //Start is called before the first frame update.
    protected void Start()
    {
        
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        playerMovement = GetComponent<BasePlayerController>();
        currEnemy = GetComponent<Enemy>();

        //fix this up later, not necessary here
        equipment = new Equipment(1, 1, 0.778f, 1, 1, 1, "Medium", Equipment.ArmorClass.Medium, 1);
        equipmentWasUpdated = true;

        //Equipment UI
        equipText.text = "Armor Class: " + equipment.GetName();

        //for testing purposes, remove later!
        if (isDowned)
        {
            sr.color = Color.red;
        }
        
        // populate all necessary fields
        speed = baseSpeed;
        damage = baseDamage;
        damageReduction = baseDamageReduction;
        startTimeBtwAttack = equipment.GetAttackCooldown();
        startTimeBtwBackstab = baseStartTimeBtwBackstab;
        knockback = baseKnockback;

        // Sets the player's health to max when the game starts
        currentHealth = startingHealth;
        //invincibilityTime = maxInvincibilityTime;

        //spawnPoint = new Vector2(0, 0);
        playerAnim = GetComponent<Animator>();
        rend = GetComponent<Renderer>();
        //rend.enabled = true;
        someScale = transform.localScale.x;
        facingLeft = false;
        _posX = transform.position.x;
        StartCoroutine(UpdateHealthBar());
        healthOnRevive = startingHealth;
    }

    //Update is called once per frame
    protected void Update()
    {
        UpdatePlayerStats(); // should this be here at the top?
        //Updates the player's current armor class
        equipText.text = "Armor Class: " + equipment.GetName();

        if (!isDowned)
        {
            //knockback function
            if (knockbackCount <= 0)
            {
                Vector2 moveInput = new Vector2(Input.GetAxisRaw(horizontalCtrl), Input.GetAxisRaw(verticalCtrl));
                moveVelocity = moveInput.normalized * speed;

                //Moves the player
                //transform.Translate(moveVelocity * Time.deltaTime);
                //rb.velocity = moveVelocity;
                rb.MovePosition((Vector2)transform.position + moveVelocity * Time.deltaTime);
                //rb.simulated = true;
                Flip(moveVelocity.x);

                canBeAttacked = true;
            }
            else if ((knockbackCount > 0) && (canBeAttacked = true))
            {
                canBeAttacked = false;
                if ((knockFromRight) && (!knockFromTop))
                {
                    rb.velocity = new Vector2(-knockback, knockback);
                }
                else if (knockFromTop)
                {
                    rb.velocity = new Vector2(0, -knockback);
                }
                if ((!knockFromRight) && (!knockFromTop))
                {
                    rb.velocity = new Vector2(knockback, knockback);
                }
                else if (knockFromRight)
                {
                    rb.velocity = new Vector2(-knockback, 0);
                }
                if ((knockFromTop) && (knockFromRight))
                {
                    rb.velocity = new Vector2(-knockback, -knockback);
                }
                else if (!knockFromRight)
                {
                    rb.velocity = new Vector2(knockback, 0);
                }
                if ((knockFromTop) && (!knockFromRight))
                {
                    rb.velocity = new Vector2(knockback, -knockback);
                }
                else if (!knockFromTop)
                {
                    rb.velocity = new Vector2(0, knockback);
                }
                knockbackCount -= Time.deltaTime;
            }
        }
        else
        {
            //rb.simulated = false;
            rb.velocity = Vector2.zero;
            return;
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
        if (Input.GetButton(revive) && currDownedPlayer)
        {
            Debug.Log(reviveTimer);
            reviveTimer += Time.deltaTime;
            if (reviveTimer >= timeToRevive)
            {
                reviveTimer = 0;
                Revive();
            }
        } else if (Input.GetButtonUp(revive) || !currDownedPlayer)
        {
            reviveTimer = 0;
        }
    }

    #region movement_functions
    //https://answers.unity.com/questions/1447323/how-to-flip-2d-character-to-face-direction-of-move.html
    private void Flip(float input)
    {
        if (input < 0 && !facingLeft || input > 0 && facingLeft)
        {
            facingLeft = !facingLeft;
            Vector2 theScale = transform.localScale;
            theScale.x *= -1;
            transform.localScale = theScale;
        }
    }
    #endregion

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

        playerAnim.SetTrigger("attacking");
        //Deals damage to all enemies with in a certain AOE
        Collider2D[] enemiesToDamage = Physics2D.OverlapCircleAll(attackPos.position, attackRange, whatIsEnemy);
        //Loops through all enemies in the circle and deals each one damage
        for (int i = 0; i < enemiesToDamage.Length; i++)
        {
            Debug.Log("Damage");
            enemiesToDamage[i].GetComponent<Enemy>().TakeDamage(damage, this);
        }
        isAttacking = true;
        yield return new WaitForSeconds(startTimeBtwAttack);
        isAttacking = false;
    }
    #endregion

    #region backstab_functions
    public void GetBackstabbed()
    {
        //TODO: Fix this
//        Instantiate(gameObject.AddComponent<Shard>(), transform.position, transform.rotation);
        shards.loseShard();
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
        rb.isKinematic = true;
        
    }
    protected IEnumerator BackstabCoroutine()
    {
        //playerAnim.SetTrigger("backstab");
        //we seriously need to redo this, lol.
        Collider2D[] playersToBackstab = Physics2D.OverlapBoxAll(backstabPos.position, new Vector2(backstabRangeX, backstabRangeY), 0, whatIsPlayer);

        //Loops through all enemies in the circle and deals each one damage
        var playerBeingBackstabbed = playersToBackstab[0].GetComponent<BasePlayerController>();
        playerBeingBackstabbed.GetBackstabbed();

        if (playerBeingBackstabbed.shards.getNumShards() >= 0)
        {
            shards.gainShard();
        }
        isBackstabbing = true;
        yield return new WaitForSeconds(startTimeBtwBackstab);
        isBackstabbing = false;
    }
    #endregion

    #region health_functions
    protected IEnumerator UpdateHealthBar()
    {
        while (true)
        {
            float fillRatio = currentHealth / startingHealth;

            visualHealth.fillAmount = Mathf.Lerp(visualHealth.fillAmount, fillRatio, Time.deltaTime * 5);
            yield return null;
        }
    }

    //Calls this function if the player has taken damage
    public void TakeDamage (float amount)
    {

        //Reduces the players health by specified amount
        if (canBeAttacked && currentHealth > 0)
        {
            Debug.Log("Health is now " + currentHealth);
            currentHealth = Mathf.Max(currentHealth - damageReduction * amount, 0);
            StartCoroutine(Invincible());
        }

        //Changes the health bar's UI to match current health
        //healthSlider.value = currentHealth;

        //Checks to see if player has lost all their health and is not already downed
        if(currentHealth <= 0 && !isDowned)
        {
            shards.downShard();
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
        canBeAttacked = false;
        //Changes the sprite's color to indicate a player has been downed, until we have an animation to replace it
        sr.color = Color.red;

        //Prevents the player from being able to move
        playerMovement.enabled = false;

        moveVelocity = Vector2.zero;
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;
        transform.Rotate(Vector3.forward * 90);

        changeText.text = hold;

    }

    public bool IsDowned()
    {
        return isDowned;
    }

    IEnumerator Invincible()
    {
        canBeAttacked = false;
        sr.color = Color.magenta;
        int timesToFlash = 3;
        for (int i = 0; i < timesToFlash; i++)
        {
            rend.enabled = false;
            yield return new WaitForSeconds(maxInvincibilityTime/(timesToFlash*2));
            rend.enabled = true;
            yield return new WaitForSeconds(maxInvincibilityTime/(timesToFlash*2));
        }
        if (!isDowned)
        {
            sr.color = Color.white;
        }
        canBeAttacked = true;
    }
    #endregion

    #region revive_functions

    private void Revive()
    {
        Debug.Log(currDownedPlayer);
        currDownedPlayer.rb.isKinematic = false;
        float cofactor = 0.1f;
        if (currDownedPlayer.healthOnRevive > 0)
        {
            currDownedPlayer.healthOnRevive -= cofactor * currDownedPlayer.startingHealth;
        }
        currDownedPlayer.currentHealth = currDownedPlayer.healthOnRevive;
        currDownedPlayer.isDowned = false;
        currDownedPlayer.transform.Rotate(Vector3.forward * -90);
        currDownedPlayer.canBeAttacked = true;
        currDownedPlayer.sr.color = Color.white;
        currDownedPlayer.playerMovement.enabled = true;
        currDownedPlayer = null;
    }

    protected void OnTriggerEnter2D(Collider2D downedPlayer)
    {
        if (!downedPlayer.CompareTag(otherplayer)) return;
        //Debug.Log(downedPlayer.tag);
        currDownedPlayer = downedPlayer.GetComponent<BasePlayerController>();
        if (!currDownedPlayer.isDowned)
        {
            currDownedPlayer = null;
        }
    }

    protected void OnTriggerExit2D(Collider2D downedPlayer)
    {
        if (!downedPlayer.CompareTag(otherplayer)) return;
        if (downedPlayer.GetComponent<BasePlayerController>() == currDownedPlayer)
        {
            currDownedPlayer = null;
        }
    }

    #endregion

    #region equipment_functions
    public void SetPlayerEquipment(Equipment newEquipment)
    {
        equipment = newEquipment;
        playerAnim.SetInteger("ArmorClass", (int) equipment.GetArmorClass());
        equipmentWasUpdated = true;
    }

    public Equipment GetPlayerEquipment()
    {
        return equipment;
    }

    private void UpdatePlayerStats()
    {
        if (equipmentWasUpdated)
        {
            StartCoroutine(PopUpText());
            speed = baseSpeed * equipment.GetMoveSpeed();
            damage = baseDamage * equipment.GetWeaponDamage();
            damageReduction = baseDamageReduction * equipment.GetDamageReduction();
            startTimeBtwAttack = equipment.GetAttackCooldown();
            startTimeBtwBackstab = baseStartTimeBtwBackstab * equipment.GetBackstabCooldown();
            knockback = knockback * equipment.GetKnockback();

            equipmentWasUpdated = false;
        }
    }

    IEnumerator PopUpText()
    {
        Debug.Log("It gets called, Arman");
        string temp = changeText.text;
        changeText.text = "Armor: " + equipment.GetName();
        yield return new WaitForSeconds(2f);
        changeText.text = temp;
    }
    #endregion
}
