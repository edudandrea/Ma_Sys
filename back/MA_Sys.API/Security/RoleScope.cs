namespace MA_Sys.API.Security
{
    public static class RoleScope
    {
        public static bool IsSuperAdmin(string? role)
            => string.Equals(role?.Trim(), "SuperAdmin", StringComparison.OrdinalIgnoreCase);

        public static bool IsAdmin(string? role)
            => string.Equals(role?.Trim(), "Admin", StringComparison.OrdinalIgnoreCase);

        public static bool IsFederacao(string? role)
            => string.Equals(role?.Trim(), "Federacao", StringComparison.OrdinalIgnoreCase);

        public static bool IsAcademia(string? role)
            => string.Equals(role?.Trim(), "Academia", StringComparison.OrdinalIgnoreCase);

        public static bool CanViewAll(string? role) => IsSuperAdmin(role);
    }
}
