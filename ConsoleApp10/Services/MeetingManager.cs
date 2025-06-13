using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MeetingApp.Models;

namespace MeetingApp.Services
{
    /// <summary>
    /// Сервис для управления встречами: добавление, обновление, удаление и экспорт.
    /// </summary>
    public class MeetingManager
    {
        private List<Meeting> meetings = new List<Meeting>();

        /// <summary>
        /// Получает встречи на конкретную дату.
        /// </summary>
        public IEnumerable<Meeting> GetMeetingsForDay(DateTime date) =>
            meetings.Where(m => m.Start.Date == date.Date).OrderBy(m => m.Start);

        /// <summary>
        /// Добавляет новую встречу, если она не пересекается с другими и запланирована на будущее.
        /// </summary>
        public bool AddMeeting(Meeting newMeeting)
        {
            if (newMeeting.Start <= DateTime.Now || newMeeting.End <= newMeeting.Start)
                return false;

            if (meetings.Any(m => m.Start < newMeeting.End && m.End > newMeeting.Start))
                return false;

            meetings.Add(newMeeting);
            return true;
        }

        /// <summary>
        /// Обновляет встречу с указанным ID.
        /// </summary>
        public bool UpdateMeeting(Guid id, Action<Meeting> updateAction)
        {
            var meeting = meetings.FirstOrDefault(m => m.Id == id);
            if (meeting == null) return false;

            meetings.Remove(meeting);

            var clone = new Meeting
            {
                Title = meeting.Title,
                Start = meeting.Start,
                End = meeting.End,
                ReminderOffset = meeting.ReminderOffset
            };

            updateAction(clone);
            if (!AddMeeting(clone))
            {
                meetings.Add(meeting);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Удаляет встречу по ID.
        /// </summary>
        public bool DeleteMeeting(Guid id) =>
            meetings.RemoveAll(m => m.Id == id) > 0;

        /// <summary>
        /// Экспортирует встречи на день в указанный файл.
        /// </summary>
        public void ExportDaySchedule(DateTime date, string path)
        {
            var lines = GetMeetingsForDay(date).Select(m => m.ToString());
            File.WriteAllLines(path, lines);
        }

        /// <summary>
        /// Получает встречи, по которым нужно отправить напоминание.
        /// </summary>
        public List<Meeting> GetUpcomingReminders()
        {
            var now = DateTime.Now;
            return meetings
                .Where(m => m.ReminderTime.HasValue && m.ReminderTime.Value <= now && m.Start > now)
                .ToList();
        }

        /// <summary>
        /// Удаляет напоминание у встречи.
        /// </summary>
        public void RemoveReminder(Guid id)
        {
            var meeting = meetings.FirstOrDefault(m => m.Id == id);
            if (meeting != null)
                meeting.ReminderOffset = null;
        }
    }
}
