using UnityEngine;
using System.Collections.Generic;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance { get; private set; }

    private HashSet<RoomTrigger> allRooms = new HashSet<RoomTrigger>();
    private HashSet<RoomTrigger> visitedRooms = new HashSet<RoomTrigger>();
    
    public int TotalRoomCount => allRooms.Count;
    public int VisitedRoomCount => visitedRooms.Count;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void RegisterRoom(RoomTrigger room)
    {
        allRooms.Add(room);
    }

    public void MarkRoomVisited(RoomTrigger room)
    {
        if (!visitedRooms.Contains(room))
        {
            visitedRooms.Add(room);
            Debug.Log($"[RoomManager] Room visited. Progress: {visitedRooms.Count} / {allRooms.Count}");
        }
    }

    public bool HaveAllRoomsBeenVisited()
    {
        if (allRooms.Count == 0) return true;
        return visitedRooms.Count >= allRooms.Count;
    }
}
