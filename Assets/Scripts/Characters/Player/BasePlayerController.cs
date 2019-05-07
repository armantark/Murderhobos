using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class BasePlayerController : MonoBehaviour
{
    //strings unique to the player
    public string attack, backstab, revive, otherplayer, player, hold;

    #region movement_variables
    [SerializeField]
    private float baseSpeed; // The base speed a player can move
    private float _speed; //The speed the player can move
    private Vector2 _moveVelocity; //The velocity of the player's movement
    private BasePlayerController _playerMovement; //Allows us to refer to the player's movement
    private bool isMoving;
//    public float someScale;
    public bool facingLeft;
//    protected float _posX;

    //Sets the controller inputs
    public string horizontalCtrl; 
    public string verticalCtrl;

    private bool _canMove = true;
    #endregion

    #region physics_variables

    private Rigidbody2D _rb;
    private SpriteRenderer _sr;
    #endregion

    #region attack_variables
    //Variables for how often player can attack
    private bool _isAttacking;
    [SerializeField]
    private float baseStartTimeBtwAttack;
    private float _startTimeBtwAttack;
    private float _attackWindupTime;

    //Position and range of AOE
    public Transform attackPos;
    public float attackRange;
    public LayerMask whatIsEnemy; //Only enemies affected by AOE
//    public Enemy currEnemy;

    //Damage variable
    [SerializeField]
    private float baseDamage;
    private float _damage;

    //Damage reduction variables
    [SerializeField]
    private float baseDamageReduction;
    private float _damageReduction;

    //Attack animation
    public Animator playerAnim;
    private static readonly int Attacking = Animator.StringToHash("attacking");
    private static readonly int HitDir = Animator.StringToHash("HitDirection");
    private enum HitDirection { LR = 0, DOWN = 1, UP = 2 }
    #endregion

    #region health_variables
    public float startingHealth; //How much health the player starts the game with
    public float currentHealth;  //How much health the player currently has
    
    private bool _isDowned; //True when the player's health reaches zero

    public float maxInvincibilityTime;
    private bool _canBeAttacked = true;
    public Image visualHealth;
    private float _healthOnRevive;
    
    private const float ReviveCofactor = 0.1f; //amount to reduce hp per revive.
    private static readonly int isHit = Animator.StringToHash("isHit");

    //Knockback variables
    public float baseKnockback;
    public float knockback;
    public float beingKnockedBackStrength;
    public float beingKnockedBackTime;
    private float _knockbackTimer;

    public GameManager gameManager;
    
    #endregion

    #region backstab_variables
    //Variables for how often player can backstab
    private bool _isBackstabbing;
    [SerializeField]
    private float baseStartTimeBtwBackstab;
    private float _startTimeBtwBackstab;
    private static readonly int backstabbing = Animator.StringToHash("backstabbing");

    //Position and range of backstab
    public Transform backstabPos;
    public float backstabRangeX;
    public float backstabRangeY;

    //Who the backstab can affect
    public LayerMask whatIsPlayer;

    //Attack animation
//    public Animator b_playerAnim;

    //Respawn
    public Vector2 spawnPoint;
    //public GameObject newPlayer;
    public Renderer rend;
    private const int NumberOfShardsStolen = 1;
    #endregion

    #region revive_variables

    private BasePlayerController _currDownedPlayer;
    public float timeToRevive;
    private float _reviveTimer;

    #endregion

    #region shard_variables
    public Color color;
    public ShardSystem shards;
    public GameObject downShard;
    public GameObject backstabbedShard;
    #endregion

    #region equipment_variables
    private Equipment _equipment;
    private bool _equipmentWasUpdated;

    //for the current equipment text
    [SerializeField] private Text equipText;

    //Equipment change text
    [SerializeField] private Text changeText;
    private string _temp;

    #endregion



    //Start is called before the first frame update.
    protected void Start()
    {
        
        _rb = GetComponent<Rigidbody2D>();
        _sr = GetComponent<SpriteRenderer>();
        _playerMovement = GetComponent<BasePlayerController>();
        FindObjectOfType<AudioManager>().Play("BG_Music");

        //        currEnemy = GetComponent<Enemy>();

        //Sets the game winning condition to inactive
        //winGame.SetActive(false);

        //fix this up later, not necessary here
        _equipment = new Equipment(1, 
            1, 
            0.778f, 
            1,
            1, 
            1, 
            1,
            0.1f,
            "Medium", 
            Equipment.ArmorClass.Medium, 
            1);
        _equipmentWasUpdated = true;

        //Equipment UI
        equipText.text = "Armor Class: " + _equipment.GetName();

        //for testing purposes, remove later!
        if (_isDowned)
        {
            _sr.color = Color.red;
        }
        
        // populate all necessary fields
        _speed = baseSpeed;
        _damage = baseDamage;
        _damageReduction = baseDamageReduction;
        _startTimeBtwAttack = _equipment.GetAttackCooldown();
        _startTimeBtwBackstab = baseStartTimeBtwBackstab;
        _attackWindupTime = _equipment.GetAttackWindup();

        knockback = baseKnockback;
        isMoving = false;

        // Sets the player's health to max when the game starts
        currentHealth = startingHealth;
        //invincibilityTime = maxInvincibilityTime;

        //spawnPoint = new Vector2(0, 0);
        playerAnim = GetComponent<Animator>();
        rend = GetComponent<Renderer>();
        //rend.enabled = true;
//        someScale = transform.localScale.x;
        facingLeft = false;
//        _posX = transform.position.x;
        StartCoroutine(UpdateHealthBar());
        _healthOnRevive = startingHealth;
        _temp = changeText.text;
    }

    //Update is called once per frame
    protected void Update()
    {
        UpdatePlayerStats();
        //Updates the player's current armor class
        equipText.text = "Armor Class: " + _equipment.GetName();

        if (!_isDowned && _canMove)
        {
            
            Vector2 moveInput = new Vector2(Input.GetAxisRaw(horizontalCtrl), Input.GetAxisRaw(verticalCtrl));
            _moveVelocity = moveInput.normalized * _speed;

            // animations states for movement
            bool isMovingNow = _moveVelocity != Vector2.zero;
            if (isMovingNow != isMoving)
            {
                playerAnim.SetBool("isMoving", isMovingNow);
                isMoving = isMovingNow;
            }

            //Moves the player
            _rb.MovePosition((Vector2)transform.position + _moveVelocity * Time.deltaTime);
            Flip(_moveVelocity.x);

            _canBeAttacked = true;
        }
        else
        {
            return;
        }

        //Checks if the time between player attacks is less than or equal to zero
        if (Input.GetButtonDown(attack) && !_isAttacking)
        {
            StartCoroutine(AttackCoroutine());
        }

        //Checks if the time between player backstab is less than or equal to zero
        if (!_isBackstabbing && Input.GetButtonDown(backstab) && !_isDowned) {
            StartCoroutine(BackstabCoroutine());
        }
        //Checks to see if the player presses the revive button
        if (Input.GetButton(revive) && _currDownedPlayer)
        {
            //Debug.Log(reviveTimer);
            _temp = changeText.text;
            _reviveTimer -= Time.deltaTime;
            _currDownedPlayer.changeText.text = _reviveTimer.ToString("0.00");
            if (_reviveTimer <= 0.0f)
            {
                _reviveTimer = timeToRevive;
                _currDownedPlayer.changeText.text = _temp;
                Revive();
            }
        } else if (Input.GetButtonUp(revive) || !_currDownedPlayer)
        {
            if (_currDownedPlayer)
            {
                _currDownedPlayer.changeText.text = _temp;
            }
            _reviveTimer = timeToRevive;
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
            Vector2 theScale2 = changeText.transform.localScale;
            theScale2.x *= -1;
            transform.localScale = theScale;
            changeText.transform.localScale = theScale2;
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

    private int CheckDirection(Vector2 vel)
    {
        float u = Math.Abs(vel.x);
        if (u > 0)
            return (int) HitDirection.LR;
        if (vel.y > 0)
            return (int) HitDirection.UP;
        return (int) HitDirection.DOWN;
    }

    protected IEnumerator AttackCoroutine()
    {
        //then player can attack
        int attack_direction = CheckDirection(_moveVelocity);
        playerAnim.SetInteger("HitDirection", attack_direction);
        playerAnim.SetTrigger(Attacking);
   
        if (_equipment.GetName() == "Heavy")
        {
            FindObjectOfType<AudioManager>().Play("Sword_Attack");
        }
        else
        {
            FindObjectOfType<AudioManager>().Play("Dagger_Attack");
        }
        yield return new WaitForSeconds(_attackWindupTime);

        //Loops through all enemies in the circle and deals each one damage
        //Deals damage to all enemies with in a certain AOE
        Vector3 addition = Vector3.zero;
        if (attack_direction == (int)HitDirection.DOWN)
            addition = Vector3.down;
        else if (attack_direction == (int)HitDirection.UP)
            addition = Vector3.up;

        Collider2D[] enemiesToDamage = Physics2D.OverlapCircleAll(attackPos.position + addition, attackRange, whatIsEnemy);
        foreach (var t in enemiesToDamage)
        {
            //Debug.Log("Damage");
            t.GetComponent<Enemy>().TakeDamage(_damage, this);
        }
        _isAttacking = true;
        yield return new WaitForSeconds(_startTimeBtwAttack - _attackWindupTime);
        _isAttacking = false;
    }
    #endregion

    #region backstab_functions

    private void GetBackstabbed(BasePlayerController otherplayerref)
    {
        if (_isDowned) return;
        SpawnShards(otherplayerref, backstabbedShard, NumberOfShardsStolen, 
            (transform.position - otherplayerref.transform.position).normalized);
        shards.LoseShards(NumberOfShardsStolen);
        StartCoroutine(Respawn(3f));
    }

    private IEnumerator Respawn(float spawnDelay)
    {
        
        rend.enabled = false;
        _playerMovement.enabled = false;
        _isDowned = true;
        transform.position = new Vector3(100,100);
        _canBeAttacked = false;
        _rb.isKinematic = true;
        yield return new WaitForSeconds(spawnDelay);
        //need to fix this, it might be exploitable, but for the time being it's okay
        _canBeAttacked = true;
        _isDowned = false;
        _playerMovement.enabled = true;
        transform.position = spawnPoint;
        rend.enabled = true;
        _rb.isKinematic = false;
        
    }

    private IEnumerator BackstabCoroutine()
    {
        playerAnim.SetTrigger(backstabbing);
        //we seriously need to redo this, lol.
        var playersToBackstab = Physics2D.OverlapBoxAll(backstabPos.position, new Vector2(backstabRangeX, backstabRangeY), 0, whatIsPlayer);

        //Loops through all enemies in the circle and deals each one damage
        if (playersToBackstab.Length > 0)
        {
            var playerBeingBackstabbed = playersToBackstab[0].GetComponent<BasePlayerController>();
            playerBeingBackstabbed.GetBackstabbed(this);
        }

        _isBackstabbing = true;
        yield return new WaitForSeconds(_startTimeBtwBackstab);
        _isBackstabbing = false;
    }
    #endregion

    #region health_functions

    private IEnumerator UpdateHealthBar()
    {
        while (true)
        {
            var fillRatio = currentHealth / startingHealth;

            visualHealth.fillAmount = Mathf.Lerp(visualHealth.fillAmount, fillRatio, Time.deltaTime * 5);
            yield return null;
        }
    }

    //Calls this function if the player has taken damage
    public void TakeDamage (float amount, Enemy enemy)
    {

        //Reduces the players health by specified amount
        if (_canBeAttacked && currentHealth > 0)
        {
            FindObjectOfType<AudioManager>().Play("Player_Hurt");
            //Debug.Log("Health is now " + currentHealth);
            currentHealth = Mathf.Max(currentHealth - _damageReduction * amount, 0);
            StartCoroutine(GetKnockedBack(enemy));
            StartCoroutine(Invincible());
        }

        //Changes the health bar's UI to match current health
        //healthSlider.value = currentHealth;

        //Checks to see if player has lost all their health and is not already downed
        if(currentHealth <= 0 && !_isDowned)
        {
            //Player is downed
            FindObjectOfType<AudioManager>().Play("Player_Down");
           
            Down();
        }
    }

    //Calls this function if the player is downed
    private void Down()
    {
        //Indicates that the player is already downed, so they cannot be downed again
        //Debug.Log("Downed!");
        _isDowned = true;
        _canBeAttacked = false;
        //Changes the sprite's color to indicate a player has been downed, until we have an animation to replace it
        _sr.color = Color.red;

        //Prevents the player from being able to move
        _playerMovement.enabled = false;

        playerAnim.SetTrigger(Attacking);
        playerAnim.enabled = false;

        _moveVelocity = Vector2.zero;
        _rb.velocity = Vector2.zero;
        _rb.isKinematic = true;

        //        transform.Rotate(Vector3.forward * 90);
        if (_healthOnRevive <= 0)
        {
            changeText.text = "Dead";
        }


        changeText.text = "Hold " + hold + " to revive!";
        if(facingLeft)
        {
            Flip(1);
        }

        SpawnShards(this, downShard, ShardSystem.NumberOfShardsOnDown, Vector3.zero);
        shards.LoseShards(ShardSystem.NumberOfShardsOnDown);

        bool bothDowned = true;
        foreach(var playerRef in GameManager.players)
        {
            bothDowned &= playerRef.IsDowned();
        }
        if (bothDowned)
        {
            gameManager.GameLost();
        }
    }

    private void SpawnShards(BasePlayerController playerRef, GameObject item, int number, Vector3 pos)
    {
        number = Math.Min(shards.GetNumShards(), number);
        for (var i = 0; i < number; i++)
        {
            Debug.Log("test");
            var randNumX = Random.Range(-1f, 1f);
            var randNumY = Random.Range(-1f, 1f);
            var vec = new Vector3(randNumX, randNumY);
            pos += vec;
            var go = Instantiate(item, transform.position + pos, transform.rotation);
            go.GetComponent<Shard>().target = playerRef;
        }
    }

    public bool IsDowned()
    {
        return _isDowned;
    }

    private IEnumerator Invincible()
    {
        _canBeAttacked = false;
        //_sr.color = Color.magenta;
        int timesToFlash = 3;
        for (int i = 0; i < timesToFlash; i++)
        {
            rend.enabled = false;
            yield return new WaitForSeconds(maxInvincibilityTime/(timesToFlash*2));
            rend.enabled = true;
            yield return new WaitForSeconds(maxInvincibilityTime/(timesToFlash*2));
        }
        if (!_isDowned)
        {
            _sr.color = Color.white;
        }
        _canBeAttacked = true;
    }

    private IEnumerator GetKnockedBack(Enemy enemy)
    {
        playerAnim.SetTrigger(isHit);
        _canMove = false;
        Vector2 direction = transform.position - enemy.transform.position;
        direction.Normalize();
            //Debug.Log(direction);
        _rb.velocity = direction * beingKnockedBackStrength;
        yield return new WaitForSeconds(beingKnockedBackTime);
        _canMove = true;
        _rb.velocity = Vector2.zero;
    }
    #endregion

    #region revive_functions

    private void Revive()
    {
        //Debug.Log(currDownedPlayer);
        _currDownedPlayer.GetRevived();
        _currDownedPlayer = null;
    }

    private void GetRevived()
    {
        if (_healthOnRevive > 0)
        {
            _healthOnRevive -= ReviveCofactor * startingHealth;
        }
        _rb.isKinematic = false;
        currentHealth = _healthOnRevive;
        _isDowned = false;
//        _currDownedPlayer.transform.Rotate(Vector3.forward * -90);
        _canBeAttacked = true;
        _sr.color = Color.white;
        playerAnim.enabled = true;
        _playerMovement.enabled = true;
        changeText.text = "";
    }
    

    protected void OnTriggerEnter2D(Collider2D downedPlayer)
    {
        if (!downedPlayer.CompareTag(otherplayer)) return;
        ////Debug.Log(downedPlayer.tag);
        _currDownedPlayer = downedPlayer.GetComponent<BasePlayerController>();
        if (!_currDownedPlayer._isDowned)
        {
            _currDownedPlayer = null;
        }
    }

    protected void OnTriggerExit2D(Collider2D downedPlayer)
    {
        if (!downedPlayer.CompareTag(otherplayer)) return;
        if (downedPlayer.GetComponent<BasePlayerController>() == _currDownedPlayer)
        {
            _currDownedPlayer = null;
        }
    }

    #endregion

    #region equipment_functions
    public void SetPlayerEquipment(Equipment newEquipment)
    {
        _equipment = newEquipment;
        playerAnim.SetInteger("ArmorClass", (int) _equipment.GetArmorClass());
        _equipmentWasUpdated = true;
    }

    public Equipment GetPlayerEquipment()
    {
        return _equipment;
    }

    private void UpdatePlayerStats()
    {
        if (!_equipmentWasUpdated) return;
        StartCoroutine(PopUpText());
        _speed = baseSpeed * _equipment.GetMoveSpeed();
        _damage = baseDamage * _equipment.GetWeaponDamage();
        _damageReduction = baseDamageReduction * _equipment.GetDamageReduction();
        _startTimeBtwAttack = _equipment.GetAttackCooldown();
        _startTimeBtwBackstab = baseStartTimeBtwBackstab * _equipment.GetBackstabCooldown();
        _attackWindupTime = _equipment.GetAttackWindup();

        attackRange = _equipment.GetAttackRange();
        knockback = knockback * _equipment.GetKnockback();

        _equipmentWasUpdated = false;
    }

    private IEnumerator PopUpText()
    {
        //Debug.Log("It gets called, Arman");
        _temp = "";
        if (!changeText.text.Contains("Armor: ")) { 
            _temp = changeText.text;
        }
        changeText.text = "Armor: " + _equipment.GetName();
        yield return new WaitForSeconds(2f);
        changeText.text = _temp;
    }
    #endregion

    #region getter_functions
    public Rigidbody2D GetRigidBody()
    {
        return _rb;
    }
    #endregion
}
