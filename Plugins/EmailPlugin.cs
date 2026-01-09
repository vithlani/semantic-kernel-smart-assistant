using System.ComponentModel;
using Microsoft.SemanticKernel;
using SmartEmailAssistant.Services;

namespace SmartEmailAssistant.Plugins
{
    /// <summary>
    /// Email Plugin - Simulates your Email/Notification service
    /// In production, this would call your actual Email API
    /// </summary>
    public class EmailPlugin
    {
        [KernelFunction]
        [Description("Drafts a professional email response based on context")]
        public string DraftEmailResponse(
            [Description("The context for the email (meeting request, availability status, etc.)")] string context,
            [Description("Whether user is available or busy")] string availability,
            [Description("The tone of the response (professional, friendly, formal)")] string tone = "professional")
        {
            Console.WriteLine($" [EmailPlugin] Drafting {tone} email response...");

            var user = MockDataService.GetCurrentUser();
            var template = availability.ToUpper().Contains("BUSY")
                ? MockDataService.GetEmailTemplate("meeting_busy")
                : MockDataService.GetEmailTemplate("meeting_available");

            // In real app, you'd use a proper template engine
            var email = template.Body
                .Replace("{{Signature}}", user.Signature)
                .Replace("{{AdditionalContext}}", context);

            Console.WriteLine($" Email draft created");
            return email;
        }

        [KernelFunction]
        [Description("Checks if a topic or keyword relates to any active projects")]
        public string CheckProjectRelevance(
            [Description("The topic or keyword to check (e.g., 'budget', 'Q1', 'website')")] string topic)
        {
            Console.WriteLine($"[EmailPlugin] Checking if '{topic}' relates to active projects...");

            var relevantProjects = MockDataService.FindProjectsByKeyword(topic);

            if (relevantProjects.Any())
            {
                var projectNames = string.Join(", ", relevantProjects.Select(p => $"'{p.Name}'"));
                var result = $"YES - Related to {relevantProjects.Count} active project(s): {projectNames}";

                Console.WriteLine($" Found {relevantProjects.Count} related project(s)");
                return result;
            }

            Console.WriteLine($"  No related projects found");
            return "NO - No active projects found related to this topic";
        }

        [KernelFunction]
        [Description("Gets information about the current user")]
        public string GetUserInfo()
        {
            Console.WriteLine($" [EmailPlugin] Fetching current user info...");

            var user = MockDataService.GetCurrentUser();
            return $"Name: {user.Name}\nEmail: {user.Email}\nTimezone: {user.TimeZone}";
        }
    }
}