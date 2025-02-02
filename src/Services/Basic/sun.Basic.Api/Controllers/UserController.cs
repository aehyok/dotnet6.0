﻿using sun.Basic.Domains;
using sun.Basic.Dtos;
using sun.Basic.Dtos.Query;
using sun.Basic.Services;
using sun.EntityFrameworkCore.Repository;
using sun.Infrastructure.Exceptions;
using Ardalis.Specification;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using sun.Core.Dtos.Query;
using sun.Core.Domains;
using sun.Core.Dtos.Create;
using sun.Core.Dtos;
using sun.Core.Services;
using sun.Infrastructure.Enums;
using LinqKit;
using sun.Infrastructure.Utils;
using System.Linq;
using System.Data;

namespace sun.Basic.Api.Controllers
{
    /// <summary>
    /// 用户管理
    /// </summary>
    public class UserController(
        IUserService userService, IUserRoleService userRoleService) : BasicControllerBase
    {
        /// <summary>
        /// 获取用户列表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpGet("list")]
        public async Task<IPagedList<UserDto>> GetListAsync([FromQuery] UserQueryDto model)
        {
            //var spec = Specifications<User>.Create();

            //spec.Query.OrderByDescending(a => a.Id).Include(a => a.Roles);

            //if (!string.IsNullOrWhiteSpace(model.Keyword))
            //{
            //    spec.Query.Search(a => a.UserName, $"%{model.Keyword}%")
            //              .Search(a => a.Mobile, $"%{model.Keyword}%")
            //              .Search(a => a.NickName, $"%{model.Keyword}%")
            //              .Search(a => a.RealName, $"%{model.Keyword}%");
            //}


            var filter = PredicateBuilder.New<User>(true);
            var userRoleFilter = PredicateBuilder.New<UserRole>(true);


            if (!string.IsNullOrWhiteSpace(model.Keyword))
            {
                //filter.And(a => a.UserName.Contains(model.Keyword) || a.Mobile.Contains(model.Keyword) || a.NickName.Contains(model.Keyword) || a.RealName.Contains(model.Keyword));
                filter.Or(a => a.UserName.Contains(model.Keyword));
                filter.Or(a => a.Mobile.Contains(model.Keyword));
                filter.Or(a => a.NickName.Contains(model.Keyword));
                filter.Or(a => a.RealName.Contains(model.Keyword));
            }

            filter.And(a => a.UserName != "root");

            if (model.IsEnable.HasValue)
            {
                filter.And(a => a.IsEnable == model.IsEnable.Value);
                //spec.Query.Where(a => a.IsEnable == model.IsEnable.Value);
            }

            if (model.RoleId.HasValue && model.RoleId != 0)
            {
                userRoleFilter.And(item => item.RoleId == model.RoleId.Value);
                //spec.Query.Where(a => a.UserRoles.Any(c => c.RoleId == model.RoleId.Value));
            }

            if (model.RegionId.HasValue && model.RegionId != 0)
            {
                if (model.IncludeChilds)
                {
                    userRoleFilter.And(c => EF.Functions.Like(c.Region.IdSequences, $"%.{model.RegionId.Value}.%"));
                    //spec.Query.Where(a => a.UserRoles.Any(c => EF.Functions.Like(c.Region.IdSequences, $"%.{model.RegionId.Value}.%")));
                }
                else
                {
                    userRoleFilter.And(a => a.RegionId == model.RegionId.Value);
                    //spec.Query.Where(a => a.UserRoles.Any(c => c.RegionId == model.RegionId.Value));
                }
            }

            IQueryable<UserDto> query;
            if (model.RegionId.HasValue && model.RoleId != 0)
            {
                query = (from u in userService.GetExpandable().Where(filter)
                         join ur in userRoleService.GetExpandable().Where(userRoleFilter) on u.Id equals ur.UserId
                         select new UserDto
                         {
                             Id = u.Id,
                             UserName = u.UserName,
                             Mobile = u.Mobile,
                             NickName = u.NickName,
                             RealName = u.RealName,
                             IsEnable = u.IsEnable,
                         })
                        .Distinct()
                        .OrderByDescending(a => a.Id);
            } 
            else
            {
                query = (from u in userService.GetExpandable().Where(filter)
                         select new UserDto
                         {
                             Id = u.Id,
                             UserName = u.UserName,
                             Mobile = u.Mobile,
                             NickName = u.NickName,
                             RealName = u.RealName,
                             IsEnable = u.IsEnable,
                             Gender = u.Gender
                         })
                        .Distinct()
                        .OrderByDescending(a => a.Id);
            }


            var list = await query.ToPagedListAsync(model.Page, model.Limit);

            foreach(var item in list)
            {
                item.UserRoles = (from urs in userRoleService.GetExpandable()
                              where urs.UserId == item.Id
                              select new UserRoleDto
                              {
                                  Id = urs.Id,
                                  PlatformType = urs.Role.PlatformType,
                                  RoleId = urs.RoleId,
                                  RoleName = urs.Role.Name,
                                  RegionId = urs.RegionId,
                                  RegionName = urs.Region.Name
                              }).ToList();
                if (item.UserRoles is not null && item.UserRoles.Count > 0)
                    item.UserRoles.OrderBy(a => a.PlatformType);
            }
            return list;
        }

        /// <summary>
        /// 创建用户
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<long> PostAsync(CreateUserDto model)
        {
            if (model.UserName.IsNullOrEmpty())
                throw new ErrorCodeException(-1, "账号不能为空");
            if (model.Mobile.IsNullOrEmpty())
                throw new ErrorCodeException(-1, "手机号码不能为空");
            //if (model.UserRoles.IsNull())
            //    throw new ErrorCodeException(-1, "请为用户选择角色");

            var entity = this.Mapper.Map<User>(model);

            if (model.UserRoles != null && model.UserRoles.Count > 0)
            {
                var roles = new List<UserRole>();

                model.UserRoles.ForEach((item =>
                {
                    roles.Add(new UserRole
                    {
                        RoleId = item.RoleId,
                        UserId = entity.Id,
                        RegionId = item.RegionId
                    });
                }));

                await userRoleService.InsertAsync(roles);
            }

            //entity.UserRoles = model.Roles.Select(a => new UserRole
            //{
            //    RoleId = a.RoleId,
            //    UserId = entity.Id,
            //    RegionId = a.RegionId
            //}).ToList();

            //if (model.Departments.IsNotNull())// 插入新部门
            //    await userDepartmentService.InsertAsync(model.Departments.Select(a => new UserDepartment
            //    {
            //        UserId = entity.Id,
            //        RegionId = a.RegionId,
            //        DepartmentId = a.DepartmentId
            //    }).ToList());

            // 设置默认密码为手机号码后 6 位
            entity.Password = model.Mobile[^6..];
            await userService.InsertAsync(entity);
            return entity.Id;
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpDelete("{id}")]
        public async Task<StatusCodeResult> DeleteAsync(long id)
        {
            var entity = await userService.GetAsync(a => a.Id == id);
            if (entity is null)
            {
                throw new Exception("你要删除的用户不存在");
            }

            await userService.DeleteAsync(entity);
            return Ok();
        }

        /// <summary>
        /// 修改用户信息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<StatusCodeResult> PutAsync(long id, CreateUserDto model)
        {
            var entity = await userService.GetAsync(item => item.Id == id);
            if (entity is null)
            {
                throw new ErrorCodeException(-1, "你要修改的用户不存在");
            }

            entity = this.Mapper.Map(model, entity);

            var strategy = userService.GetDbContext.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(
            async () =>
            {
                using var trans = await userService.BeginTransactionAsync();

                try
                {
                    await userRoleService.BatchSoftDeleteAsync(a => a.UserId == id);

                    foreach (var item in model.UserRoles)
                    {
                        var userRole = new UserRole
                        {
                            RoleId = item.RoleId,
                            UserId = entity.Id,
                            RegionId = item.RegionId
                        };
                        await userRoleService.InsertAsync(userRole);
                    }

                    entity.UserRoles = null;

                    await userService.UpdateAsync(entity);

                    await trans.CommitAsync();
                }
                catch (Exception ex)
                {
                    await trans.RollbackAsync();
                    throw new Exception(ex.Message);
                }
            });
            return Ok();
        }

        /// <summary>
        /// 启用用户
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPut("enable/{id}")]
        public async Task<StatusCodeResult> EnableAsync(long id)
        {
            var entity = await userService.GetByIdAsync(id);
            if (entity == null)
            {
                throw new ErrorCodeException(-1, "你要启用的数据不存在");
            }

            entity.IsEnable = true;

            await userService.UpdateAsync(entity);

            return Ok();
        }

        /// <summary>
        /// 禁用用户
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPut("disable/{id}")]
        public async Task<StatusCodeResult> DisableAsync(long id)
        {
            var entity = await userService.GetByIdAsync(id);
            if (entity == null)
            {
                throw new ErrorCodeException(-1, "你要禁用的数据不存在");
            }

            entity.IsEnable = false;

            await userService.UpdateAsync(entity);

            return Ok();
        }

        /// <summary>
        /// 重置密码
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        //[HttpPut("reset/{id}")]
        //public async Task<StatusCodeResult> ResetPasswordAsync(long id)
        //{
        //    await userService.ResetPasswordAsync(id);
        //    return Ok();
        //}
    }
}
