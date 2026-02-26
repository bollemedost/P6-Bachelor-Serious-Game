using UnityEngine;

public class WalkState : MovementBaseState
{
    public override void EnterState(MovementStateManager movement)
    {
        movement.anim.SetBool("Walking", true);
    }

    public override void UpdateState(MovementStateManager movement)
    {
        if (movement.dir.magnitude <= 0.1f)
        {
            movement.SwitchState(movement.idle);
        }
    }
}