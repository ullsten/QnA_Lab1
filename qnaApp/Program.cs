using System;
using Azure;
using Azure.AI.Language.QuestionAnswering;
using Azure.AI.TextAnalytics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Text;

namespace question_answering
{
    class Program
    {
        // Load environment variables
        private static string qnaKey = Environment.GetEnvironmentVariable("QNA_KEY");
        private static string qnaEndpoint = Environment.GetEnvironmentVariable("QNA_ENDPOINT");
        private static string cogSvcKey = Environment.GetEnvironmentVariable("COGNITIVE_SERVICE_KEY");
        private static string textAnalyticsEndpoint = Environment.GetEnvironmentVariable("TEXT_ANALYTICS_ENDPOINT");
        private static string cogSvcRegion = Environment.GetEnvironmentVariable("COGNITIVE_SERVICE_REGION");
        private static string translatorEndpoint = Environment.GetEnvironmentVariable("TRANSLATOR_ENDPOINT");

        static async Task Main(string[] args)
        {
            // Load environment variables from .env file
            DotNetEnv.Env.Load();

            // Create an instance of LanguageUtility to manage language-related operations
            LanguageUtility languageUtility = new LanguageUtility(cogSvcKey, cogSvcRegion, translatorEndpoint);

            // Set console encoding to unicode to handle international characters
            Console.InputEncoding = Encoding.Unicode;
            Console.OutputEncoding = Encoding.Unicode;

            // Configure QnA Maker client
            Uri endpoint = new Uri(qnaEndpoint);
            AzureKeyCredential credential = new AzureKeyCredential(qnaKey);
            string projectName = "QuestionLab1Test";
            string deploymentName = "production";
            QuestionAnsweringClient client = new QuestionAnsweringClient(endpoint, credential);
            QuestionAnsweringProject project = new QuestionAnsweringProject(projectName, deploymentName);

            // Configure Text Analytics client for language detection
            TextAnalyticsClient textAnalyticsClient = new TextAnalyticsClient(new Uri(textAnalyticsEndpoint), new AzureKeyCredential(cogSvcKey));

            // Print a welcome message
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Welcome to Surface QNA Info");
            Console.ResetColor();

            while (true)
            {
                // Prompt user for a question
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.Write("Ask a question (or type 'exit' to quit): ");
                Console.ResetColor();
                string userInput = Console.ReadLine();

                if (userInput.ToLower() == "exit")
                {
                    Console.WriteLine("Goodbye!");
                    break;
                }

                // Detect the language of the user input question
                DetectedLanguage language = textAnalyticsClient.DetectLanguage(userInput);
                string detectedLanguageCode = language.Iso6391Name;

                string translatedInput = userInput;

                // Get answers from the QnA Maker system
                Response<AnswersResult> response = client.GetAnswers(translatedInput, project);

                foreach (KnowledgeBaseAnswer answer in response.Value.Answers)
                {
                    // Detect the language of the answer
                    DetectedLanguage languageAnswer = textAnalyticsClient.DetectLanguage(answer.Answer);
                    var detectedLanguageCodeAnswer = languageAnswer.Iso6391Name;

                    var qnaAnswer = answer.Answer;
                    if (detectedLanguageCodeAnswer != detectedLanguageCode)
                    {
                        // Translate the answer to the detected user input question language
                        qnaAnswer = await languageUtility.Translate(answer.Answer, detectedLanguageCodeAnswer, detectedLanguageCode);
                    }

                    // Print the question, answer, and confidence level
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Q:{translatedInput}");
                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine($"A:{qnaAnswer}");
                    Console.WriteLine($"({answer.Confidence})");
                    Console.ResetColor();
                }
            }
        }
    }
}
