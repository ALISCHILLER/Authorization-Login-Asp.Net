namespace Authorization_Login_Asp.Net.API.Authorization
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string Permission { get; }

        public PermissionRequirement(string permission)
        {   
            Permission = permission;
        }
    }
}
}