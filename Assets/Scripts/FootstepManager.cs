using UnityEngine;

public class FootstepManager : MonoBehaviour
{
    public AudioSource footstepSource;
    public AudioClip[] footstepSounds;

    public float stepInterval = 0.5f;
    private float stepTimer;

    private CharacterController characterController;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (characterController.isGrounded && characterController.velocity.magnitude > 0.1f)
        {
            stepTimer -= Time.deltaTime;

            if (stepTimer <= 0)
            {
                PlayFootstep();
                stepTimer = stepInterval;
            }
        }
        else
        {
            stepTimer = 0;
        }
    }

    void PlayFootstep()
    {
        if (footstepSounds.Length == 0) return;

        int index = Random.Range(0, footstepSounds.Length);
        footstepSource.clip = footstepSounds[index];

        footstepSource.pitch = Random.Range(0.8f, 1.1f);
        footstepSource.volume = Random.Range(0.7f, 1.0f);

        footstepSource.Play();
    }
}
