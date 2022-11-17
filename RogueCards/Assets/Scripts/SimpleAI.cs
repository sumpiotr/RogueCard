using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleAI : BaseCharacter, IComparable<SimpleAI>
{

    PlayerController player;
    [SerializeField] SpriteRenderer enemy;
    bool simulation = false;
    CharacterSimulation playerSimulation = null;


    private void Start()
    {
        player = GameController.Instance.player;
    }


    public override IEnumerator MoveCoroutine(List<Node> nodes, Action callback = null)
    {   
        foreach (Node node in nodes)
        {
            transform.position = node.nodePosition;
            SetVisibility(!IsInFog());
            yield return new WaitForSeconds(0.5f);
        }
        grid.GetTileByPosition(transform.position).SetOccupyingCharacter(this);
        isMoving = false;
        if (callback != null) callback();
        else
        {
            EndAction();
        }

    }

    #region Cards

    //returns true if card was prepared and false if made idle action
    public bool MakeAction()
    {
        preparedCard = null;
        List<Node> path = AStar.findPath(grid, transform.position, player.transform.position, stats.getActualStat(Stats.viewDistance));

        SetVisibility(!IsInFog());
        if (grid.GetTileByPosition(transform.position).isFogEnabled)
        {
            IdleAction();
            return false;
        }

        if (path == null)
        {
            return false;
        }

        DrawFullHand();
        preparedCard = PrepareCard(hand);
        return preparedCard != null;
    }

    public void PlayCard()
    {
        if (preparedCard == null)
        {
            EndAction();
            return;
        }
        preparedCard.PrepareCard(this);
        preparedCard.CardPlayed(this);

        DiscardAllCardsFromHand();
        if (!busy)
        {
            EndAction();
        }
    }

    public Card PrepareCard(List<Card> cardHand)
    {
        List<Card> moveCards = new List<Card>();
        List<Card> attackCards = new List<Card>();

        foreach (Card card in hand)
        {
            if (!card.PrepareCard(this)) continue;
            if (card.cardData.GetCardType() == CardDataType.Attack)
            {
                attackCards.Add(card);
            }
            else
            {
                moveCards.Add(card);
            }

        }


        Card bestCard = null;
        if (attackCards.Count > 0)
        {
            bestCard = attackCards[0];
            List<Card> toSimulate = new List<Card>();
            toSimulate.Add(bestCard);
            (CharacterSimulation player, CharacterSimulation instance) simulation = SimulateCards(toSimulate);
            int dealtDamage = player.stats.getActualStat(Stats.health) - simulation.player.stats.getActualStat(Stats.health);
            foreach (Card card in attackCards)
            {
                toSimulate.Clear();
                toSimulate.Add(card);
                simulation = SimulateCards(toSimulate);
                int newDamage = player.stats.getActualStat(Stats.health) - simulation.player.stats.getActualStat(Stats.health);
                if (newDamage > dealtDamage) 
                {
                    dealtDamage = newDamage;
                    bestCard = card;
                }
            }
        }
        else if (moveCards.Count > 0)
        {
            bestCard = moveCards[0];
            List<Card> toSimulate = new List<Card>();
            toSimulate.Add(bestCard);
            (CharacterSimulation player, CharacterSimulation instance) simulation = SimulateCards(toSimulate);
            float move = Mathf.Abs(Vector2.Distance(player.transform.position, simulation.player.position));
            foreach (Card card in moveCards)
            {
                toSimulate.Clear();
                toSimulate.Add(card);
                simulation = SimulateCards(toSimulate);
                float newMove = Mathf.Abs(Vector2.Distance(player.transform.position, simulation.player.position));
                if (newMove > move)
                {
                    move = newMove;
                    bestCard = card;
                }
            }

        }
        return bestCard;
    }

    private (CharacterSimulation player, CharacterSimulation instance) SimulateCards(List<Card> cards) 
    {
        CharacterSimulation instanceSimulation = new CharacterSimulation(this);
        this.playerSimulation = new CharacterSimulation(player);
        simulation = true;

        foreach(Card card in cards) 
        {
            if (card.PrepareCard(this))
            {
                card.CardPlayed(instanceSimulation);
            }
        }

        CharacterSimulation playerSimulation = this.playerSimulation;
        simulation = false;
        this.playerSimulation = null;
        return (playerSimulation, instanceSimulation);
    }

    #endregion


    #region Actions

    public override void EndAction()
    {
        base.EndAction();
        StopAllCoroutines();
        StartCoroutine(GameController.Instance.EnemyTurn());
    }
    public override bool PrepareMove(int range, Card card)
    {
        List<Node> path = AStar.findPath(grid, transform.position, player.transform.position);
        if (path == null) return false;
        if (path.Count <= 1) return false;
        path.RemoveAt(path.Count - 1);
        if (path.Count <= range)
        {
            card.destination = path[path.Count - 1].nodePosition;
        }
        else
        {
            card.destination = path[range - 1].nodePosition;
        }
        return true;
    }

    public override bool PrepareAttack(int maxRange, int minRange, Card card, Vector2 startPosition)
    {
        if (!CanHit(player.transform.position, startPosition)) return false;
        ICharacter target = simulation ? playerSimulation : (ICharacter)player;
        int distance = Mathf.FloorToInt(Vector2.Distance(startPosition,   target.position));
        if (distance > maxRange || distance < minRange) return false;
        card.destination = player.transform.position;
        card.target = target;
        return true;
    }

    public override bool PrepareCharge(int maxRange, int minRange, Card card)
    {
        if (!CanCharge(player.transform.position)) return false;
        int distance = Mathf.FloorToInt(Vector2.Distance(transform.position, player.transform.position));
        if (distance > maxRange || distance < minRange) return false;
        card.target = simulation ? playerSimulation : (ICharacter)player;
        return PrepareMove(maxRange, card);
    }

    public void IdleAction()
    {
        // pasywna akcja przeciwnika
        List<Node> path = AStar.findPath(grid, transform.position, player.transform.position, stats.getActualStat(Stats.viewDistance));
        if (path != null)
        {
            if (path.Count <= stats.getActualStat(Stats.viewDistance))
            {
                path.RemoveAt(path.Count - 1);
                if (path.Count == 0) return;
                int destinationIndex = stats.getActualStat(Stats.speed);
                if (destinationIndex > path.Count - 1) destinationIndex = path.Count - 1;
                Vector2 destination = path[destinationIndex].nodePosition;
                for (int i = 0; i <= destinationIndex; i++)
                {
                    if (!grid.GetTileByPosition(path[i].nodePosition).isFogEnabled)
                    {
                        destination = path[i].nodePosition;
                        break;
                    }
                }

                grid.GetTileByPosition(transform.position).SetOccupyingCharacter();
                transform.position = destination;
            }
        }
        else
        {
            List<Vector2> tiles = AStar.GetWalkableAndEmptyTilesInRange(transform.position, 2, grid);
            Vector2 randomTile = tiles[(int)UnityEngine.Random.Range(0, tiles.Count - 1)];
            grid.GetTileByPosition(transform.position).SetOccupyingCharacter();
            transform.position = new Vector3(randomTile.x, randomTile.y, transform.position.z);
        }
        grid.GetTileByPosition(transform.position).SetOccupyingCharacter(this);
        SetVisibility(!IsInFog());
        if (!IsInFog())
        {
            if (!GameController.Instance.battleMode)
            {
                GameController.Instance.EnterBattleMode();
            }
            GameController.Instance.AddActiveEnemy(this);
        }
    }

    #endregion

    public override void Kill()
    {
        if (busy) EndAction();
        GameController.Instance.DestoryEnemy(this);
    }

    public int CompareTo(SimpleAI other)
    {
        if (stats.getActualStat(Stats.speed) != other.stats.getActualStat(Stats.speed))
        {
            return other.stats.getActualStat(Stats.speed) - stats.getActualStat(Stats.speed);
        }
        else
        {
            return Mathf.Abs(Vector2.Distance(transform.position, player.transform.position)).CompareTo(Math.Abs(Vector2.Distance(other.transform.position, player.transform.position)));
        }
    }

    public bool IsInFog()
    {
        return grid.GetTileByPosition(transform.position).isFogEnabled;
    }

    public override void SetVisibility(bool action)
    {
        enemy.gameObject.SetActive(action);
    }

    public override bool ShouldDrop()
    {
        if (UnityEngine.Random.Range(0, 100) <= stats.getActualStat(Stats.dropChance)) return true;
        return false;
    }

    public override (GameObject instance, int value) Spawn(Tile tile)
    {
        (GameObject instance, int value) instanceData = base.Spawn(tile);
        SimpleAI instance = instanceData.instance.GetComponent<SimpleAI>();
        GameController.Instance.AddEnemy(instance);
        instance.SetVisibility(!instance.IsInFog());
        instance.Initialize();
        return instanceData;
    }
}
