using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Imp : Enemy
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
}
