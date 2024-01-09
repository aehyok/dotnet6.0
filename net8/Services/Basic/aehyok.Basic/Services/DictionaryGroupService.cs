﻿using aehyok.Basic.Domains;
using aehyok.EntityFrameworkCore.Repository;
using aehyok.Infrastructure;
using Ardalis.Specification;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aehyok.Basic.Services
{
    /// <summary>
    /// 字典分组服务
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="mapper"></param>
    public class DictionaryGroupService(DbContext dbContext, IMapper mapper) : ServiceBase<DictionaryGroup>(dbContext, mapper), IDictionaryGroupService, IScopedDependency
    {
    }
}
