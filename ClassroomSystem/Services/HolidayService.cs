using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace ClassroomSystem.Services
{
    public class HolidayService : IHolidayService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly string _countryCode;

        public HolidayService(IConfiguration configuration, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _baseUrl = configuration["HolidayApi:BaseUrl"];
            _countryCode = configuration["HolidayApi:CountryCode"];
        }

        public async Task<bool> IsHolidayAsync(DateTime date)
        {
            var holidays = await GetHolidaysForYearAsync(date.Year);
            return holidays.Contains(date.Date);
        }

        public async Task<List<DateTime>> GetHolidaysForYearAsync(int year)
        {
            var allHolidays = new HashSet<DateTime>();

            // Get holidays from the API
            try
            {
                var url = $"{_baseUrl}/{year}/{_countryCode}";
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var apiHolidays = JsonSerializer.Deserialize<List<HolidayResponse>>(content);
                foreach (var holiday in apiHolidays)
                {
                    allHolidays.Add(DateTime.Parse(holiday.Date).Date);
                }
            }
            catch (Exception)
            {
                // Log the error but continue to get Turkish holidays
            }

            // Add Turkish holidays
            var turkishHolidays = TurkishHolidays.GetHolidaysForYear(year);
            foreach (var holiday in turkishHolidays)
            {
                allHolidays.Add(holiday.Date.Date);
            }

            return allHolidays.ToList();
        }

        public async Task<List<DateTime>> GetHolidaysForDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var holidays = new List<DateTime>();
            var currentYear = startDate.Year;

            while (currentYear <= endDate.Year)
            {
                var yearHolidays = await GetHolidaysForYearAsync(currentYear);
                holidays.AddRange(yearHolidays.Where(d => d >= startDate && d <= endDate));
                currentYear++;
            }

            return holidays;
        }

        public async Task<string> GetHolidayNameAsync(DateTime date)
        {
            try
            {
                // Check Turkish holidays first
                var turkishHolidays = TurkishHolidays.GetHolidaysForYear(date.Year);
                var turkishHoliday = turkishHolidays.FirstOrDefault(h => h.Date.Date == date.Date);
                if (turkishHoliday != default)
                {
                    return turkishHoliday.Name;
                }

                // If not found in Turkish holidays, check API
                var response = await _httpClient.GetAsync($"{_baseUrl}/{date.Year}/{_countryCode}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var holidays = JsonSerializer.Deserialize<List<HolidayResponse>>(content);
                    var holiday = holidays?.FirstOrDefault(h => DateTime.Parse(h.Date).Date == date.Date);
                    return holiday?.Name ?? "Unknown Holiday";
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> IsHolidayInRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var holidays = await GetHolidaysForDateRangeAsync(startDate, endDate);
                return holidays.Any();
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<DateTime[]> GetHolidaysInRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var holidays = await GetHolidaysForDateRangeAsync(startDate, endDate);
                return holidays.ToArray();
            }
            catch (Exception)
            {
                return Array.Empty<DateTime>();
            }
        }

        public async Task<IEnumerable<DateTime>> GetHolidaysAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                return await GetHolidaysForDateRangeAsync(startDate, endDate);
            }
            catch (Exception)
            {
                return Enumerable.Empty<DateTime>();
            }
        }

        private class HolidayResponse
        {
            public string Date { get; set; }
            public string Name { get; set; }
            public string LocalName { get; set; }
            public string CountryCode { get; set; }
            public bool Fixed { get; set; }
            public bool Global { get; set; }
            public string[] Counties { get; set; }
            public int? LaunchYear { get; set; }
            public string[] Types { get; set; }
        }
    }
} 