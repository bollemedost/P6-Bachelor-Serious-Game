using UnityEngine;

public class WalkState : MovementBaseState
{
    public override void EnterState(MovementStateManager movement)
    {
        movement.anim.SetBool("Walking", true);

        // Start footsteps
        if (movement.footstepSource != null && !movement.footstepSource.isPlaying)
        {
            movement.footstepSource.Play();
        }
    }

    public override void UpdateState(MovementStateManager movement)
    {
        if (movement.dir.magnitude <= 0.1f)
        {
            movement.SwitchState(movement.idle);
        }
    }

    public override void ExitState(MovementStateManager movement)
    {
        // Stop footsteps when leaving walk state
        if (movement.footstepSource != null && movement.footstepSource.isPlaying)
        {
            movement.footstepSource.Stop();
        }
    }
}