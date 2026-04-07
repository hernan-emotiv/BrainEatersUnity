using BrainEaters.Input;
using UnityEngine;

namespace BrainEaters.Player
{
    [RequireComponent(typeof(PlayerInputRouter))]
    [RequireComponent(typeof(PlayerMovement))]
    [RequireComponent(typeof(PlayerEnergyCharge))]
    [RequireComponent(typeof(PlayerBombAttack))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private PlayerInputRouter inputRouter;
        [SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private PlayerEnergyCharge playerEnergyCharge;
        [SerializeField] private PlayerBombAttack playerBombAttack;

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnValidate()
        {
            ResolveReferences();
        }

        public void SetCamera(Transform cameraTransform)
        {
            playerMovement.SetCamera(cameraTransform);
        }

        private void Update()
        {
            if (inputRouter == null)
            {
                return;
            }

            playerMovement.Tick(inputRouter.Move, Time.deltaTime);
            playerEnergyCharge.Tick(inputRouter.IsChargeHeld, Time.deltaTime);

            if (inputRouter.WasBombPressedThisFrame)
            {
                playerBombAttack.TryTrigger(playerEnergyCharge);
            }
        }

        private void ResolveReferences()
        {
            if (inputRouter == null)
            {
                inputRouter = GetComponent<PlayerInputRouter>();
            }

            if (playerMovement == null)
            {
                playerMovement = GetComponent<PlayerMovement>();
            }

            if (playerEnergyCharge == null)
            {
                playerEnergyCharge = GetComponent<PlayerEnergyCharge>();
            }

            if (playerBombAttack == null)
            {
                playerBombAttack = GetComponent<PlayerBombAttack>();
            }
        }
    }
}
