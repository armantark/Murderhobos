using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    #region movement_variables
    public float speed;
    protected Vector3 _tempPosition;
    protected bool _paused;
    #endregion

    #region physics_components
    protected Rigidbody2D _rb;
    protected SpriteRenderer _sr;
    #endregion

    #region targeting_variables
    public BasePlayerController[] players;
    #endregion

    #region health_variables
    public float health;

    //knockback variables
    public float beingKnockedBackTime;
    public float beingKnockedBackStrength;
    #endregion

    #region damage_variables
    public float attackTimer;
    protected bool _isAttacking;
    public int damage;
    public float damageTimer;
    public Vector2 curDirection;
    public float windupTime;
    #endregion

    #region shard_variables
    public GameObject shard;
    public GameObject healthorb;
    public float chanceToDropShard;
    public float chanceToDropOrb;
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



    #region health_functions
    public void TakeDamage(float damage, BasePlayerController player)
    {
        health -= damage;
        //Debug.Log("Damage taken!");
        StartCoroutine(TakeDamageAnim());

        //Enemy knockback
        StartCoroutine(GetKnockedBack(player));
        if (health <= 0)
        {
            FindObjectOfType<AudioManager>().Play("Skeleton_Death");
            StartCoroutine(Die(player));
        }
    }

    private IEnumerator Die(BasePlayerController player)
    {
        yield return new WaitForSeconds(beingKnockedBackTime);
        _isAttacking = true; //just so it doesn't attack while it's dying because that makes 0 sense
//        _paused = true;
        _rb.simulated = false;
        DeathAnim();
        const float deathAnimLength = 2f; //should be calculated dynamically
        yield return new WaitForSeconds(deathAnimLength);
        var randNum = Random.Range(0.0f, 1.0f);
        if (randNum <= chanceToDropOrb)
        {
            //Debug.Log(randNum);
            SpawnItem(player, healthorb, 1);
        }
        else if (randNum <= chanceToDropShard)
        {
            //Debug.Log(randNum);
            SpawnItem(player, shard, 1);
        }
        EnemyManager.numEnemies--;
        Destroy(gameObject);
    }

    private void SpawnItem(BasePlayerController player, GameObject item, int number)
    {
        for (var i = 0; i < number; i++)
        {
//            Debug.Log("test");
            var randNumX = Random.Range(-0.5f, 0.5f);
            var randNumY = Random.Range(-0.5f, 0.5f);
            var vec = new Vector3(randNumX, randNumY);
            var go = Instantiate(item, transform.position + vec, transform.rotation);
            go.GetComponent<Shard>().target = player;
        }
    }

    protected virtual IEnumerator TakeDamageAnim()
    {
        _sr.color = Color.red;
        yield return new WaitForSeconds(damageTimer);
        _sr.color = Color.white;
    }

    protected void SetDamageTimer(float newTimer)
    {
        damageTimer = newTimer;
    }

   protected virtual void DeathAnim()
    {
    }

    #endregion

    #region movement_functions
    //calculate the closest player so that the enemy will go there, borrowed from 
    //https://forum.unity.com/threads/clean-est-way-to-find-nearest-object-of-many-c.44315/
    protected Transform ClosestPlayer()
    {
        Transform bestTarget = null;
        var closestDistanceSqr = Mathf.Infinity;
        var currentPosition = transform.position;
        var bothAreDowned = true;
        foreach (var potentialTarget in players)
        {
            var directionToTarget = potentialTarget.transform.position - currentPosition;
            var dSqrToTarget = directionToTarget.sqrMagnitude;
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
        return bothAreDowned ? transform : bestTarget;
    }

    //creates a sort of stepping motion, instead of a boring straight movement

    protected virtual IEnumerator MovingCoroutine()
    {

        //perhaps make these into serialized variables?
        var originalMS = speed;
        float elapsedTime = 0;
        const float speedCofactor = 0.1f;
        while (true)
        {
            while (_paused)
            {
                yield return null;
            }
            speed *= Time.deltaTime;
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

    private IEnumerator GetKnockedBack(Component player)
    {
        _paused = true;
        Vector2 direction = transform.position - player.transform.position;
        direction.Normalize();
//        Debug.Log(direction);
        _rb.velocity = direction * beingKnockedBackStrength;
        yield return new WaitForSeconds(beingKnockedBackTime);
        _paused = false;
        _rb.velocity = Vector2.zero;
    }
    #endregion

    #region attack_functions
    protected void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(new Vector2(0, 0) + curDirection, new Vector2(0.7f, 2.5f));
    }

    private void DealDamage(Collider2D other)
    {
        //perhaps too much repeated logic between here and attacktimer, I wonder if there's a different way
        if (!_isAttacking && CheckIfPlayerTag(other.transform)) {
            var player = other.transform.GetComponent<BasePlayerController>();
            if (!player.IsDowned())
            {
                    
                StartCoroutine(AttackTimer());
            }
        }
    }

    protected virtual IEnumerator AttackTimer()
    {
        _isAttacking = true;
        FindObjectOfType<AudioManager>().Play("Skeleton_Attack");
        _rb.velocity = Vector2.zero;
        var temp = speed;
        speed = 0;
        //should be calculated dynamically
        yield return new WaitForSeconds(windupTime);
        speed = temp;
        var box = new Vector2(0.7f, 1.5f); // should make hitbox its own field
        //while (_paused) ;
        var hits = Physics2D.BoxCastAll(_rb.position + curDirection, box, 0f, Vector2.zero, 0f);
        foreach (var hit in hits)
        {
            if (CheckIfPlayerTag(hit.transform))
            {
                var player = hit.transform.GetComponent<BasePlayerController>();
                player.TakeDamage(damage, this);
            }
        }
        yield return new WaitForSeconds(attackTimer - windupTime);

        _isAttacking = false;
    }

    protected void SetAttackTimer(float newTimer)
    {
        attackTimer = newTimer;
    }

    protected void OnTriggerEnter2D(Collider2D collision)
    {
        DealDamage(collision);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        DealDamage(other);
    }

    #endregion

    private static bool CheckIfPlayerTag(Component transform)
    {
        return transform.CompareTag("Player_1") || transform.CompareTag("Player_2");
    }
}
