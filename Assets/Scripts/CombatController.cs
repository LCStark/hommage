using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CombatController : MonoBehaviour {

public Tile tilePrefab;
public GameObject obstaclePrefab;
public CombatMonsterGroup monsterPrefab;
public CombatMonsterGroup knightPrefab;
public TimelineTurn timelineTurnPrefab;

public GameObject combatMap;
public GameObject timeline;
public GameObject armyContainer;
public GameObject turnTitle;
public GameObject contextGUI;
public GameObject lostCanvas;
public GameObject wonCanvas;

public Text debugPosition;

public int mapWidth;
public int mapHeight;
private int mapDX;
private int mapDY;

private Tile[,] tileContainer;
private CameraController mainCamera;

private int monsterSpread;
private int monsterStart;
private MonsterGroup monsters;
private int knightSpread;
private int knightStart;
private MonsterGroup knights;

private List<CombatMonsterGroup> armies;

private Pathfinder pathfinder;
private int currentPlayer;
private CombatMonsterGroup currentArmy;

private bool[,] terrainPassable = new bool[11, 15] {
    { true, true, true, true, true, true, true, true, true, true, true, true, true, true, true },
    { true, true, true, true, true, true, true, true, true, true, true, true, true, true, true },
    { true, true, false, true, true, true, false, false, true, true, true, true, true, true, true },
    { true, true, false, false, true, true, false, false, true, true, true, true, true, true, true },
    { true, true, true, false, true, true, true, true, true, true, true, true, true, true, true },
    { true, true, true, true, true, true, true, true, true, true, true, true, true, true, true },
    { true, true, true, true, true, true, true, true, true, true, true, false, true, true, true },
    { true, true, true, true, true, true, false, false, true, true, true, false, false, true, true },
    { true, true, true, true, true, true, false, false, true, true, true, true, false, true, true },
    { true, true, true, true, true, true, true, true, true, true, true, true, true, true, true },
    { true, true, true, true, true, true, true, true, true, true, true, true, true, true, true }
};

private bool[,] currentTerrain;

private List<Tile> highlightedTiles;

private bool combatOver;

void PlaceMonsterArmy(Tile tile, Vector3 position, Vector2i gamePosition, int armyID) {
    CombatMonsterGroup group = Instantiate(monsterPrefab, armyContainer.transform);
    group.transform.position = position;
    group.transform.Find("Monster").transform.rotation = Quaternion.Euler(0.0f, 90.0f, 0.0f);
    group.SetCounter(monsters.armies[armyID].count);
    group.creatureId = monsters.armies[armyID].creatureID;
    group.initiative = Creatures.Get(group.creatureId).initiative + Random.Range(-2, 3);
    group.SetHP(Creatures.Get(group.creatureId).hp);
    group.playerId = 1;
    group.gamePosition = gamePosition;
    armies.Add(group);
}

void PlaceKnightArmy(Tile tile, Vector3 position, Vector2i gamePosition, int armyID) {
    CombatMonsterGroup group = Instantiate(knightPrefab, armyContainer.transform);
    group.transform.position = position;
    group.transform.Find("Knight").transform.rotation = Quaternion.Euler(0.0f, -90.0f, 0.0f);
    group.SetCounter(knights.armies[armyID].count);
    group.creatureId = knights.armies[armyID].creatureID;
    group.initiative = Creatures.Get(group.creatureId).initiative + Random.Range(-2, 3);
    group.hp = Creatures.Get(group.creatureId).hp;
    group.SetHP(Creatures.Get(group.creatureId).hp);
    group.playerId = 0;
    group.gamePosition = gamePosition;
    armies.Add(group);
}

void GenerateMap() {
    // Set monsters
    monsters = PersistentData.monsterGroup;
    monsterSpread = (int)Mathf.Clamp(mapHeight / monsters.armyCount, 1.0f, 4.0f);
    int freeSpace = mapHeight - monsterSpread * (monsters.armyCount - 1); 
    monsterStart = freeSpace / 2;
    int nextMonsterArmy = 0;
    int nextMonsterPosition = monsterStart;

    // Set knights
    knights = PersistentData.playerArmy;
    knightSpread = (int)Mathf.Clamp(mapHeight / knights.armyCount, 1.0f, 4.0f);
    freeSpace = mapHeight - knightSpread * (knights.armyCount - 1);
    knightStart = freeSpace / 2;
    int nextKnightArmy = 0;
    int nextKnightPosition = knightStart;

    tileContainer = new Tile[mapHeight, mapWidth];
    mapDX = mapWidth / 2;
    mapDY = mapHeight / 2;
    for (var j = 0; j < mapHeight; j++) {
        for (var i = 0; i < mapWidth; i++) {
            int x = i - mapDX;
            int y = j - mapDY;
                
            Vector3 position = new Vector3(
                x * tilePrefab.transform.localScale.x,
                0.0f,
                y * tilePrefab.transform.localScale.z
            );

            Quaternion rotation = new Quaternion();
            Tile groundTile = Instantiate(tilePrefab, position, rotation, combatMap.transform) as Tile;
            groundTile.gamePosition = new Vector2i(x, y);
            groundTile.GetComponent<Renderer>().material.color = new Color(0.375f, 0.375f, 0.375f);
            groundTile.passable = terrainPassable[y + mapDY, x + mapDX];

            if (!terrainPassable[y+mapDY,x+mapDX]) {
                Instantiate(obstaclePrefab, groundTile.transform);
            }

            if (nextMonsterArmy < monsters.armyCount && i == mapWidth - 1 && j == nextMonsterPosition) {
                PlaceMonsterArmy(groundTile, position, groundTile.gamePosition, nextMonsterArmy);
                nextMonsterArmy++;
                nextMonsterPosition += monsterSpread;
            }

            if (nextKnightArmy < knights.armyCount && i == 0 && j == nextKnightPosition) {
                PlaceKnightArmy(groundTile, position, groundTile.gamePosition, nextKnightArmy);
                nextKnightArmy++;
                nextKnightPosition += knightSpread;
            }

            tileContainer[y+mapDY,x+mapDX] = groundTile;
        }
    }
}

void PregenerateArmies() {
    MonsterGroup monsters = new MonsterGroup(5);
    monsters.armies[0] = new Army(1, 15);
    monsters.armies[1] = new Army(1, 19);
    monsters.armies[2] = new Army(2, 15);
    monsters.armies[3] = new Army(1, 19);
    monsters.armies[4] = new Army(1, 15);
    PersistentData.monsterGroup = monsters;

    MonsterGroup player = new MonsterGroup(5);
    player.armies[0] = new Army(0, 20);
    player.armies[1] = new Army(0, 20);
    player.armies[2] = new Army(0, 20);
    player.armies[3] = new Army(0, 20);
    player.armies[4] = new Army(0, 20);
    PersistentData.playerArmy = player;
}

void PrepareArmies() {
    armies.Sort(delegate(CombatMonsterGroup a, CombatMonsterGroup b) {
        if (a.initiative > b.initiative) return -1;
        if (b.initiative < a.initiative) return 1;
        return 0;
    });

    for (int i = 0; i < armies.Count; i++) {
        TimelineTurn turn = Instantiate(timelineTurnPrefab, timeline.transform);
        turn.group = armies[i];
        armies[i].armyId = i;
        turn.combatGroupId = i;
        turn.GetComponentInChildren<Text>().text = armies[i].count.ToString();
        if (turn.group.playerId == 0) {
            turn.GetComponentInChildren<Text>().color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            turn.GetComponentInChildren<Outline>().effectColor = new Color(0.0f, 0.0f, 0.0f, 0.5f);
        } else {
            turn.GetComponentInChildren<Text>().color = new Color(0.0f, 0.0f, 0.0f, 1.0f);
            turn.GetComponentInChildren<Outline>().effectColor = new Color(1.0f, 1.0f, 1.0f, 0.5f);
        }
    }
}

string GetTurnTitle(int playerID) {
    if (currentPlayer == 0) {
        return "Knights";
    } else {
        return "Monsters";
    }
}

void ClearHighlightedTiles() {
    for (int i = 0; i < highlightedTiles.Count; i++) {
        Tile tile = highlightedTiles[i];
        tile.GetComponent<Renderer>().material.color = new Color(0.375f, 0.375f, 0.375f, 1.0f);
    }
    highlightedTiles.Clear();
}

void HighlightTiles(List<Vector2i> tiles) {
    for (int i = 0; i < tiles.Count; i++) {
        Tile tile = tileContainer[tiles[i].y + mapDY, tiles[i].x + mapDX];
        tile.GetComponent<Renderer>().material.color = new Color(0.3f, 0.3f, 0.3f);
        highlightedTiles.Add(tile);
    }
}

float computerTurnStartDelayTime = 1.0f;
float computerTurnStartCurrent = 0.0f;
bool computerTurnStartDelay = false;

void NextTurn() {
    TimelineTurn turn = timeline.transform.GetChild(0).GetComponent<TimelineTurn>();
    currentPlayer = turn.group.playerId;
    turnTitle.GetComponent<Text>().text = GetTurnTitle(currentPlayer);

    if (currentArmy) currentArmy.SetColor(new Color(1.0f, 1.0f, 1.0f, 1.0f));
    currentArmy = turn.group;
    currentArmy.SetColor(new Color(0.3f, 0.3f, 0.6f, 1.0f));
    currentArmy.hasMoved = false;
    currentArmy.hasAttacked = false;
        
    Camera.main.GetComponent<CameraController>().MoveTo(currentArmy.transform.position + new Vector3(0.0f, 0.0f, -3.0f));
        
    pathfinder.SetTerrain(terrainPassable);
    List<Vector2i> tilesInRange = pathfinder.GetInRange(currentArmy.gamePosition, Creatures.Get(currentArmy.creatureId).speed);

    ClearHighlightedTiles();
    HighlightTiles(tilesInRange);

    pathfinder.SetTerrain(GetPassableTerrain());
    if (currentPlayer != 0) {
        computerTurnStartDelay = true;
        computerTurnStartCurrent = 0.0f;
    }
}

void Start() {
    combatOver = false;
    Creatures.PrepareCreatures();
    if (PersistentData.state != PersistentData.State.EnteringCombat) {
        PregenerateArmies();
        PersistentData.state = PersistentData.State.EnteringCombat;
    }
    armies = new List<CombatMonsterGroup>();
    GenerateMap();

    PrepareArmies();

    pathfinder = new Pathfinder();
        
    mainCamera = Camera.main.gameObject.GetComponent<CameraController>();
    mainCamera.SetConstraint(
        -mapWidth / 2 * tilePrefab.transform.localScale.x - 4,
        mapWidth / 2 * tilePrefab.transform.localScale.x + 4,
        5,
        5,
        -mapHeight / 2 * tilePrefab.transform.localScale.z - 3,
        mapHeight / 2 * tilePrefab.transform.localScale.z - 3
    );

    highlightedTiles = new List<Tile>();
    NextTurn();
        
#if UNITY_EDITOR
    DynamicGI.UpdateEnvironment();
#endif
}

Tile hoverTile;

TimelineTurn GetTimelineItem(int armyId) {
    for (int i = 0; i < timeline.transform.childCount; i++) {
        TimelineTurn turn = timeline.transform.GetChild(i).GetComponent<TimelineTurn>();
        if (turn.combatGroupId == armyId) return turn;
    }
    return null;
}
    
void ClearHoverTile() {
    if (hoverTile == null) return;
    bool highlighted = false;
    for (int i = 0; i < highlightedTiles.Count; i++) {
        if (highlightedTiles[i] == hoverTile) {
            highlighted = true;
            break;
        }
    }
    if (highlighted)
        hoverTile.GetComponent<Renderer>().material.color = new Color(0.3f, 0.3f, 0.3f, 1.0f);
    else
        hoverTile.GetComponent<Renderer>().material.color = new Color(0.375f, 0.375f, 0.375f, 1.0f);

    CombatMonsterGroup group = GetArmyAt(hoverTile.gamePosition);

    hoverTile = null;
    debugPosition.text = "";

    if (group == null) return;
    if (group.dead) return;

    TimelineTurn turn = GetTimelineItem(group.armyId);
    turn.SetColor(new Color(1.0f, 1.0f, 1.0f, 0.4f));

    if (currentArmy == group) {
        group.SetColor(new Color(0.3f, 0.3f, 0.6f, 1.0f));
    } else {
        group.SetColor(new Color(1.0f, 1.0f, 1.0f));
    }
}

CombatMonsterGroup GetArmyAt(Vector2i position) {
    for (int i = 0; i < armyContainer.transform.childCount; i++) {
        GameObject child = armyContainer.transform.GetChild(i).gameObject;
        CombatMonsterGroup group = child.GetComponent<CombatMonsterGroup>();
        if (group.gamePosition == position) {
            return group;
        }
    }
    return null;
}

void ShowContext(string text, Vector3 position) {
    GameObject context = contextGUI.transform.Find("Context").gameObject;
    context.transform.position = position + new Vector3(1.0f, 0.75f, 0.0f);
    context.GetComponent<Text>().text = text;
    context.SetActive(true);
}

void HideContext() {
    GameObject context = contextGUI.transform.Find("Context").gameObject;
    context.SetActive(false);
}

void ShowAttackFeedback(string text, Vector3 position) {
    feedbackDisplayCurrent = 0.0f;
    GameObject feedback = contextGUI.transform.Find("AttackFeedback").gameObject;
    feedback.transform.position = position + new Vector3(2.0f, 0.75f, 0.0f);
    feedback.GetComponent<Text>().text = text;
    feedback.SetActive(true);
}

void HideAttackFeedback() {
    GameObject feedback = contextGUI.transform.Find("AttackFeedback").gameObject;
    feedback.SetActive(false);
}

void ShowCounterFeedback(string text, Vector3 position) {
    GameObject feedback = contextGUI.transform.Find("CounterFeedback").gameObject;
    feedback.transform.position = position + new Vector3(-2.0f, 0.75f, 0.0f);
    feedback.GetComponent<Text>().text = text;
    feedback.SetActive(true);
}

void HideCounterFeedback() {
    GameObject feedback = contextGUI.transform.Find("CounterFeedback").gameObject;
    feedback.SetActive(false);
}

void HoverTile() {
    Vector3 mousePosition = Input.mousePosition;
    Ray ray = Camera.main.ScreenPointToRay(mousePosition);
    RaycastHit hit;
    if (Physics.Raycast(ray, out hit)) {
        if (hit.collider.tag == "Tile") {
            Tile tile = hit.collider.GetComponent<Tile>();
            bool isthere = GetArmyAt(tile.gamePosition) == null;
            string has = isthere ? "true" : "false";
            debugPosition.text = tile.gamePosition.ToString() + ": " + has;
            CombatMonsterGroup group = GetArmyAt(tile.gamePosition);
            if (group != null) {
                int armyId = group.armyId;
                TimelineTurn turn = GetTimelineItem(armyId);
                turn.SetColor(new Color(0.75f, 0.75f, 0.0f, 0.4f));
                group.SetColor(new Color(0.75f, 0.75f, 0.0f));

                string contextString = "";
                Creature creature = Creatures.Get(group.creatureId);
                if (group.playerId != 0) contextString = GetChanceToHit(Creatures.Get(currentArmy.creatureId).attack, creature.defense).ToString() + "%\n";
                contextString += "A: " + creature.attack + "\nD: " + creature.defense + "\nDMG: " + creature.damage + "\nSPD: " + creature.speed;
                ShowContext(contextString, group.transform.position);                
            } else {
                HideContext();
            }
            tile.GetComponent<Renderer>().material.color = new Color(0.2f, 0.2f, 0.2f, 1.0f);
            if (hoverTile && hoverTile.gamePosition != tile.gamePosition) {
                ClearHoverTile();
            }
            hoverTile = tile;
        } else {
            ClearHoverTile();
            HideContext();
        }
    } else {
        ClearHoverTile();
        HideContext();
    }
}

bool[,] GetPassableTerrain() {
    bool[,] terrain = new bool[11,15];
    int dx = mapWidth / 2;
    int dy = mapHeight / 2;
    for (var j = 0; j < mapHeight; j++) {
        for (var i = 0; i < mapWidth; i++) {
            int x = i - dx;
            int y = j - dy;
            terrain[y+dy, x+dx] = terrainPassable[y+dy, x+dx];
        }
    }
    for (int i = 0; i < armyContainer.transform.childCount; i++) {
        Vector2i position = armyContainer.transform.GetChild(i).GetComponent<CombatMonsterGroup>().gamePosition;
        if (position == currentArmy.gamePosition) continue;
        terrain[position.y + dy, position.x + dx] = false;
    }

    return terrain;
}

Tile pressTile;
Tile releaseTile;
float pressTime;
float releaseTime;

bool movingToAttack;
CombatMonsterGroup attackTarget;

void CheckClick() {
    if (releaseTime < 0.0f || pressTime < 0.0f) return;
    if (releaseTime - pressTime > 0.5f) return;
    if (pressTile != releaseTile) return;

    if (currentArmy.hasMoved) return;
    
    Vector2i position = hoverTile.gamePosition;

    if (currentArmy.gamePosition == position) return;
    
    CombatMonsterGroup monstersAt = GetArmyAt(position);
    bool hasMonsters = monstersAt != null;

    if (!hasMonsters) {
        if (!pathfinder.IsPassable(position)) return;
    } else {
        if (monstersAt.playerId == currentPlayer) return;
    }

    int range = Creatures.Get(currentArmy.creatureId).speed;
    if (hasMonsters) range++;

    List<MovementPoint> path = pathfinder.FindPath(currentArmy.gamePosition, releaseTile.gamePosition, range);
    if (path == null) return;

    if (hasMonsters) {
        path.RemoveAt(path.Count - 1);
        movingToAttack = true;
        attackTarget = monstersAt;
    } else {
        movingToAttack = false;
    }

    movementPath = path;
    movementNextStep = 0;
    isMoving = true;
    currentArmy.hasMoved = true;
    MovementNextStep();
}
    
void PlayerControls() {
    if (Input.GetMouseButtonDown(1)) {
        if (hoverTile != null) {
            pressTile = hoverTile;
            pressTime = Time.realtimeSinceStartup;
        } else {
            pressTile = null;
            pressTime = -1.0f;
        }
    }

    if (Input.GetMouseButtonUp(1)) {
        if (hoverTile != null) {
            releaseTile = hoverTile;
            releaseTime = Time.realtimeSinceStartup;
            CheckClick();
        } else {
            releaseTile = null;
            releaseTime = -1.0f;
        }
    }
}

void RotateArmy(CombatMonsterGroup group, float angle) {
    GameObject child;
    if (currentPlayer == 0)
        child = group.transform.Find("Knight").gameObject;
    else
        child = group.transform.Find("Monster").gameObject;
    child.transform.rotation = Quaternion.Euler(0.0f, 180.0f-angle, 0.0f);
}

int GetChanceToHit(int attack, int defense) {
    float chanceToHit = 0.5f + (attack - defense) / (2.0f * defense);

    if (chanceToHit < 0.05f) chanceToHit = 0.05f;
    if (chanceToHit > 1.0f) chanceToHit = 1.0f;

    chanceToHit = Mathf.Floor(chanceToHit * 100.0f);

    return (int)chanceToHit;
}

GameObject GetArmyObject(int armyId) {
    for (int i = 0; i < armyContainer.transform.childCount; i++) {
        if (armyContainer.transform.GetChild(i).GetComponent<CombatMonsterGroup>().armyId == armyId) return armyContainer.transform.GetChild(i).gameObject;
    }
    return null;
}

int CountPlayerArmies(int playerId) {
    int count = 0;
    for (int i = 0; i < armyContainer.transform.childCount; i++) {
        if (armyContainer.transform.GetChild(i).GetComponent<CombatMonsterGroup>().playerId == playerId) count++;
    }
    return count;
}

void KillArmy(CombatMonsterGroup group) {
    int playerId = group.playerId;
        
    armies.Remove(group);
    TimelineTurn turn = GetTimelineItem(group.armyId);
    GameObject army = GetArmyObject(group.armyId);

    Destroy(army);
    Destroy(group.gameObject);
    Destroy(turn.gameObject);

    int count = CountPlayerArmies(playerId) - 1;
    if (count == 0) {
        combatOver = true;
        if (playerId == 0)
            lostCanvas.SetActive(true);
        else
            wonCanvas.SetActive(true);
    }
}

int ApplyDamage(CombatMonsterGroup attacking, CombatMonsterGroup defending) {
    Creature attCreature = Creatures.Get(attacking.creatureId);
    int damage = attCreature.damage * attacking.count;
    int startingCount = defending.count;
    defending.ApplyDamage(damage);
    int newCount = defending.count;
    int diff = startingCount - newCount;
    return diff;
}

int Hit(CombatMonsterGroup attacker, CombatMonsterGroup defender) {
    Creature attacking = Creatures.Get(attacker.creatureId);
    Creature defending = Creatures.Get(defender.creatureId);

    float chanceToHit = GetChanceToHit(attacking.attack, defending.defense);
    int chance = Random.Range(1, 101);
    if (chance < chanceToHit) {
        return ApplyDamage(attacker, defender);
    } else {
        return -1;
    }
}

float feedbackDisplayTime = 2.0f;
float feedbackDisplayCurrent;

void UpdateTimelineItem(CombatMonsterGroup group) {
    TimelineTurn turn = GetTimelineItem(group.armyId);
    if (!turn) {
        Debug.Log("No turn found for this army.");
        return;
    }
    turn.GetComponentInChildren<Text>().text = group.count.ToString();
    if (group.dead) turn.GetComponentInChildren<Image>().color = new Color(1.0f, 0.0f, 0.0f, 1.0f);
}

void Attack(CombatMonsterGroup attacker, CombatMonsterGroup defender) {
    attacker.hasAttacked = true;
    int killed = Hit(attacker, defender);
    switch (killed) {
        case -1:
            ShowAttackFeedback("Missed", defender.transform.position);
            break;
        case 0:
            ShowAttackFeedback("0", defender.transform.position);
            break;
        default:
            ShowAttackFeedback("-" + killed.ToString(), defender.transform.position);
            break;
    }
    UpdateTimelineItem(defender);

    if (defender.dead) {
        KillArmy(defender);
        return;
    }

    killed = Hit(defender, attacker);
    switch(killed) {
        case -1:
            ShowCounterFeedback("Missed", defender.transform.position);
            break;
        case 0:
            ShowCounterFeedback("0", defender.transform.position);
            break;
        default:
            ShowCounterFeedback("-" + killed.ToString(), defender.transform.position);
            break;
    }
    UpdateTimelineItem(attacker); 
        
    if (attacker.dead) {
        KillArmy(attacker);
        return;
    }
}

bool isMoving = false;
List<MovementPoint> movementPath;
int movementNextStep;
Tile nextTile;

float turnDelay = 1.0f;
float turnDelayCurrent = 0.0f;
bool waitForNextTurn = false;

void MovementNextStep() {
    if (movementNextStep == movementPath.Count - 1) {
        isMoving = false;
        if (movingToAttack) Attack(currentArmy, attackTarget);
        turnDelayCurrent = 0.0f;
        waitForNextTurn = true;
        turnTitle.GetComponent<Text>().text = "Waiting...";
        return;
    }

    Vector2i currentPosition = movementPath[movementNextStep].position;
    movementNextStep++;
    Vector2i nextPosition = movementPath[movementNextStep].position;

    Vector2i direction = nextPosition - currentPosition;
    float angle = Vector3.SignedAngle(new Vector3(direction.x, 0.0f, direction.y), transform.forward, transform.up);
    RotateArmy(currentArmy, angle);

    nextTile = tileContainer[nextPosition.y + mapDY, nextPosition.x + mapDX];
    currentArmy.gamePosition = nextTile.gamePosition;
}

void MoveFeedbackText() {
    GameObject attackFeedback = contextGUI.transform.Find("AttackFeedback").gameObject;
    GameObject counterFeedback = contextGUI.transform.Find("CounterFeedback").gameObject;

    attackFeedback.transform.position = Vector3.MoveTowards(attackFeedback.transform.position, attackFeedback.transform.position + new Vector3(0.0f, 1.0f, 0.0f), 1.0f * Time.deltaTime);
    counterFeedback.transform.position = Vector3.MoveTowards(counterFeedback.transform.position, counterFeedback.transform.position + new Vector3(0.0f, 1.0f, 0.0f), 1.0f * Time.deltaTime);

    feedbackDisplayCurrent += Time.deltaTime;
    if (feedbackDisplayCurrent > feedbackDisplayTime) {
        HideAttackFeedback();
        HideCounterFeedback();
    }
}

void MoveTimeline() {
    if (currentArmy == null) return;
    GameObject timelineItem = timeline.transform.GetChild(0).gameObject;
    timelineItem.transform.SetParent(null);
    timelineItem.transform.SetParent(timeline.transform);
}

void WaitForNextTurn() {
    turnDelayCurrent += Time.deltaTime;
    if (turnDelayCurrent > turnDelay) {
        waitForNextTurn = false;
        MoveTimeline();
        NextTurn();
    }
}

int GetTotalHP(CombatMonsterGroup army) {
    return Creatures.Get(army.creatureId).hp * (army.count - 1) + army.hp;
}

void ComputerTurn() {
    Vector2i position = currentArmy.gamePosition;

    CombatMonsterGroup targetPlayerArmy = null;

    List<Vector2i> neighbours = pathfinder.GetNeighbours(position);
    for (int i = 0; i < neighbours.Count; i++) {
        CombatMonsterGroup army = GetArmyAt(neighbours[i]);
        if (army == null || army.playerId != 0) continue;
        if (targetPlayerArmy == null || GetTotalHP(army) > GetTotalHP(targetPlayerArmy)) targetPlayerArmy = army;
    }
    
    List<MovementPoint> path = null;

        
    if (targetPlayerArmy) {
        path = pathfinder.FindPath(currentArmy.gamePosition, targetPlayerArmy.gamePosition, Creatures.Get(currentArmy.creatureId).speed);
        if (path == null) {
            Debug.Log("path error");
            return;
        }
        path.RemoveAt(path.Count - 1);
        movingToAttack = true;
        attackTarget = targetPlayerArmy;
    } else {
        
        // find the player army with lowest amount of HP
        for (int i = 0; i < armies.Count; i++) {
            if (armies[i].playerId != 0) continue;
            CombatMonsterGroup army = armies[i];
            if (targetPlayerArmy == null || GetTotalHP(army) > GetTotalHP(targetPlayerArmy)) targetPlayerArmy = army;
        }
        
        if (targetPlayerArmy == null) {
            Debug.Log("Error: no enemy army chosen :(");
            return;
        }
        
        path = pathfinder.FindPath(currentArmy.gamePosition, targetPlayerArmy.gamePosition);
        
        if (path.Count > Creatures.Get(currentArmy.creatureId).speed + 2) {
            path = path.GetRange(0, Creatures.Get(currentArmy.creatureId).speed + 1);
            movingToAttack = false;
        } else {
            path.RemoveAt(path.Count - 1);
            movingToAttack = true;
            attackTarget = targetPlayerArmy;
        }
        
    }
    movementPath = path;
    movementNextStep = 0;
    isMoving = true;
    currentArmy.hasMoved = true;
    MovementNextStep();
}

void DelayBeforeComputerTurn() {
    computerTurnStartCurrent += Time.deltaTime;
    if (computerTurnStartCurrent > computerTurnStartDelayTime) {
        computerTurnStartDelay = false;
        ComputerTurn();
    }
}

void Update() {
    mainCamera.Drag(Input.mousePosition);

    if (contextGUI.transform.Find("AttackFeedback").gameObject.activeSelf) {
        MoveFeedbackText();
    }

    if (combatOver) return;

    if (waitForNextTurn) {
        WaitForNextTurn();
    }

    if (currentPlayer == 0 && !isMoving && !waitForNextTurn) {
        HoverTile();
        PlayerControls();
    }

    if (currentPlayer != 0 && computerTurnStartDelay) {
        DelayBeforeComputerTurn();
    }

    if (isMoving) {
        float step = Time.deltaTime * 2.0f;
        if (Input.GetMouseButton(2)) step *= 4.0f;
        currentArmy.transform.position = Vector3.MoveTowards(currentArmy.transform.position, nextTile.transform.position, step);
        if (currentArmy.transform.position == nextTile.transform.position) MovementNextStep();
    }
}

public void ReturnToMainMenu() {
    PersistentData.Clear();
    SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
}

public void ReturnToWorldMap() {
    List<Army> pArmies = new List<Army>();
    for (int i = 0; i < armies.Count; i++) {
        if (armies[i].playerId != 0) continue;
        pArmies.Add(new Army(armies[i].creatureId, armies[i].count));
    }
    PersistentData.EndCombat(pArmies);
    SceneManager.LoadScene("WorldScreen", LoadSceneMode.Single);
}

}
