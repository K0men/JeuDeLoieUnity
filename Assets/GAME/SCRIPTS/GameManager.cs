using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Références")]
    public Board board;
    public PlayerMover player;
    public Button rollButton;
    public TMP_Text infoText;

    [Header("Dé")]
    public Image diceImage;
    [Tooltip("6 sprites, dans l'ordre : élément 0 = face 1, élément 5 = face 6")]
    public Sprite[] diceFaces = new Sprite[6];
    public float rollDuration = 0.8f;
    public float faceChangeInterval = 0.06f;

    [Header("Effets du dé")]
    [Tooltip("Vitesse de rotation en degrés par seconde")]
    public float spinSpeed = 720f;
    [Tooltip("Hauteur max des rebonds (en pixels UI)")]
    public float bounceHeight = 60f;
    [Tooltip("Nombre de rebonds pendant le lancer")]
    public int bounceCount = 3;
    [Tooltip("Agrandissement au point haut du rebond (1 = aucun)")]
    public float bounceScale = 1.25f;
    public float settleDuration = 0.25f;

    private bool isBusy;

    private void Start()
    {
        rollButton.onClick.AddListener(OnRollClicked);
        infoText.text = "Relancer le dé";

        // Affiche la face 1 au départ
        if (diceImage != null && diceFaces.Length == 6)
            diceImage.sprite = diceFaces[0];
    }

    private void OnRollClicked()
    {
        if (!isBusy) StartCoroutine(TurnRoutine());
    }

    private IEnumerator TurnRoutine()
    {
        isBusy = true;
        rollButton.interactable = false;

        // 1. Résultat tiré à l'avance (1 à 6)
        int dice = Random.Range(1, 7);

        // 2. Animation du dé, qui s'arrête sur le résultat
        yield return DiceRollAnimation(dice);
        infoText.text = $"Dé : {dice}";
        yield return new WaitForSeconds(0.5f);

        // 3. Déplacement
        yield return player.MoveSteps(dice);

        // 4. Case d'arrivée (bonus / malus)
        yield return ResolveTile();

        rollButton.interactable = true;
        isBusy = false;
    }

    private IEnumerator DiceRollAnimation(int result)
    {
        if (diceImage == null || diceFaces.Length != 6)
        {
            Debug.LogWarning("GameManager : assigne DiceImage et les 6 faces du dé.", this);
            yield break;
        }

        infoText.text = "Suspens...";

        RectTransform rt = diceImage.rectTransform;
        Vector2 basePos = rt.anchoredPosition;

        float elapsed = 0f;
        float faceTimer = 0f;
        float angle = 0f;
        int lastFace = -1;

        while (elapsed < rollDuration)
        {
            float dt = Time.deltaTime;
            elapsed += dt;
            faceTimer += dt;

            // Changement de face à intervalle régulier
            if (faceTimer >= faceChangeInterval)
            {
                faceTimer = 0f;
                int face;
                do { face = Random.Range(0, 6); } while (face == lastFace);
                lastFace = face;
                diceImage.sprite = diceFaces[face];
            }

            // Rotation continue
            angle += spinSpeed * dt;
            rt.localRotation = Quaternion.Euler(0f, 0f, angle);

            // Rebonds qui s'amortissent au fil du temps
            float t = Mathf.Clamp01(elapsed / rollDuration);
            float bounce = Mathf.Abs(Mathf.Sin(t * Mathf.PI * bounceCount));
            float damping = 1f - t;

            rt.anchoredPosition = basePos + Vector2.up * (bounce * bounceHeight * damping);
            rt.localScale = Vector3.one * (1f + (bounceScale - 1f) * bounce * damping);

            yield return null;
        }

        // Face finale = résultat
        diceImage.sprite = diceFaces[result - 1];

        // Le dé se redresse en douceur, avec un petit "pop"
        float startAngle = Mathf.Repeat(angle, 360f);
        if (startAngle > 180f) startAngle -= 360f; // chemin le plus court vers 0°

        float s = 0f;
        while (s < settleDuration)
        {
            s += Time.deltaTime;
            float k = Mathf.Clamp01(s / settleDuration);
            float eased = 1f - (1f - k) * (1f - k); // ease-out

            rt.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(startAngle, 0f, eased));
            rt.localScale = Vector3.one * (1f + 0.2f * Mathf.Sin(k * Mathf.PI));
            rt.anchoredPosition = basePos;

            yield return null;
        }

        // Remise à l'état de repos
        rt.localRotation = Quaternion.identity;
        rt.localScale = Vector3.one;
        rt.anchoredPosition = basePos;
    }

    private IEnumerator ResolveTile()
    {
        Tile tile = board.GetTile(player.CurrentIndex);

        int safety = 0;
        while (tile.type == TileType.Bonus && tile.moveAmount != 0 && safety < 10)
        {
            string txt = tile.moveAmount > 0
                ? $"Bonus ! Avance de {tile.moveAmount} cases"
                : $"Malus ! Recule de {-tile.moveAmount} cases";
            infoText.text = txt;
            yield return new WaitForSeconds(1f);

            yield return player.MoveSteps(tile.moveAmount);
            tile = board.GetTile(player.CurrentIndex);
            safety++;
        }

        infoText.text = "Relancer le dé";
    }
}