using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleAI : BaseCharacter, IComparable<SimpleAI>
{

    PlayerController player;
    [SerializeField] SpriteRenderer enemy;


    private void Start()
    {
        player = GameController.instance.player;
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
            EndAsyncAction();
        }

    }

    public bool PrepareCard()
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
        preparedCard = CardSimulation(hand);
        return preparedCard != null;
    }

    public void PlayCard()
    {
        if (preparedCard == null)
        {
            EndAsyncAction();
            return;
        }
        preparedCard.PrepareCard(this);
        preparedCard.CardPlayed(this);

        DiscardAllCardsFromHand();
        if (!busy)
        {
            EndAsyncAction();
        }

    }

   /* public void MakeAction()
    {
        List<Node> path = AStar.findPath(grid, transform.position, player.transform.position);
        if (path == null)
        {
            EndAsyncAction();
            return;
        }
        int distanceFromPlayer = path.Count;
        if (distanceFromPlayer > triggerRange)
        {
            IdleAction();
            return;
        }

        DrawFullHand();
        Card bestCard = CardSimulation(hand);

        if (bestCard != null)
        {
            bestCard.PrepareCard(this);
            bestCard.CardPlayed(this);
        }
        DiscardAllCardsFromHand();
        if (!busy)
        {
            StartCoroutine(GameController.gameController.EnemyTurn());
        }
    }*/

    


    #region Actions

    public override void EndAsyncAction()
    {
        base.EndAsyncAction();
        StopAllCoroutines();
        StartCoroutine(GameController.instance.EnemyTurn());
    }
    public override bool PrepareMove(int range, Card card)
    {
        List<Node> path = AStar.findPath(grid, transform.position, player.transform.position);
        if (path == null) return false;
        if (path.Count <= 1) return false;
        path.RemoveAt(path.Count - 1);
        if (path.Count <= range)
        {
            destination = path[path.Count - 1].nodePosition;
        }
        else
        {
            destination = path[range - 1].nodePosition;
        }
        return true;
    }

    public override bool PrepareAttack(int range, Card card, bool fromDestination = false)
    {
        destination = player.transform.position;
        return true;
    }

    public void IdleAction()
    {
        // pasywna akcja przeciwnika
        List<Node> path = AStar.findPath(grid, transform.position, player.transform.position, stats.getActualStat(Stats.viewDistance));
        if(path != null) 
        {
            if(path.Count <= stats.getActualStat(Stats.viewDistance)) 
            {
                path.RemoveAt(path.Count - 1);
                if (path.Count == 0) return;
                int destinationIndex = stats.getActualStat(Stats.speed);
                if (destinationIndex > path.Count - 1) destinationIndex = path.Count - 1;
                Vector2 destination = path[destinationIndex].nodePosition;
                for(int i = 0; i <= destinationIndex; i++) 
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
            if (!GameController.instance.battleMode) 
            {
                GameController.instance.EnterBattleMode();
            }
            GameController.instance.AddActiveEnemy(this); 
        }
    }

    public Card CardSimulation(List<Card> cardHand)
    {
        List<Card> moveCards = new List<Card>();
        List<Card> attackCards = new List<Card>();

        foreach (Card card in hand)
        {
            if (card.cardData.attack >= 0)
            {
                attackCards.Add(card);
            }
            else if (card.cardData.move >= 0)
            {
                moveCards.Add(card);
            }

        }

        bool canHit = CanHit(player.transform.position);
        if (canHit)
        {
            int distance = Mathf.FloorToInt(Vector2.Distance(transform.position, player.transform.position));
            List<Card> attacksToRemove = new List<Card>();
            foreach (Card card in attackCards)
            {
                int move = 0;
                if (card.cardData.move > 0) move = stats.getActualStat(Stats.speed); // ruch przed atakiem
                if (distance > card.cardData.range + move)
                {
                    attacksToRemove.Add(card);
                }
            }

            foreach (Card attack in attacksToRemove)
            {
                attackCards.Remove(attack);
            }
        }

        Card bestCard = null;
        if (attackCards.Count > 0 && canHit)
        {
            bestCard = attackCards[0];
            foreach (Card card in attackCards)
            {
                if (bestCard.cardData.attack < card.cardData.attack)
                {
                    bestCard = card;
                }
                else if(bestCard.cardData.attack == card.cardData.attack) 
                {
                    if(bestCard.cardData.move > card.cardData.move) 
                    {
                        bestCard = card;
                    }
                }
            }
        }
        else if (moveCards.Count > 0)
        {
            bestCard = moveCards[0];
            foreach (Card card in moveCards)
            {
                if (bestCard.cardData.move < card.cardData.move)
                {
                    bestCard = card;
                }
            }

        }
        return bestCard;
    }


    #endregion

    public override void Kill()
    {
        if (busy) EndAsyncAction();
        GameController.instance.DestoryEnemy(this);
    }

    public int CompareTo(SimpleAI other)
    {
        if(stats.getActualStat(Stats.speed) != other.stats.getActualStat(Stats.speed)) 
        {
            return other.stats.getActualStat(Stats.speed) - stats.getActualStat(Stats.speed);
        }
        else 
        {
            return Math.Abs(Vector2.Distance(other.transform.position, GameController.instance.transform.position)).CompareTo(Mathf.Abs(Vector2.Distance(transform.position, GameController.instance.transform.position)));
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
}
