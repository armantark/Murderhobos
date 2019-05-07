using System.Collections;
using UnityEngine;

public class Shard : MonoBehaviour
{
//    public float radius = 3f;
    public BasePlayerController target;
    Vector2 _shardVelocity = Vector2.zero;
    private float _minModifier = 7;
    private float _maxModifier = 11;
    public bool isFollowing;
    public float despawnTime;
    public float secondsToStartFlashing = 5;
    private Renderer _rend;
    public bool canFollow;

    private void Start()
    {
        _rend = GetComponent<Renderer>();
        SetColor();
        StartCoroutine(DespawnTimer());
    }

    private void Update()
    { 
        if (isFollowing && canFollow)
        {
            transform.position = Vector2.SmoothDamp(transform.position, target.transform.position, ref _shardVelocity, Time.deltaTime * Random.Range(_minModifier, _maxModifier));
            if (System.Math.Abs(transform.position.x - target.transform.position.x) < 0.1 &&
                System.Math.Abs(transform.position.y - target.transform.position.y) < 0.1)
            {
                AffectPlayer();
                Destroy(gameObject);
            }
        }
    }

    protected virtual void SetColor()
    {
        GetComponent<SpriteRenderer>().color = target.color;
    }
    protected virtual void AffectPlayer()
    {
        target.shards.GainShard();
    }
    IEnumerator DespawnTimer()
    {
        yield return new WaitForSeconds(despawnTime-secondsToStartFlashing);
        const int timesToFlash = 10;
        for (var i = 0; i < timesToFlash; i++)
        {
            _rend.enabled = false;
            if (isFollowing)
            {
                _rend.enabled = true;
                break;
            }
            yield return new WaitForSeconds(secondsToStartFlashing/(timesToFlash*2));
            _rend.enabled = true;
            yield return new WaitForSeconds(secondsToStartFlashing/(timesToFlash*2));
        }
        Destroy(gameObject);
    }

    
}
