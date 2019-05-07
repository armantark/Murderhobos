using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShardRadius : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        var parent = GetComponentInParent<Shard>();
        if (parent.canFollow)
        {
            var target = GetComponentInParent<Shard>().target;
//        Debug.Log(target);
            parent.isFollowing |= collision.CompareTag("Player_1") && target.player == "Player_1" || collision.CompareTag("Player_2") && target.player == "Player_2";
            //Destroy(this);
        }
    }
}
