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

    private bool isAimed = false;
    public static bool IsAimingGlobal { get; private set; }
    private Vector3 defaultModelPosition;
    private Quaternion defaultModelRotation;

    void Start()
    {
        // Uložení výchozí pozice a rotace 3D modelu kamery na zaèátku hry
        if (cameraModel != null)
        {
            defaultModelPosition = cameraModel.transform.localPosition;
            defaultModelRotation = cameraModel.transform.localRotation;
        }

        // Skrytí UI hledáèku a nastavení blesku na prùhledný stav pøi startu
        if (cameraUI != null) cameraUI.SetActive(false);
        if (flashGroup != null) flashGroup.alpha = 0f;

        // Ujistìte se, že kamera má správné výchozí FOV
        if (playerCamera != null) playerCamera.fieldOfView = defaultFOV;
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

        // Zde by normálnì probíhala animace pøesunu modelu pøed oblièej.
        // Pro tuto implementaci pouze pøepínáme viditelnost UI a model schováváme.

        if (cameraModel != null) cameraModel.SetActive(false);
        if (cameraUI != null) cameraUI.SetActive(true);
    }

    /// <summary>
    /// Logika pro návrat do normálního stavu.
    /// </summary>
    void StopAiming()
    {
        isAimed = false;

        // Zobrazíme model zpìt v ruce a skryjeme UI hledáèku.
        if (cameraModel != null) cameraModel.SetActive(true);
        if (cameraUI != null) cameraUI.SetActive(false);
    }

    /// <summary>
    /// Akce focení.
    /// </summary>
    void TakePhoto()
    {
        Debug.Log("Fotíme! Blesk aktivován.");

        // Spustíme coroutine, která se postará o efekt blesku
        StartCoroutine(FlashEffect());

        // Zde je místo pro skuteènou logiku ukládání screenshotu na disk, napø:
        // ScreenCapture.CaptureScreenshot("MojeFotka_" + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".png");
    }

    /// <summary>
    /// Coroutine, která na krátkou dobu rozsvítí a zhasne bílý panel (blesk).
    /// </summary>
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