using System;
using Azure;
using Azure.AI.Language.QuestionAnswering;
using Azure.AI.TextAnalytics;

namespace question_answering
{
    class Program
    {
        static void Main(string[] args)
        {

            DotNetEnv.Env.Load();

            var qnaKey = Environment.GetEnvironmentVariable("QNA_KEY");
            var qnaEndpoint = Environment.GetEnvironmentVariable("QNA_ENDPOINT");
            var textAnalyticsKey = Environment.GetEnvironmentVariable("TEXT_ANALYTICS_KEY");
            var textAnalyticsEndpoint = Environment.GetEnvironmentVariable("TEXT_ANALYTICS_ENDPOINT");

            // This example requires environment variables named "LANGUAGE_KEY" and "LANGUAGE_ENDPOINT"
            Uri endpoint = new Uri(qnaEndpoint);
            AzureKeyCredential credential = new AzureKeyCredential(qnaKey);
            string projectName = "QuestionLab1Test";
            string deploymentName = "production";

            QuestionAnsweringClient client = new QuestionAnsweringClient(endpoint, credential);
            QuestionAnsweringProject project = new QuestionAnsweringProject(projectName, deploymentName);

            TextAnalyticsClient textAnalyticsClient = new TextAnalyticsClient(new Uri(textAnalyticsEndpoint), new AzureKeyCredential(textAnalyticsKey));


            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Welcome to Surface QNA Info");
            Console.ResetColor();

            while (true)
            {
              Console.ForegroundColor = ConsoleColor.Magenta;
                Console.Write("Ask a question (or type 'exit' to quit): ");
                Console.ResetColor();
                string userInput = Console.ReadLine();
                if (userInput.ToLower() == "exit")
                {
                    Console.WriteLine("Goodbye!");
                    break;
                }

                   // Detect the language of the user input
                DetectedLanguage language = textAnalyticsClient.DetectLanguage(userInput);
                string detectedLanguageCode = language.Iso6391Name;

                Console.WriteLine($"Detected Language: {detectedLanguageCode}");

                Response<AnswersResult> response = client.GetAnswers(userInput, project);

                foreach (KnowledgeBaseAnswer answer in response.Value.Answers)
                {
                  Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Q:{userInput}");
                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine($"A:{answer.Answer}");
                    Console.WriteLine($"({answer.Confidence})");
                    Console.ResetColor();
                }
            }            
        }
    }
}
