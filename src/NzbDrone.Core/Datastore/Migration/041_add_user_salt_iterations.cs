using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(041)]
    public class add_user_salt_iterations : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Users").AddColumn("Salt").AsString().Nullable();
            Alter.Table("Users").AddColumn("Iterations").AsInt32().NotNullable().WithDefaultValue(0);
        }
    }
}
