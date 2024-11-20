using System;
using System.Collections.Generic;

namespace YuChingECommerce.Utility
{
    public static class ZodiacHelper
    {
        // 計算星座
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
                6 => day <= 21 ? "Gemini" : "Cancer",
                7 => day <= 22 ? "Cancer" : "Leo",
                8 => day <= 22 ? "Leo" : "Virgo",
                9 => day <= 22 ? "Virgo" : "Libra",
                10 => day <= 23 ? "Libra" : "Scorpio",
                11 => day <= 22 ? "Scorpio" : "Sagittarius",
                12 => day <= 21 ? "Sagittarius" : "Capricorn",
                _ => "Unknown"
            };
        }

        // 計算幸運色
        public static string GetLuckyColor(string zodiacSign, DateTime date)
        {
            // 假設每個星座有固定的幸運色
            var zodiacColors = new Dictionary<string, string>
            {
                { "Aries", "Red" },
                { "Taurus", "Green" },
                { "Gemini", "Yellow" },
                { "Cancer", "Silver" },
                { "Leo", "Gold" },
                { "Virgo", "Blue" },
                { "Libra", "Pink" },
                { "Scorpio", "Black" },
                { "Sagittarius", "Purple" },
                { "Capricorn", "Brown" },
                { "Aquarius", "Sky Blue" },
                { "Pisces", "Sea Green" }
            };

            return zodiacColors.ContainsKey(zodiacSign) ? zodiacColors[zodiacSign] : "White";
        }

        // 計算今日運勢（簡單模擬）
        public static string GetDailyHoroscope(string zodiacSign)
        {
            var horoscopes = new Dictionary<string, string>
            {
                { "Aries", "今天是展現自我的好日子！" },
                { "Taurus", "耐心將為你帶來回報。" },
                { "Gemini", "與朋友的交流將帶來新的機會。" },
                { "Cancer", "關注家庭，幸福圍繞你。" },
                { "Leo", "自信將助你達成目標。" },
                { "Virgo", "細節決定成敗，注意小事物。" },
                { "Libra", "平衡是關鍵，找到你的節奏。" },
                { "Scorpio", "勇敢面對挑戰，成功屬於你。" },
                { "Sagittarius", "探索新事物，擴展你的視野。" },
                { "Capricorn", "努力工作將獲得認可。" },
                { "Aquarius", "創意將引領你前進。" },
                { "Pisces", "相信直覺，做出正確決定。" }
            };

            return horoscopes.ContainsKey(zodiacSign) ? horoscopes[zodiacSign] : "今天是美好的一天！";
        }
    }
}
