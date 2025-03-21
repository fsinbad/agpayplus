﻿using AGooday.AgPay.Domain.Interfaces;
using AGooday.AgPay.Domain.Models;
using AGooday.AgPay.Infrastructure.Context;

namespace AGooday.AgPay.Infrastructure.Repositories
{
    public class SysRoleEntRelaRepository : AgPayRepository<SysRoleEntRela>, ISysRoleEntRelaRepository
    {
        public SysRoleEntRelaRepository(AgPayDbContext context)
            : base(context)
        {
        }

        public void RemoveByRoleId(string roleId)
        {
            var entities = DbSet.Where(w => w.RoleId.Equals(roleId));
            DbSet.RemoveRange(entities);
        }
    }
}
