using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PhotoReviewUI : MonoBehaviour
{
    public RawImage photoImage;
    public Image resultIcon;
    public Sprite correctSprite;
    public Sprite incorrectSprite;

    public void Setup(PhotoData data)
    {
        photoImage.texture = data.snapshot;

        if (data.isCorrect)
        {
            resultIcon.sprite = correctSprite;
            resultIcon.color = Color.green;
        }
        else
        {
            resultIcon.sprite = incorrectSprite;
            resultIcon.color = Color.red;
        }
    }
}
