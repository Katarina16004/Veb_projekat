using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Veb_Projekat.Models;
using Veb_Projekat.Models.Enums;

namespace Veb_Projekat.Repositories
{
    public class CommentRepository
    {
        private static string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data/Data/Comments.txt");

        public static List<Comment> GetAll()
        {
            var comments = new List<Comment>();

            if (!File.Exists(filePath))
                return comments;

            var lines = File.ReadAllLines(filePath);

            for (int i = 1; i < lines.Length; i++)
            {
                var parts = lines[i].Split(';');
                if (parts.Length < 6)
                    continue;

                Guid id = Guid.TryParse(parts[0], out Guid commentId) ? commentId : Guid.NewGuid();
                string touristUsername = parts[1];
                int accommodationId = int.TryParse(parts[2], out int accId) ? accId : 0;
                string text = parts[3];
                int rating = int.TryParse(parts[4], out int r) ? r : 0;
                CommentStatusEnum status = Enum.TryParse(parts[5], out CommentStatusEnum s) ? s : CommentStatusEnum.Pending;

                DateTime createdAt = DateTime.Now;
                if (parts.Length > 6)
                    DateTime.TryParse(parts[6], out createdAt);

                var tourist = UserRepository.GetByUsername(touristUsername);
                var accommodation = AccommodationRepository.GetById(accommodationId);

                if (tourist != null && accommodation != null)
                {
                    comments.Add(new Comment
                    {
                        Id = id,
                        Tourist = tourist,
                        Accommodation = accommodation,
                        Text = text,
                        Rating = rating,
                        Status = status,
                        CreatedAt = createdAt
                    });
                }
            }

            return comments;
        }

        public static List<Comment> GetApprovedOnly()
        {
            return GetAll().Where(c => c.Status == CommentStatusEnum.Approved).ToList();
        }

        public static List<Comment> GetByManagerUsername(string managerUsername)
        {
            var accommodations = AccommodationRepository.GetByManagerUsername(managerUsername);
            var accommodationIds = accommodations.Select(a => a.Id).ToList();

            return GetAll().Where(c => accommodationIds.Contains(c.Accommodation.Id)).ToList();
        }

        public static List<Comment> GetPendingByManagerUsername(string managerUsername)
        {
            return GetByManagerUsername(managerUsername)
                .Where(c => c.Status == CommentStatusEnum.Pending)
                .ToList();
        }

        public static Comment GetById(Guid id)
        {
            return GetAll().FirstOrDefault(c => c.Id == id);
        }

        public static void Add(Comment comment)
        {
            bool fileExists = File.Exists(filePath);

            using (var sw = new StreamWriter(filePath, true))
            {
                if (!fileExists)
                    sw.WriteLine("Id;TouristUsername;AccommodationId;Text;Rating;Status;CreatedAt");

                string line = $"{comment.Id};{comment.Tourist.Username};{comment.Accommodation.Id};" +
                             $"{comment.Text};{comment.Rating};{comment.Status};" +
                             $"{comment.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")}";
                sw.WriteLine(line);
            }
        }

        public static void UpdateStatus(Guid commentId, CommentStatusEnum newStatus)
        {
            if (!File.Exists(filePath))
                return;

            var lines = File.ReadAllLines(filePath).ToList();

            for (int i = 1; i < lines.Count; i++)
            {
                var parts = lines[i].Split(';');
                if (parts.Length >= 6 && Guid.TryParse(parts[0], out Guid id) && id == commentId)
                {
                    parts[5] = newStatus.ToString();
                    lines[i] = string.Join(";", parts);
                    break;
                }
            }

            File.WriteAllLines(filePath, lines);
        }
    }
}