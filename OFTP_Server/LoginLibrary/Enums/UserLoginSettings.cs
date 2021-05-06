using System;

namespace LoginLibrary.Enums
{
    [Flags]
    public enum UserLoginSettings
    {
        LoggedIn,
        BadPassword
    }
}
