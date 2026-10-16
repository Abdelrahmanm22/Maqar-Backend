using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Maqar.Domain.Authorization
{
    public sealed record PermissionDescriptor(string Value, string DisplayName, string? Description = null);

    public sealed record PermissionModule(
        string Key,
        string DisplayName,
        int Order,
        IReadOnlyList<PermissionDescriptor> Permissions);

    /// <summary>
    /// Display metadata for every permission, grouped by module. Drives the permission picker UI and
    /// validates anything posted back from it.
    /// </summary>
    public static class PermissionCatalog
    {
        public static IReadOnlyList<PermissionModule> Modules { get; } = new List<PermissionModule>
        {
            new("Users", "Users", 1, new[]
            {
                new PermissionDescriptor(Permissions.Users.View, "View users"),
                new PermissionDescriptor(Permissions.Users.Create, "Create users"),
                new PermissionDescriptor(Permissions.Users.Edit, "Edit users"),
                new PermissionDescriptor(Permissions.Users.Delete, "Delete users"),
                new PermissionDescriptor(Permissions.Users.ToggleActive, "Activate / deactivate users"),
                new PermissionDescriptor(Permissions.Users.ResetPassword, "Reset user passwords"),
                new PermissionDescriptor(Permissions.Users.AssignRoles, "Assign roles to users"),
                new PermissionDescriptor(Permissions.Users.ManagePermissions, "Manage direct permissions",
                    "Grant a user extra permissions on top of the ones their roles give them."),
            }),
            new("Roles", "Roles", 2, new[]
            {
                new PermissionDescriptor(Permissions.Roles.View, "View roles"),
                new PermissionDescriptor(Permissions.Roles.Create, "Create roles"),
                new PermissionDescriptor(Permissions.Roles.Edit, "Edit roles"),
                new PermissionDescriptor(Permissions.Roles.Delete, "Delete roles"),
                new PermissionDescriptor(Permissions.Roles.ManagePermissions, "Manage role permissions",
                    "Change which permissions a role grants."),
            }),
            // Appended rather than inserted, so Users/Roles keep their Order.
            new("Properties", "Properties", 3, new[]
            {
                new PermissionDescriptor(Permissions.Properties.View, "View properties",
                    "See every listing, including drafts and archived ones."),
                new PermissionDescriptor(Permissions.Properties.Create, "Create properties"),
                new PermissionDescriptor(Permissions.Properties.Edit, "Edit properties",
                    "Change a listing's details, price and images."),
                new PermissionDescriptor(Permissions.Properties.Delete, "Delete properties",
                    "Remove a listing. It is soft-deleted and disappears from the site."),
                new PermissionDescriptor(Permissions.Properties.Publish, "Change property status",
                    "Publish, unpublish, or mark a listing sold, rented or archived. Only published listings appear on the public site."),
                new PermissionDescriptor(Permissions.Properties.Feature, "Feature properties",
                    "Choose which listings appear in the featured section of the home page."),
            }),
            new("Areas", "Areas", 4, new[]
            {
                new PermissionDescriptor(Permissions.Areas.View, "View areas"),
                new PermissionDescriptor(Permissions.Areas.Create, "Create areas"),
                new PermissionDescriptor(Permissions.Areas.Edit, "Edit areas"),
                new PermissionDescriptor(Permissions.Areas.Delete, "Delete areas",
                    "Only an area with no listings can be deleted."),
            }),
            new("Features", "Features", 5, new[]
            {
                new PermissionDescriptor(Permissions.Features.View, "View features",
                    "See the amenities catalogue (elevator, pool, …) and pick features on a listing."),
                new PermissionDescriptor(Permissions.Features.Create, "Create features"),
                new PermissionDescriptor(Permissions.Features.Edit, "Edit features"),
                new PermissionDescriptor(Permissions.Features.Delete, "Delete features",
                    "Also removes the feature from every listing that has it."),
            }),
            new("ContactMessages", "Contact Messages", 6, new[]
            {
                new PermissionDescriptor(Permissions.ContactMessages.View, "View contact messages",
                    "Read messages and listing inquiries sent from the public site."),
                new PermissionDescriptor(Permissions.ContactMessages.ChangeStatus, "Change message status",
                    "Mark a message read, replied or archived."),
                new PermissionDescriptor(Permissions.ContactMessages.Delete, "Delete contact messages"),
            }),
        }
        .OrderBy(m => m.Order)
        .ToList();

        public static IReadOnlyList<PermissionDescriptor> All { get; } =
            Modules.SelectMany(m => m.Permissions).ToList();

        public static int Count => All.Count;

        private static readonly Dictionary<string, PermissionDescriptor> ByValue =
            All.GroupBy(p => p.Value, StringComparer.Ordinal)
               .ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);

        public static bool IsValid(string? value) => value is not null && ByValue.ContainsKey(value);

        public static string DisplayName(string value) =>
            ByValue.TryGetValue(value, out var descriptor) ? descriptor.DisplayName : value;

        public static string? ModuleOf(string value) =>
            Modules.FirstOrDefault(m => m.Permissions.Any(p => p.Value == value))?.Key;

        /// <summary>Drops unknown and duplicate values. Every posted permission list must go through this.</summary>
        public static IReadOnlyList<string> Sanitize(IEnumerable<string>? values) =>
            (values ?? Enumerable.Empty<string>())
                .Where(IsValid)
                .Distinct(StringComparer.Ordinal)
                .ToList();

        /// <summary>
        /// Startup self-check: every constant declared in <see cref="Permissions"/> must appear
        /// exactly once here, and vice versa.
        /// </summary>
        public static IReadOnlyList<string> FindCatalogGaps()
        {
            var declared = typeof(Permissions).GetNestedTypes()
                .SelectMany(t => t.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy))
                .Where(f => f.IsLiteral && f.FieldType == typeof(string))
                .Select(f => (string)f.GetRawConstantValue()!)
                .ToList();

            var gaps = declared.Where(d => !IsValid(d))
                .Select(d => $"Missing from catalog: {d}")
                .ToList();

            gaps.AddRange(All.Select(p => p.Value)
                .Except(declared, StringComparer.Ordinal)
                .Select(v => $"Catalog entry has no constant: {v}"));

            gaps.AddRange(All.GroupBy(p => p.Value, StringComparer.Ordinal)
                .Where(g => g.Count() > 1)
                .Select(g => $"Duplicate catalog entry: {g.Key}"));

            return gaps;
        }
    }
}
