using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{



    [SerializeField]GameObject playerPrefab;
    [SerializeField]Grid grid;
    [SerializeField]Transform cardContainer;
    public PlayerController player;

    public Image tileInfoImage;
    public Text tileInfoTitle;
    public Text tileInfoDescription;

    public bool isCardSelected = false;

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
        Vector2 playerSpawn = grid.LoadMap(MapGenerator.generateMap(grid, 50));

        grid.GetTileByPosition(playerSpawn).SetOccupyingCharacter(player);
        player.transform.position = playerSpawn;
        player.SetGrid(grid);
        FindObjectOfType<CameraController>().SetPlayer(player);

    }

   
}
