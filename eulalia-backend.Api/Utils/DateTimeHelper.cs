namespace eulalia_backend.Api.Utils
{
    public class DateTimeHelper
    {
        public static DateTime EnsureUtc(DateTime dt)
        {
            if (dt.Kind == DateTimeKind.Unspecified)
                return DateTime.SpecifyKind(dt, DateTimeKind.Utc);
            return dt;
        }

        public static DateTime? EnsureUtc(DateTime? dt)
        {
            if (!dt.HasValue) return null;
            if (dt.Value.Kind == DateTimeKind.Unspecified)
                return DateTime.SpecifyKind(dt.Value, DateTimeKind.Utc);
            return dt;
        }
    }
}
