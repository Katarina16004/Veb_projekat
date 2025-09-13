using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Veb_Projekat.Models;

namespace Veb_Projekat.Services
{
    public class ArrangementService
    {
        public static List<Arrangement> SearchArrangements(string name = "", string location = "", string type = "", string transport = "", DateTime? startDateFrom = null,
                 DateTime? startDateTo = null, DateTime? endDateFrom = null, DateTime? endDateTo = null)
        {
            var all = Repositories.ArrangementRepository.GetAll();
            var result = new List<Arrangement>();

            foreach (var arr in all)
            {
                bool match = true;

                if (!string.IsNullOrEmpty(name) && !arr.Name.Equals(name))
                    match = false;

                if (!string.IsNullOrEmpty(location) && !arr.Location.Equals(location))
                    match = false;

                if (!string.IsNullOrEmpty(type) && !arr.Type.ToString().Equals(type))
                    match = false;

                if (!string.IsNullOrEmpty(transport) && !arr.Transport.ToString().Equals(transport))
                    match = false;

                if (startDateFrom.HasValue && arr.StartDate < startDateFrom.Value)
                    match = false;

                if (startDateTo.HasValue && arr.StartDate > startDateTo.Value)
                    match = false;

                if (endDateFrom.HasValue && arr.EndDate < endDateFrom.Value)
                    match = false;

                if (endDateTo.HasValue && arr.EndDate > endDateTo.Value)
                    match = false;

                if (match)
                    result.Add(arr);
            }

            return result;
        }

        public static Arrangement GetDetails(int id)
        {
            return Repositories.ArrangementRepository.GetById(id);
        }
    }
}