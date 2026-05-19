using UnityEngine;

namespace BrainEaters.Input
{
    public class CortexV5MentalCommandSource : MonoBehaviour, ICommandSignalSource
    {
        [SerializeField] private bool warnWhenSessionMissing = true;

        public bool HasSignal => CortexV5SessionBootstrap.Instance != null && CortexV5SessionBootstrap.Instance.HasSignal;
        public MentalCommandSignal LatestSignal => CortexV5SessionBootstrap.Instance != null
            ? CortexV5SessionBootstrap.Instance.LatestSignal
            : default;

        public string StatusText => CortexV5SessionBootstrap.Instance != null
            ? CortexV5SessionBootstrap.Instance.StatusText
            : "Cortex v5 session bootstrap missing";

        private void Start()
        {
            if (warnWhenSessionMissing && CortexV5SessionBootstrap.Instance == null)
            {
                Debug.LogWarning(
                    "Cortex v5 input source is installed, but no CortexV5SessionBootstrap exists. " +
                    "Install it in the initial flow scene before loading gameplay.",
                    this);
            }
        }
    }
}
