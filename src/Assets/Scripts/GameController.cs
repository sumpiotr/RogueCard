using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{



    [SerializeField] GameObject playerPrefab;
    [SerializeField] Grid grid;
    [SerializeField] Transform cardContainer;
    [SerializeField] TileDataScriptableObject exitDoorData;
    [HideInInspector] public bool playerTurn;
    public PlayerController player;
    private List<SimpleAI> enemies = new List<SimpleAI>();

    //Enemies that take part in fight
    private List<SimpleAI> activeEnemies = new List<SimpleAI>();

    public Image tileInfoImage;
    public Text tileInfoTitle;
    public Text tileInfoDescription;
    [SerializeField] private GameObject endTurnButton;

    public bool isCardSelected = false;
    public bool battleMode = false;

    static public GameController instance = null;

    private int activeEnemyIndex = -1;

    public void Awake()
    {
        if (instance != null) return;
        instance = this;
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
        grid.SpawnEnemies(12, 5, 50);
        grid.CreateExit();
        endTurnButton.SetActive(false);
        playerTurn = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && playerTurn && battleMode) 
        {
            FindObjectOfType<CardPreview>().HideCardPreview();
            StopAllCoroutines();
            ChangeTurn();
        }
    }

    public void SpawnExit(Vector2 position) 
    {
        Debug.Log(position);
        Tile tile = grid.GetTileByPosition(position);
        tile.ChangeTileData(exitDoorData);
    }


    public void AddEnemy(SimpleAI enemy)
    {
        enemies.Add(enemy);
    }

    public void SpawnEnemy(SimpleAI enemyPreab, Vector2 position) 
    {
        Tile tile = grid.GetTileByPosition(position);

        GameObject enemyObject = Instantiate(enemyPreab.gameObject, transform);
        enemyObject.transform.position = new Vector3(position.x, position.y, 0);
        BaseCharacter enemy = enemyObject.GetComponent<BaseCharacter>();
        enemy.SetGrid(grid);

        enemy.Initialize();
        tile.SetOccupyingCharacter(enemy);
        enemy.SetVisibility(!enemy.GetComponent<SimpleAI>().IsInFog());
        AddEnemy(enemy.GetComponent<SimpleAI>());
    }
    
    public bool CanSpawnEnemy(Vector2 position, int minDistanceBetween) 
    {
        foreach(SimpleAI enemy in enemies) 
        {
            if(Mathf.Abs(Vector2.Distance(position, enemy.transform.position)) < minDistanceBetween) 
            {
                return false;
            }
        }
        return true;
    }

    public void AddActiveEnemy(SimpleAI enemy) 
    {
        if(!activeEnemies.Contains(enemy))activeEnemies.Add(enemy);
    }

    public void RemoveActiveEnemy(SimpleAI enemy) {
        if (activeEnemies.Contains(enemy))
        {
            activeEnemies.Remove(enemy);
            if (activeEnemies.Count == 0)
            {
                ExitBattleMode();
            }
        }
    }

    public void DestoryEnemy(SimpleAI ai) 
    {
        enemies.Remove(ai);
        RemoveActiveEnemy(ai);
        grid.GetTileByPosition(ai.transform.position).SetOccupyingCharacter();
        Destroy(ai.gameObject);
    }

    public void EnterBattleMode() 
    {
        if (battleMode) return;
        Debug.Log("Lets the batle begin!");
        battleMode = true;
        playerTurn = false;
    }

    public void ExitBattleMode() 
    {
        battleMode = false;
        endTurnButton.SetActive(false);
        player.DiscardAllCardsFromHand();
        player.ReshuffleDeck();
    }

    public void ChangeTurn()
    {
        if (player.busy || !battleMode) return;
        playerTurn = !playerTurn;
        if (playerTurn) 
        {
            endTurnButton.SetActive(true);
            player.DrawFullHand();
        }
        else{
            endTurnButton.SetActive(false);
            player.Undo();
            player.DiscardAllCardsFromHand();
            enemies.Sort();
            List<SimpleAI> toRemove = new List<SimpleAI>();
            foreach(SimpleAI enemy in activeEnemies) 
            {
                if (enemy.IsInFog()) 
                {
                    toRemove.Add(enemy);
                }
            }
            foreach (SimpleAI enemy in toRemove) RemoveActiveEnemy(enemy);
            StartCoroutine(EnemyTurn());
        }
    }

    public IEnumerator EnemyTurn() 
    {
        activeEnemyIndex++;
        if (activeEnemyIndex >= enemies.Count) 
        {
            activeEnemyIndex = -1;
            ChangeTurn();
        }
        else 
        {
            if(enemies[activeEnemyIndex].PrepareCard()) yield return new WaitForSeconds(1f);
            enemies[activeEnemyIndex].PlayCard();
        }

    }
}
