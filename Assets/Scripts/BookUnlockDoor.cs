using UnityEngine;

public class BookUnlockDoor : MonoBehaviour
{
    [Header("UI Reference")]
    public BookInteract book;
    public Door door;
    public Light lightToTurnOff;

    private bool unlocked = false;

    void Update()
    {
        if (!unlocked && book != null && book.HasBeenReadOnce)
        {
            unlocked = true;

            if (lightToTurnOff != null)
                lightToTurnOff.enabled = false;

            if (door != null)
                door.UnlockDoor();
        }
    }
}
