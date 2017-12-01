using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour {

public Vector2i gamePosition;
public bool passable;
public bool hasMonsters;

public RecruitmentCamp GetRecruitmentCamp() {
    for (int i = 0; i < transform.childCount; i++) {
        if (transform.GetChild(i).tag == "RecruitmentCamp") return transform.GetChild(i).GetComponent<RecruitmentCamp>();
    }
    return null;
}

}
