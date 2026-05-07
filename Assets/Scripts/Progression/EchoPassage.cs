using UnityEngine;

public class EchoPassage : MonoBehaviour
{
    [SerializeField] private Transform exitPoint;

    public bool TryGetDestination(out Vector3 destination)
    {
        if (exitPoint != null)
        {
            destination = exitPoint.position;
            return true;
        }

        destination = transform.position;
        return false;
    }
}
