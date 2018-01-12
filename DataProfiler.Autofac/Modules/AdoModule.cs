#region license
// Data Profiler
// Copyright � 2013-2018 Dale Newman
//  
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//   
//       http://www.apache.org/licenses/LICENSE-2.0
//   
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion
using System.Linq;
using Autofac;
using Transformalize.Configuration;
using Transformalize.Contracts;
using Transformalize.Extensions;
using Transformalize.Providers.Ado;
using Transformalize.Providers.MySql;
using Transformalize.Providers.PostgreSql;
using Transformalize.Providers.SqlServer;
using Transformalize.Providers.SQLite;

namespace DataProfiler.Autofac.Modules {
    public class AdoModule : Module {
        private readonly Process _process;

        public AdoModule(Process process)
        {
            _process = process;
        }

        protected override void Load(ContainerBuilder builder) {
            foreach (var connection in _process.Connections.Where(c => c.Provider.In("sqlserver", "mysql", "postgresql", "sqlite"))) {

                // Connection Factory
                builder.Register<IConnectionFactory>(ctx => {
                    switch (connection.Provider) {
                        case "sqlserver":
                            return new SqlServerConnectionFactory(connection);
                        case "mysql":
                            return new MySqlConnectionFactory(connection);
                        case "postgresql":
                            return new PostgreSqlConnectionFactory(connection);
                        case "sqlite":
                            return new SqLiteConnectionFactory(connection);
                        default:
                            return new NullConnectionFactory();
                    }
                }).Named<IConnectionFactory>(connection.Key).InstancePerLifetimeScope();

                // Schema Reader
                builder.Register<ISchemaReader>(ctx => {
                    var factory = ctx.ResolveNamed<IConnectionFactory>(connection.Key);
                    return new AdoSchemaReader(ctx.ResolveNamed<IConnectionContext>(connection.Key), factory);
                }).Named<ISchemaReader>(connection.Key);

            }
        }

    }
}