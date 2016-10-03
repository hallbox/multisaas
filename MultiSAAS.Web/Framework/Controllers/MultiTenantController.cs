﻿namespace MultiSAAS.Web.Framework.Controllers
{
  using System.Linq;
  using System.Web.Mvc;
  using System.Web.Routing;
  using Data;

  public abstract class MultiTenantController : Controller
  {
    // ReSharper disable once InconsistentNaming
    public readonly TenantContext db;

    protected MultiTenantController()
    {
      db = new TenantContext();
    }

    public string TenantCode
    {
      get { return Session["TenantCode"]?.ToString(); }
      set { Session["TenantCode"] = value; }
    }

    public ActionResult FormActionResult(object model, object id)
    {
      if (id != null && model == null)
      {
        return HttpNotFound();
      }
      if (Request.IsAjaxRequest())
      {
        return PartialView("Form", model);
      }
      return View("Form", model);
    }

    public ActionResult Grid(object model)
    {
      return View("Grid", model);
    }

    protected override void Initialize(RequestContext requestContext)
    {
      base.Initialize(requestContext);
      var ctx = requestContext;
      TenantCode = (string) ctx.RouteData.Values["tenant"];
      if (string.IsNullOrEmpty(TenantCode))
      {
        var fullAddress = ctx.HttpContext.Request.Headers["Host"].Split('.');
        if (fullAddress.Length >= 2)
        {
          ctx.RouteData.Values["tenant"] = fullAddress[0];
          TenantCode = fullAddress[0];
        }
      }
      if (!string.IsNullOrEmpty(TenantCode) && TenantCode != Constants.Default.TenantCode)
      {
        var cs = db.Tenants.Where(u => u.TenantCode == TenantCode).Select(u => u.ConnectionString).FirstOrDefault();
        if (!string.IsNullOrEmpty(cs))
        {
          db.Database.Connection.ConnectionString = cs;
        }
      }
    }

    protected override void Dispose(bool disposing)
    {
      db.Dispose();
      base.Dispose(disposing);
    }
  }
}