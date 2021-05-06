namespace LoginLibrary.Services
{
    public interface ILoginService
    {
        //bool RegisterAccount(string data);
        //bool CheckData(string data);
        //bool ChangePassword(string data);
        byte[] Encrypt(string data);
        string Decrypt(byte[] data);
    }
}
