using System;

[System.Serializable]
public class Constraint {
    public float minX;
    public float maxX;
    public float minY;
    public float maxY;
    public float minZ;
    public float maxZ;

    public Constraint(float minX, float maxX, float minY, float maxY, float minZ, float maxZ) {
        this.minX = minX; this.maxX = maxX;
        this.minY = minY; this.maxY = maxY;
        this.minZ = minZ; this.maxZ = maxZ;
    }
}