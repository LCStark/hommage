using System;
using UnityEngine;
using System.Collections.Generic;

public class SavedCamp {
    public Vector2i gamePosition;
    public int current;
    public int max;
}

public static class PersistentData
{
public enum State { WorldMap, EnteringCombat, ExitingCombat };
public static State state = State.WorldMap;

public static MonsterGroup monsterGroup;
public static MonsterGroup playerArmy;

// World data
public static Vector2i playerPosition;
public static int movementPoints;
public static List<MonsterGroup> monsterGroups;
public static List<SavedCamp> recruitmentCamps;
public static int day;
public static int week;
public static int month;

public static void Clear() {
    monsterGroup = null;
    playerArmy = null;
    playerPosition = null;
    movementPoints = 0;
    monsterGroups = null;
    recruitmentCamps = null;
    day = 0;
    week = 0;
    month = 0;
    state = State.WorldMap;
}

public static void StartCombat(MonsterGroup monsters, List<Army> playerArmies) {
    PersistentData.monsterGroup = monsters;

    MonsterGroup army = new MonsterGroup(playerArmies.Count);
    for (int i = 0; i < playerArmies.Count; i++) {
        army.armies[i] = new Army(playerArmies[i].creatureID, playerArmies[i].count);
    }
    PersistentData.playerArmy = army;

    state = State.EnteringCombat;
}

public static void EndCombat(List<Army> playerArmies) {
    MonsterGroup army = new MonsterGroup(playerArmies.Count);
    for (int i = 0; i < playerArmies.Count; i++) {
        army.armies[i] = playerArmies[i];
    }
    PersistentData.playerArmy = army;
        
    int remove = -1;
    for (int i = 0; i < monsterGroups.Count; i++) {
        MonsterGroup group = monsterGroups[i];
        if (group.id == monsterGroup.id) {
            remove = i;
            break;
        }
    }
    monsterGroups.RemoveAt(remove);

    state = State.ExitingCombat;
}

public static void SaveRecruitmentCamps(List<RecruitmentCamp> camps) {
    recruitmentCamps = new List<SavedCamp>();
    for (int i = 0; i < camps.Count; i++) {
        SavedCamp camp = new SavedCamp {
            gamePosition = camps[i].gamePosition,
            max = camps[i].max,
            current = camps[i].current
        };
        recruitmentCamps.Add(camp);
    }
}

public static void SaveMonsterGroups(MonsterGroup[] groups, int count) {
    monsterGroups = new List<MonsterGroup>();
    for (int i = 0; i < count; i++) {
        monsterGroups.Add(groups[i]);
    }
}

}
