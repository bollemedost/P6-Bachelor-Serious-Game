using UnityEngine;

public class IdleState : MovementBaseState
{
    public override void EnterState(MovementStateManager movement)
    {
        movement.anim.SetBool("Walking", false);
    }

    public override void UpdateState(MovementStateManager movement)
    {
        if (movement.dir.magnitude > 0.1f)
        {
            movement.SwitchState(movement.walk);
        }
    }
}