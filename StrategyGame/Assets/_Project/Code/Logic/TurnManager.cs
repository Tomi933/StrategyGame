using System.Collections;
using UnityEngine;

public enum TurnState { PlayerTurn, BotTurn }

public class TurnManager : MonoBehaviour
{
    private BotAI _botAI;

    public TurnState CurrentTurn { get; private set; } = TurnState.PlayerTurn;

    public bool IsPlayerTurn => CurrentTurn == TurnState.PlayerTurn;

    public void Init(BotAI botAI)
    {
        _botAI = botAI;
    }

    public void EndPlayerTurn()
    {
        if (CurrentTurn != TurnState.PlayerTurn) return;

        CurrentTurn = TurnState.BotTurn;
        StartCoroutine(BotTurnRoutine());
    }

    private IEnumerator BotTurnRoutine()
    {
        yield return new WaitForSeconds(0.5f); // пауза для читабельності
        _botAI.ExecuteTurn();
        yield return new WaitForSeconds(0.5f);

        CurrentTurn = TurnState.PlayerTurn;
    }
}