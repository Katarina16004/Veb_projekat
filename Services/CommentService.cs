using System;
using System.Linq;
using Veb_Projekat.Models;
using Veb_Projekat.Models.Enums;
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
            var accommodation = AccommodationRepository.GetById(accommodationId);

            if (accommodation == null)
            {
                errorMessage = "Accommodation not found.";
                return false;
            }

            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                Tourist = tourist,
                Accommodation = accommodation,
                Text = commentText,
                Rating = rating,
                Status = CommentStatusEnum.Pending,
                CreatedAt = DateTime.Now
            };

            CommentRepository.Add(comment);
            return true;
        }

        public static bool ValidateCommentData(string commentText, int rating)
        {
            return !string.IsNullOrEmpty(commentText) && commentText.Trim().Length >= 10 && rating >= 1 && rating <= 5;
        }


        public static bool CanManageComment(Guid commentId, string managerUsername, out Comment comment, out string errorMessage)
        {
            errorMessage = "";
            comment = CommentRepository.GetById(commentId);

            if (comment == null)
            {
                errorMessage = "Comment not found.";
                return false;
            }

            if (!AccommodationService.CanManageAccommodation(comment.Accommodation, managerUsername))
            {
                errorMessage = "You can only manage comments for your own accommodations.";
                return false;
            }

            if (comment.Status != CommentStatusEnum.Pending)
            {
                errorMessage = "Only pending comments can be approved or rejected.";
                return false;
            }

            return true;
        }

        public static bool ApproveComment(Guid commentId, string managerUsername, out string errorMessage)
        {
            errorMessage = "";

            if (!CanManageComment(commentId, managerUsername, out Comment comment, out errorMessage))
                return false;

            try
            {
                CommentRepository.UpdateStatus(commentId, CommentStatusEnum.Approved);
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = "Error approving comment: " + ex.Message;
                return false;
            }
        }

        public static bool RejectComment(Guid commentId, string managerUsername, out string errorMessage)
        {
            errorMessage = "";

            if (!CanManageComment(commentId, managerUsername, out Comment comment, out errorMessage))
                return false;

            try
            {
                CommentRepository.UpdateStatus(commentId, CommentStatusEnum.Rejected);
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = "Error rejecting comment: " + ex.Message;
                return false;
            }
        }
    }
}