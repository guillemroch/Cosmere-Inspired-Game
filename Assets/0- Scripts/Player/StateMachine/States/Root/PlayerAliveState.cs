using UnityEngine;

namespace Player.StateMachine.States.Alive{
    public class PlayerAliveState : PlayerBaseState {
    
        public PlayerAliveState(PlayerStateMachine currentCtx, PlayerStateFactory stateFactory)
            : base(currentCtx, stateFactory, "Alive") {
            IsRootState = true;
            InitializeSubState();
        }
        public override void EnterState() {
        }

        public override void UpdateState() {
            HandleInteraction();
            HandleStormlight();
            CheckSwitchStates();
        }

        public override void FixedUpdateState() { }

        public override void ExitState() {
        }

        public override void CheckSwitchStates() {
            //TODO: If health == 0 => die
            /*if (Ctx.InputManager.StormlightInput ) {
                //SwitchStates(Factory.Stormlight());
            }*/
            if (Ctx.InputManager.StormlightInput) {
                Ctx.InputManager.ResetStormlightInput();
                Ctx.IsUsingStormlight = !Ctx.IsUsingStormlight;
                if (!Ctx.IsUsingStormlight) {
                    //EXIT STORMLIGHT STATE
                    Ctx.ParticleSystem.Stop();
                }
                else {
                    //ENTER STORMLIGHT STATE
                    Ctx.AnimatorManager.PlayTargetAnimation("Buff");
                    Ctx.AnimatorManager.animator.SetLayerWeight(4,1);
                    Ctx.ParticleSystem.Play();
                }
            }

           
        }

        public override void InitializeSubState() {
            SetSubStates(Factory.Grounded());
        }

        private void HandleInteraction() {
            Collider[] colliders = Physics.OverlapSphere(Ctx.PlayerTransform.position, Ctx.MaxInteractionDistance, Ctx.InteractionLayer);

            foreach (var collider in colliders) {
                if (collider.gameObject.TryGetComponent(out Pickable interactableObject)) {
                    interactableObject.Interact(out int stormlight);
                    Ctx.Stormlight += stormlight;
                }
            }
        }
        
        private void HandleStormlight() {
            if (!Ctx.IsUsingStormlight)
                return;
            
            //UPDATE STORMLIGHT STATE
            //1- Calculate stormlight drain
            Ctx.StormlightDepletionRate = Ctx.StormlightBaseDrain +
                                          Ctx.StormlightHealingDrain +
                                          Ctx.StormlightInfusingDrain +
                                          Ctx.StormlightLashingDrain +
                                          Ctx.StormlightMovementDrain;
           
            Ctx.Stormlight -= Ctx.StormlightDepletionRate;
            if (Ctx.Stormlight < 0) Ctx.Stormlight = 0;
                     
            Ctx.UIManager.StormlightBar.Set(Ctx.Stormlight);
        }

       
    }
}