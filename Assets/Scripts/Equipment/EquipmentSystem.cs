using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EquipmentSystem : MonoBehaviour
{
    #region equipment
    Dictionary<Equipment.ArmorClass, Equipment> equipment;
    #endregion

    #region ecl_fields
    BasePlayerController[] players;
    const int levels = 4;
    const float shards_for_last_level = 10;
    const float shards_for_first_level = 1;

    float invB, invA; // for calculating the ecl

    #endregion

    // Use this for initialization
    void Start()
    {
        double B = Mathf.Log(shards_for_last_level / shards_for_first_level) / (levels - 1.0);
        double A = shards_for_first_level / (Mathf.Exp((float) B) - 1.0);

        invB = 1 / (float) B;
        invA = 1 / (float) A;

        players = GetPlayers();

        // get these declarations somewhere more appropriate
        equipment = new Dictionary<Equipment.ArmorClass, Equipment>();
        equipment.Add(0, new Equipment(1.2f, 0.8f, 0.636f, 0.7f, 0.8f, 1.3f, 1.2f, 0.1f, "Light", Equipment.ArmorClass.Light, 1));
        equipment.Add((Equipment.ArmorClass) 1, new Equipment(1, 1, 0.778f, 1, 0.95f, 1, 1, 0.1f, "Medium", Equipment.ArmorClass.Medium, 1));
        equipment.Add((Equipment.ArmorClass)2, new Equipment(0.7f, 1.2f, 1f, 1.3f, 1.05f, 0.6f, 0.7f, 0.6f, "Heavy", Equipment.ArmorClass.Heavy, 1));
    }

    // Update is called once per frame
    void Update()
    {
        Equipment[] player_equipment = DeterminePlayerEquipment();
        ChangePlayerEquipment(player_equipment);
    }


    //what if players dont exist

    /*
     * Get the player objects from the game
     */
    BasePlayerController[] GetPlayers()
    {
        BasePlayerController[] player_list = FindObjectsOfType<BasePlayerController>();
        Debug.Log(player_list.Length);
        return player_list;
    }

    /*
     * Gets the number of shards for each player   
     */
    int GetPlayerShards(BasePlayerController player)
    {
        return player.shards.GetNumShards();
    }

    float CalculateEffectiveCharacterLevel(int shards)
    {
        return Mathf.Floor(invB * Mathf.Log((shards + 1) * invA));
    }


    //what if players dont exist

    /*
     * Determines the appropriate equipment for players based on their
     * relative levels    
     */
    Equipment[] DeterminePlayerEquipment()
    {
        int player1_shards = GetPlayerShards(players[0]);
        int player2_shards = GetPlayerShards(players[1]);

        float player1_ecl = CalculateEffectiveCharacterLevel(player1_shards);
        float player2_ecl = CalculateEffectiveCharacterLevel(player2_shards);

        int diff = (int) (player2_ecl - player1_ecl);
        Equipment.ArmorClass p1, p2;
        if (diff > 0) {
            if (diff == 1)
            {
                //assign p2 med, p1 light
                p1 = 0;
                p2 = (Equipment.ArmorClass) 1;
            }
            else
            {
                //assign p2 heavy, p1 light
                p1 = 0;
                p2 =(Equipment.ArmorClass) 2;
            }
        } else if (diff < 0) {
            if (diff == -1)
            {
                //assign p1 med, p2 light
                p1 = (Equipment.ArmorClass) 1;
                p2 = 0;
            }
            else
            {
                //assign p1 heavy, p2 light
                p1 = (Equipment.ArmorClass) 2;
                p2 = 0;
            }
        } else {
            //assign both medium
            p1 = (Equipment.ArmorClass) 1;
            p2 = (Equipment.ArmorClass) 1;
        }
        return new Equipment[] {equipment[p1], equipment[p2]};
        //should fix this with more intelligent updates
    }

    //what if players don't exist?

    /*
     * Automatically re-equips new equipment to players when
     * change criteria in DeterminePlayerEquipment are met    
     */
    void ChangePlayerEquipment(Equipment[] equipment)
    {
        Equipment player1_equipment = players[0].GetPlayerEquipment();
        if (!player1_equipment.Equals(equipment[0]))
        {
            // do some equipment change animation and notification here
            players[0].SetPlayerEquipment(equipment[0]);
        }

        Equipment player2_equipment = players[1].GetPlayerEquipment();
        if (!player2_equipment.Equals(equipment[1]))
        {
            // do some equipment change animation and notification here
            players[1].SetPlayerEquipment(equipment[1]);
        }
    }
}
