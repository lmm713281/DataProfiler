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
using Autofac;
using Transformalize.Configuration;
using Transformalize.Context;
using Transformalize.Contracts;
using Transformalize.Nulls;
using Transformalize.Provider.Internal;
using Transformalize.Providers.Ado;
using Transformalize.Providers.Excel;
using Transformalize.Providers.File;

namespace DataProfiler.Autofac.Modules {
    public class EntityInputModule : EntityModule {

        public EntityInputModule(Process process) : base(process) { }

        public override void LoadEntity(ContainerBuilder builder, Process process, Entity entity) {
            builder.Register<IRead>(ctx => {
                var input = ctx.ResolveNamed<InputContext>(entity.Key);
                var rowFactory = ctx.ResolveNamed<IRowFactory>(entity.Key, new NamedParameter("capacity", input.RowCapacity));

                switch (input.Connection.Provider) {
                    case "internal":
                        return new InternalReader(input, rowFactory);
                    case "file":
                        return new DelimitedFileReader(input, rowFactory);
                    case "excel":
                        return new ExcelReader(input, rowFactory);
                    case "mysql":
                    case "postgresql":
                    case "sqlite":
                    case "sqlserver":
                        return new AdoInputReader(input, input.InputFields, ctx.ResolveNamed<IConnectionFactory>(input.Connection.Key), rowFactory);
                    default:
                        return new NullReader(input);
                }
            }).Named<IRead>(entity.Key);

        }
    }
}