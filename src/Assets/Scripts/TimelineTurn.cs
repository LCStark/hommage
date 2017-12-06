using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimelineTurn : MonoBehaviour {

public int combatGroupId;
public CombatMonsterGroup group;
    
public void SetColor(Color color) {
    GetComponent<Image>().color = color;
}

public void OnTriggerEnter() {
    GetComponent<Image>().color = new Color(0.75f, 0.75f, 0.0f, 0.4f);
    Renderer[] renderers = group.GetComponentsInChildren<Renderer>();
    foreach (var renderer in renderers) renderer.material.color = new Color(0.75f, 0.75f, 0.0f);
}

public void OnTriggerLeave() {
    GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f, 0.4f);
    Renderer[] renderers = group.GetComponentsInChildren<Renderer>();
    foreach (var renderer in renderers) renderer.material.color = new Color(1.0f, 1.0f, 1.0f);
}

}
