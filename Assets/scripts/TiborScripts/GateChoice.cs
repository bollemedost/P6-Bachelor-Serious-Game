using UnityEngine;

public class GateChoice : MonoBehaviour
{
    [Header("Debug")]
    public bool debugLogs = true;

    [Header("Option UI over this gate")]
    [Tooltip("Drag the parent GameObject that contains the option texts (left/right) above this gate.")]
    public GameObject optionsRoot;

    private GameManagerQuizRunner gm;
    private int gateIndex;
    private bool solved = false;

    public void Setup(GameManagerQuizRunner manager, int index)
    {
        gm = manager;
        gateIndex = index;
        solved = false;

        if (debugLogs)
            Debug.Log($"[GateChoice] Setup on '{name}' gateIndex={gateIndex}");
    }

    public void SetOptionsVisible(bool visible)
    {
        if (optionsRoot != null)
            optionsRoot.SetActive(visible);
    }

    public void TriggerSide(GameManagerQuizRunner.Side side, PlayerRunnerT player)
    {
        if (debugLogs)
            Debug.Log($"[GateChoice] TriggerSide '{name}' side={side} solved={solved}");

        // If already solved, ignore triggers (don’t bounce anymore)
        if (solved) return;

        if (gm == null)
        {
            Debug.Log("[GateChoice] gm is NULL -> bouncing anyway");
            player.BounceBack("gm NULL");
            return;
        }

        bool correct = gm.CheckAnswer(gateIndex, side);

        if (correct)
        {
            solved = true;
            if (debugLogs) Debug.Log($"[GateChoice] ✅ Solved '{name}'");
        }
        else
        {
            if (debugLogs) Debug.Log($"[GateChoice] ❌ Wrong -> BounceBack()");
            player.BounceBack($"Wrong side. gateIndex={gateIndex}, chosen={side}");
        }
    }
}