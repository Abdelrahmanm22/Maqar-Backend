namespace Maqar.Domain.Authorization
{
    /// <summary>
    /// Every permission in the system. Values must be compile-time constants so they can be used
    /// in <c>[Authorize(Policy = ...)]</c>. Add a matching entry to <see cref="PermissionCatalog"/>
    /// whenever a constant is added here.
    /// </summary>
    public static class Permissions
    {
        public static class Users
        {
            public const string View = "Users.View";
            public const string Create = "Users.Create";
            public const string Edit = "Users.Edit";
            public const string Delete = "Users.Delete";
            public const string ToggleActive = "Users.ToggleActive";
            public const string ResetPassword = "Users.ResetPassword";
            public const string AssignRoles = "Users.AssignRoles";
            public const string ManagePermissions = "Users.ManagePermissions";
        }

        public static class Roles
        {
            public const string View = "Roles.View";
            public const string Create = "Roles.Create";
            public const string Edit = "Roles.Edit";
            public const string Delete = "Roles.Delete";
            public const string ManagePermissions = "Roles.ManagePermissions";
        }

        public static class Properties
        {
            public const string View = "Properties.View";
            public const string Create = "Properties.Create";
            public const string Edit = "Properties.Edit";
            public const string Delete = "Properties.Delete";
            public const string Publish = "Properties.Publish";
            public const string Feature = "Properties.Feature";

            /// <summary>
            /// Lifts the ownership scope. Without it every property query and action is limited to the
            /// listings the user created (brokers); with it, the other Properties grants apply to all.
            /// Also unlocks general contact messages and other brokers' inquiries.
            /// </summary>
            public const string ViewAll = "Properties.ViewAll";
        }

        public static class Areas
        {
            public const string View = "Areas.View";
            public const string Create = "Areas.Create";
            public const string Edit = "Areas.Edit";
            public const string Delete = "Areas.Delete";
        }

        public static class Features
        {
            public const string View = "Features.View";
            public const string Create = "Features.Create";
            public const string Edit = "Features.Edit";
            public const string Delete = "Features.Delete";
        }

        public static class ContactMessages
        {
            public const string View = "ContactMessages.View";
            public const string ChangeStatus = "ContactMessages.ChangeStatus";
            public const string Delete = "ContactMessages.Delete";
        }
    }
}
