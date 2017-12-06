using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecruitmentCamp : MonoBehaviour {

public int current;
public int max;
public Vector2i gamePosition;

public void SetCurrent(int value) {
    current = value;
    GetComponentInChildren<Text>().text = current.ToString() + " / " + max.ToString();
}

public void SetMax(int value) {
    max = value;
    GetComponentInChildren<Text>().text = current.ToString() + " / " + max.ToString();
}

}
