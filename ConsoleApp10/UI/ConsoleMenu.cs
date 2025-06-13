

using MeetingApp.Models;
using MeetingApp.Services;
using System;
using System.Linq;
using System.Threading;

namespace MeetingApp.UI
{
    /// <summary>
    /// Отвечает за отображение консольного меню и взаимодействие с пользователем.
    /// </summary>
    public static class ConsoleMenu
    {
        private static readonly MeetingManager manager = new();

        /// <summary>
        /// Запускает меню и опрос пользовательского ввода.
        /// </summary>
        public static void Run()
        {
            Console.WriteLine("Приложение для управления встречами.");
            var reminderThread = new Thread(ReminderChecker)
            {
                IsBackground = true
            };
            reminderThread.Start();

            while (true)
            {
                Console.WriteLine("\nМеню:");
                Console.WriteLine("1. Добавить встречу");
                Console.WriteLine("2. Изменить встречу");
                Console.WriteLine("3. Удалить встречу");
                Console.WriteLine("4. Просмотреть встречи на день");
                Console.WriteLine("5. Экспорт в файл");
                Console.WriteLine("0. Выход");
                Console.Write("Выбор: ");
                var input = Console.ReadLine();

                switch (input)
                {
                    case "1": AddMeeting(); break;
                    case "2": UpdateMeeting(); break;
                    case "3": DeleteMeeting(); break;
                    case "4": ViewMeetings(); break;
                    case "5": ExportSchedule(); break;
                    case "0": return;
                    default: Console.WriteLine("Неверный ввод."); break;
                }
            }
        }

        private static void AddMeeting()
        {
            Console.Write("Название: ");
            var title = Console.ReadLine();

            Console.Write("Начало (yyyy-MM-dd HH:mm): ");
            if (!DateTime.TryParse(Console.ReadLine(), out var start)) return;

            Console.Write("Окончание (yyyy-MM-dd HH:mm): ");
            if (!DateTime.TryParse(Console.ReadLine(), out var end)) return;

            Console.Write("Напоминание (в минутах до начала, пусто — без): ");
            var reminderInput = Console.ReadLine();
            TimeSpan? reminder = null;
            if (int.TryParse(reminderInput, out var minutes))
                reminder = TimeSpan.FromMinutes(minutes);

            var meeting = new Meeting
            {
                Title = title,
                Start = start,
                End = end,
                ReminderOffset = reminder
            };

            Console.WriteLine(manager.AddMeeting(meeting)
                ? "Встреча добавлена."
                : "Ошибка: встреча пересекается или некорректна.");
        }

        private static void UpdateMeeting()
        {
            Console.Write("Введите дату (yyyy-MM-dd): ");
            if (!DateTime.TryParse(Console.ReadLine(), out var date)) return;

            var meetings = manager.GetMeetingsForDay(date).ToList();
            for (var i = 0; i < meetings.Count; i++)
                Console.WriteLine($"{i + 1}. {meetings[i]}");

            Console.Write("Выберите встречу для редактирования: ");
            if (!int.TryParse(Console.ReadLine(), out var index) || index < 1 || index > meetings.Count) return;

            var oldMeeting = meetings[index - 1];

            Console.Write("Новое название (enter чтобы оставить): ");
            var title = Console.ReadLine();
            Console.Write("Новое начало (enter чтобы оставить): ");
            var startStr = Console.ReadLine();
            Console.Write("Новое окончание (enter чтобы оставить): ");
            var endStr = Console.ReadLine();
            Console.Write("Новое напоминание (в минутах, enter чтобы оставить / удалить): ");
            var reminderStr = Console.ReadLine();

            manager.UpdateMeeting(oldMeeting.Id, m =>
            {
                if (!string.IsNullOrWhiteSpace(title)) m.Title = title;
                if (DateTime.TryParse(startStr, out var newStart)) m.Start = newStart;
                if (DateTime.TryParse(endStr, out var newEnd)) m.End = newEnd;
                if (int.TryParse(reminderStr, out var min)) m.ReminderOffset = TimeSpan.FromMinutes(min);
                else if (reminderStr == "") m.ReminderOffset = null;
            });

            Console.WriteLine("Встреча обновлена.");
        }

        private static void DeleteMeeting()
        {
            Console.Write("Дата встречи (yyyy-MM-dd): ");
            if (!DateTime.TryParse(Console.ReadLine(), out var date)) return;

            var meetings = manager.GetMeetingsForDay(date).ToList();
            for (var i = 0; i < meetings.Count; i++)
                Console.WriteLine($"{i + 1}. {meetings[i]}");

            Console.Write("Выберите номер для удаления: ");
            if (!int.TryParse(Console.ReadLine(), out var index) || index < 1 || index > meetings.Count) return;

            var meeting = meetings[index - 1];
            manager.DeleteMeeting(meeting.Id);
            Console.WriteLine("Встреча удалена.");
        }

        private static void ViewMeetings()
        {
            Console.Write("Введите дату (yyyy-MM-dd): ");
            if (!DateTime.TryParse(Console.ReadLine(), out var date)) return;

            var list = manager.GetMeetingsForDay(date).ToList();
            if (!list.Any())
            {
                Console.WriteLine("Нет встреч на этот день.");
                return;
            }

            foreach (var m in list)
                Console.WriteLine(m);
        }

        private static void ExportSchedule()
        {
            Console.Write("Введите дату (yyyy-MM-dd): ");
            if (!DateTime.TryParse(Console.ReadLine(), out var date)) return;

            Console.Write("Введите путь к файлу: ");
            var path = Console.ReadLine();

            manager.ExportDaySchedule(date, path);
            Console.WriteLine("Экспорт завершён.");
        }

        private static void ReminderChecker()
        {
            while (true)
            {
                var toRemind = manager.GetUpcomingReminders();
                foreach (var m in toRemind)
                {
                    Console.WriteLine($"\n🔔 Напоминание: встреча \"{m.Title}\" начнется в {m.Start:HH:mm}");
                    manager.RemoveReminder(m.Id);
                }

                Thread.Sleep(30000);
            }
        }
    }
}
