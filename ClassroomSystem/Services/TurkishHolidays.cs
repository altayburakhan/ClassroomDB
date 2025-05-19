using System;
using System.Collections.Generic;

namespace ClassroomSystem.Services
{
    public static class TurkishHolidays
    {
        public static List<(DateTime Date, string Name, bool IsReligious)> GetHolidaysForYear(int year)
        {
            var holidays = new List<(DateTime Date, string Name, bool IsReligious)>();

            // National Holidays (Fixed dates)
            holidays.Add((new DateTime(year, 1, 1), "Yılbaşı", false));
            holidays.Add((new DateTime(year, 4, 23), "Ulusal Egemenlik ve Çocuk Bayramı", false));
            holidays.Add((new DateTime(year, 5, 1), "Emek ve Dayanışma Günü", false));
            holidays.Add((new DateTime(year, 5, 19), "Atatürk'ü Anma, Gençlik ve Spor Bayramı", false));
            holidays.Add((new DateTime(year, 7, 15), "Demokrasi ve Milli Birlik Günü", false));
            holidays.Add((new DateTime(year, 8, 30), "Zafer Bayramı", false));
            holidays.Add((new DateTime(year, 10, 29), "Cumhuriyet Bayramı", false));
            
            // Add religious holidays for the specified year
            var religiousHolidays = GetReligiousHolidaysForYear(year);
            holidays.AddRange(religiousHolidays);

            return holidays;
        }

        private static List<(DateTime Date, string Name, bool IsReligious)> GetReligiousHolidaysForYear(int year)
        {
            var holidays = new List<(DateTime Date, string Name, bool IsReligious)>();
            
            if (year == 2024)
            {
                // Ramazan Bayramı (3 days) - 2024 dates
                holidays.Add((new DateTime(2024, 4, 10), "Ramazan Bayramı Arifesi (Yarım Gün)", true));
                holidays.Add((new DateTime(2024, 4, 11), "Ramazan Bayramı 1. Gün", true));
                holidays.Add((new DateTime(2024, 4, 12), "Ramazan Bayramı 2. Gün", true));
                holidays.Add((new DateTime(2024, 4, 13), "Ramazan Bayramı 3. Gün", true));

                // Kurban Bayramı (4 days) - 2024 dates
                holidays.Add((new DateTime(2024, 6, 16), "Kurban Bayramı Arifesi (Yarım Gün)", true));
                holidays.Add((new DateTime(2024, 6, 17), "Kurban Bayramı 1. Gün", true));
                holidays.Add((new DateTime(2024, 6, 18), "Kurban Bayramı 2. Gün", true));
                holidays.Add((new DateTime(2024, 6, 19), "Kurban Bayramı 3. Gün", true));
                holidays.Add((new DateTime(2024, 6, 20), "Kurban Bayramı 4. Gün", true));
            }
            else if (year == 2025)
            {
                // Ramazan Bayramı (3 days) - 2025 dates
                holidays.Add((new DateTime(2025, 3, 30), "Ramazan Bayramı Arifesi (Yarım Gün)", true));
                holidays.Add((new DateTime(2025, 3, 31), "Ramazan Bayramı 1. Gün", true));
                holidays.Add((new DateTime(2025, 4, 1), "Ramazan Bayramı 2. Gün", true));
                holidays.Add((new DateTime(2025, 4, 2), "Ramazan Bayramı 3. Gün", true));

                // Kurban Bayramı (4 days) - 2025 dates
                holidays.Add((new DateTime(2025, 6, 6), "Kurban Bayramı Arifesi (Yarım Gün)", true));
                holidays.Add((new DateTime(2025, 6, 7), "Kurban Bayramı 1. Gün", true));
                holidays.Add((new DateTime(2025, 6, 8), "Kurban Bayramı 2. Gün", true));
                holidays.Add((new DateTime(2025, 6, 9), "Kurban Bayramı 3. Gün", true));
                holidays.Add((new DateTime(2025, 6, 10), "Kurban Bayramı 4. Gün", true));
            }

            return holidays;
        }
    }
} 