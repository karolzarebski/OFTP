namespace LoginLibrary.Services
{
    public interface ILoginService
    {
        bool RegisterAccount(string data);
        bool CheckData(string data);
        bool ChangePassword(string data);
    }
}
