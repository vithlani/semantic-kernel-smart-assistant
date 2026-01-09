using SmartEmailAssistant.Models;

namespace SmartEmailAssistant.Services
{
    /// <summary>
    /// Simulates database/API responses with realistic data
    /// In production, this would be replaced with actual API calls
    /// </summary>
    public class MockDataService
    {
        private static readonly User _currentUser = new User
        {
            Id = "user-001",
            Name = "Krupali Vithlani",
            Email = "kv@company.com",
            TimeZone = "India",
            Signature = @"
                    Best regards,
                    Krupali Vithlani
                    Senior Engineer
                    Company Inc.
                    Phone: (+91) 77123-45670"
        };

        // Simulated calendar with realistic meetings
        private static readonly List<CalendarEvent> _calendarEvents = new()
        {
            // This week's meetings
            new CalendarEvent
            {
                Id = "evt-001",
                Title = "Team Standup",
                StartTime = GetNextWeekday(DayOfWeek.Monday, 9, 0),
                EndTime = GetNextWeekday(DayOfWeek.Monday, 9, 30),
                Location = "Conference Room A",
                Attendees = new() { "dipalic@company.com", "smitp@company.com", "shivanij@company.com" }
            },
            new CalendarEvent
            {
                Id = "evt-002",
                Title = "Q1 Budget Review",
                StartTime = GetNextWeekday(DayOfWeek.Tuesday, 14, 0),
                EndTime = GetNextWeekday(DayOfWeek.Tuesday, 15, 30),
                Location = "Zoom Meeting",
                Attendees = new() { "dipalic@company.com", "koel@company.com" }
            },
            new CalendarEvent
            {
                Id = "evt-003",
                Title = "Client Presentation",
                StartTime = GetNextWeekday(DayOfWeek.Wednesday, 10, 0),
                EndTime = GetNextWeekday(DayOfWeek.Wednesday, 11, 0),
                Location = "Client Office",
                Attendees = new() { "dewa@company.com", "client@external.com" }
            },
            new CalendarEvent
            {
                Id = "evt-004",
                Title = "Sprint Planning",
                StartTime = GetNextWeekday(DayOfWeek.Thursday, 13, 0),
                EndTime = GetNextWeekday(DayOfWeek.Thursday, 15, 0),
                Location = "Conference Room B",
                Attendees = new() { "koel@company.com", "dipali@company.com", "roshan@company.com" }
            },
            new CalendarEvent
            {
                Id = "evt-005",
                Title = "One-on-One with Manager",
                StartTime = GetNextWeekday(DayOfWeek.Friday, 15, 0),
                EndTime = GetNextWeekday(DayOfWeek.Friday, 15, 30),
                Location = "Manager's Office",
                Attendees = new() { "kv@company.com", "arpit@company.com" }
            }
        };

        private static readonly List<Project> _activeProjects = new()
        {
            new Project
            {
                Id = "proj-001",
                Name = "Q1 Budget Planning",
                Status = "Active",
                Keywords = new() { "budget", "q1", "financial", "planning", "forecast" },
                StartDate = new DateTime(2024, 1, 1),
                EndDate = new DateTime(2024, 3, 31)
            },
            new Project
            {
                Id = "proj-002",
                Name = "Website Redesign",
                Status = "Active",
                Keywords = new() { "website", "redesign", "ui", "ux", "frontend" },
                StartDate = new DateTime(2024, 1, 15),
                EndDate = new DateTime(2024, 6, 30)
            },
            new Project
            {
                Id = "proj-003",
                Name = "Customer Analytics Dashboard",
                Status = "Active",
                Keywords = new() { "analytics", "dashboard", "customer", "metrics", "data" },
                StartDate = new DateTime(2024, 2, 1),
                EndDate = null
            },
            new Project
            {
                Id = "proj-004",
                Name = "Mobile App Launch",
                Status = "Planning",
                Keywords = new() { "mobile", "app", "ios", "android", "launch" },
                StartDate = new DateTime(2024, 3, 1),
                EndDate = new DateTime(2024, 12, 31)
            }
        };

        private static readonly Dictionary<string, EmailTemplate> _emailTemplates = new()
        {
            ["meeting_busy"] = new EmailTemplate
            {
                Name = "meeting_busy",
                Subject = "Re: Meeting Request",
                Body = @"Hi {{SenderName}},

                Thank you for reaching out regarding {{MeetingTopic}}.
                Unfortunately, I have a prior commitment at {{RequestedTime}} - I have {{ConflictingMeeting}} scheduled.
                {{AlternativeTimes}}
                Looking forward to connecting!
                {{Signature}}"
            },
            ["meeting_available"] = new EmailTemplate
            {
                Name = "meeting_available",
                Subject = "Re: Meeting Request",
                Body = @"Hi {{SenderName}},
                        Thank you for reaching out regarding {{MeetingTopic}}.
                        I'm available at {{RequestedTime}} and would be happy to meet.
                        {{AdditionalContext}}
                        Looking forward to our discussion!
                        {{Signature}}"
            }
        };

        public static User GetCurrentUser() => _currentUser;

        public static List<CalendarEvent> GetCalendarEvents() => _calendarEvents;

        public static List<CalendarEvent> GetEventsForDate(DateTime date)
        {
            return _calendarEvents
                .Where(e => e.StartTime.Date == date.Date)
                .OrderBy(e => e.StartTime)
                .ToList();
        }

        public static CalendarEvent? GetEventAtTime(DateTime dateTime)
        {
            return _calendarEvents
                .FirstOrDefault(e => dateTime >= e.StartTime && dateTime < e.EndTime);
        }

        public static List<Project> GetActiveProjects() => _activeProjects;

        public static List<Project> FindProjectsByKeyword(string keyword)
        {
            return _activeProjects
                .Where(p => p.Status == "Active" &&
                           p.Keywords.Any(k => k.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        public static EmailTemplate GetEmailTemplate(string templateName)
        {
            return _emailTemplates.ContainsKey(templateName)
                ? _emailTemplates[templateName]
                : _emailTemplates["meeting_available"];
        }

        private static DateTime GetNextWeekday(DayOfWeek dayOfWeek, int hour, int minute)
        {
            var today = DateTime.Today;
            var daysUntilTarget = ((int)dayOfWeek - (int)today.DayOfWeek + 7) % 7;

            if (daysUntilTarget == 0)
            {
                // If it's today, check if the time has passed
                var targetTime = today.AddHours(hour).AddMinutes(minute);
                if (DateTime.Now > targetTime)
                {
                    daysUntilTarget = 7; // Move to next week
                }
            }

            return today.AddDays(daysUntilTarget).AddHours(hour).AddMinutes(minute);
        }

        public static List<DateTime> SuggestAlternativeTimes(DateTime requestedDate)
        {
            var alternatives = new List<DateTime>();
            var currentDate = requestedDate.Date;

            // Check next 7 days
            for (int day = 0; day < 7 && alternatives.Count < 3; day++)
            {
                var checkDate = currentDate.AddDays(day);

                // Check common meeting times: 9 AM, 10 AM, 2 PM, 3 PM, 4 PM
                var possibleTimes = new[] { 9, 10, 14, 15, 16 };

                foreach (var hour in possibleTimes)
                {
                    if (alternatives.Count >= 3) break;

                    var potentialTime = checkDate.AddHours(hour);

                    // Check if this slot is free
                    var hasConflict = _calendarEvents.Any(e =>
                        potentialTime >= e.StartTime && potentialTime < e.EndTime);

                    if (!hasConflict && potentialTime > DateTime.Now)
                    {
                        alternatives.Add(potentialTime);
                    }
                }
            }

            return alternatives;
        }
    }
}