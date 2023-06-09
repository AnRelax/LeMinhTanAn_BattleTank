using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;
using Unity.Services.Core.Internal;
using Unity.Services.Core.Internal.Serialization;
using Unity.Services.Core.Scheduler.Internal;
using NotNull = JetBrains.Annotations.NotNullAttribute;

namespace Unity.Services.Core.Telemetry.Internal
{
    class TelemetrySender
    {
        public string TargetUrl { get; }

        readonly ExponentialBackOffRetryPolicy m_RetryPolicy;

        readonly IActionScheduler m_Scheduler;

        readonly IUnityWebRequestSender m_RequestSender;

        internal IJsonSerializer Serializer { get; }

        public TelemetrySender(
            [NotNull] string targetUrl,
            [NotNull] string servicePath,
            [NotNull] IActionScheduler scheduler,
            [NotNull] ExponentialBackOffRetryPolicy retryPolicy,
            [NotNull] IUnityWebRequestSender requestSender,
            [NotNull] IJsonSerializer serializer)
        {
            TargetUrl = $"{targetUrl}/{servicePath}";
            m_RetryPolicy = retryPolicy;
            m_Scheduler = scheduler;
            m_RequestSender = requestSender;
            Serializer = serializer;
        }

        public Task SendAsync<TPayload>(TPayload payload)
            where TPayload : ITelemetryPayload
        {
            var completionSource = new TaskCompletionSource<object>();
            var sendCount = 0;
            byte[] serializedPayload;

            try
            {
                serializedPayload = SerializePayload(payload);
                SendWebRequest();
            }
            catch (Exception e)
            {
                completionSource.TrySetException(e);
            }

            return completionSource.Task;

            void SendWebRequest()
            {
                var request = CreateRequest(serializedPayload);

                sendCount++;
                CoreLogger.LogTelemetry($"Attempt #{sendCount.ToString()} to send {typeof(TPayload).Name}.");

                m_RequestSender.SendRequest(request, OnRequestCompleted);
            }

            void OnRequestCompleted(WebRequest webRequest)
            {
                try
                {
                    if (webRequest.IsSuccess)
                    {
                        CoreLogger.LogTelemetry($"{typeof(TPayload).Name} sent successfully");
                        completionSource.TrySetResult(null);
                    }
                    else if (m_RetryPolicy.CanRetry(webRequest, sendCount))
                    {
                        var delayBeforeSendingSeconds = m_RetryPolicy.GetDelayBeforeSendingSeconds(sendCount);
                        m_Scheduler.ScheduleAction(SendWebRequest, delayBeforeSendingSeconds);
                    }
                    else
                    {
                        var errorMessage = $"Error: {webRequest.ErrorMessage}\nBody: {webRequest.ErrorBody}";
                        completionSource.TrySetException(new Exception(errorMessage));
                        CoreLogger.LogTelemetry(
                            $"{typeof(TPayload).Name} couldn't be sent after {sendCount.ToString()} tries."
                            + $"\n{errorMessage}");
                    }
                }
                catch (Exception e)
                    when (TelemetryUtils.LogTelemetryException(e, true))
                {
                    completionSource.TrySetException(e);
                }
            }
        }

        internal byte[] SerializePayload<TPayload>(TPayload payload)
            where TPayload : ITelemetryPayload
        {
            var jsonPayload = Serializer.SerializeObject(payload);
            var serializedPayload = Encoding.UTF8.GetBytes(jsonPayload);
            return serializedPayload;
        }

        internal UnityWebRequest CreateRequest(byte[] serializedPayload)
        {
            var request = new UnityWebRequest(TargetUrl, UnityWebRequest.kHttpVerbPOST)
            {
                uploadHandler = new UploadHandlerRaw(serializedPayload)
                {
                    contentType = UnityWebRequestUtils.JsonContentType,
                },
                downloadHandler = new DownloadHandlerBuffer()
            };
            request.SetRequestHeader("Content-Type", UnityWebRequestUtils.JsonContentType);
            return request;
        }
    }
}
