using System;

namespace question_answering
{
    public static class EnvironmentVariables
    {
        public static string QnaKey => Environment.GetEnvironmentVariable("QNA_KEY");
        public static string QnaEndpoint => Environment.GetEnvironmentVariable("QNA_ENDPOINT");
        public static string CogSvcKey => Environment.GetEnvironmentVariable("COGNITIVE_SERVICE_KEY");
        public static string TextAnalyticsEndpoint => Environment.GetEnvironmentVariable("TEXT_ANALYTICS_ENDPOINT");
        public static string CogSvcRegion => Environment.GetEnvironmentVariable("COGNITIVE_SERVICE_REGION");
        public static string TranslatorEndpoint => Environment.GetEnvironmentVariable("TRANSLATOR_ENDPOINT");
    }
}
