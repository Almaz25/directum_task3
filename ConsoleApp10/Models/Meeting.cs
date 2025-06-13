using System;

namespace MeetingApp.Models
{
    /// <summary>
    /// Представляет встречу с названием, временем начала, окончания и напоминанием.
    /// </summary>
    public class Meeting
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string Title { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public TimeSpan? ReminderOffset { get; set; }

        public DateTime? ReminderTime => ReminderOffset.HasValue ? Start - ReminderOffset : null;

        public override string ToString()
        {
            return $"{Title} | {Start:yyyy-MM-dd HH:mm} - {End:HH:mm}" +
                   (ReminderOffset.HasValue ? $" | Напоминание за {ReminderOffset}" : "");
        }
    }
}
