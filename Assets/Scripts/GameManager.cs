using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;
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
    [Header("Score")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private TextMeshProUGUI bonusText;
    [Header("Other Properties")]
    [SerializeField] private float spawnTileSpeed;
    [SerializeField] private SongInfo songData;
    [SerializeField] private GraphicRaycaster raycaster;
    [SerializeField] private EventSystem eventSystem;
    

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
        SoundManager.Instance.PlaySound(SoundManager.Instance.cupidAudio);
        secondPerBeat = spawnTileSpeed / songData.bpm;
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

            yield return new WaitForSeconds(secondPerBeat);
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
        Debug.Log("Miss");
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

        if (bonus != 0)
            StartCoroutine(EnableBonusText());
    }

    IEnumerator EnableScoreText()
    {
        scoreText.gameObject.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        scoreText.gameObject.SetActive(false);
    }

    IEnumerator EnableResultText()
    {
        resultText.gameObject.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        resultText.gameObject.SetActive(false);
    }

    IEnumerator EnableBonusText()
    {
        bonusText.gameObject.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        bonusText.gameObject.SetActive(false);
    }
}
