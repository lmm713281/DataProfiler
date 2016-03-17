﻿#region license
// DataProfiler.Autofac
// Copyright 2013 Dale Newman
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
using System.Collections.Generic;
using Autofac;
using Pipeline;
using Pipeline.Contracts;
using Process = Pipeline.Configuration.Process;

namespace DataProfiler.Autofac.Modules {
    public class ProcessControlModule : Module {
        private readonly Process _process;

        public ProcessControlModule(Process process) {
            _process = process;
        }

        protected override void Load(ContainerBuilder builder) {

            if (!_process.Enabled)
                return;

            builder.Register<IProcessController>(ctx => {

                var pipelines = new List<IPipeline>();

                // entity-level pipelines
                foreach (var entity in _process.Entities) {
                    var pipeline = ctx.ResolveNamed<IPipeline>(entity.Key);

                    pipelines.Add(pipeline);
                    if (entity.Delete && _process.Mode != "init") {
                        pipeline.Register(ctx.ResolveNamed<IEntityDeleteHandler>(entity.Key));
                    }
                }

                // process-level pipeline
                pipelines.Add(ctx.ResolveNamed<IPipeline>(_process.Key));

                var context = new PipelineContext(ctx.Resolve<IPipelineLogger>(), _process);

                var controller = new ProcessController(pipelines, context);

                return controller;
            }).Named<IProcessController>(_process.Key);
        }


    }
}