using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CombatMonsterGroup : MonoBehaviour {

public int creatureId;
public int count;
public int hp;
public int initiative;
public int playerId;
public int armyId;
public Vector2i gamePosition;
public bool hasMoved;
public bool hasAttacked;
public bool dead;

public void SetColor(Color color) {
    Renderer[] renderers = GetComponentsInChildren<Renderer>();
    foreach (var renderer in renderers) renderer.material.color = color;
}

public void SetCounter(int count) {
    transform.Find("Canvas").Find("Count").GetComponent<Text>().text = count.ToString();
    this.count = count;
}

public void SetHP(int count) {
    transform.Find("Canvas").Find("HP").GetComponent<Text>().text = count.ToString() + " / " + Creatures.Get(creatureId).hp;
    this.hp = count;
}

public void ApplyDamage(int damage) {
    int currentHP = hp;
    if (damage > currentHP) {
        hp = Creatures.Get(creatureId).hp;
        count--;
        if (count == 0) {
            dead = true;
            return;
        }
        damage -= currentHP;
        while (damage >= Creatures.Get(creatureId).hp) {
            damage -= Creatures.Get(creatureId).hp;
            count--;
            if (count == 0) {
                dead = true;
                return;
            }
        }
    }
    hp -= damage;
    SetCounter(count);
    SetHP(hp);
}

}
