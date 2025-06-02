using System;

namespace Authorization_Login_Asp.Net.Core.Domain.ValueObjects
{
    /// <summary>
    /// کلاس مقدار موقعیت مکانی
    /// </summary>
    public class Location
    {
        /// <summary>
        /// عرض جغرافیایی
        /// </summary>
        public double Latitude { get; private set; }

        /// <summary>
        /// طول جغرافیایی
        /// </summary>
        public double Longitude { get; private set; }

        /// <summary>
        /// کشور
        /// </summary>
        public string Country { get; private set; }

        /// <summary>
        /// شهر
        /// </summary>
        public string City { get; private set; }

        /// <summary>
        /// آدرس کامل
        /// </summary>
        public string Address { get; private set; }

        /// <summary>
        /// کد پستی
        /// </summary>
        public string PostalCode { get; private set; }

        /// <summary>
        /// سازنده
        /// </summary>
        public Location(
            double latitude,
            double longitude,
            string country,
            string city,
            string address = null,
            string postalCode = null)
        {
            if (latitude < -90 || latitude > 90)
                throw new ArgumentException("عرض جغرافیایی باید بین -90 و 90 باشد", nameof(latitude));
            if (longitude < -180 || longitude > 180)
                throw new ArgumentException("طول جغرافیایی باید بین -180 و 180 باشد", nameof(longitude));
            if (string.IsNullOrWhiteSpace(country))
                throw new ArgumentException("کشور نمی‌تواند خالی باشد", nameof(country));
            if (string.IsNullOrWhiteSpace(city))
                throw new ArgumentException("شهر نمی‌تواند خالی باشد", nameof(city));

            Latitude = latitude;
            Longitude = longitude;
            Country = country.Trim();
            City = city.Trim();
            Address = address?.Trim();
            PostalCode = postalCode?.Trim();
        }

        /// <summary>
        /// محاسبه فاصله با موقعیت دیگر (به کیلومتر)
        /// </summary>
        public double DistanceTo(Location other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            const double EarthRadiusKm = 6371;
            var dLat = ToRadians(other.Latitude - Latitude);
            var dLon = ToRadians(other.Longitude - Longitude);
            var lat1 = ToRadians(Latitude);
            var lat2 = ToRadians(other.Latitude);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(lat1) * Math.Cos(lat2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return EarthRadiusKm * c;
        }

        private static double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }

        public override bool Equals(object obj)
        {
            if (obj is Location other)
            {
                return Latitude.Equals(other.Latitude) &&
                       Longitude.Equals(other.Longitude) &&
                       Country.Equals(other.Country, StringComparison.OrdinalIgnoreCase) &&
                       City.Equals(other.City, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Latitude, Longitude, Country.ToLowerInvariant(), City.ToLowerInvariant());
        }

        public override string ToString()
        {
            return $"{City}, {Country} ({Latitude}, {Longitude})";
        }
    }
} 