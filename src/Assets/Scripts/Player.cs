using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

public Vector2i gamePosition;

public GameController gc;

public int movementMax;
public int movementCurrent;
    

public void SetPosition(int x, int y) {
    gamePosition = new Vector2i(x, y);
    transform.position = gc.MapToWorldPosition(gamePosition);
}

}
