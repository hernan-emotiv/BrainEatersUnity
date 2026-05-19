using System.Threading.Tasks;
using UnityEngine;

#if UNITY_ANDROID
using UnityEngine.Android;
#endif

namespace BrainEaters.Input
{
    public static class CortexAndroidPermissionRequester
    {
        public static Task<bool> RequestRequiredPermissionsAsync()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            string[] permissions = GetRequiredPermissions();
            bool needsRequest = false;
            for (int i = 0; i < permissions.Length; i++)
            {
                if (!Permission.HasUserAuthorizedPermission(permissions[i]))
                {
                    needsRequest = true;
                    break;
                }
            }

            if (!needsRequest)
            {
                return Task.FromResult(true);
            }

            TaskCompletionSource<bool> completion = new TaskCompletionSource<bool>();
            int pending = permissions.Length;
            bool allGranted = true;
            PermissionCallbacks callbacks = new PermissionCallbacks();
            callbacks.PermissionGranted += _ => CompleteOne();
            callbacks.PermissionDenied += _ =>
            {
                allGranted = false;
                CompleteOne();
            };
            callbacks.PermissionDeniedAndDontAskAgain += _ =>
            {
                allGranted = false;
                CompleteOne();
            };

            Permission.RequestUserPermissions(permissions, callbacks);
            return completion.Task;

            void CompleteOne()
            {
                pending--;
                if (pending <= 0)
                {
                    completion.TrySetResult(allGranted);
                }
            }
#else
            return Task.FromResult(true);
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private static string[] GetRequiredPermissions()
        {
            int sdkInt = GetAndroidSdkInt();
            return sdkInt >= 31
                ? new[]
                {
                    "android.permission.ACCESS_FINE_LOCATION",
                    "android.permission.BLUETOOTH_SCAN",
                    "android.permission.BLUETOOTH_CONNECT"
                }
                : new[]
                {
                    "android.permission.ACCESS_FINE_LOCATION",
                    "android.permission.BLUETOOTH"
                };
        }

        private static int GetAndroidSdkInt()
        {
            using AndroidJavaClass version = new AndroidJavaClass("android.os.Build$VERSION");
            return version.GetStatic<int>("SDK_INT");
        }
#endif
    }
}

