using System;
using System.Linq;
using Veb_Projekat.Models;
using Veb_Projekat.Repositories;

namespace Veb_Projekat.Services
{
    public class CommentService
    {
        public static bool CanLeaveComment(int accommodationId, string reservationId, string touristUsername, out Reservation reservation, out Accommodation accommodation, out string errorMessage)
        {
            errorMessage = "";
            accommodation = null;

            reservation = ReservationRepository.GetAll().FirstOrDefault(r => r.Id.ToString() == reservationId &&
                                     r.Tourist.Username == touristUsername);

            if (reservation == null)
            {
                errorMessage = "Invalid reservation.";
                return false;
            }

            if (reservation.SelectedArrangement.EndDate > DateTime.Now)
            {
                errorMessage = "You can only comment on completed trips.";
                return false;
            }

            accommodation = reservation.SelectedArrangement.Accommodations
                .FirstOrDefault(a => a.Id == accommodationId);

            if (accommodation == null)
            {
                errorMessage = "Accommodation not found.";
                return false;
            }

            return true;
        }

        public static bool CreateComment(int accommodationId, string touristUsername, string commentText, int rating, out string errorMessage)
        {
            errorMessage = "";

            if (string.IsNullOrEmpty(commentText) || rating < 1 || rating > 5)
            {
                errorMessage = "Please provide valid comment and rating (1-5)";
                return false;
            }

            var tourist = UserRepository.GetByUsername(touristUsername);
            var accommodation = AccommodationRepository.GetAllGrouped().SelectMany(kvp => kvp.Value).FirstOrDefault(a => a.Id == accommodationId);

            if (accommodation == null)
            {
                errorMessage = "Accommodation not found.";
                return false;
            }

            var comment = new Comment
            {
                Tourist = tourist,
                Accommodation = accommodation,
                Text = commentText,
                Rating = rating
            };

            CommentRepository.Add(comment);
            return true;
        }

        public static bool ValidateCommentData(string commentText, int rating)
        {
            return !string.IsNullOrEmpty(commentText) && commentText.Trim().Length >= 10 && rating >= 1 && rating <= 5;
        }
    }
}