using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{



    [SerializeField] GameObject playerPrefab;
    [SerializeField] Grid grid;
    [SerializeField] Transform cardContainer;


    public PlayerController player;
    private List<SimpleAI> enemies = new List<SimpleAI>();

    public Image tileInfoImage;
    public Text tileInfoTitle;
    public Text tileInfoDescription;
    [SerializeField] private Button endTurnButton;

    [HideInInspector] public bool isCardSelected = false;
    [HideInInspector] public bool playerTurn;

    static public GameController gameController = null;

    public void Awake()
    {
        if (gameController != null) return;
        gameController = this;
    }

    public void Start()
    {
        player = Instantiate(playerPrefab, Vector2.zero, Quaternion.identity).GetComponent<PlayerController>();
        player.InitializeCards(cardContainer);

        grid.Initialize();
        //Vector2 playerSpawn = grid.LoadMap(MapGenerator.generateMap(grid, 50));
        Vector2 playerSpawn = grid.LoadMap(PrimMapGenerator.GenerateMap((int)grid.getSize().x, (int)grid.getSize().y));
        grid.GetTileByPosition(playerSpawn).SetOccupyingCharacter(player);
        player.transform.position = playerSpawn;
        player.SetGrid(grid);
        FindObjectOfType<CameraController>().SetPlayer(player);
        playerTurn = true;
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ChangeTurn();
        }
    }

    public void AddEnemy(SimpleAI enemy)
    {
        enemies.Add(enemy);
    }

    public void ChangeTurn()
    {
        playerTurn = !playerTurn;
        endTurnButton.gameObject.SetActive(playerTurn);
        player.SetHandActive(playerTurn);
        if (!playerTurn)
        {
            StartCoroutine("MakeEnemiesTurn");
        }
    }



    IEnumerator MakeEnemiesTurn()
    {
        foreach (SimpleAI ai in enemies)
        {
            yield return new WaitForSeconds(1);
            ai.MakeAction();
        }
        ChangeTurn();
    }
}
