using System;

namespace IsdemBot.Managers
{
    public static class RandomTime
    {
        public static string Create(DateTime dateTime)
        {
            var random = new Random();

            var hour = random.Next(10, 16);
            var minute = random.Next(0, 60);

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
