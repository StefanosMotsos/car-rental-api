namespace CarRentalApp.Security
{
    public interface IEncryptionUtil
    {
        string Encrypt(string plainText);
        bool IsValidPassword(string plainText, string cipherText);
    }
}
