using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
using UnityEngine.XR;

public enum GameState
{
    None,
    GamePlay,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private GameState currentState;

    [Header("GameObject")]
    //[SerializeField] private GameObject tilePrefabs;
    [SerializeField] private GameObject checkPointLine;
    [SerializeField] private GameObject[] spawnPoes;
    [SerializeField] private GameObject gameOverPanel;
    [Header("Score")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private TextMeshProUGUI bonusText;
    [Header("Other Properties")]
    [SerializeField] private float spawnTileSpeed; // per minutes (spawnTileSpeed % 60 = 0)
    [SerializeField] private SongInfo songData;
    [SerializeField] private GraphicRaycaster raycaster;
    [SerializeField] private EventSystem eventSystem;
    [SerializeField] private Image decorImage;
    [Header("Difficulty")]
    [SerializeField] private float baseFallSpeed; // fallSpeed default value
    [SerializeField] private float fallSpeedIncrement; // increase fallSpeed
    [SerializeField] private float spawnSpeedDecrement; // spawn tile
    [SerializeField] private int scoreTileThreshold; // score mark to increase the difficulty
    [SerializeField] private int scoreSpawnThreshold; // score mark to increase the difficulty

    private float secondPerBeat;
    private int score = 0;
    private int bonus = 0;

    public void SetState(GameState state)
    {
        currentState = state;
    }

    public bool IsGamePlay()
    {
        if (currentState == GameState.GamePlay)
            return true;
        else
            return false;
    }

    public bool IsGameOver()
    {
        if(currentState == GameState.GameOver)
            return true;
        else
            return false;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        SoundManager.Instance.PlaySoundAudioClip(songData.audioClip);
        //secondPerBeat = spawnTileSpeed / songData.bpm;
        StartCoroutine(SpawnTileObject());
    }

    IEnumerator SpawnTileObject()
    {
        while (!IsGameOver())
        {
            int number = Random.Range(0, spawnPoes.Length);

            GameObject tile = TileObjectPooling.Instance.GetObject();
            tile.transform.SetParent(spawnPoes[number].transform, false);
            tile.transform.localPosition = Vector3.zero;

            Tile tileComp = tile.GetComponent<Tile>();
            if (tileComp != null)
            {
                tileComp.ResetTile();
            }

            yield return new WaitForSeconds(GetCurrentSpawnDelay());
        }
    }

    private void Update()
    {
        if (IsGameOver()) return;

        if (Input.GetMouseButtonDown(0))
        {
            PointerEventData pointerData = new PointerEventData(eventSystem)
            {
                position = Input.mousePosition
            };

            List<RaycastResult> results = new List<RaycastResult>();
            raycaster.Raycast(pointerData, results);

            bool hitTile = false;

            foreach (RaycastResult result in results)
            {
                Tile tile = result.gameObject.GetComponent<Tile>();
                if (tile != null)
                {
                    StartCoroutine(DecorEffect());
                    tile.OnHitTile(checkPointLine.transform.position.y);
                    hitTile = true;
                    break;
                }
            }

            if (!hitTile)
            {
                OnMissTile();
            }
        }
    }

    private void OnMissTile()
    {
        bonus = 0;
        SoundManager.Instance.PlaySound(SoundManager.Instance.wrongAudio);
    }

    public void AddScore(string result)
    {
        StartCoroutine(EnableResultText());
        StartCoroutine(EnableScoreText());
        

        int point = 0;
        int resultPoint;

        switch (result) 
        {
            case "Perfect":
                {
                    bonus++;
                    point = 4;
                }
                break;
            case "Great":
                {
                    point = 3;
                    bonus = 0;
                }
                break;
            case "Good":
                {
                    point = 2;
                    bonus = 0;
                }
                break;
            case "Cool":
                {
                    point = 1;
                    bonus = 0;
                }
                break;
        }

        resultPoint = point + bonus;
        score += resultPoint;

        scoreText.text = "" + score;
        bonusText.text = "x" + bonus;
        resultText.text = result;

        StartCoroutine(EnableBonusText());
    }

    IEnumerator EnableScoreText()
    {
        scoreText.gameObject.SetActive(true);
        yield return new WaitForSeconds(.5f);
        scoreText.gameObject.SetActive(false);
    }

    IEnumerator EnableResultText()
    {
        resultText.gameObject.SetActive(true);
        yield return new WaitForSeconds(.5f);
        resultText.gameObject.SetActive(false);
    }

    IEnumerator EnableBonusText()
    {
        if (bonus != 0)
        {
            bonusText.gameObject.SetActive(true);
            yield return new WaitForSeconds(.5f);
            bonusText.gameObject.SetActive(false);
        }
        else
        {
            bonusText.gameObject.SetActive(false);
        }
    }

    IEnumerator DecorEffect()
    {
        Color currentColor = decorImage.color;
        currentColor.a = 1f;
        decorImage.color = currentColor;
        yield return new WaitForSeconds(.5f);
        currentColor.a = .667f;
        decorImage.color = currentColor;
    }

    public float GetCurrentFallSpeed()
    {
        int step = score / scoreTileThreshold;
        return baseFallSpeed + step * fallSpeedIncrement;
    }

    public float GetCurrentSpawnDelay()
    {
        int step = score / scoreSpawnThreshold;
        float currentSpawnSpeed = spawnTileSpeed - step * spawnSpeedDecrement;

        if (currentSpawnSpeed < 60f)
            currentSpawnSpeed = 60f;

        return currentSpawnSpeed / songData.bpm;
    }

    public void GameOver()
    {
        gameOverPanel.SetActive(true);
        SoundManager.Instance.StopSound(SoundManager.Instance.gamePlayAudio);
    }

    public void OnPlayAgainBtnClicked()
    {
        SceneManager.LoadScene("MagicTitles");
    }

    public void OnQuitGameBtnClicked()
    {
        UnityEditor.EditorApplication.isPlaying = false; // quit game in editor

        Application.Quit(); // quit game in build
    }
}
