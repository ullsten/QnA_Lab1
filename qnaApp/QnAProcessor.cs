using System;
using System.Text;
using System.Threading.Tasks;
using Azure;
using Azure.AI.Language.QuestionAnswering;
using Azure.AI.TextAnalytics;

namespace question_answering
{
    public class QnAProcessor
    {
        private LanguageUtility _languageUtility;
        private TextAnalyticsClient _textAnalyticsClient;
        private QuestionAnsweringClient _qnaClient;
        private QuestionAnsweringProject _qnaProject;

        public QnAProcessor(string qnaKey, string qnaEndpoint, string cogSvcKey, string textAnalyticsEndpoint, string cogSvcRegion, string translatorEndpoint)
        {
            // Initialize instances of various services and clients
            _languageUtility = new LanguageUtility(cogSvcKey, cogSvcRegion, translatorEndpoint);
            _textAnalyticsClient = new TextAnalyticsClient(new Uri(textAnalyticsEndpoint), new AzureKeyCredential(cogSvcKey)); //null?
            _qnaClient = new QuestionAnsweringClient(new Uri(qnaEndpoint), new AzureKeyCredential(qnaKey));
            _qnaProject = new QuestionAnsweringProject("QuestionLab1Test", "production");
        }

        public async Task ProcessUserInput()
        {
            // Set console encoding and display a welcome message
            Console.InputEncoding = Encoding.Unicode;
            Console.OutputEncoding = Encoding.Unicode;
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
                DetectedLanguage language = _textAnalyticsClient.DetectLanguage(userInput);
                string detectedLanguageCode = language.Iso6391Name;

                // Get answers from the QnA Maker system
                Response<AnswersResult> response = _qnaClient.GetAnswers(userInput, _qnaProject);

                foreach (KnowledgeBaseAnswer answer in response.Value.Answers)
                {
                    // Detect the language of the answer
                    DetectedLanguage languageAnswer = _textAnalyticsClient.DetectLanguage(answer.Answer);
                    var detectedLanguageCodeAnswer = languageAnswer.Iso6391Name;

                    var qnaAnswer = answer.Answer;
                    if (detectedLanguageCodeAnswer != detectedLanguageCode)
                    {
                        // Translate the answer to the detected user input question language
                        qnaAnswer = await _languageUtility.Translate(answer.Answer, detectedLanguageCodeAnswer, detectedLanguageCode);
                    }

                    // Print the question, answer, and confidence level
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Q:{userInput}");
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
