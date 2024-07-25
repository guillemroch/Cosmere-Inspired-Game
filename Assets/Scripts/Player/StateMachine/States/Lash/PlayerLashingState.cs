namespace Player.StateMachine.States.Lash{
    //Root State of the Lash States
    public class PlayerLashingState : PlayerBaseState{
        public PlayerLashingState(PlayerStateMachine currentCtx, PlayerStateFactory stateFactory) : base(currentCtx,
            stateFactory, "Lashing") {
            IsRootState = true;
            InitializeSubState();
        }

        public override void EnterState() { }

        public override void UpdateState() {
            //TODO: add stamina
            HandleStamina();
        }

        public override void FixedUpdateState() { }

        public override void ExitState() { }

        public override void CheckSwitchStates() { }

        public override void InitializeSubState() {
            if (Ctx.InputManager.LashInput ) {
                SwitchStates(Factory.Halflash());
            }

            if (Ctx.InputManager.SmallLashInput > 0) {
                SwitchStates(Factory.Lash());
            }

            if (Ctx.Stormlight <= 0) {
                SwitchStates(Factory.Grounded());
            }
        }

        private void HandleStamina() {
            Ctx.Stormlight -= Ctx.StormlightDepletionRate;
            if (Ctx.Stormlight < 0) Ctx.Stormlight = 0;
            
            Ctx.UIManager.StormlightBar.Set(Ctx.Stormlight);
        }
    }
}
