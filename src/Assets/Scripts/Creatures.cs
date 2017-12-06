using System;

public struct Creature {

public string name;
public int initiative;
public int speed;
public int hp;
public int damage;
public int attack;
public int defense;

}

public class Army {

public int creatureID;
public int count;

public Army() {}

public Army(int creatureID, int count) {
    this.creatureID = creatureID;
    this.count = count;
}

}

public class MonsterGroup {

public Army[] armies;
public int armyCount;
public Vector2i gamePosition;
public int id;

public MonsterGroup(int armyCount) {
    this.armyCount = armyCount;
    armies = new Army[armyCount];
}

public int GetGroupCount() {
    int count = 0;
    for (var i = 0; i < armyCount; i++) count += armies[i].count;
    return count;
}

}

public static class Creatures {

private static Creature[] creatures;

public static void PrepareCreatures() {
    if (creatures != null) return;
    creatures = new Creature[3];
    creatures[0] = new Creature {
        name = "Knight",
        initiative = 10,
        speed = 5,
        hp = 20,
        damage = 3,
        attack = 10,
        defense = 10
    };
    creatures[1] = new Creature {
        name = "Imp",
        initiative = 12,
        speed = 6,
        hp = 10,
        damage = 2,
        attack = 15,
        defense = 7
    };
    creatures[2] = new Creature {
        name = "Goblin",
        initiative = 6,
        speed = 3,
        hp = 30,
        damage = 4,
        attack = 8,
        defense = 11
    };
}

public static Creature Get(int id) {
    return creatures[id];
}

public static string GetName(int id) {
    return creatures[id].name;
}

}