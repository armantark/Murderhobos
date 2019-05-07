using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skeleton : Enemy
{
    private Animator _skeletonAnim;
    private static readonly int Death = Animator.StringToHash("death");
    private static readonly int Attack = Animator.StringToHash("attack");
    private static readonly int TakenDamage = Animator.StringToHash("takenDamage");

    private new void Start()
    {
        _skeletonAnim = GetComponent<Animator>();
//        SetAttackTimer(0.8f);
//        SetDamageTimer(0.8f);
        base.Start();
    }

    protected override IEnumerator TakeDamageAnim()
    {
        _skeletonAnim.SetTrigger(TakenDamage);
        return base.TakeDamageAnim();
    }

    protected override IEnumerator AttackTimer()
    {
        //        Debug.Log("trying to attacc in child");
        _skeletonAnim.SetTrigger(Attack);
        return base.AttackTimer();
    }

    protected override void DeathAnim()
    {
        _skeletonAnim.SetTrigger(Death);
    }
}
