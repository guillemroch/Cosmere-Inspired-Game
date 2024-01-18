using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdleState : PlayerBaseState
{
    public PlayerIdleState(PlayerStateMachine currentCtx, PlayerStateFactory stateFactory) : base(currentCtx, stateFactory) { }
    public override void EnterState() {
        //Setup starting things for animation or logic

    }

    public override void UpdateState() {
        CheckSwitchStates();
    }

    public override void FixedUpdateState() {    }

    public override void ExitState() {    }

    public override void CheckSwitchStates() {
        if (Ctx.InputManager.movementInput != Vector2.zero && Ctx.InputManager.IsSprintPressed) {
            SwitchStates(Factory.Run());
        }
        else if (Ctx.InputManager.movementInput != Vector2.zero && Ctx.InputManager.IsSprintPressed) {
            SwitchStates(Factory.Walk());
        }
    }

    public override void InitializeSubState() {    }
}
