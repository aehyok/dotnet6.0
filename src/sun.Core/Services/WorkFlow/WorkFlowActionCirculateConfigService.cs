﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using sun.Core.Domains.WorkFlow;
using sun.EntityFrameworkCore.Repository;
using sun.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sun.Core.Services.WorkFlow
{
    public class WorkFlowActionCirculateConfigService(DbContext dbContext, IMapper mapper) : ServiceBase<WorkFlowActionCirculateConfig>(dbContext, mapper), IWorkFlowActionCirculateConfigService, IScopedDependency
    {

    }
}
