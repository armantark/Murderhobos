using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    #region movement_variables
    public float speed;
    private Vector3 _tempPosition;
    private bool paused;
    #endregion

    #region physics_components
    private Rigidbody2D _rb;
    private SpriteRenderer _sr;
    #endregion

    #region targeting_variables
    public BasePlayerController[] players;
    #endregion

    #region health_variables
    public float health;

    //knockback variables
    public float knockback;
    public float knockbackLength;
    public float knockbackCount;
    public bool knockFromRight;
    public bool knockFromTop;
    #endregion

    #region damage_variables
    public float attackTimer;
    private bool _isAttacking;
    public int damage;
    public float damageTimer;
    public Vector2 curDirection;
    #endregion

    #region shard_variables
    public GameObject shard;
    public float chanceToDrop;
    #endregion


    // Start is called before the first frame update
    protected void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _sr = GetComponent<SpriteRenderer>();
        _tempPosition = ClosestPlayer().position;
        StartCoroutine(MovingCoroutine());
        players = GameManager.players;
        //shardTarget = GameObject.FindGameObjectWithTag("ShardTracker");
        EnemyManager.numEnemies++;
        curDirection = Vector2.left;
    }

    // Update is called once per frame
    private void Update()
    {
        //Checks to see if knocked back
        if (knockbackCount > 0)
        {
            paused = true;
            if (knockFromRight)
            {
                _rb.velocity = new Vector2(-knockback, 0);
                Debug.Log("Knocked back from the right");
            }
            if (!knockFromRight)
            {
                _rb.velocity = new Vector2(knockback, 0);
            }
            if (knockFromTop)
            {
                _rb.velocity = new Vector2(0, -knockback);
            }
            if (!knockFromTop)
            {
                _rb.velocity = new Vector2(0, knockback);
            }
            knockbackCount -= Time.deltaTime;
        }
        else
        {
            paused = false;
        }
    }


    #region health_functions
    public void TakeDamage(float damage, BasePlayerController player)
    {
        health -= damage;
        Debug.Log("Damage taken!");
        StartCoroutine(TakeDamageAnim());

        //Enemy knockback
        knockbackCount = knockbackLength;
        knockFromRight = transform.position.x < player.transform.position.x;
        knockFromTop = transform.position.y < player.transform.position.y;
        if (health <= 0)
        {
            StartCoroutine(Die(player));
        }
    }

    IEnumerator Die(BasePlayerController player)
    {
        paused = true;
        DeathAnim();
        float randNum = Random.Range(0.0f, 1.0f);
        if (randNum <= chanceToDrop)
        {
            Debug.Log(randNum);
            GameObject go = Instantiate(shard, transform.position, shard.transform.rotation); //drops the shard upon death
            go.GetComponent<Shard>().target = player;
        }

        EnemyManager.numEnemies--;
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }

    virtual protected IEnumerator TakeDamageAnim()
    {

        _sr.color = Color.red;
        yield return new WaitForSeconds(damageTimer);
        _sr.color = Color.white;
    }

    protected void ChangeDamageTimer(float newTimer)
    {
        damageTimer = newTimer;
    }

   virtual protected void DeathAnim()
    {
        return;
    }

    #endregion

    #region movement_functions
    //calculate the closest player so that the enemy will go there, borrowed from 
    //https://forum.unity.com/threads/clean-est-way-to-find-nearest-object-of-many-c.44315/
    Transform ClosestPlayer()
    {
        Transform bestTarget = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;
        bool bothAreDowned = true;
        foreach (BasePlayerController potentialTarget in players)
        {
            Vector3 directionToTarget = potentialTarget.transform.position - currentPosition;
            float dSqrToTarget = directionToTarget.sqrMagnitude;
            if (dSqrToTarget < closestDistanceSqr && !potentialTarget.IsDowned())
            {
                closestDistanceSqr = dSqrToTarget;
                bestTarget = potentialTarget.transform;
            }
            if (!potentialTarget.IsDowned())
            {
                bothAreDowned = false;
            }
        }
        if (bothAreDowned)
        {
            return transform;
        }
        return bestTarget;
    }

    //creates a sort of stepping motion, instead of a boring straight movement

    protected IEnumerator MovingCoroutine()
    {

        //perhaps make these into serialized variables?
        var originalMS = speed;
        float elapsedTime = 0;
        const float totalTransitionTime = 10;
        var speedCofactor = 0.1f;
        while (true)
        {
            while (paused)
            {
                yield return null;
            }
            speed = Mathf.Lerp(speed, 0, elapsedTime / totalTransitionTime);
            elapsedTime += Time.deltaTime;
            if (speed <= speedCofactor * originalMS)
            {
                _tempPosition = ClosestPlayer().position;
                speed = originalMS;
                elapsedTime = 0;
            }       
            Vector2 direction = _tempPosition - transform.position;
            if (!_isAttacking)
            {

                _rb.velocity = direction.normalized * speed;
            }
            else
            {
                _rb.velocity = Vector2.zero;
            }
            var scale = transform.localScale;
            scale.x = direction.x < 0? 1 : -1;
            transform.localScale = scale;
            curDirection = direction.x < 0 ? Vector2.left : Vector2.right;
            yield return null;
        }
    }
    #endregion

    #region attack_functions

    private void dealDamage(Collider2D collision)
    {
        if (!_isAttacking) {
            StartCoroutine(AttackTimer());
        }
    }

    virtual protected IEnumerator AttackTimer()
    {
        _isAttacking = true;
        _rb.velocity = Vector2.zero;
        _rb.isKinematic = true;
        yield return new WaitForSeconds(attackTimer);
        _rb.isKinematic = false;
        Vector2 box = new Vector2(1, 2.5f);
        RaycastHit2D[] hits = Physics2D.BoxCastAll(_rb.position + curDirection, box, 0f, Vector2.zero, 0f);
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.transform.tag == "Player_1")
            {
                var player1 = hit.transform.GetComponent<PlayerController1>(); //variable for player 1

                //Debug.Log("Damaging player 1");
                player1.TakeDamage(damage); //changed this because I made a variable for player 1

                // Player 1 knockback
                player1.knockbackCount = player1.knockbackLength;
                player1.knockFromRight = player1.transform.position.x < transform.position.x;
                player1.knockFromTop = player1.transform.position.y < transform.position.y;
            }
            else if (hit.transform.tag == "Player_2")
            {
                var player2 = hit.transform.GetComponent<PlayerController2>(); //variable for player 2

                //Debug.Log("Damaging player 2");
                player2.TakeDamage(damage);

                // Player 2 knockback
                player2.knockbackCount = player2.knockbackLength;
                player2.knockFromRight = player2.transform.position.x < transform.position.x;
                player2.knockFromTop = player2.transform.position.y < transform.position.y;
            }
        }

        _isAttacking = false;
    
    }

    protected void ChangeAttackTimer(float newTimer)
    {
        attackTimer = newTimer;
    }

    protected void OnTriggerEnter2D(Collider2D collision)
    {
        dealDamage(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        dealDamage(collision);
    }
    #endregion
}
