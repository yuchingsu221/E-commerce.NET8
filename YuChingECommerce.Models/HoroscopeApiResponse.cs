using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace YuChingECommerce.Models {
    public class HoroscopeApiResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("data")]
        public HoroscopeData Data { get; set; }
    }

    public class HoroscopeData
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("time")]
        public string Time { get; set; }

        [JsonPropertyName("todo")]
        public HoroscopeTodo Todo { get; set; }

        [JsonPropertyName("fortune")]
        public HoroscopeFortune Fortune { get; set; }

        [JsonPropertyName("shortcomment")]
        public string ShortComment { get; set; }

        [JsonPropertyName("fortunetext")]
        public HoroscopeFortuneText FortuneText { get; set; }

        [JsonPropertyName("luckynumber")]
        public string LuckyNumber { get; set; }

        [JsonPropertyName("luckycolor")]
        public string LuckyColor { get; set; }

        [JsonPropertyName("luckyconstellation")]
        public string LuckyConstellation { get; set; }

        [JsonPropertyName("index")]
        public HoroscopeIndex Index { get; set; }
    }

    public class HoroscopeTodo
    {
        [JsonPropertyName("yi")]
        public string Yi { get; set; }

        [JsonPropertyName("ji")]
        public string Ji { get; set; }
    }

    public class HoroscopeFortune
    {
        [JsonPropertyName("all")]
        public int All { get; set; }

        [JsonPropertyName("love")]
        public int Love { get; set; }

        [JsonPropertyName("work")]
        public int Work { get; set; }

        [JsonPropertyName("money")]
        public int Money { get; set; }

        [JsonPropertyName("health")]
        public int Health { get; set; }
    }

    public class HoroscopeFortuneText
    {
        [JsonPropertyName("all")]
        public string All { get; set; }

        [JsonPropertyName("love")]
        public string Love { get; set; }

        [JsonPropertyName("work")]
        public string Work { get; set; }

        [JsonPropertyName("money")]
        public string Money { get; set; }

        [JsonPropertyName("health")]
        public string Health { get; set; }
    }

    public class HoroscopeIndex
    {
        [JsonPropertyName("all")]
        public string All { get; set; }

        [JsonPropertyName("love")]
        public string Love { get; set; }

        [JsonPropertyName("work")]
        public string Work { get; set; }

        [JsonPropertyName("money")]
        public string Money { get; set; }

        [JsonPropertyName("health")]
        public string Health { get; set; }
    }
}
