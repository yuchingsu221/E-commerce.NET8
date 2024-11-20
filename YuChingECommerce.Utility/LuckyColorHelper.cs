using System;

namespace YuChingECommerce.Utility
{
    public static class LuckyColorHelper
    {
        public static string GetZodiacSign(DateTime birthDate)
        {
            int month = birthDate.Month;
            int day = birthDate.Day;

            return month switch
            {
                1 => day <= 19 ? "Capricorn" : "Aquarius",
                2 => day <= 18 ? "Aquarius" : "Pisces",
                3 => day <= 20 ? "Pisces" : "Aries",
                4 => day <= 19 ? "Aries" : "Taurus",
                5 => day <= 20 ? "Taurus" : "Gemini",
                6 => day <= 20 ? "Gemini" : "Cancer",
                7 => day <= 22 ? "Cancer" : "Leo",
                8 => day <= 22 ? "Leo" : "Virgo",
                9 => day <= 22 ? "Virgo" : "Libra",
                10 => day <= 22 ? "Libra" : "Scorpio",
                11 => day <= 21 ? "Scorpio" : "Sagittarius",
                12 => day <= 21 ? "Sagittarius" : "Capricorn",
                _ => "Unknown"
            };
        }

        public static string GetLuckyColor(string zodiacSign, DateTime date)
        {
            // 今年的幸運色邏輯
            string[] colors = { "Red", "Blue", "Green", "Yellow", "Purple", "Orange" };
            int year = date.Year;
            int month = date.Month;

            // 假設不同星座有對應顏色
            var zodiacColors = new Dictionary<string, string>
            {
                { "Aries", "Red" },
                { "Taurus", "Green" },
                { "Gemini", "Yellow" },
                { "Cancer", "White" },
                { "Leo", "Gold" },
                { "Virgo", "Blue" },
                { "Libra", "Pink" },
                { "Scorpio", "Black" },
                { "Sagittarius", "Purple" },
                { "Capricorn", "Brown" },
                { "Aquarius", "Sky Blue" },
                { "Pisces", "Sea Green" }
            };

            if (zodiacColors.ContainsKey(zodiacSign))
            {
                return zodiacColors[zodiacSign];
            }

            // 預設顏色
            return colors[(year + month) % colors.Length];
        }
    }
}
