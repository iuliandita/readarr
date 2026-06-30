using System;
using System.Globalization;

namespace NzbDrone.Common.Http
{
    public class TooManyRequestsException : HttpException
    {
        public TimeSpan RetryAfter { get; private set; }

        public TooManyRequestsException(HttpRequest request, HttpResponse response)
            : base(request, response)
        {
            if (response.Headers.ContainsKey("Retry-After"))
            {
                var retryAfter = response.Headers["Retry-After"].ToString();

                // Retry-After is normally an integer number of seconds, but some
                // servers (e.g. Discord) send fractional seconds like "0.5".
                if (double.TryParse(retryAfter, NumberStyles.Float, CultureInfo.InvariantCulture, out var seconds))
                {
                    RetryAfter = TimeSpan.FromSeconds(seconds);
                }
                else if (DateTime.TryParse(retryAfter, out var date))
                {
                    RetryAfter = date.ToUniversalTime() - DateTime.UtcNow;
                }
            }
        }
    }
}
