using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShardSystem : MonoBehaviour
{
    [SerializeField] private Text shardText;

    private int _shardValue;

    public const int NumberOfShardsOnDown = 3;
    private const int NumberOfShardsToWin = 20;

    public GameManager gameManager;

    public BasePlayerController player;

    private void Awake()
    {
        UpdateText();
    }
    
    public void LoseShard()
    {
         if (_shardValue > 0) {
            _shardValue -= 1;
         }
         UpdateText();
    }

    public void GainShard()
    {
        _shardValue += 1;
        FindObjectOfType<AudioManager>().Play("Gain_Shard");
        UpdateText();
        if (_shardValue >= ShardSystem.NumberOfShardsToWin)
        {
            gameManager.GameWon(player.player);
        }
    }

    public int GetNumShards()
    {
        return _shardValue;
    }

    public void LoseShards(int value)
    {
        _shardValue -= value;
        _shardValue = (int) Mathf.Clamp(_shardValue, 0, Mathf.Infinity);
        UpdateText();
    }
    public void GainShards(int value)
    {
        _shardValue += value;
        UpdateText();
        if (_shardValue >= ShardSystem.NumberOfShardsToWin)
        {
            gameManager.GameWon(player.player);
        }
    }

    private void UpdateText()
    {
        shardText.text = "Shards: ";
        shardText.text += _shardValue; 
        shardText.text += "/" + NumberOfShardsToWin;
    }
}
