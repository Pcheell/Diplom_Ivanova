namespace Ivanova_UchitDn.Core
{
    public interface IBaseCommands
    {
        ReloadCommand ReloadSelf { get; set; }
        ReloadCommand Reload { get; }
        InsertCommand InsertSelf { get; set; }
        InsertCommand Insert { get; }
        EditCommand EditSelf { get; set; }
        EditCommand Edit { get; }
    }
}
