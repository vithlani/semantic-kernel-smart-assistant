using System.ComponentModel;
using System.Globalization;
using Microsoft.SemanticKernel;
using SmartEmailAssistant.Models;
using SmartEmailAssistant.Services;

namespace SmartEmailAssistant.Plugins
{
    /// <summary>
    /// Calendar Plugin - Simulates your Calendar API
    /// In production, this would call your actual Calendar service/API
    /// </summary>
    public class CalendarPlugin
    {
        [KernelFunction]
        [Description("Gets the list of attendees for a specific meeting")]
        public string GetMeetingAttendees(
            [Description("The meeting title or ID to get attendees for")] string meetingIdentifier)
        {
            Console.WriteLine($"[CalendarPlugin] Fetching attendees for meeting: {meetingIdentifier}...");
            try
            {
                // Search by title or ID
                    var allEvents = MockDataService.GetCalendarEvents();
                    var meeting = allEvents.FirstOrDefault(e =>
                        e.Title.Contains(meetingIdentifier, StringComparison.OrdinalIgnoreCase) ||
                        e.Id.Equals(meetingIdentifier, StringComparison.OrdinalIgnoreCase));

                    if (meeting == null)
                    {
                        Console.WriteLine($"  ❌ Meeting not found");
                        return $"Meeting '{meetingIdentifier}' not found";
                    }

                    if (!meeting.Attendees.Any())
                    {
                        return $"Meeting '{meeting.Title}' has no attendees listed";
                    }

                    var result = $"Attendees for '{meeting.Title}':\n";
                    result += $"Date: {meeting.StartTime:dddd, MMMM d} at {meeting.StartTime:h:mm tt}\n";
                    result += $"Location: {meeting.Location}\n\n";
                    result += $"Attendees ({meeting.Attendees.Count}):\n";

                    foreach (var attendee in meeting.Attendees)
                    {
                        result += $"• {attendee}\n";
                    }

                    Console.WriteLine($" Found {meeting.Attendees.Count} attendee(s)");
                    return result.Trim();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return $"ERROR - Could not retrieve attendees: {ex.Message}";
            }
        }
        [KernelFunction]
        [Description("Checks if a specific date and time is available on the calendar")]
        public string CheckAvailability(
            [Description("The day to check (e.g., 'next Tuesday', 'January 15', 'tomorrow')")] string day,
            [Description("The time to check (e.g., '2 PM', '14:00', '2:30 PM')")] string time)
        {
            Console.WriteLine($"  📅 [CalendarPlugin] Checking availability for {day} at {time}...");

            try
            {
                // Parse the natural language date and time
                var targetDateTime = ParseDateTime(day, time);

                // Check if there's a meeting at that time
                var conflictingEvent = MockDataService.GetEventAtTime(targetDateTime);

                if (conflictingEvent != null)
                {
                    var response = $"BUSY - You have '{conflictingEvent.Title}' scheduled from " +
                                 $"{conflictingEvent.StartTime:h:mm tt} to {conflictingEvent.EndTime:h:mm tt}";

                    if (!string.IsNullOrEmpty(conflictingEvent.Location))
                    {
                        response += $" at {conflictingEvent.Location}";
                    }

                    Console.WriteLine($"  ❌ Conflict found: {conflictingEvent.Title}");
                    return response;
                }

                Console.WriteLine($"  ✅ Time slot is available");
                return $"AVAILABLE - No conflicts found for {targetDateTime:dddd, MMMM d} at {targetDateTime:h:mm tt}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ⚠️ Error: {ex.Message}");
                return $"ERROR - Could not parse date/time: {ex.Message}";
            }
        }

        [KernelFunction]
        [Description("Gets all meetings scheduled for a specific day")]
        public string GetMeetingsForDay(
            [Description("The day to retrieve meetings for (e.g., 'today', 'tomorrow', 'next Monday')")] string day)
        {
            Console.WriteLine($"  📋 [CalendarPlugin] Fetching meetings for {day}...");

            try
            {
                var targetDate = ParseDate(day);
                var meetings = MockDataService.GetEventsForDate(targetDate);

                if (!meetings.Any())
                {
                    return $"No meetings scheduled for {targetDate:dddd, MMMM d, yyyy}";
                }

                var result = $"Meetings for {targetDate:dddd, MMMM d, yyyy}:\n";
                foreach (var meeting in meetings)
                {
                    result += $"- {meeting.StartTime:h:mm tt} - {meeting.EndTime:h:mm tt}: {meeting.Title}";

                    if (!string.IsNullOrEmpty(meeting.Location))
                    {
                        result += $" ({meeting.Location})";
                    }
                    result += "\n";
                }

                Console.WriteLine($"  ✅ Found {meetings.Count} meeting(s)");
                return result.Trim();
            }
            catch (Exception ex)
            {
                return $"ERROR - Could not retrieve meetings: {ex.Message}";
            }
        }

        [KernelFunction]
        [Description("Suggests alternative meeting times when the requested time is not available")]
        public string SuggestAlternativeTimes(
            [Description("The originally requested date/time")] string requestedDateTime,
            [Description("Number of alternatives to suggest")] int count = 3)
        {
            Console.WriteLine($"  💡 [CalendarPlugin] Finding {count} alternative times...");

            try
            {
                var requestedDate = ParseDate(requestedDateTime);
                var alternatives = MockDataService.SuggestAlternativeTimes(requestedDate);

                if (!alternatives.Any())
                {
                    return "No alternative times found in the next 7 days";
                }

                var result = "Alternative times available:\n";
                foreach (var altTime in alternatives.Take(count))
                {
                    result += $"- {altTime:dddd, MMMM d} at {altTime:h:mm tt}\n";
                }

                Console.WriteLine($"  ✅ Found {alternatives.Count} alternative slot(s)");
                return result.Trim();
            }
            catch (Exception ex)
            {
                return $"ERROR - Could not suggest alternatives: {ex.Message}";
            }
        }

        // Helper methods for date parsing
        private DateTime ParseDateTime(string day, string time)
        {
            var date = ParseDate(day);
            var timeSpan = ParseTime(time);
            return date.Add(timeSpan);
        }

        private DateTime ParseDate(string day)
        {
            day = day.ToLower().Trim();
            var today = DateTime.Today;

            // Handle common relative dates
            if (day == "today") return today;
            if (day == "tomorrow") return today.AddDays(1);

            // Handle "next [weekday]"
            if (day.StartsWith("next "))
            {
                var weekdayStr = day.Replace("next ", "");
                if (Enum.TryParse<DayOfWeek>(weekdayStr, true, out var targetDay))
                {
                    var daysUntilTarget = ((int)targetDay - (int)today.DayOfWeek + 7) % 7;
                    if (daysUntilTarget == 0) daysUntilTarget = 7; // Next week if it's today
                    return today.AddDays(daysUntilTarget);
                }
            }

            // Handle specific weekdays (assumes next occurrence)
            if (Enum.TryParse<DayOfWeek>(day, true, out var weekDay))
            {
                var daysUntilTarget = ((int)weekDay - (int)today.DayOfWeek + 7) % 7;
                if (daysUntilTarget == 0) daysUntilTarget = 7;
                return today.AddDays(daysUntilTarget);
            }

            // Try parsing as actual date
            if (DateTime.TryParse(day, out var parsedDate))
            {
                return parsedDate;
            }

            throw new ArgumentException($"Could not parse date: {day}");
        }

        private TimeSpan ParseTime(string time)
        {
            time = time.ToLower().Trim();

            // Handle "2 PM", "2PM", "14:00", "2:30 PM" formats
            var formats = new[]
            {
                "h tt", "htt", "h:mm tt", "h:mmtt", "HH:mm", "H:mm"
            };

            foreach (var format in formats)
            {
                if (DateTime.TryParseExact(time, format, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var parsed))
                {
                    return parsed.TimeOfDay;
                }
            }

            throw new ArgumentException($"Could not parse time: {time}");
        }
    }
}