using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player.StateMachine.States.Attack{

    public class PlayerNormalBlockState : PlayerBaseState{
        public PlayerNormalBlockState(PlayerStateMachine currentCtx, PlayerStateFactory stateFactory) : 
            base(currentCtx, stateFactory, "Normal Block") { }
        public override void EnterState() {
        }

        public override void UpdateState() {
        }

        public override void FixedUpdateState() {
        }

        public override void ExitState() {
        }

        public override void CheckSwitchStates() {
        }

        public override void InitializeSubState() {
        }
    }
}