#region license
// DataProfiler
// Copyright 2013 Dale Newman
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//  
//      http://www.apache.org/licenses/LICENSE-2.0
//  
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion
using Autofac;
using Pipeline;
using Pipeline.Configuration;
using Pipeline.Contracts;
using Pipeline.Nulls;
using Pipeline.Provider.Ado;
using Pipeline.Provider.Excel;
using Pipeline.Provider.File;

namespace DataProfiles.Autofac.Modules {
    public class EntityInputModule : EntityModule {

        public EntityInputModule(Root root) : base(root) { }

        public override void LoadEntity(ContainerBuilder builder, Process process, Entity entity) {
            builder.Register<IRead>(ctx => {
                var input = ctx.ResolveNamed<InputContext>(entity.Key);

                switch (input.Connection.Provider) {
                    case "internal":
                        return new DataSetEntityReader(input);
                    case "file":
                        return new DelimitedFileReader(input);
                    case "excel":
                        return new ExcelReader(input);
                    case "mysql":
                    case "postgresql":
                    case "sqlite":
                    case "sqlserver":
                        return new AdoInputReader(input, input.InputFields, ctx.ResolveNamed<IConnectionFactory>(input.Connection.Key));
                    default:
                        return new NullEntityReader();
                }
            }).Named<IRead>(entity.Key);

        }
    }
}