using System;
using System.Collections.Generic;
using UnityEngine;

public class MovementPoint {
    public Vector2i position;
    public MovementPoint origin;
    public int distance;
    public int length;
}

public class Pathfinder
{

private bool[,] terrain;
private int terrainWidth;
private int terrainHeight;

public Pathfinder()
{
}

public int Chebyshev(Vector2i p1, Vector2i p2) {
    Vector2i dist = new Vector2i(
        (int)Mathf.Abs(p1.x - p2.x),
        (int)Mathf.Abs(p1.y - p2.y)
    );
    return (int)Mathf.Max(dist.x, dist.y);
}

public void SetTerrain(bool[,] terrainInfo) {
    terrain = terrainInfo;
    terrainWidth = terrain.GetLength(1);
    terrainHeight = terrain.GetLength(0);
}

public List<Vector2i> GetNeighbours(Vector2i position) {

    List<Vector2i> neighbours = new List<Vector2i>();

    for (int x = -1; x < 2; x++) for (int y = -1; y < 2; y++) {
        if (x == 0 && y == 0) continue;
        Vector2i coords = new Vector2i(position.x + x, position.y + y);
        if (coords.x + terrainWidth / 2 < 0 || coords.x + terrainWidth / 2 > terrainWidth - 1) continue;
        if (coords.y + terrainHeight / 2 < 0 || coords.y + terrainHeight / 2 > terrainHeight - 1) continue;
        neighbours.Add(coords);
    }

    return neighbours;
}

public bool IsPassable(Vector2i position) {
    return terrain[position.y + terrainHeight / 2, position.x + terrainWidth / 2];
}

public List<MovementPoint> FindPath(Vector2i source, Vector2i target, int? maxRange = null) {
    MovementPoint entryPoint = new MovementPoint {
        position = target,
        origin = null,
        distance = Chebyshev(target, source),
        length = 0
    };

    List<MovementPoint> waiting = new List<MovementPoint>();
    List<MovementPoint> done = new List<MovementPoint>();

    waiting.Add(entryPoint);

    bool isFinished = false;

    while(!isFinished) {
    
        // iterate
        if (waiting.Count == 0) {
            // The waiting list is empty. That means no path can be found.
            isFinished = true;
            return null;
        }

        List<MovementPoint> newWaiting = new List<MovementPoint>();

        waiting.Sort(delegate(MovementPoint a, MovementPoint b) {
            if (a.distance < b.distance) return -1;
            if (b.distance < a.distance) return 1;
            return 0;
        });
        waiting.Reverse();

        for (int i = waiting.Count - 1; i > -1; i--) {
            MovementPoint current = waiting[i];

            // check if the currently checked point is the source, regenerate and return path if true
            if (current.position == source) {
                isFinished = true;
                // retrace path and return it

                List<MovementPoint> path = new List<MovementPoint>();
                MovementPoint breadCrumb = current;
                while (breadCrumb.position != target) {
                    path.Add(breadCrumb);
                    breadCrumb = breadCrumb.origin;
                }
                path.Add(entryPoint);
                return path;
            }

            //Debug.Log("current.length: " + current.length.ToString() + "; maxRange: " + maxRange.ToString());
            if (maxRange == null || current.length < maxRange) {
                List<Vector2i> neighbours = GetNeighbours(current.position);

                foreach (var neighbour in neighbours) {
                    if (!IsPassable(neighbour)) continue;

                    if (waiting.Find(x => x.position == neighbour) != null) continue;
                    if (done.Find(x => x.position == neighbour) != null) continue;

                    MovementPoint newPoint = new MovementPoint {
                        position = neighbour,
                        origin = current,
                        distance = Chebyshev(neighbour, source),
                        length = current.length + 1
                    };
                    
                    newWaiting.Add(newPoint);
                }
            }
            
            waiting.Remove(current);
            done.Add(current);

        }

        waiting.AddRange(newWaiting);
        newWaiting.Clear();
        if (waiting.Count > 50) {
            waiting.RemoveRange(50, waiting.Count - 50);
        }
    }

    return null;
}

public List<Vector2i> GetInRange(Vector2i position, int range) {
    List<Vector2i> tiles = new List<Vector2i>();
    tiles.Add(position);
        
    float start = Time.realtimeSinceStartup;

    int currentPosition = 0;
    int r = range;
    while (r-- > 0) {
        int currentCount = tiles.Count;
        for (int j = currentPosition; j < currentCount; j++) {
            List<Vector2i> neighbours = GetNeighbours(tiles[j]);
            for (int k = 0; k < neighbours.Count; k++) {
                Vector2i found = tiles.Find(x => x == neighbours[k]);
                if (null == found) tiles.Add(neighbours[k]);
            }
        }
        currentPosition = currentCount;
    }

    float end = Time.realtimeSinceStartup;
    //Debug.Log("Time spent looking for neighbours: " + (end - start));

    //Debug.Log(tiles.Count + " tiles found, checking passability and path length");

    start = Time.realtimeSinceStartup;

    List<Vector2i> closeTiles = new List<Vector2i>();
    for (int i = 0; i < tiles.Count; i++) {
        if (!IsPassable(tiles[i])) continue;
        List<MovementPoint> path = FindPath(position, tiles[i], range);
        if (path != null && path.Count - 1 <= range) closeTiles.Add(tiles[i]);
    }

    end = Time.realtimeSinceStartup;
    //Debug.Log("Time spent checking neighbours paths: " + (end - start));

    return closeTiles;
}

}
