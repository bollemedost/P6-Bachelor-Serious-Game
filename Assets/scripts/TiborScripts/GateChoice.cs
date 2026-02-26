using UnityEngine;

public class GateChoice : MonoBehaviour
{
    [Header("Debug")]
    public bool debugLogs = true;

    private GameManagerQuizRunner gm;
    private int gateIndex;
    private bool solved = false;

    public void Setup(GameManagerQuizRunner manager, int index)
    {
        gm = manager;
        gateIndex = index;
        solved = false;

        if (debugLogs)
            Debug.Log($"[GateChoice] Setup on '{name}' gateIndex={gateIndex}, gm={(gm != null)}");
    }

    public void TriggerSide(GameManagerQuizRunner.Side side, PlayerRunnerT player)
    {
        if (debugLogs)
            Debug.Log($"[GateChoice] TriggerSide '{name}' side={side}, solved={solved}");

        if (solved) return;

        if (gm == null)
        {
            Debug.Log("[GateChoice] STOP: gm is NULL (GameManager didn't Setup gates array).");
            return;
        }

        bool correct = gm.CheckAnswer(gateIndex, side);

        if (!correct)
        {
            player.BounceBack($"Wrong side. gateIndex={gateIndex}, chosen={side}");
        }
        else
        {
            solved = true;
            if (debugLogs) Debug.Log($"[GateChoice] ✅ Solved '{name}'");
        }
    }
}