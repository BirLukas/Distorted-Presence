using UnityEngine;

public class FootstepManager : MonoBehaviour
{
    public AudioSource footstepSource;
    public AudioClip[] footstepSounds;
    public AudioClip[] carpetFootstepSounds;

    public float stepInterval = 0.5f;
    public float sprintStepInterval = 0.3f;
    private float stepTimer;

    private CharacterController characterController;
    private PlayerMovement playerMovement;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        if (characterController.isGrounded && characterController.velocity.magnitude > 0.1f)
        {
            stepTimer -= Time.deltaTime;

            if (stepTimer <= 0)
            {
                PlayFootstep();
                stepTimer = (playerMovement != null && playerMovement.isSprinting) ? sprintStepInterval : stepInterval;
            }
        }
        else
        {
            stepTimer = 0;
        }
    }

    void PlayFootstep()
    {
        AudioClip[] currentSounds = footstepSounds;

        // Check the surface below the player
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 2f))
        {
            if (hit.collider.CompareTag("Carpet"))
            {
                currentSounds = carpetFootstepSounds;
            }
        }

        if (currentSounds == null || currentSounds.Length == 0) return;

        int index = Random.Range(0, currentSounds.Length);
        footstepSource.clip = currentSounds[index];

        footstepSource.pitch = Random.Range(0.8f, 1.1f);
        footstepSource.volume = Random.Range(0.7f, 1.0f);

        footstepSource.Play();
    }
}
