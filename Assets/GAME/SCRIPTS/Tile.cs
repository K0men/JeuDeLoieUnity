using UnityEngine;

public enum TileType { Normal, Bonus }

public class Tile : MonoBehaviour
{
    public TileType type = TileType.Normal;

    [Tooltip("Positif = avance, négatif = recule")]
    public int moveAmount = 0;

    // Couleur pour repérer les cases dans l'éditeur
    private void OnDrawGizmos()
    {
        Gizmos.color = type == TileType.Normal ? Color.white :
                       (moveAmount > 0 ? Color.green : Color.red);
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.5f, 0.3f);
    }
}