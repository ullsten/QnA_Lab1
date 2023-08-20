using System;
using System.Threading.Tasks;

namespace question_answering
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Load environment variables from the .env file
            DotNetEnv.Env.Load();

            // Get environment variables for various services
            string qnaKey = EnvironmentVariables.QnaKey;
            string qnaEndpoint = EnvironmentVariables.QnaEndpoint;
            string cogSvcKey = EnvironmentVariables.CogSvcKey;
            string textAnalyticsEndpoint = EnvironmentVariables.TextAnalyticsEndpoint;
            string cogSvcRegion = EnvironmentVariables.CogSvcRegion;
            string translatorEndpoint = EnvironmentVariables.TranslatorEndpoint;

            // Create an instance of QnAProcessor to handle the question-answering process
            QnAProcessor qnaProcessor = new QnAProcessor(qnaKey, qnaEndpoint, cogSvcKey, textAnalyticsEndpoint, cogSvcRegion, translatorEndpoint);

            // Begin processing user input and providing answers
            await qnaProcessor.ProcessUserInput();
        }
    }
}
