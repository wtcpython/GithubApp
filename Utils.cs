using System;

namespace GithubApp
{
    public static class Utils
    {
        public static string FormatNumber(double number)
        {
            if (number < 1000) return number.ToString();
            else
            {
                if (number < 1000000) return (number / 1000).ToString("0.0") + "K";
                else return (number / 1000000).ToString("0.0") + "M";
            }
        }

        public static string FormatFileSize(long size)
        {
            if (size < 1024) return size.ToString() + "B";
            else if (size < 1024 * 1024) return (size / 1024).ToString("0.0") + "KiB";
            else if (size < 1024 * 1024 * 1024) return (size / (1024 * 1024)).ToString("0.0") + "MiB";
            else return (size / (1024 * 1024 * 1024)).ToString("0.0") + "GiB";
        }

        public static string FormatTimeSpan(DateTime time)
        {
            TimeSpan span = DateTime.Now - time;

            if (span.TotalMinutes < 1.0) return "just now";
            else if (span.TotalMinutes < 60.0) return $"{(int)span.TotalMinutes} minutes ago";
            else if (span.TotalHours < 24.0) return $"{(int)span.TotalHours} hours ago";
            else if (span.TotalDays < 7.0) return $"{(int)span.TotalDays} days ago";
            else if (span.TotalDays < 7.0 * 2) return $"last week";
            else if (span.TotalDays < 30.0) return $"{(int)(span.TotalDays / 7)} weeks ago";
            else if (span.TotalDays < 30.0 * 2) return $"last month";
            else if (span.TotalDays < 365.0) return $"{(int)(span.TotalDays / 30)} months ago";
            else if (span.TotalDays < 365.0 * 2) return $"last year";
            else return $"{(int)(span.TotalDays / 365)} years ago";
        }
    }
}

