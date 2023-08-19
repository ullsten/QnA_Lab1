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
            private static string qnaKey = Environment.GetEnvironmentVariable("QNA_KEY");
            private static string qnaEndpoint = Environment.GetEnvironmentVariable("QNA_ENDPOINT");
            private static string cogSvcKey = Environment.GetEnvironmentVariable("COGNITIVE_SERVICE_KEY");
            private static string textAnalyticsEndpoint = Environment.GetEnvironmentVariable("TEXT_ANALYTICS_ENDPOINT");
            private static string cogSvcRegion = Environment.GetEnvironmentVariable("COGNITIVE_SERVICE_REGION");
            private static string translatorEndpoint = Environment.GetEnvironmentVariable("TRANSLATOR_ENDPOINT");
        static void Main(string[] args)
        {
            DotNetEnv.Env.Load();
            
            Uri endpoint = new Uri(qnaEndpoint);
            AzureKeyCredential credential = new AzureKeyCredential(qnaKey);
            string projectName = "QuestionLab1Test";
            string deploymentName = "production";

            QuestionAnsweringClient client = new QuestionAnsweringClient(endpoint, credential);
            QuestionAnsweringProject project = new QuestionAnsweringProject(projectName, deploymentName);

            TextAnalyticsClient textAnalyticsClient = new TextAnalyticsClient(new Uri(textAnalyticsEndpoint), new AzureKeyCredential(cogSvcKey));


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
        static async Task<string> GetLanguage(string text)
        {            
            // Default language is English
            string language = "en";

            // Use the Translator detect function
            object[] body = new object[] { new { Text = text } };
            var requestBody = JsonConvert.SerializeObject(body);
            using (var client = new HttpClient())
            {
                using (var request = new HttpRequestMessage())
                {
                    // Build the request
                    string path = "/detect?api-version=3.0";
                    request.Method = HttpMethod.Post;
                    request.RequestUri = new Uri(translatorEndpoint + path);
                    request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                    request.Headers.Add("Ocp-Apim-Subscription-Key", cogSvcKey);
                    request.Headers.Add("Ocp-Apim-Subscription-Region", cogSvcRegion);

                    // Send the request and get response
                    HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
                    // Read response as a string
                    string responseContent = await response.Content.ReadAsStringAsync();

                    // Parse JSON array and get language
                    JArray jsonResponse = JArray.Parse(responseContent);
                    language = (string)jsonResponse[0]["language"];
                }
            }
            // return the language
            return language;
        }

        static async Task<string> Translate(string text, string sourceLanguage)
        {
            string translation = "";

            // Use the Translator translate function
            object[] body = new object[] { new { Text = text } };
            var requestBody = JsonConvert.SerializeObject(body);
            using (var client = new HttpClient())
            {
                using (var request = new HttpRequestMessage())
                {
                    // Build the request
                    string path = "/translate?api-version=3.0&from=" + sourceLanguage + "&to=en";
                    request.Method = HttpMethod.Post;
                    request.RequestUri = new Uri(translatorEndpoint + path);
                    request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                    request.Headers.Add("Ocp-Apim-Subscription-Key", cogSvcKey);
                    request.Headers.Add("Ocp-Apim-Subscription-Region", cogSvcRegion);

                    // Send the request and get response
                    HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
                    // Read response as a string
                    string responseContent = await response.Content.ReadAsStringAsync();

                    // Parse JSON array and get translation
                    JArray jsonResponse = JArray.Parse(responseContent);
                    translation = (string)jsonResponse[0]["translations"][0]["text"];
                }
            }
            // Return the translation
            return translation;
        }
    }
}
