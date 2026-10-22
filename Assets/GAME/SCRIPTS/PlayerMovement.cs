using System.Collections;
using UnityEngine;

public class PlayerMover : MonoBehaviour
{
    public Board board;
    public float moveSpeed = 4f;
    public float pauseBetweenTiles = 0.05f;
    public Vector3 offset = new Vector3(0, 1f, 0);

    public int CurrentIndex { get; private set; } = 0;
    public bool IsMoving { get; private set; }

    private void Start()
    {
        transform.position = board.GetTile(CurrentIndex).transform.position + offset;
    }

    // steps > 0 : avance, steps < 0 : recule
    public IEnumerator MoveSteps(int steps)
    {
        IsMoving = true;
        int direction = steps >= 0 ? 1 : -1;

        for (int i = 0; i < Mathf.Abs(steps); i++)
        {
            CurrentIndex = board.Wrap(CurrentIndex + direction);
            Vector3 target = board.GetTile(CurrentIndex).transform.position + offset;
            yield return MoveTo(target);
            yield return new WaitForSeconds(pauseBetweenTiles);
        }

        IsMoving = false;
    }

    private IEnumerator MoveTo(Vector3 target)
    {
        // Orientation vers la prochaine case
        Vector3 dir = target - transform.position;
        dir.y = 0;
        if (dir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(dir);

        while ((transform.position - target).sqrMagnitude > 0.0001f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }
    }
}