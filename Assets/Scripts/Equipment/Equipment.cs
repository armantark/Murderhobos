using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Equipment
{
    public enum ArmorClass {Light = 0, Medium = 1, Heavy = 2}

    #region armor_fields

    private float damage_reduction_pct; // damage reduction percentage from enemies
    private float knockback_pct; // knockback change on class

    #endregion

    #region weapon_fields
    private float enemy_weapon_pct; // attack increase percentage on enemy

    // the formula for this is (frames + gaps) / samples
    private float enemy_attack_cooldown; // cooldown change on enemy attack
    private float enemy_attack_windup; // windup time on enemy attack
    private float backstab_cooldown_pct; // cooldown change on attack
    private float attack_range;

    #endregion


    #region equipment_fields

    private string name; // equipment name
    private ArmorClass armor_class; // light, medium, or heavy
    private float level; // armor level
    private float movespeed_pct; // character movespeed with this armor

    #endregion

    public Equipment(float damage_red, float enemy_w_pct, float enemy_ac, 
        float backstab_c_pct, float range, float move_pct, float knock, float windup, string n, ArmorClass a_class,
        float lev)
    {
        damage_reduction_pct = damage_red;
        enemy_weapon_pct = enemy_w_pct;
        enemy_attack_cooldown = enemy_ac;
        backstab_cooldown_pct = backstab_c_pct;
        attack_range = range;
        movespeed_pct = move_pct;
        knockback_pct = knock;
        enemy_attack_windup = windup;
        name = n;
        armor_class = a_class;
        level = lev;
    }

    #region getter_methods
    public bool Equals(Equipment other)
    {
        return armor_class == other.armor_class;
    }

    public float GetDamageReduction()
    {
        return damage_reduction_pct;
    }

    public float GetWeaponDamage()
    {
        return enemy_weapon_pct;
    }

    public float GetAttackCooldown()
    {
        return enemy_attack_cooldown;
    }

    public float GetBackstabCooldown()
    {
        return backstab_cooldown_pct;
    }

    public float GetKnockback()
    {
        return knockback_pct;
    }

    public string GetName()
    {
        return name;
    }

    public ArmorClass GetArmorClass()
    {
        return armor_class;
    }

    public float GetLevel()
    {
        return level;
    }

    public float GetMoveSpeed()
    {
        return movespeed_pct;
    }

    public float GetAttackRange()
    {
        return attack_range;
    }

    public float GetAttackWindup()
    {
        return enemy_attack_windup;
    }
    #endregion
}
