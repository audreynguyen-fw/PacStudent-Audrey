using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [Header("Level Prefabs")]
    public GameObject Corner_Outside;
    public GameObject Wall_Outside;
    public GameObject Corner_Inside;
    public GameObject Wall_Inside;
    public GameObject Pellet_Normal;
    public GameObject Pellet_Power_0;
    public GameObject TJunction;
    public GameObject Wall_GhostExit;

    [Header("Reference to Manual Level")]
    public GameObject manualLevelParent;

    private int[,] levelMap = new int[,]
    {
        {1,2,2,2,2,2,2,2,2,2,2,2,2,7},
        {2,5,5,5,5,5,5,5,5,5,5,5,5,4},
        {2,5,3,4,4,3,5,3,4,4,4,3,5,4},
        {2,6,4,0,0,4,5,4,0,0,0,4,5,4},
        {2,5,3,4,4,3,5,3,4,4,4,3,5,3},
        {2,5,5,5,5,5,5,5,5,5,5,5,5,5},
        {2,5,3,4,4,3,5,3,3,5,3,4,4,4},
        {2,5,3,4,4,3,5,4,4,5,3,4,4,3},
        {2,5,5,5,5,5,5,4,4,5,5,5,5,4},
        {1,2,2,2,2,1,5,4,3,4,4,3,0,4},
        {0,0,0,0,0,2,5,4,3,4,4,3,0,3},
        {0,0,0,0,0,2,5,4,4,0,0,0,0,0},
        {0,0,0,0,0,2,5,4,4,0,3,4,4,8},
        {2,2,2,2,2,1,5,3,3,0,4,0,0,0},
        {0,0,0,0,0,0,5,0,0,0,4,0,0,0}
    };

    private int rows;
    private int cols;

    void Start()
    {
        if (manualLevelParent != null) manualLevelParent.SetActive(false);

        rows = levelMap.GetLength(0);
        cols = levelMap.GetLength(1);

        GenerateFullLevel();
    }

    void GenerateFullLevel()
    {
        GameObject rootLevel = new GameObject("Generated_Level");

        Transform qTopLeft = CreateQuadrantContainer("Generated_TopLeft", new Vector3(0, 0, 0), rootLevel.transform);
        Transform qTopRight = CreateQuadrantContainer("Generated_TopRight", new Vector3(cols * 2 - 1, 0, 0), rootLevel.transform);
        Transform qBottomLeft = CreateQuadrantContainer("Generated_BottomLeft", new Vector3(0, -(rows - 1) * 2, 0), rootLevel.transform);
        Transform qBottomRight = CreateQuadrantContainer("Generated_BottomRight", new Vector3(cols * 2 - 1, -(rows - 1) * 2, 0), rootLevel.transform);

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                int val = levelMap[r, c];
                if (val == 0) continue;
                
                float rotTL = CalculateRotation(r, c, val);
                SpawnTile(val, new Vector3(c, -r, 0), Quaternion.Euler(0, 0, rotTL), qTopLeft);
                
                float rotTR = GetMirroredRotationX(val, rotTL);
                SpawnTile(val, new Vector3(-c, -r, 0), Quaternion.Euler(0, 0, rotTR), qTopRight);

                if (r < rows - 1)
                {
                    // 3. Bottom-Left (Lật đối xứng qua trục Y)
                    float rotBL = GetMirroredRotationY(val, rotTL);
                    SpawnTile(val, new Vector3(c, r, 0), Quaternion.Euler(0, 0, rotBL), qBottomLeft);

                    // 4. Bottom-Right (Lật đối xứng cả X và Y)
                    float rotBR = GetMirroredRotationXY(val, rotTL);
                    SpawnTile(val, new Vector3(-c, r, 0), Quaternion.Euler(0, 0, rotBR), qBottomRight);
                }
            }
        }
    }

    int GetMapValue(int r, int c)
    {
        if (c == cols) c = cols - 1;
        if (r == rows) r = rows - 1;

        if (r < 0 || r >= rows || c < 0 || c >= cols) return -1;
        return levelMap[r, c];
    }

    bool IsWall(int r, int c)
    {
        int val = GetMapValue(r, c);
        return (val == 1 || val == 2 || val == 3 || val == 4 || val == 7 || val == 8);
    }

    float CalculateRotation(int r, int c, int val)
    {
        if (val == 8) return 90f;
        if (val == 5 || val == 6) return 0f;

        bool up = IsWall(r - 1, c);
        bool down = IsWall(r + 1, c);
        bool left = IsWall(r, c - 1);
        bool right = IsWall(r, c + 1);
        
        if (val == 1)
        {
            if (right && down) return 0f;
            if (right && up) return 90f;
            if (left && up) return 180f;
            if (left && down) return 270f;
        }
        else if (val == 3)
        {
            bool wUp = IsWall(r - 1, c);
            bool wDown = IsWall(r + 1, c);
            bool wLeft = IsWall(r, c - 1);
            bool wRight = IsWall(r, c + 1);
            
            if (wRight && wDown && !wLeft && !wUp) return 0f;
            if (wRight && wUp && !wLeft && !wDown) return 90f;
            if (wLeft && wUp && !wRight && !wDown) return 180f;
            if (wLeft && wDown && !wRight && !wUp) return 270f;
            
            if (wUp && wDown && wLeft && wRight)
            {
                bool diagTopRight    = !IsWall(r - 1, c + 1);
                bool diagBottomRight = !IsWall(r + 1, c + 1);
                bool diagBottomLeft  = !IsWall(r + 1, c - 1);
                bool diagTopLeft     = !IsWall(r - 1, c - 1);

                if (diagTopRight)    return 90f;
                if (diagBottomRight) return 0f;
                if (diagBottomLeft)  return 270f;
                if (diagTopLeft)     return 180f;
            }

            if (!wLeft)  return 90f;
            if (!wRight) return 180f;
            if (!wUp)    return 270f;
            if (!wDown)  return 0f;
        }
        else if (val == 2 || val == 4)
        {
            int upVal = GetMapValue(r - 1, c);
            int downVal = GetMapValue(r + 1, c);

            if (upVal == 5 || upVal == 6 || upVal == 0 || downVal == 5 || downVal == 6 || downVal == 0)
            {
                return 90f;
            }

            return 0f;
        }
        else if (val == 7)
        {
            if (!right) return 180f;
            if (!up) return 270f;
            if (!left) return 90f;
            if (!down) return 0f; 
        }

        return 0f;
    }
    
    float GetMirroredRotationX(int val, float rot)
    {
        if (val == 2 || val == 4) return (rot == 0f) ? 0f : 90f;
        if (val == 1 || val == 3)
        {
            if (rot == 0f) return 270f;
            if (rot == 90f) return 180f;
            if (rot == 180f) return 90f;
            if (rot == 270f) return 0f;
        }
        if (val == 7)
        {
            if (rot == 180f) return 90f;
            if (rot == 90f) return 180f;
            return rot;                   // 
        }
        return rot;
    }
    
    float GetMirroredRotationY(int val, float rot)
    {
        float normalizedRot = Mathf.Repeat(rot, 360f);
        if (val == 2 || val == 4) return (rot == 0f) ? 0f : 90f;
        if (val == 1 || val == 3)
        {
            if (rot == 0f) return 90f;
            if (rot == 90f) return 0f;
            if (rot == 180f) return 270f;
            if (rot == 270f) return 180f;
        }
        if (val == 7)
        {
            if (rot == 270f) return 90f;
            if (rot == 90f) return 270f;
            return rot;                   //
        }
        return rot;
    }
    
    float GetMirroredRotationXY(int val, float rot)
    {
        float normalizedRot = Mathf.Repeat(rot, 360f);
        int r = Mathf.RoundToInt(normalizedRot);
        
        if (val == 2 || val == 4) return (rot == 0f) ? 0f : 90f;
        if (val == 1 || val == 3)
        {
            if (rot == 0f) return 180f; 
            if (rot == 90f) return 270f;
            if (rot == 180f) return 0f;
            if (rot == 270f) return 90f;
        }
        if (val == 7)
        {
            if (r == 270) return 90f;   
            if (r == 90)  return 270f;  
            if (r == 180) return 0f;    
            if (r == 0)   return 180f;  
        }
        return rot;
    }

    Transform CreateQuadrantContainer(string name, Vector3 pos, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.parent = parent;
        go.transform.position = pos;
        return go.transform;
    }

    void SpawnTile(int type, Vector3 localPos, Quaternion rot, Transform parent)
    {
        GameObject prefab = GetPrefabByType(type);
        if (prefab != null)
        {
            GameObject obj = Instantiate(prefab, parent);
            obj.transform.localPosition = localPos;
            obj.transform.localRotation = rot;

            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingOrder = 10;
            }
        }
    }

    GameObject GetPrefabByType(int type)
    {
        switch (type)
        {
            case 1: return Corner_Outside;
            case 2: return Wall_Outside;
            case 3: return Corner_Inside;
            case 4: return Wall_Inside;
            case 5: return Pellet_Normal;
            case 6: return Pellet_Power_0;
            case 7: return TJunction;
            case 8: return Wall_GhostExit;
            default: return null;
        }
    }
}