namespace SQLitePCL.Extensions
{
    public static class ISQLiteStatementExtensions
    {
        public static void ResetAndClearBindings(this ISQLiteStatement statement)
        {
            statement.Reset();
            statement.ClearBindings();
        }
    }
}
