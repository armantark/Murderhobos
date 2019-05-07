using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wraith : Enemy
{
    private Animator _wraithAnim;
    private static readonly int Death = Animator.StringToHash("death");
    private static readonly int Attack = Animator.StringToHash("attack");

    private new void Start()
    {
        _wraithAnim = GetComponent<Animator>();
        //        SetAttackTimer(0.8f);
        //        SetDamageTimer(0.8f);
        base.Start();
    }

    protected override IEnumerator MovingCoroutine()
    {
        var originalMS = speed;
        float elapsedTime = 0;
        const float totalTransitionTime = 10;
        const float speedCofactor = 0.1f;
        while (true)
        {
            while (_paused)
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
            scale.x = direction.x < 0 ? 1 : -1;
            transform.localScale = scale;
            curDirection = direction.x < 0 ? Vector2.left : Vector2.right;
            yield return null;
        }
    }

    protected override IEnumerator AttackTimer()
    {
        //        Debug.Log("trying to attacc in child")

        _wraithAnim.SetTrigger(Attack);
        return base.AttackTimer();
    }

    protected override void DeathAnim()
    {
        _wraithAnim.SetTrigger(Death);
        StartCoroutine(FadeAway());
    }

    private IEnumerator FadeAway()
    {
        float elapsedTime = 0f;
        float totalTransitionTime = 2f;
        while (true)
        {
            _sr.color = Color.Lerp(_sr.color, new Color(1,1,1,0), elapsedTime / totalTransitionTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
}
