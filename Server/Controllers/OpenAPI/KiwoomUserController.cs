﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using ShareInvest.Mappers;
using ShareInvest.Models.OpenAPI;
using ShareInvest.Server.Data;

namespace ShareInvest.Server.Controllers.OpenAPI;

public class KiwoomUserController : KiwoomController
{
    [ApiExplorerSettings(GroupName = "user"),
     HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] KiwoomUser user)
    {
        if (context.KiwoomUsers is not null &&
            string.IsNullOrEmpty(user.Key) is false)
        {
            for (int i = 0; i < user.Accounts?.Length; i++)
            {
                if (user.Accounts[i].Length < 0xA)
                    continue;

                var tuple = await context.KiwoomUsers.FindAsync(user.Key, user.Accounts[i]);

                if (tuple != null)
                    service.SetValuesOfColumn(tuple,
                                              new KiwoomUser
                                              {
                                                  IsAdministrator = tuple.IsAdministrator,
                                                  AccNo = user.Accounts[i],
                                                  Id = user.Id,
                                                  IsNotMock = user.IsNotMock,
                                                  Key = user.Key,
                                                  Name = user.Name,
                                                  NumberOfAccounts = user.NumberOfAccounts,
                                                  Accounts = user.Accounts
                                              });
                else
                {
                    user.AccNo = user.Accounts[i];

                    context.KiwoomUsers.Add(user);
                }
            }
            return Ok(context.SaveChanges());
        }
        LogWarning(user.Key);

        return NoContent();
    }
    [ApiExplorerSettings(GroupName = "user"),
     HttpPut]
    public async Task<IActionResult> PutAsync([FromBody] KiwoomUser user)
    {
        if (string.IsNullOrEmpty(user.Key) is false &&
            context.KiwoomUsers != null)
        {
            foreach (var account in from o in context.KiwoomUsers.AsNoTracking()
                                    where user.Key.Equals(o.Key)
                                    select o.AccNo)
            {
                var model = await context.KiwoomUsers.FindAsync(user.Key,
                                                                account);
                if (model != null)
                    model.IsAdministrator = user.IsAdministrator;
            }
            return Ok(context.SaveChanges());
        }
        LogWarning(user.Key);

        return NoContent();
    }
    [ApiExplorerSettings(GroupName = "user"),
     HttpGet]
    public async Task<IActionResult> GetAsync([FromQuery] string key)
    {
        if (context.KiwoomUsers != null)
        {
            var dao = context.KiwoomUsers.AsNoTracking();

            if (await dao.AnyAsync(o => key.Equals(o.Key) && o.IsAdministrator))
            {
                foreach (var user in from o in dao
                                     where key.Equals(o.Key)
                                     select o)
                {
                    logger.LogInformation("{ } is administrator.", key);

                    return Ok(user);
                }
            }
        }
        LogWarning(key);

        return NoContent();
    }
    public KiwoomUserController(CoreContext context,
                                IPropertyService service,
                                ILogger<KiwoomUserController> logger)
    {
        this.context = context;
        this.service = service;
        this.logger = logger;
    }
    void LogWarning(string? arg)
    {
        logger.LogWarning("problem with the parameters sent by the { }.", arg);
    }
    readonly CoreContext context;
    readonly IPropertyService service;
    readonly ILogger<KiwoomUserController> logger;
}