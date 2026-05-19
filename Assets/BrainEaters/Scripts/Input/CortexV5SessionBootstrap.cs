using System;
using System.Threading.Tasks;
using UnityEngine;

#if EMOTIV_CORTEX_V5
using System.Collections.Generic;
using Emotiv.Cortex.API;
using Emotiv.Cortex.API.Models;
using Emotiv.Cortex.Settings;
#endif

namespace BrainEaters.Input
{
    public class CortexV5SessionBootstrap : MonoBehaviour, ICommandSignalSource
    {
        public static CortexV5SessionBootstrap Instance { get; private set; }

        [SerializeField] private bool initializeOnStart = true;
        [SerializeField] private bool persistAcrossScenes = true;
        [SerializeField] private string headsetIdOverride = string.Empty;
        [SerializeField, Min(1f)] private float headsetScanTimeoutSeconds = 22f;
        [SerializeField] private bool loadProfileOnConnect = true;
        [SerializeField] private bool configureMentalCommandsOnConnect = true;
        [SerializeField] private string chargeCommandId = "pull";
        [SerializeField, Range(1, 10)] private int chargeSensitivity = 5;
        [SerializeField] private string bombCommandId = "push";
        [SerializeField, Range(1, 10)] private int bombSensitivity = 7;
        [SerializeField] private bool ignoreNeutral = true;

        private bool isInitializing;
        private bool isConnected;
        private string connectedHeadsetId = string.Empty;
        private string previousCommandId = string.Empty;

#if EMOTIV_CORTEX_V5
        private ICortexRuntimeManager runtime;
#endif

        public bool HasSignal { get; private set; }
        public MentalCommandSignal LatestSignal { get; private set; }
        public string StatusText { get; private set; } = "Cortex v5 idle";
        public bool IsConnected => isConnected;
        public bool IsInitializing => isInitializing;
        public string ConnectedHeadsetId => connectedHeadsetId;

        private async void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            if (persistAcrossScenes)
            {
                DontDestroyOnLoad(gameObject);
            }

            if (initializeOnStart)
            {
                await InitializeAsync();
            }
        }

        private void Update()
        {
            HasSignal = false;

#if EMOTIV_CORTEX_V5
            if (!isConnected || runtime == null || string.IsNullOrWhiteSpace(connectedHeadsetId))
            {
                return;
            }

            if (!runtime.Headset.TakeLatestSample(connectedHeadsetId, DataSampleType.MentalCommand, out DataSample sample)
                || sample is not MentalCommandDataSample mentalCommand)
            {
                return;
            }

            EmitSignal(mentalCommand);
#else
            StatusText = "Cortex v5 package missing. Install com.emotiv.cortex from tarball.";
#endif
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

#if EMOTIV_CORTEX_V5
            runtime?.Dispose();
            runtime = null;
#endif
        }

        public async Task InitializeAsync()
        {
            if (isInitializing || isConnected)
            {
                return;
            }

            isInitializing = true;

#if EMOTIV_CORTEX_V5
            try
            {
                StatusText = "Requesting Android Bluetooth permissions";
                bool permissionsGranted = await CortexAndroidPermissionRequester.RequestRequiredPermissionsAsync();
                if (!permissionsGranted)
                {
                    StatusText = "Required Android Bluetooth permissions were denied";
                    Debug.LogError(StatusText, this);
                    return;
                }

                StatusText = "Loading Cortex v5 settings";
                CortexSettings settings = CortexSettings.Load();
                if (settings == null)
                {
                    StatusText = "Cortex settings not found. Configure Tools > Emotiv Cortex SDK.";
                    Debug.LogError(StatusText, this);
                    return;
                }

                runtime = new CortexRuntimeManager(settings.ToRuntimeConfig());

                if (!await AuthorizeAsync())
                {
                    return;
                }

                if (!await ConnectHeadsetAsync())
                {
                    return;
                }

                if (loadProfileOnConnect && !await LoadProfileAsync())
                {
                    return;
                }

                if (configureMentalCommandsOnConnect)
                {
                    await ConfigureMentalCommandsAsync();
                }

                isConnected = true;
                StatusText = $"Cortex v5 ready: {connectedHeadsetId}";
                Debug.Log(StatusText, this);
            }
            catch (Exception ex)
            {
                StatusText = $"Cortex v5 initialization failed: {ex.Message}";
                Debug.LogError(StatusText, this);
            }
            finally
            {
                isInitializing = false;
            }
#else
            StatusText = "Cortex v5 package missing. Install com.emotiv.cortex from tarball.";
            Debug.LogError(StatusText, this);
            await Task.CompletedTask;
            isInitializing = false;
#endif
        }

#if EMOTIV_CORTEX_V5
        private async Task<bool> AuthorizeAsync()
        {
            StatusText = "Checking Cortex API info";
            CortexResponse<AppAccessInfo> infoResult = await runtime.Auth.GetApiInfoAsync();
            if (!infoResult.IsSuccess)
            {
                StatusText = $"Cortex api.info failed: {FormatError(infoResult.Error)}";
                Debug.LogError(StatusText, this);
                return false;
            }

            AppAccessInfo info = infoResult.Data;
            if (info == null)
            {
                StatusText = "Cortex api.info returned no data";
                Debug.LogError(StatusText, this);
                return false;
            }

            bool hasUser = info.User != null && !string.IsNullOrWhiteSpace(info.User.EmotivId);
            if (!hasUser)
            {
                StatusText = "No Emotiv user logged in. On mobile, login flow is required.";
#if UNITY_ANDROID || UNITY_IOS
                CortexResponse<AppAccessInfo> loginResult = await runtime.Auth.LoginAsync();
                if (!loginResult.IsSuccess)
                {
                    StatusText = $"Cortex login failed: {FormatError(loginResult.Error)}";
                    Debug.LogError(StatusText, this);
                    return false;
                }

                return true;
#else
                Debug.LogError(StatusText, this);
                return false;
#endif
            }

            if (info.InitDone)
            {
                return true;
            }

            StatusText = "Authorizing Cortex app";
            CortexResponse<AppAccessInfo> initResult = await runtime.Auth.InitAsync();
            if (!initResult.IsSuccess)
            {
                StatusText = $"Cortex api.init failed: {FormatError(initResult.Error)}";
                Debug.LogError(StatusText, this);
                return false;
            }

            return true;
        }

        private async Task<bool> ConnectHeadsetAsync()
        {
            StatusText = "Scanning Cortex headsets";
            CortexResponse scanResult = await runtime.Headset.ScanHeadsetAsync();
            if (!scanResult.IsSuccess)
            {
                StatusText = $"Cortex headset scan failed: {FormatError(scanResult.Error)}";
                Debug.LogError(StatusText, this);
                return false;
            }

            float startedAt = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - startedAt < headsetScanTimeoutSeconds)
            {
                IReadOnlyList<Headset> headsets = runtime.Headset.GetHeadsets();
                Headset headset = SelectHeadset(headsets);
                if (headset != null)
                {
                    connectedHeadsetId = headset.Id;
                    break;
                }

                await Task.Delay(250);
            }

            if (string.IsNullOrWhiteSpace(connectedHeadsetId))
            {
                StatusText = "No Cortex headset found before scan timeout";
                Debug.LogError(StatusText, this);
                return false;
            }

            StatusText = $"Connecting Cortex headset {connectedHeadsetId}";
            CortexResponse connectResult = await runtime.Headset.ConnectHeadsetAsync(
                connectedHeadsetId,
                streams: new List<DataSampleType>
                {
                    DataSampleType.DeviceInfo,
                    DataSampleType.MentalCommand
                });

            if (!connectResult.IsSuccess)
            {
                StatusText = $"Cortex headset connect failed: {FormatError(connectResult.Error)}";
                Debug.LogError(StatusText, this);
                return false;
            }

            return true;
        }

        private async Task<bool> LoadProfileAsync()
        {
            StatusText = $"Loading Cortex BCI profile for {connectedHeadsetId}";
            CortexResponse profileResult = await runtime.SimpleBCI.LoadProfileAsync(connectedHeadsetId);
            if (!profileResult.IsSuccess)
            {
                StatusText = $"Cortex profile load failed: {FormatError(profileResult.Error)}";
                Debug.LogError(StatusText, this);
                return false;
            }

            return true;
        }

        private async Task ConfigureMentalCommandsAsync()
        {
            List<string> activeActions = new List<string>();
            Dictionary<string, int> sensitivities = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            AddMentalCommandConfig(activeActions, sensitivities, chargeCommandId, chargeSensitivity);
            AddMentalCommandConfig(activeActions, sensitivities, bombCommandId, bombSensitivity);

            if (activeActions.Count == 0)
            {
                return;
            }

            StatusText = $"Configuring Cortex MC actions: {string.Join(", ", activeActions)}";
            CortexResponse<MentalCommandInfo> configureResult = await runtime.SimpleBCI.ConfigureMentalCommandAsync(
                sensitivities,
                activeActions);

            if (!configureResult.IsSuccess)
            {
                StatusText = $"Cortex MC configure failed: {FormatError(configureResult.Error)}";
                Debug.LogWarning(StatusText, this);
            }
        }

        private void EmitSignal(MentalCommandDataSample mentalCommand)
        {
            string commandId = mentalCommand.Action ?? string.Empty;
            if (ignoreNeutral && string.Equals(commandId, "neutral", StringComparison.OrdinalIgnoreCase))
            {
                previousCommandId = string.Empty;
                StatusText = $"Cortex MC neutral power {mentalCommand.Power:0.00}";
                return;
            }

            MentalCommandPhase phase = string.Equals(previousCommandId, commandId, StringComparison.OrdinalIgnoreCase)
                ? MentalCommandPhase.Held
                : MentalCommandPhase.Started;

            previousCommandId = commandId;
            float power = Mathf.Clamp01(mentalCommand.Power);
            LatestSignal = new MentalCommandSignal(commandId, power, power, mentalCommand.Timestamp, phase);
            HasSignal = true;
            StatusText = $"Cortex MC {commandId} {phase} power {power:0.00}";
        }

        private Headset SelectHeadset(IReadOnlyList<Headset> headsets)
        {
            if (headsets == null || headsets.Count == 0)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(headsetIdOverride))
            {
                for (int i = 0; i < headsets.Count; i++)
                {
                    if (string.Equals(headsets[i].Id, headsetIdOverride, StringComparison.OrdinalIgnoreCase))
                    {
                        return headsets[i];
                    }
                }

                return null;
            }

            return headsets[0];
        }

        private static void AddMentalCommandConfig(
            List<string> activeActions,
            Dictionary<string, int> sensitivities,
            string action,
            int sensitivity)
        {
            if (string.IsNullOrWhiteSpace(action) || sensitivities.ContainsKey(action))
            {
                return;
            }

            activeActions.Add(action);
            sensitivities[action] = sensitivity;
        }

        private static string FormatError(ApiError error)
        {
            return error == null
                ? "Unknown error"
                : $"{error.CortexCode} {error.Message}";
        }
#endif
    }
}
