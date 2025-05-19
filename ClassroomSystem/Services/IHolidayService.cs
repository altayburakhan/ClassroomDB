using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClassroomSystem.Services
{
    public interface IHolidayService
    {
        Task<IEnumerable<DateTime>> GetHolidaysAsync(DateTime startDate, DateTime endDate);
        Task<bool> IsHolidayAsync(DateTime date);
        Task<List<DateTime>> GetHolidaysForYearAsync(int year);
        Task<List<DateTime>> GetHolidaysForDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<string> GetHolidayNameAsync(DateTime date);
    }
} 