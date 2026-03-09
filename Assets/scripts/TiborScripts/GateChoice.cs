using UnityEngine;

public class GateChoice : MonoBehaviour
{
    public bool debugLogs = true;

    [Header("Text/options above this gate")]
    public GameObject optionsRoot;

    private GameManagerQuizRunner gm;
    private int gateIndex;
    private bool solved = false;

    public void Setup(GameManagerQuizRunner manager, int index)
    {
        gm = manager;
        gateIndex = index;
        solved = false;
    }

    public void ResetSolvedState()
    {
        solved = false;
    }

    public void SetOptionsVisible(bool visible)
    {
        if (optionsRoot != null)
            optionsRoot.SetActive(visible);
    }

    public void TriggerSide(GameManagerQuizRunner.Side side, PlayerRunnerT player)
    {
        if (solved) return;
        if (gm == null) return;

        bool correct = gm.CheckAnswer(gateIndex, side);

        if (correct)
        {
            solved = true;
        }
        else
        {
            gm.ResetRun(player);
        }
    }
}