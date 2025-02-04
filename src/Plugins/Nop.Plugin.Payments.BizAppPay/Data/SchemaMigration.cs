using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Plugin.Payments.BizAppPay.Domain;

namespace Nop.Plugin.Payments.BizAppPay.Data
{
    [NopMigration("2022/07/17 12:00:00", "Payments.BizAppPay base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        #region Methods

        /// <summary>
        /// Collect the UP migration expressions
        /// </summary>
        public override void Up()
        {
            Create.TableFor<BizAppPayTransaction>();
        }

        #endregion
    }
}