using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Vector2i {

public int x;
public int y;
  
    public static Vector2i zero {
        get {
            return new Vector2i(0, 0);
        }
    }

    public Vector2i(int x, int y) {
        this.x = x;
        this.y = y;
    }
  
    public static Vector2i operator +(Vector2i v1, Vector2i v2) {
        return new Vector2i(v1.x + v2.x, v1.y + v2.y);
    }
  
    public static Vector2i operator -(Vector2i v1, Vector2i v2) {
        return new Vector2i(v1.x - v2.x, v1.y - v2.y);
    }

    public static Vector2i operator *(int mult, Vector2i vec) {
        return new Vector2i(vec.x * mult, vec.y * mult);
    }

    public static Vector2i operator *(Vector2i vec, int mult) {
        return new Vector2i(vec.x * mult, vec.y * mult);
    }

    public static bool operator ==(Vector2i v1, Vector2i v2) {
        if (object.ReferenceEquals(v1, null)) {
             return object.ReferenceEquals(v2, null);
        }
        return (v1.x == v2.x && v1.y == v2.y);
    }

    public static bool operator !=(Vector2i v1, Vector2i v2) {
        return (v1.x != v2.x || v1.y != v2.y);
    }

    public override string ToString() {
        return this.x + ", " + this.y;
    }

    public override bool Equals(object obj) {
        Vector2i vObj = obj as Vector2i;
        if (vObj == null) return false;
        return vObj == this;
    }

    public override int GetHashCode() {
        return x ^ y;
    }
}