using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Veb_Projekat.Models;

namespace Veb_Projekat.Repositories
{
    public class CommentRepository
    {
        private static string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data/Comments.txt");

        public static List<Comment> GetAll()
        {
            var comments = new List<Comment>();

            if (!File.Exists(filePath))
                return comments;

            var lines = File.ReadAllLines(filePath);

            for (int i = 1; i < lines.Length; i++)
            {
                var parts = lines[i].Split(';');
                if (parts.Length < 4)
                    continue;

                string touristUsername = parts[0];
                string accommodationName = parts[1];
                string text = parts[2];
                int rating = int.TryParse(parts[3], out int r) ? r : 0;

                var tourist = UserRepository.GetByUsername(touristUsername);
                var accommodation = AccommodationRepository.GetByName(accommodationName);

                if (tourist != null && accommodation != null)
                {
                    comments.Add(new Comment
                    {
                        Tourist = tourist,
                        Accommodation = accommodation,
                        Text = text,
                        Rating = rating
                    });
                }
            }

            return comments;
        }

        public static void Add(Comment comment)
        {
            bool fileExists = File.Exists(filePath);

            using (var sw = new StreamWriter(filePath, true))
            {
                if (!fileExists)
                    sw.WriteLine("TouristUsername;AccommodationName;Text;Rating");

                string line = $"{comment.Tourist.Username};{comment.Accommodation.Name};{comment.Text};{comment.Rating}";
                sw.WriteLine(line);
            }
        }
    }
}