namespace Emissary
{
    public interface IDatabaseAccessor
    {
        string ExecuteCommand(string commandText, string databasePath);
    }
}