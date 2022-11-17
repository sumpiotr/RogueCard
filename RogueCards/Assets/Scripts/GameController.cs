using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{

    //prefabs
    [SerializeField]private GameObject playerPrefab;
    [SerializeField]private GameObject exitDoor;
    
    //cards canvas
    [SerializeField]private Transform cardContainer;


    public PlayerController player;
    public Grid grid;
      
    public List<LayerMask> obstaclesMasks = new List<LayerMask>();
    public int obstalcesLayers = 0;
   
   
    //battle
    [HideInInspector] public bool playerTurn;
   
    private List<SimpleAI> _enemies = new List<SimpleAI>();


    private List<SimpleAI> _activeEnemies = new List<SimpleAI>();//Enemies that take part in fight

    public Image tileInfoImage;
    public Text tileInfoTitle;
    public Text tileInfoDescription;
    [SerializeField] private GameObject endTurnButton;

    public bool isCardSelected = false;
    public bool battleMode = false;

    public static GameController Instance = null;

    private int _activeEnemyIndex = -1;

    public void Awake()
    {
        if (Instance != null) return;
        Instance = this;
    }

    public void Start()
    {
   
        foreach (LayerMask mask in obstaclesMasks)
        {
            obstalcesLayers = obstalcesLayers | mask.value;
        }

        grid.Initialize();
        Vector2 playerSpawn = grid.LoadMap(PrimMapGenerator.GenerateMap((int)grid.GetSize().x, (int)grid.GetSize().y));
        player = playerPrefab.GetComponent<BaseCharacter>().Spawn(grid.GetTileByPosition(playerSpawn)).instance.GetComponent<PlayerController>();
        player.InitializeCards(cardContainer);
        player.UpdateFog(false);
        //player.SetGrid(grid);
        FindObjectOfType<CameraController>().SetPlayer(player);
        //grid.SpawnEnemies(12, 5, 50);
        List<ISpawnable> enemiesToSpawn = new List<ISpawnable>();
        foreach(ISpawnable enemy in grid.GetGridData().enemies) 
        {
            enemiesToSpawn.Add(enemy);
        }
        grid.Spawn(12, 5, 50, enemiesToSpawn);
        grid.CreateExit();
        endTurnButton.SetActive(false);
        playerTurn = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && playerTurn && battleMode) 
        {
            CardPreview.Instance.HideCardPreview();
            StopAllCoroutines();
            ChangeTurn();
        }
    }

    public void SpawnExit(Vector2 position) 
    {
        Debug.Log(position); 
        Tile tile = grid.GetTileByPosition(position);
        GameObject door =  Instantiate(exitDoor, tile.transform);
        door.transform.localPosition = new Vector3(0, 0, -1);
        tile.SetOnCharacterEnter((BaseCharacter character) => {
            if(character is PlayerController)LevelManager.instance.NextLevel();
        });
    }


    public void AddEnemy(SimpleAI enemy)
    {
        _enemies.Add(enemy);
    }

    public void AddActiveEnemy(SimpleAI enemy) 
    {
        if(!_activeEnemies.Contains(enemy))_activeEnemies.Add(enemy);
    }

    public void RemoveActiveEnemy(SimpleAI enemy) {
        if (_activeEnemies.Contains(enemy))
        {
            _activeEnemies.Remove(enemy);
            if (_activeEnemies.Count == 0)
            {
                ExitBattleMode();
            }
        }
    }

    public void DestoryEnemy(SimpleAI ai) 
    {
        _enemies.Remove(ai);
        RemoveActiveEnemy(ai);
        grid.GetTileByPosition(ai.transform.position).SetOccupyingCharacter();
        Destroy(ai.gameObject);
    }

    public void EnterBattleMode() 
    {
        if (battleMode) return;
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
            _enemies.Sort();
            List<SimpleAI> toRemove = new List<SimpleAI>();
            foreach(SimpleAI enemy in _activeEnemies) 
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
        _activeEnemyIndex++;
        if (_activeEnemyIndex >= _enemies.Count) 
        {
            _activeEnemyIndex = -1;
            ChangeTurn();
        }
        else 
        {
            if(_enemies[_activeEnemyIndex].MakeAction()) yield return new WaitForSeconds(1f);
            _enemies[_activeEnemyIndex].PlayCard();
        }

    }
}
