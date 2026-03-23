using UnityEngine;
using TMPro;

public class SplashText : MonoBehaviour
{
    public TextMeshProUGUI splashText;
    
    [TextArea(2, 5)]
    public string[] messages = new string[]
    {
        "Kamera běží!",
        "Je to jen hra. Nebo ne?",
        "Někdo tě sleduje...",
        "Zase pondělí...",
        "Úsměv prosím!",
        "Máš dost baterek?",
        "Zavři oči...",
        "Distorted Reality",
        "Neotáčej se..."
    };

    void Start()
    {
        if (splashText != null && messages.Length > 0)
        {
            splashText.text = messages[Random.Range(0, messages.Length)];
        }
    }
}
