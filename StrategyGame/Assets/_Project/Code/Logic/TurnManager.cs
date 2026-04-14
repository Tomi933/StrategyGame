using Assets._Project.Code.UI;
using System.Collections;
using UnityEngine;

public enum TurnState { PlayerTurn, BotTurn }

public class TurnManager : MonoBehaviour
{
    private BotAI _botAI;

    private GameUIManager _gameUIManager;
    public TurnState CurrentTurn { get; private set; } = TurnState.PlayerTurn;

    public bool IsPlayerTurn => CurrentTurn == TurnState.PlayerTurn;

    public void Init(BotAI botAI, GameUIManager gameUIManager)
    {
        _botAI = botAI;
        _gameUIManager = gameUIManager;
    }

    public void EndPlayerTurn()
    {
        if (CurrentTurn != TurnState.PlayerTurn) return;
        CurrentTurn = TurnState.BotTurn;
        StartCoroutine(BotTurnRoutine());
    }

    private IEnumerator BotTurnRoutine()
    {
        _gameUIManager.SetPlayerTurn(false);
        yield return new WaitForSeconds(0.5f); 
        _botAI.ExecuteTurn();
        _gameUIManager.RefreshEnemyVisibility();
        yield return new WaitForSeconds(0.5f);
        CurrentTurn = TurnState.PlayerTurn;
        _gameUIManager.SetPlayerTurn(true);
    }
}