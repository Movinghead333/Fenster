using UnityEngine;
using UnityEngine.UI;

public class PlayCardUIC : MonoBehaviour
{
    public Vector2Int CardCoordinates;
    public Image CardImage;


    private CardHighlightStatus CardHighlightStatus = CardHighlightStatus.None;

    public void SetSprite(Sprite sprite)
    {
        CardImage.sprite = sprite;
    }

    public void OnCardImageClicked()
    {
        FensterUIC.Instance.OnCardSelected(CardCoordinates);
    }

    public void SetCardHighlightStatus(CardHighlightStatus status)
    {
        CardHighlightStatus = status;
        if (CardHighlightStatus == CardHighlightStatus.None)
        {
            CardImage.color = Color.white;
        }
        else if (CardHighlightStatus == CardHighlightStatus.Selectable)
        {
            CardImage.color = new Color(0.5f, 1f, 0.5f);
        }
    }

    private void Update()
    {
        switch (CardHighlightStatus)
        {
            case CardHighlightStatus.None:
                break;
            case CardHighlightStatus.Selectable:
                break;
            case CardHighlightStatus.Selected:
                int mod1 = (int)(Time.time * 200) % 200;
                float t1 = (mod1 < 100 ? mod1 : 200 - mod1) / 100.0f;
                CardImage.color = Color.Lerp(Color.white, Color.green, t1);
                break;
            case CardHighlightStatus.IncorrectGuess:
                int mod2 = (int)(Time.time * 200) % 200;
                float t2 = (mod2 < 100 ? mod2 : 200 - mod2) / 100.0f;
                CardImage.color = Color.Lerp(Color.white, new Color(1f, 0.5f, 0.5f, 1f), t2);
                break;
        }
    }
}
