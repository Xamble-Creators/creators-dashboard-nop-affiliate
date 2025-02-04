using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Plugin.BizApp.SalesPage.Domain;

namespace Nop.Plugin.BizApp.SalesPage.Data
{
    [NopMigration("2023/02/20 12:00:00", "BizApp.SalesPage base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        #region Methods

        /// <summary>
        /// Collect the UP migration expressions
        /// </summary>
        public override void Up()
        {
            Create.TableFor<SalesPageRecord>();
            Create.TableFor<SalesPageOrder>();
            Create.TableFor<SalesPageOrderItem>();
            Create.TableFor<SalesPageVisit>();
        }

        #endregion
    }
}