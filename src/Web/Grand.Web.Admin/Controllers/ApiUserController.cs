﻿using Grand.Business.Core.Interfaces.Common.Security;
using Grand.Business.Core.Interfaces.Customers;
using Grand.Domain.Permissions;
using Grand.SharedKernel.Extensions;
using Grand.Web.Admin.Extensions.Mapping;
using Grand.Web.Admin.Models.Customers;
using Grand.Web.Common.DataSource;
using Grand.Web.Common.Security.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Grand.Web.Admin.Controllers;

[PermissionAuthorize(PermissionSystemName.Maintenance)]
public class ApiUserController : BaseAdminController
{
    private readonly IEncryptionService _encryptionService;
    private readonly IUserApiService _userApiService;

    public ApiUserController(IUserApiService userApiService, IEncryptionService encryptionService)
    {
        _userApiService = userApiService;
        _encryptionService = encryptionService;
    }

    protected (string hashpassword, string privatekey) HashPassword(string password)
    {
        var pk = CommonHelper.GenerateRandomDigitCode(24);
        return (_encryptionService.EncryptText(password, pk), pk);
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> List(string email, DataSourceRequest command)
    {
        var model = (await _userApiService.GetUsers(email, command.Page - 1, command.PageSize))
            .Select(x => x.ToModel())
            .ToList();
        var gridModel = new DataSourceResult {
            Data = model,
            Total = model.Count
        };
        return Json(gridModel);
    }

    [HttpPost]
    public async Task<IActionResult> Update(UserApiModel model)
    {
        var userapi = await _userApiService.GetUserById(model.Id);
        if (userapi == null)
            throw new ArgumentException("No user api found with the specified id");
        if (ModelState.IsValid)
        {
            userapi = model.ToEntity(userapi);
            if (!string.IsNullOrEmpty(model.Password))
            {
                var keys = HashPassword(model.Password);
                userapi.Password = keys.hashpassword;
                userapi.PrivateKey = keys.privatekey;
                userapi.Token = Guid.NewGuid().ToString();
            }

            await _userApiService.UpdateUserApi(userapi);
            return new JsonResult("");
        }

        return ErrorForKendoGridJson(ModelState);
    }

    [HttpPost]
    public async Task<IActionResult> Insert(UserApiCreateModel model)
    {
        if (ModelState.IsValid)
        {
            var userapi = model.ToEntity();
            var keys = HashPassword(model.Password);
            userapi.Password = keys.hashpassword;
            userapi.PrivateKey = keys.privatekey;
            userapi.Token = Guid.NewGuid().ToString();
            await _userApiService.InsertUserApi(userapi);
            return new JsonResult("");
        }

        return ErrorForKendoGridJson(ModelState);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
        var userapi = await _userApiService.GetUserById(id);
        if (userapi == null)
            throw new ArgumentException("No user found with the specified id");
        if (ModelState.IsValid)
        {
            await _userApiService.DeleteUserApi(userapi);
            return new JsonResult("");
        }

        return ErrorForKendoGridJson(ModelState);
    }
}