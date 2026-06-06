using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RoomTrigger : MonoBehaviour
{
    private bool isVisited = false;

    void Start()
    {
        // Zajistíme, že collider je trigger
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }

        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.RegisterRoom(this);
        }
        else
        {
            Debug.LogWarning($"[RoomTrigger] RoomManager nenalezen ve scéně! Zkontrolujte, zda je přítomen.");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isVisited && other.CompareTag("Player"))
        {
            isVisited = true;
            if (RoomManager.Instance != null)
            {
                RoomManager.Instance.MarkRoomVisited(this);
            }
        }
    }
}
