using System;

namespace IsdemBot.Managers
{
    public static class RandomTime
    {
        static readonly Random _random = new();
        public static string Create(DateTime dateTime)
        {
            var hour = _random.Next(10, 16);
            var minute = _random.Next(0, 60);

            var strMinute = minute.ToString();

            if (minute < 10)
                strMinute = $"0{minute}";

            var date = dateTime.ToString("dd/MM/yyyy")
                .Replace("/", "")
                .Replace(".", "")
                .Replace("-", "");

            return string.Concat(date, hour, strMinute); ;
        }
    }
}
