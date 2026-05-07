using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class UndiscoveredAreaManager : MonoBehaviour
{
    [SerializeField] private Tilemap undiscoveredTilemap;

    private readonly HashSet<string> discoveredAreas = new HashSet<string>();
    private readonly Dictionary<Vector3Int, TileBase> initialTiles = new Dictionary<Vector3Int, TileBase>();

    private void Awake()
    {
        if (undiscoveredTilemap == null)
        {
            undiscoveredTilemap = FindUndiscoveredTilemap();
        }

        CacheInitialTiles();
    }

    public bool IsDiscovered(string areaId)
    {
        if (string.IsNullOrWhiteSpace(areaId))
        {
            return false;
        }

        if (discoveredAreas.Contains(areaId))
        {
            return true;
        }

        return false;
    }

    public void RevealArea(string areaId, BoundsInt cellBounds)
    {
        if (string.IsNullOrWhiteSpace(areaId))
        {
            return;
        }

        if (!IsDiscovered(areaId))
        {
            discoveredAreas.Add(areaId);
        }

        ClearAreaTiles(cellBounds);
    }

    public void ClearAreaTiles(BoundsInt cellBounds)
    {
        if (undiscoveredTilemap == null)
        {
            return;
        }

        for (int x = cellBounds.xMin; x < cellBounds.xMax; x++)
        {
            for (int y = cellBounds.yMin; y < cellBounds.yMax; y++)
            {
                undiscoveredTilemap.SetTile(new Vector3Int(x, y, 0), null);
            }
        }
    }

    public BoundsInt WorldBoundsToCellBounds(Bounds worldBounds)
    {
        if (undiscoveredTilemap == null)
        {
            return new BoundsInt();
        }

        Vector3Int min = undiscoveredTilemap.WorldToCell(worldBounds.min);
        Vector3Int max = undiscoveredTilemap.WorldToCell(worldBounds.max);

        return new BoundsInt(
            min.x,
            min.y,
            0,
            max.x - min.x + 1,
            max.y - min.y + 1,
            1
        );
    }

    public List<string> GetDiscoveredAreaIds()
    {
        return new List<string>(discoveredAreas);
    }

    public void LoadDiscoveredAreas(List<string> areaIds)
    {
        discoveredAreas.Clear();

        if (areaIds != null)
        {
            for (int i = 0; i < areaIds.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(areaIds[i]))
                {
                    discoveredAreas.Add(areaIds[i]);
                }
            }
        }

        RestoreInitialTiles();
        RefreshAllDiscoverableAreas();
    }

    private void RefreshAllDiscoverableAreas()
    {
        DiscoverableArea[] areas = FindObjectsOfType<DiscoverableArea>(true);

        for (int i = 0; i < areas.Length; i++)
        {
            DiscoverableArea area = areas[i];
            if (area == null)
            {
                continue;
            }

            string id = area.GetAreaId();
            if (!string.IsNullOrWhiteSpace(id) && discoveredAreas.Contains(id))
            {
                area.ForceReveal();
            }
        }
    }

    private Tilemap FindUndiscoveredTilemap()
    {
        Tilemap[] tilemaps = FindObjectsOfType<Tilemap>(true);

        for (int i = 0; i < tilemaps.Length; i++)
        {
            if (tilemaps[i] != null && tilemaps[i].name == "undiscovered")
            {
                return tilemaps[i];
            }
        }

        return null;
    }
    private void CacheInitialTiles()
    {
        initialTiles.Clear();

        if (undiscoveredTilemap == null)
        {
            return;
        }

        BoundsInt bounds = undiscoveredTilemap.cellBounds;
        foreach (Vector3Int position in bounds.allPositionsWithin)
        {
            TileBase tile = undiscoveredTilemap.GetTile(position);
            if (tile != null)
            {
                initialTiles[position] = tile;
            }
        }
    }

    private void RestoreInitialTiles()
    {
        if (undiscoveredTilemap == null)
        {
            return;
        }

        undiscoveredTilemap.ClearAllTiles();

        foreach (KeyValuePair<Vector3Int, TileBase> pair in initialTiles)
        {
            undiscoveredTilemap.SetTile(pair.Key, pair.Value);
        }
    }
}
