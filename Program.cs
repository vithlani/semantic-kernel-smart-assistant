using Microsoft.SemanticKernel;
using dotenv.net;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SmartEmailAssistant.Plugins;
using Microsoft.SemanticKernel.ChatCompletion;

namespace SmartEmailAssistant
{
    class Program
    {
        static async Task Main(string[] args)
        {

            // ============================================================
            // STEP 1: Loading Application key
            // ============================================================

            DotEnv.Load();
            var api_key = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

            if (string.IsNullOrEmpty(api_key))
            {
                Console.WriteLine("❌ Error: OPENAI_API_KEY not found in .env file");
                Console.WriteLine("Please create a .env file with your OpenAI API key:");
                Console.WriteLine("OPENAI_API_KEY=sk-your-key-here");
                return;
            }

            // ============================================================
            // STEP 2: Create Kernel & Register Services
            // ============================================================
            var builder = Kernel.CreateBuilder();

            // Services
            builder.AddOpenAIChatCompletion(
                modelId: "gpt-4o-mini",
                apiKey: api_key
            );

            // Plugins (Your Business Logic)
            builder.Plugins.AddFromType<CalendarPlugin>();
            builder.Plugins.AddFromType<EmailPlugin>();

            Kernel kernel = builder.Build();

            // ============================================================
            // STEP 3: Setup Chat Service
            // ============================================================
            var chatService = kernel.GetRequiredService<IChatCompletionService>();
            ChatHistory chatMessages = new ChatHistory();

            // Add persona to the system message to set context
            chatMessages.AddSystemMessage(@"
                    You are a helpful assistant that helps manage emails and calendar.
                    You have access to calendar and email management functions.
                    When responding to meeting requests:
                    1. Always check calendar availability first
                    2. Check if the topic relates to any active projects
                    3. Draft appropriate email responses
                    4. Suggest alternative times if the user is busy
                    Be concise and professional in your responses.");

            // ============================================================
            // STEP 4: Display Welcome Message
            // ============================================================
            Console.WriteLine("╔════════════════════════════════════════════════════════╗");
            Console.WriteLine("║     Smart Email Assistant - Semantic Kernel Demo      ║");
            Console.WriteLine("║            Interactive Mode - Type 'exit' to quit      ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════╝\n");

            // ============================================================
            // STEP 5: Interactive Loop
            // ============================================================
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("You: ");
                Console.ResetColor();

                var userInput = Console.ReadLine() ?? string.Empty;

                // Exit condition
                if (string.IsNullOrWhiteSpace(userInput) ||
                    userInput.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
                    userInput.Equals("quit", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("\n👋 Goodbye!");
                    break;
                }

                // Add user message to history
                chatMessages.AddUserMessage(userInput);

                // Get streaming response with auto function calling
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("Assistant: ");
                Console.ResetColor();

                var completion = chatService.GetStreamingChatMessageContentsAsync(
                    chatMessages,
                    executionSettings: new OpenAIPromptExecutionSettings()
                    {
                        ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
                    },
                    kernel: kernel
                );

                string fullMessage = "";

                try
                {
                    await foreach (var messagePart in completion)
                    {
                        Console.Write(messagePart.Content);
                        fullMessage += messagePart.Content;
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\n❌ Error: {ex.Message}");
                    Console.ResetColor();
                    continue;
                }

                // Add assistant response to history
                chatMessages.AddAssistantMessage(fullMessage);
                Console.WriteLine("\n");
            }
        }
    }
}