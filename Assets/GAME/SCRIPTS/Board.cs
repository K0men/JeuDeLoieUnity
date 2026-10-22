using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    [Header("Génération du plateau")]
    public GameObject tilePrefab;
    public int tileCount = 24;
    public float radius = 8f;

    [Header("Cases (remplies automatiquement)")]
    public List<Tile> tiles = new List<Tile>();

    public int TileCount => tiles.Count;

    private void Awake()
    {
        // Si la liste est vide, on récupère les cases enfants dans l'ordre de la hiérarchie
        if (tiles.Count == 0)
            tiles.AddRange(GetComponentsInChildren<Tile>());
    }

    // Génère les cases en cercle (clic droit sur le composant > Générer le plateau)
    [ContextMenu("Générer le plateau")]
    public void Build()
    {
        if (tilePrefab == null)
        {
            Debug.LogError("Board : aucun prefab de case assigné.", this);
            return;
        }

        // Nettoyage des anciennes cases
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);
        tiles.Clear();

        for (int i = 0; i < tileCount; i++)
        {
            float angle = i * Mathf.PI * 2f / tileCount;
            Vector3 localPos = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
            Vector3 worldPos = transform.TransformPoint(localPos);

            GameObject go = Instantiate(tilePrefab, worldPos, Quaternion.identity, transform);
            go.name = $"Case_{i:00}";

            // Orientée vers le centre du plateau (à la même hauteur)
            Vector3 center = new Vector3(transform.position.x, worldPos.y, transform.position.z);
            go.transform.LookAt(center);

            // Enregistre directement la case dans la liste
            Tile tile = go.GetComponent<Tile>();
            if (tile != null)
                tiles.Add(tile);
            else
                Debug.LogWarning($"Board : {go.name} n'a pas de composant Tile.", go);
        }

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    // Modulo toujours positif : gère la boucle dans les deux sens
    public int Wrap(int index)
    {
        return ((index % TileCount) + TileCount) % TileCount;
    }

    public Tile GetTile(int index) => tiles[Wrap(index)];
}