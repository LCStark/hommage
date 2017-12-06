using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GameController : MonoBehaviour {

    public Canvas inGameGUI;
    public Canvas mainMenuConfirm;
    public Canvas debugGUI;
    public Canvas loading;
    public Canvas victory;

    public enum State {Playing, MainMenuConfirm, Manage};
    public State state;

    public GameObject armyRowPrefab;

    public List<Army> playerArmies;

    public GameObject armyPanel;

    public GameObject worldMap;

    public GameObject pathDisplay;

    public Tile tilePrefab;
    public GameObject obstaclePrefab;
    public GameObject movementArrowPrefab;
    public GameObject movementTurnsPrefab;
    public GameObject flagPrefab;
    public GameObject[] monsterGroupPrefabs;
    public RecruitmentCamp recruitmentCampPrefab;

    public Player player;

    public int mapWidth;
    public int mapHeight;
    private int mapDX;
    private int mapDY;

    public int currentMonth;
    public int currentWeek;
    public int currentDay;
    private enum WeekDay {Monday = 1, Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday};

    private Tile[,] tileContainer;
    
    public GameObject movementCursor;

    public Text movementCounter;

    public Text monthValue;
    public Text weekValue;
    public Text dayValue;
    public Text weekdayValue;
    
    public GameObject pathTurnsContainer;
    
    private Pathfinder pathfinder;

    private Tile hoveredTile;

    private List<MovementPoint> currentPath;
    
    private CameraController mainCamera;
    
    private MonsterGroup[] monsterGroups;
    private int monsterGroupCount;

    private List<RecruitmentCamp> recruitmentCamps;

void PrepareMonsterGroups() {
    monsterGroupCount = 8;
    monsterGroups = new MonsterGroup[8];

    monsterGroups[0] = new MonsterGroup(1);
    monsterGroups[0].armies[0] = new Army(1, 3);
    monsterGroups[0].id = 0;
        
    monsterGroups[1] = new MonsterGroup(1);
    monsterGroups[1].armies[0] = new Army(1, 3);
    monsterGroups[1].id = 1;
        
    monsterGroups[2] = new MonsterGroup(2);
    monsterGroups[2].armies[0] = new Army(1, 7);
    monsterGroups[2].armies[1] = new Army(2, 5);
    monsterGroups[2].id = 2;

    monsterGroups[3] = new MonsterGroup(2);
    monsterGroups[3].armies[0] = new Army(1, 7);
    monsterGroups[3].armies[1] = new Army(2, 5);
    monsterGroups[3].id = 3;
        
    monsterGroups[4] = new MonsterGroup(3);
    monsterGroups[4].armies[0] = new Army(1, 11);
    monsterGroups[4].armies[1] = new Army(2, 9);
    monsterGroups[4].armies[2] = new Army(1, 11);
    monsterGroups[4].id = 4;

    monsterGroups[5] = new MonsterGroup(3);
    monsterGroups[5].armies[0] = new Army(1, 11);
    monsterGroups[5].armies[1] = new Army(2, 9);
    monsterGroups[5].armies[2] = new Army(1, 11);
    monsterGroups[5].id = 5;

    monsterGroups[6] = new MonsterGroup(5);
    monsterGroups[6].armies[0] = new Army(1, 15);
    monsterGroups[6].armies[1] = new Army(1, 19);
    monsterGroups[6].armies[2] = new Army(2, 15);
    monsterGroups[6].armies[3] = new Army(1, 19);
    monsterGroups[6].armies[4] = new Army(1, 15);
    monsterGroups[6].id = 6;

    monsterGroups[7] = new MonsterGroup(5);
    monsterGroups[7].armies[0] = new Army(1, 15);
    monsterGroups[7].armies[1] = new Army(1, 19);
    monsterGroups[7].armies[2] = new Army(2, 15);
    monsterGroups[7].armies[3] = new Army(1, 19);
    monsterGroups[7].armies[4] = new Army(1, 15);
    monsterGroups[7].id = 7;
}

void Start () {
    state = State.Playing;

    Creatures.PrepareCreatures();

    playerArmies = new List<Army>();
    if (PersistentData.state == PersistentData.State.ExitingCombat) {
        MonsterGroup pa = PersistentData.playerArmy;
        for (int i = 0; i < pa.armyCount; i++) {
            AddSoldiers(pa.armies[i].count);
        };
    } else {
        AddSoldiers(5);
        AddSoldiers(5);
        AddSoldiers(5);
    }

    recruitmentCamps = new List<RecruitmentCamp>();
        
    if (PersistentData.state == PersistentData.State.ExitingCombat) {
        monsterGroups = PersistentData.monsterGroups.ToArray();
        monsterGroupCount = PersistentData.monsterGroups.Count;
    } else {
        PrepareMonsterGroups();
    }

    GenerateMap();

    if (PersistentData.state == PersistentData.State.ExitingCombat) {
        List<SavedCamp> camps = PersistentData.recruitmentCamps;

        for (int i = 0; i < camps.Count; i++) {
            SavedCamp camp = camps[i];
            RecruitmentCamp rCamp = Instantiate(recruitmentCampPrefab, tileContainer[camp.gamePosition.y + mapDY, camp.gamePosition.x + mapDX].transform);
            rCamp.transform.position = tileContainer[camp.gamePosition.y + mapDY, camp.gamePosition.x + mapDX].transform.position;
            rCamp.SetMax(camp.max);
            rCamp.SetCurrent(camp.current);
            rCamp.gamePosition = camp.gamePosition;
            recruitmentCamps.Add(rCamp);
        }

        for (int i = 0; i < monsterGroupCount; i++) {
            MonsterGroup group = monsterGroups[i];
            int count = group.GetGroupCount();
            GameObject mgPrefab = GetMonsterGroupPrefab(count);
            GameObject mgIcon = Instantiate(mgPrefab, tileContainer[group.gamePosition.y + mapDY, group.gamePosition.x + mapDX].transform);
            mgIcon.transform.localScale = new Vector3(1.0f, 10.0f, 1.0f);
            tileContainer[group.gamePosition.y + mapDY, group.gamePosition.x + mapDX].hasMonsters = true;
        }
    }

    mainCamera = Camera.main.gameObject.GetComponent<CameraController>();
    mainCamera.SetConstraint(
        -mapWidth / 2 * tilePrefab.transform.localScale.x + 4,
        mapWidth / 2 * tilePrefab.transform.localScale.x + 4,
        5,
        5,
        -mapHeight / 2 * tilePrefab.transform.localScale.z - 3,
        mapHeight / 2 * tilePrefab.transform.localScale.z - 3
    );

    if (PersistentData.state == PersistentData.State.ExitingCombat) {
        Vector2i position = PersistentData.playerPosition;
        player.SetPosition(position.x, position.y);
    } else {
        player.SetPosition(3, -4);
    }

    pathfinder = new Pathfinder();
    pathfinder.SetTerrain(terrainPassable);

    if (PersistentData.state == PersistentData.State.ExitingCombat) {
        currentDay = PersistentData.day;
        currentWeek = PersistentData.week;
        currentMonth = PersistentData.month;
    } else {
        currentDay = 1;
        currentWeek = 1;
        currentMonth = 1;
    }

    if (PersistentData.state == PersistentData.State.ExitingCombat) {
        SetPlayerMovement(PersistentData.movementPoints);
    } else {
        SetPlayerMovement(6);
    }

    monthValue.text = currentMonth.ToString();
    weekValue.text = currentWeek.ToString();
    dayValue.text = currentDay.ToString();
    weekdayValue.text = ((WeekDay)currentDay).ToString();
        
    if (monsterGroupCount == 0) {
        victory.gameObject.SetActive(true);
    }

    PersistentData.state = PersistentData.State.WorldMap;

#if UNITY_EDITOR
    DynamicGI.UpdateEnvironment();
#endif
}

bool[,] terrainPassable = new bool[8, 8] {
    { true, true, true, true, false, true, true, true },
    { true, false, false, true, false, true, false, true },
    { true, true, true, true, false, true, true, true },
    { true, false, false, true, false, false, true, true },
    { true, true, false, false, true, false, false, true },
    { true, true, true, false, true, true, true, true },
    { true, false, true, false, true, false, false, true },
    { true, true, true, false, true, true, true, true }
};

int[,] camps = new int[8, 8] {
    { 1, 0, 0, 0, 0, 1, 0, 0 },
    { 0, 0, 0, 0, 0, 0, 0, 0 },
    { 0, 0, 0, 0, 0, 0, 0, 0 },
    { 0, 0, 0, 0, 0, 0, 0, 0 },
    { 0, 0, 0, 0, 0, 0, 0, 0 },
    { 0, 0, 0, 0, 0, 0, 0, 0 },
    { 0, 0, 0, 0, 0, 0, 0, 0 },
    { 0, 0, 1, 0, 0, 0, 0, 1 }
};

int[,] monsters = new int[8, 8] {
    {  6, -1, -1, -1, -1, -1, -1, -1 },
    { -1, -1, -1, -1, -1, -1, -1, -1 },
    { -1, -1, -1, -1, -1,  0, -1, -1 },
    {  2, -1, -1,  4, -1, -1,  -1, -1 },
    { -1, -1, -1, -1, 5, -1, -1,  3 },
    { -1, -1,  1, -1, -1, -1, -1, -1 },
    { -1, -1, -1, -1, -1, -1, -1, -1 },
    { -1, -1, -1, -1, -1, -1, -1, 7 }
};

GameObject GetMonsterGroupPrefab(int monsterCount) {
    if (monsterCount > 40) return monsterGroupPrefabs[3];
    if (monsterCount > 20) return monsterGroupPrefabs[2];
    if (monsterCount > 10) return monsterGroupPrefabs[1];
    return monsterGroupPrefabs[0];
}

void GenerateMap() {
    tileContainer = new Tile[mapHeight, mapWidth];
    mapDX = mapWidth / 2;
    mapDY = mapHeight / 2;
    for (var j = 0; j < mapHeight; j++) {
        for (var i = 0; i < mapWidth; i++) {
            int x = i - mapDX;
            int y = j - mapDY;

            float pX = x * tilePrefab.transform.localScale.x;
            float pZ = y * tilePrefab.transform.localScale.z;
            Vector3 position = new Vector3(pX, 0.0f, pZ);
            Quaternion rotation = new Quaternion();
            Tile groundTile = Instantiate(tilePrefab, position, rotation, worldMap.transform) as Tile;
            groundTile.gamePosition = new Vector2i(x, y);
            groundTile.GetComponent<Renderer>().material.color = new Color(0.375f, 0.375f, 0.375f);
            groundTile.passable = terrainPassable[y + mapDY, x + mapDX];

            if (!terrainPassable[y+mapDY,x+mapDX]) {
                Instantiate(obstaclePrefab, groundTile.transform);
            }
            if (PersistentData.state != PersistentData.State.ExitingCombat) {
                if (camps[y+mapDY,x+mapDX] > 0) {
                    RecruitmentCamp camp = Instantiate(recruitmentCampPrefab, groundTile.transform);
                    camp.transform.position = position;
                    camp.SetMax(10);
                    camp.SetCurrent(5);
                    camp.gamePosition = new Vector2i(x, y);
                    recruitmentCamps.Add(camp);
                }
            
                if (monsters[y+mapDY, x+mapDX] > -1) {
                    int id = monsters[y+mapDY, x+mapDX];
                    int count = monsterGroups[id].GetGroupCount();
                    monsterGroups[id].gamePosition = new Vector2i(x, y);
                    GameObject mgPrefab = GetMonsterGroupPrefab(count);
                    GameObject mgIcon = Instantiate(mgPrefab, groundTile.transform);
                    mgIcon.transform.localScale = new Vector3(1.0f, 10.0f, 1.0f);
                    groundTile.hasMonsters = true;
                }
            }

            tileContainer[y+mapDY,x+mapDX] = groundTile;
        }
    }
}

public Vector3 MapToWorldPosition(Vector2i position) {
    Vector3 worldPosition = new Vector3(position.x * tilePrefab.transform.localScale.x, 0.0f, position.y * tilePrefab.transform.localScale.z);
    return worldPosition;
}

void DebugHoverTile() {
    Vector3 mousePosition = Input.mousePosition;

    Ray ray = Camera.main.ScreenPointToRay(mousePosition);
    RaycastHit hit;
    if (Physics.Raycast(ray, out hit)) {
        if (hit.collider.tag != "Tile") return;
        GameObject hover = debugGUI.transform.Find("Hover").gameObject;
        Tile tile = hit.collider.GetComponent<Tile>();
        hover.GetComponent<Text>().text = tile.gamePosition.ToString() + " " + tile.transform.position.ToString() + " " + MapToWorldPosition(tile.gamePosition).ToString() + ": " + pathfinder.IsPassable(tile.gamePosition).ToString();
    }
}

void ShowMovementCursor(Tile tile) {
    hoveredTile = tile;
    movementCursor.transform.position = tile.transform.position;

    if (tile.hasMonsters) {
        movementCursor.transform.Find("Standard").gameObject.SetActive(false);
        movementCursor.transform.Find("Flag").gameObject.SetActive(false);
        movementCursor.transform.Find("Fight").gameObject.SetActive(true);
        movementCursor.SetActive(true);
        return;
    };

    RecruitmentCamp camp = tile.GetRecruitmentCamp();
    if (camp != null && camp.current > 0) {
        movementCursor.transform.Find("Standard").gameObject.SetActive(false);
        movementCursor.transform.Find("Flag").gameObject.SetActive(true);
        movementCursor.transform.Find("Fight").gameObject.SetActive(false);
        movementCursor.SetActive(true);
        return;
    }

    movementCursor.transform.Find("Standard").gameObject.SetActive(true);
    movementCursor.transform.Find("Flag").gameObject.SetActive(false);
    movementCursor.transform.Find("Fight").gameObject.SetActive(false);
    movementCursor.SetActive(true);
}

void HideMovementCursor() {
    hoveredTile = null;
    movementCursor.SetActive(false);
}

string GetMonsterCountString(int count) {
    if (count == 4) return "Lots";
    if (count == 3) return "Pack";
    if (count == 2) return "Several";
    return "Few";
}

void ShowTileStatus(Tile tile) {
    string status;
    List<string>statuses = new List<string>();

    for (int i = 0; i < tile.transform.childCount; i++) {
        GameObject child = tile.transform.GetChild(i).gameObject;
        switch (child.tag) {
            case "RecruitmentCamp":
                statuses.Add("Recruitment camp");
                break;
            case "MonsterGroup":
                int count = child.transform.childCount;
                statuses.Add("Monsters (" + GetMonsterCountString(count) + ")");
                break;
        }
    }

    if (statuses.Count > 0) {
        status = string.Join("; ", statuses.ToArray());
        inGameGUI.transform.Find("StatusPanel").GetChild(0).gameObject.GetComponent<Text>().text = status;
        inGameGUI.transform.Find("StatusPanel").gameObject.SetActive(true);
    } else {
        inGameGUI.transform.Find("StatusPanel").gameObject.SetActive(false);
    }
}

void HoverTile() {
    if (followingPath) return;
    Vector3 mousePosition = Input.mousePosition;
    Ray ray = Camera.main.ScreenPointToRay(mousePosition);
    RaycastHit hit;
    if (Physics.Raycast(ray, out hit)) {
        if (hit.collider.tag != "Tile") {
            HideMovementCursor();
            return;
        }
        Tile tile = hit.collider.GetComponent<Tile>();
        if (tile.gamePosition == player.gamePosition) {
            HideMovementCursor();
            return;
        }
        if (!tile.passable) {
            HideMovementCursor();
            return;
        }
        ShowMovementCursor(tile);
        ShowTileStatus(tile);
    }
}

void RenderPath() {
    int moveCounter = player.movementCurrent;
    int turnCounter = 0;

    if (moveCounter == 0) {
        turnCounter++;
        moveCounter = player.movementMax;
    }

    for (int i = 1; i < currentPath.Count; i++) {
        moveCounter--;
        if (moveCounter == 0) {
            turnCounter++;
            moveCounter = player.movementMax;
            GameObject turns = Instantiate(movementTurnsPrefab, pathTurnsContainer.transform);
            turns.transform.position = MapToWorldPosition(currentPath[i].position);
            turns.GetComponentInChildren<Text>().text = turnCounter.ToString();
            turns.GetComponentInChildren<Text>().color = turnCounter == 1 ? new Color(0.25f, 1.0f, 0.25f) : new Color(1.0f, 0.25f, 0.25f);
        } else {
            GameObject arrow = Instantiate(movementArrowPrefab, pathDisplay.transform);
            arrow.transform.position = MapToWorldPosition(currentPath[i].position);

            Vector2i direction = currentPath[i].position - currentPath[i-1].position;

            float angle = Vector3.SignedAngle(new Vector3(direction.x, 0.0f, direction.y), transform.forward, transform.up);
            arrow.transform.rotation = Quaternion.Euler(0.0f, -angle, 0.0f);

            if (turnCounter == 0) {
                Renderer[] renderers = arrow.GetComponentsInChildren<Renderer>();
                foreach (var renderer in renderers) renderer.material.color = new Color(0.25f, 1.0f, 0.25f);
            } else {
                Renderer[] renderers = arrow.GetComponentsInChildren<Renderer>();
                foreach (var renderer in renderers) renderer.material.color = new Color (1.0f, 0.25f, 0.25f);
             }
        }
    }
}

void DestroyPath() {
    for (int i = 0; i < pathDisplay.transform.childCount; i++) {
        Destroy(pathDisplay.transform.GetChild(i).gameObject);
    }

    for (int i = 0; i < pathTurnsContainer.transform.childCount; i++) {
        Destroy(pathTurnsContainer.transform.GetChild(i).gameObject);
    }
}

void AddToSmallestArmy(int count) {
    int smallestCount = -1;
    int smallestId = -1;
    for (var i = 0; i < playerArmies.Count; i++) {
        if (smallestId == -1) {
            smallestId = i;
            smallestCount = playerArmies[i].count;
        }
        if (playerArmies[i].count < smallestCount) {
            smallestId = i;
            smallestCount = playerArmies[i].count;
        }
    }

    playerArmies[smallestId].count += count;

    GameObject armyRow = armyPanel.transform.GetChild(smallestId).gameObject;
    armyRow.transform.GetChild(0).GetComponent<Text>().text = playerArmies[smallestId].count.ToString();
}

void AddArmy(int count, int position) {
    Army newArmy = new Army {
        count = count,
        creatureID = 0
    };
    playerArmies.Add(newArmy);

    GameObject armyRow = Instantiate(armyRowPrefab, armyPanel.transform);
    armyRow.transform.GetChild(0).GetComponent<Text>().text = newArmy.count.ToString();
    armyRow.transform.GetChild(1).GetComponent<Text>().text = Creatures.GetName(0);
    //armyRow.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, -5 -15 * position, 0);
}

void AddSoldiers(int count) {
    int armyCount = armyPanel.transform.childCount;
    if (armyCount >= 6)
        AddToSmallestArmy(count);
    else
        AddArmy(count, armyCount);
}

void RecruitSoldiers(RecruitmentCamp camp) {
    int count = camp.current;
    AddSoldiers(count);
    camp.SetCurrent(0);
}

public bool followingPath;
public int nextStep;
public Tile nextTile;
public bool stopFollowing;

void UpdatePath() {
    currentPath.RemoveRange(0, nextStep);
    DestroyPath();
    RenderPath();
}

void EnterCombat() {
    int dx = mapWidth / 2;
    int dy = mapHeight / 2;

    Vector2i position = currentPath[nextStep].position;
    int monsterGroupID = monsters[dy + position.y, dx + position.x];

    MonsterGroup group = null;
    for (int i = 0; i < monsterGroupCount; i++) {
        if (monsterGroups[i].id == monsterGroupID) {
            group = monsterGroups[i];
            break;
        }
    };
        
    PersistentData.playerPosition = player.gamePosition;
    PersistentData.movementPoints = player.movementCurrent;
    PersistentData.day = currentDay;
    PersistentData.week = currentWeek;
    PersistentData.month = currentMonth;
    PersistentData.SaveMonsterGroups(monsterGroups, monsterGroupCount);
    PersistentData.SaveRecruitmentCamps(recruitmentCamps);
    PersistentData.StartCombat(group, playerArmies);
        
    loading.gameObject.SetActive(true);
    SceneManager.LoadScene("CombatScreen", LoadSceneMode.Single);
}

void ReturnFromCombat() {
    player.gamePosition = PersistentData.playerPosition;
}

void NextStep() {
    if (nextTile && nextTile.hasMonsters) {
        followingPath = false;
        EnterCombat();
        return;
    }

    if (nextTile) {
        RecruitmentCamp camp = nextTile.GetRecruitmentCamp();
        if (camp != null && camp.current > 0) {
            RecruitSoldiers(camp);
        }
    }
    
    if (stopFollowing) {
        followingPath = false;
        if (nextStep > 0) UpdatePath();
        return;
    }

    if (nextStep == currentPath.Count - 1) {
        followingPath = false;
        if (nextStep > 0) UpdatePath();
        return;
    }

    if (player.movementCurrent == 0) {
        followingPath = false;
        if (nextStep > 0) UpdatePath();
        return;
    }

    SetPlayerMovement(player.movementCurrent - 1);
    Vector2i currentPosition = currentPath[nextStep].position;
    nextStep++;
    Vector2i nextPosition = currentPath[nextStep].position;
    nextTile = tileContainer[nextPosition.y + mapHeight / 2, nextPosition.x + mapWidth / 2];
    player.gamePosition = nextTile.gamePosition;

    Vector2i direction = nextPosition - currentPosition;
    float angle = Vector3.SignedAngle(new Vector3(direction.x, 0.0f, direction.y), transform.forward, transform.up);
    player.transform.rotation = Quaternion.Euler(0.0f, -angle, 0.0f);

    // check if the next path marker is the arrow or the turn number and remove it
    if (pathDisplay.transform.childCount > 0) {
        GameObject nextArrow = pathDisplay.transform.GetChild(0).gameObject;
        Vector2i nextArrowPosition = new Vector2i((int)nextArrow.transform.position.x, (int)nextArrow.transform.position.z);
        if (nextArrowPosition.x == nextPosition.x && nextArrowPosition.y == nextPosition.y) {
            Destroy(nextArrow);
            return;
        }
    }
    
    if (pathTurnsContainer.transform.childCount > 0) {
        GameObject nextTurn = pathTurnsContainer.transform.GetChild(0).gameObject;
        Vector2i nextTurnPosition = new Vector2i((int)nextTurn.transform.position.x, (int)nextTurn.transform.position.z);
        if (nextTurnPosition.x == nextPosition.x && nextTurnPosition.y == nextPosition.y) {
            Destroy(nextTurn);
            return;
        }
    }
    
}

void FollowPath() {
    nextStep = 0;
    followingPath = true;
    stopFollowing = false;
    HideMovementCursor();
    NextStep();
}
    
void TileClick() {
    if (hoveredTile == null) return;
    if (!Input.GetMouseButtonUp(1)) return;

    if (followingPath) {
        stopFollowing = true;
        return;
    }

    if (currentPath != null) {
        if (currentPath[currentPath.Count-1].position == hoveredTile.gamePosition) {
            FollowPath();
            return;
        } else {
            DestroyPath();
        }
    }

    currentPath = pathfinder.FindPath(player.gamePosition, hoveredTile.gamePosition);
    if (currentPath == null) {
        Debug.Log("No path found. :(");
        return;
    }
    RenderPath();
}

void Update () {
    DebugHoverTile();
    HoverTile();
    TileClick();

    mainCamera.Drag(Input.mousePosition);

    if (followingPath) {
        float step = Time.deltaTime * 2.0f;
        if (Input.GetMouseButton(2)) step *= 2.0f;
        player.transform.position = Vector3.MoveTowards(player.transform.position, nextTile.transform.position, step);
        Camera.main.transform.position = new Vector3(player.transform.position.x + 4, Camera.main.transform.position.y, player.transform.position.z - 3);
        if (player.transform.position == nextTile.transform.position) NextStep();
    }
}

public void ShowMainMenuConfirm() {
    mainMenuConfirm.gameObject.SetActive(true);
    state = State.MainMenuConfirm;
    inGameGUI.GetComponent<GraphicRaycaster>().enabled = false;
}

public void MainMenuConfirmCancel() {
    mainMenuConfirm.gameObject.SetActive(false);
    state = State.Playing;
    inGameGUI.GetComponent<GraphicRaycaster>().enabled = true;
}

public void MainMenuConfirmConfirm() {
    PersistentData.Clear();
    SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
}

void WeeklyActions() {
    // refill recruitment camps
    for (int i = 0; i < recruitmentCamps.Count; i++) {
        recruitmentCamps[i].SetCurrent(recruitmentCamps[i].max);
    }
}

void MonthlyActions() {
    // increase monster counts
}

void SetPlayerMovement(int moves) {
    player.movementCurrent = moves;
    movementCounter.text = player.movementCurrent.ToString();
}

void EndTurn() {
    SetPlayerMovement(player.movementMax);

    if (++currentDay > 7) {
        currentDay = 1;
        currentWeek++;

        WeeklyActions();
    }
    if (currentWeek > 4) {
        currentWeek = 1;
        currentMonth++;

        MonthlyActions();
    }

    monthValue.text = currentMonth.ToString();
    weekValue.text = currentWeek.ToString();
    dayValue.text = currentDay.ToString();
    weekdayValue.text = ((WeekDay)currentDay).ToString();

    if (currentPath != null) {
        DestroyPath();
        RenderPath();
    }
}

public void EndTurnClick() {
    EndTurn();
}

private void OnDestroy() {
    
}

}
