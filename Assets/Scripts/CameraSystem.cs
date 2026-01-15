using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CameraSystem : MonoBehaviour
{
    [Header("Setup Objekty")]
    public GameObject cameraModel;      // 3D model kamery v ruce
    public GameObject cameraUI;         // Objekt UI s rámeèkem/møížkou hledáèku (Canvas Panel)
    public CanvasGroup flashGroup;      // CanvasGroup na objektu "Flash" pro ovládání prùhlednosti
    public Camera playerCamera;         // Hlavní kamera hráèe (FPS pohled)

    [Header("Nastavení Pohledu a Zoomu")]
    public float defaultFOV = 60f;      // Výchozí FOV pøi bìžném pohybu
    public float aimedFOV = 30f;        // FOV pøi zamíøení (zoom)
    public float aimSpeed = 5f;         // Rychlost plynulého pøechodu mezi FOV

    [Header("Nastavení Blesku")]
    public float flashDuration = 0.1f;  // Doba trvání bílého záblesku (v sekundách)

    // Odkaz na skript, který provádí logiku focení (je na kameøe)
    private PhotoCapture photoCapture;

    private bool isAimed = false;
    public static bool IsAimingGlobal { get; private set; }

    void Start()
    {
        // Kontrola, zda máme referenci na hlavní kameru
        if (playerCamera == null)
        {
            Debug.LogError("Chyba: Player Camera není pøiøazena v Inspectoru CameraSystem!");
            return;
        }

        // Najdeme PhotoCapture skript na objektu kamery
        photoCapture = playerCamera.GetComponent<PhotoCapture>();

        if (photoCapture == null)
        {
            Debug.LogError("Chyba! Skript PhotoCapture nebyl nalezen na Player Camera.");
        }

        // Skrytí UI hledáèku a nastavení blesku na prùhledný stav pøi startu
        if (cameraUI != null) cameraUI.SetActive(false);
        if (flashGroup != null) flashGroup.alpha = 0f;

        // Ujistìte se, že kamera má správné výchozí FOV
        playerCamera.fieldOfView = defaultFOV;
    }

    void Update()
    {
        // Detekce podržení pravého tlaèítka myši pro zamíøení
        if (Input.GetMouseButton(1))
        {
            AimCamera();
        }
        else
        {
            StopAiming();
        }

        // Pokud je aktivnì zamìøeno, levé tlaèítko myši fotí
        if (isAimed && Input.GetMouseButtonDown(0))
        {
            TakePhoto();
        }

        // Plynulý pøechod mezi FOV pro efekt zoomu/oddálení
        float targetFOV = isAimed ? aimedFOV : defaultFOV;
        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, Time.deltaTime * aimSpeed);

        IsAimingGlobal = isAimed;
    }

    void AimCamera()
    {
        isAimed = true;

        if (cameraModel != null) cameraModel.SetActive(false);
        if (cameraUI != null) cameraUI.SetActive(true);
    }

    void StopAiming()
    {
        isAimed = false;

        if (cameraModel != null) cameraModel.SetActive(true);
        if (cameraUI != null) cameraUI.SetActive(false);
    }
    void TakePhoto()
    {
        if (photoCapture != null)
        {
            // Kontrola, zda má hráè ještì film, než se spustí vizuální efekty
            if (photoCapture.GetRemainingFilmCount() > 0)
            {
                // Vizuální efekt blesku se spustí jen pokud je èím fotit
                StartCoroutine(FlashEffect());

                // Logika snímání (v této metodì se odeète film)
                photoCapture.TryCaptureAnomaly();
            }
            else
            {
                Debug.Log("Nelze vyvolat blesk: Došel film.");
            }
        }
        else
        {
            Debug.LogError("Nelze fotit: Skript PhotoCapture není pøipojen.");
        }
    }
    IEnumerator FlashEffect()
    {
        // 1. Okamžitì zviditelnit blesk (plná alfa, viditelné)
        if (flashGroup != null) flashGroup.alpha = 1f;

        // 2. Poèkat po definovanou dobu trvání blesku
        yield return new WaitForSeconds(flashDuration);

        // 3. Okamžitì zneviditelnit blesk (nulová alfa, prùhledné)
        if (flashGroup != null) flashGroup.alpha = 0f;
    }
}