
using System.Collections;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    [SerializeField] private GameObject hitZone;
    [SerializeField] private Sprite spriteToChange;
    [SerializeField] private Sprite spriteDefault;
    [SerializeField] private float bouncePos;

    private float fallSpeed = 200f;
    private bool isHit = false;
    private RectTransform tileRectTransform;
    private Image tileImage;

    private void Start()
    {
        tileRectTransform = GetComponent<RectTransform>();
        tileImage = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.IsGameOver()) return;

        // Move tile down
        if(!isHit)
            transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);

        if (tileRectTransform.anchoredPosition.y < bouncePos)
        {
            GameManager.Instance.SetState(GameState.GameOver);
        }
    }

    public void OnHitTile(float checkPointY)
    {
        if (isHit) return;
        isHit = true;

        float distance = Mathf.Abs(transform.position.y - checkPointY);

        if (distance < 20f)
            GameManager.Instance.AddScore("Perfect");
        else if (distance < 45f) 
            GameManager.Instance.AddScore("Great");
        else if (distance < 75f) 
            GameManager.Instance.AddScore("Good");
        else if (distance < 100f) 
            GameManager.Instance.AddScore("Cool");

        StartCoroutine(HitTileEffect());
    }

    IEnumerator HitTileEffect()
    {
        tileImage.sprite = spriteToChange;

        Color originalColor = tileImage.color;
        Color flashColor;
        ColorUtility.TryParseHtmlString("#DBB79A", out flashColor);

        float duration = .3f;
        float t = 0f;

        tileImage.color = flashColor;

        while (t < duration)
        {
            t += Time.deltaTime;
            tileImage.color = Color.Lerp(flashColor, originalColor, t / duration);
            yield return null;
        }

        tileImage.color = originalColor;

        isHit = false;

        TileObjectPooling.Instance.ReturnObject(gameObject);
    }

    public void ResetTile()
    {
        isHit = false;
        if (tileImage != null)
        {
            tileImage.color = Color.white;
            tileImage.sprite = spriteDefault;
        }
    }
}
