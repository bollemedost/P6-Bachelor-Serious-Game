using UnityEngine;

public class GateTriggerProxy : MonoBehaviour
{
    public GateChoice gate;
    public GameManagerQuizRunner.Side side;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[GateTriggerProxy] TRIGGER HIT: {name} by {other.name}");

        if (gate == null) return;

        PlayerRunnerT player = other.GetComponentInParent<PlayerRunnerT>();
        if (player == null) return;

        if (!player.CanTriggerChoices())
        {
            Debug.Log("[GateTriggerProxy] Ignored trigger (cooldown after bounce).");
            return;
        }

        gate.TriggerSide(side, player);
    }
}