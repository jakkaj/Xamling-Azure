namespace Xamling.Azure.Portable.Contract
{
    public interface IBlobRepoFactory
    {
        IBlobRepo GetBlobRepo(string containerName);
    }
}
