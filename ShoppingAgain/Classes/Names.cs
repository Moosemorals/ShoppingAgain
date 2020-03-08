namespace ShoppingAgain.Classes
{
    public class Names
    {
        public const string AdminIndex = "AdminIndex";
        public const string AdminAddUser = "AdminAddUser";
        public const string AdminDelUser = "AdminRemoveUser";

        public const string ItemChangeName = "ItemChangeName";
        public const string ItemCreate = "ItemCreate";
        public const string ItemDelete = "ItemDelete";
        public const string ItemNextState = "ItemNextState";

        public const string ListCreate = "ListCreate";
        public const string ListDelete = "ListDelete";
        public const string ListDetails = "ListDetails";
        public const string ListEdit = "ListEdit";
        public const string ListIndex = "ListIndex";
        public const string ListShare = "ListShare";

        public const string Message = "message";
        public const string TopIndex = "TopIndex";

        public const string UserDenied = "UserDenied";
        public const string UserDeniedPath = "/Auth/Denied";
        public const string UserLogin = "UserLogin";
        public const string UserLoginPath = "/Auth/Login";
        public const string UserLogout = "UserLogout";
        public const string UserLogoutPath = "/Auth/Logout";

        public const string UserIndex = "UserIndex";
        public const string UserFriends = "UserFriends";
        public const string UserShareList = "UserShareList";

        public const string UserChangePassword = "UserChangePassword";
        public const string UserChangePasswordPath = "ChangePassword";

        public const string ListId = "listId";
        public const string RoleUser = "User";
        public const string RoleAdmin = "Admin";


        public static readonly object IsLoggedIn = new object();
        public static readonly object CurrentList = new object();
        public static readonly object Lists = new object();
        public static readonly object User = new object();
    }
}
