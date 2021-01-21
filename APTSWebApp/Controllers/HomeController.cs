using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using APTSWebApp.API;
using Microsoft.Extensions.Configuration;
using System.Data;
using APTSWebApp.Helpers;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using APTSWebApp.Models;
using System.Collections;
using Microsoft.EntityFrameworkCore;

namespace APTSWebApp.Controllers
{
    //[Route("[controller]/[action]")]
    public class HomeController : Controller
    {
        private readonly APTS_RZA_Context _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public HomeController(APTS_RZA_Context context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        [HttpGet]
        public int CheckUserAuthorization()
        {
            var department = _configuration.GetSection("userSettings").GetSection("adminGroupAD").Value;

            return _httpContextAccessor.HttpContext.User.IsInRole(department) ? 1 : 0;
        }

        [HttpPost]
        public async Task<JObject[]> GetMonDataAsync([FromBody] object data)
        {
            DateTime? startDate;
            DateTime? endDate;
            var isArchiveMode = false;

            var definition = new { sDate = "", eDate = "", viewTsRZA = "", viewTsOIC = "" };
            var dataList = JsonConvert.DeserializeAnonymousType(data.ToString(), definition);
            var viewTsRza = Convert.ToBoolean(dataList.viewTsRZA);
            var viewTsOic = Convert.ToBoolean(dataList.viewTsOIC);

            if (dataList.sDate.Length > 0 || dataList.eDate.Length > 0)
            {
                isArchiveMode = true;
            }
            if (dataList.sDate.Length > 0)
                startDate = DateTime.Parse(dataList.sDate);
            else
                startDate = (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue;

            if (dataList.eDate.Length > 0)
                endDate = DateTime.Parse(dataList.eDate);
            else
                endDate = (DateTime)System.Data.SqlTypes.SqlDateTime.MaxValue;

            var listObjects = new List<JObject>();
            var listAllTs = await _context.OicTs.Where(ts => !ts.IsRemoved).AsNoTracking().ToListAsync();
            var listReceivedValues = await _context.ReceivedTsvalues.Where(v => !v.IsRemoved).AsNoTracking().ToListAsync();

            List<OicTs> listTs;
            List<OicTs> listStatusTs;

            switch (viewTsRza)
            {
                case false when !viewTsOic:
                    return new JObject[] { };
                case false:
                    listTs = listAllTs
                        .Where(ts => !ts.IsStatusTs && ts.IsOicTs)
                        .ToList();
                    listStatusTs = listAllTs
                        .Where(ts => ts.IsStatusTs && ts.IsOicTs)
                        .ToList();
                    break;
                default:
                    {
                        if (!viewTsOic)
                        {
                            listTs = listAllTs.Where(ts => !ts.IsStatusTs && !ts.IsOicTs).ToList();
                            listStatusTs = listAllTs.Where(ts => ts.IsStatusTs && !ts.IsOicTs).ToList();
                        }
                        else
                        {
                            listTs = listAllTs.Where(ts => !ts.IsStatusTs).ToList();
                            listStatusTs = listAllTs.Where(ts => ts.IsStatusTs).ToList();
                        }
                        break;
                    }
            }

            var listReceivedTsValues = listReceivedValues.Where(ts =>
                listTs.Any(item => item.Id == ts.OicTsid)
                    && ts.Val == 1
                    && ts.Dt.Date >= startDate.Value.Date
                    && ts.Dt.Date <= endDate.Value.Date
                ).ToList();

            var listReceivedStatusTsValues = listReceivedValues.Where(ts =>
                listStatusTs
                    .Any(item => item.Id == ts.OicTsid)
                    && ts.Dt.Date >= startDate.Value.Date
                    && ts.Dt.Date <= endDate.Value.Date
                ).ToList();

            var summaryList = listReceivedTsValues
                .Concat(listReceivedStatusTsValues)
                .OrderByDescending(ts => ts.Dt)
                .ToList();

            summaryList = !isArchiveMode
                ?
                summaryList.Take(100).ToList()
                :
                summaryList.Take(10000).ToList();

            var devs = await _context.Devices
                .Where(d => !d.IsRemoved)
                .AsNoTracking()
                .ToListAsync();

            var po = await _context.PowerObjects
                .Where(o => !o.IsRemoved)
                .Include(o => o.PowerObjectDevices)
                .AsNoTracking()
                .ToListAsync();

            foreach (var dev in devs)
            {
                if (listAllTs.Any(item => item.DeviceShifr == dev.Shifr))
                {
                    var tsListOfDevices = listAllTs.Where(i => i.DeviceShifr == dev.Shifr).Select(i => i.Id).ToList();
                    var listReceivedValOfDev = summaryList.Where(ts => tsListOfDevices.Contains(ts.OicTsid)).ToList();
                    var listOfListsValWithDiffTime = new List<List<ReceivedTsvalues>>();

                    foreach (var ts in listReceivedValOfDev)
                    {
                        if (!listOfListsValWithDiffTime
                            .SelectMany(list => list.Select(item => item.Id))
                            .ToList()
                            .Contains(ts.Id)
                        )
                        {
                            listOfListsValWithDiffTime.Add(listReceivedValOfDev.Where(item =>
                                    item.Dt.Date == ts.Dt.Date
                                    && item.Dt.Hour == ts.Dt.Hour
                                    && item.Dt.Minute == ts.Dt.Minute)
                                .OrderByDescending(item => item.Dt)
                                .ToList()
                            );
                        }
                    }

                    foreach (var subList in listOfListsValWithDiffTime)
                    {
                        var jObject = JObject.FromObject(new
                        {
                            dt = subList.FirstOrDefault()?.Dt.ToString("dd.MM.yyyy HH:mm:ss"),
                            objName = po
                                .FirstOrDefault(item =>
                                    item.PowerObjectDevices.Any(i => i.DeviceShifr == dev.Shifr)
                                    )
                                ?.Name,
                            //primaryName = _context.PrimaryEquipments.Where(item => item.PrimaryEquipmentDevices.Where(p => p.DeviceShifr == dev.Shifr).Count() > 0).Select(item => item.Name).FirstOrDefault(),
                            tsName = listAllTs
                                .FirstOrDefault(ts => ts.Id == subList.FirstOrDefault()?.OicTsid)?.Name,
                            devName = dev.Name,
                            isOicTs = listAllTs
                                .Where(ts => ts.IsOicTs)
                                .Select(ts => ts.Id)
                                .ToList()
                                .Intersect(subList.Select(rTs => rTs.OicTsid))
                                .Any(),
                            tsList = subList
                                .Select(item => new
                                {
                                    key = item.Id,
                                    oicId = listAllTs.FirstOrDefault(ts => ts.Id == item.OicTsid)?.OicId,
                                    dt = item.Dt.ToString("dd.MM.yyyy HH:mm:ss"),
                                    label = listAllTs
                                        .Where(ts => ts.Id == item.OicTsid)
                                        .Select(ts => ts.Name)
                                        .FirstOrDefault(),
                                    comment = listAllTs
                                        .Where(ts => ts.Id == subList.FirstOrDefault()?.OicTsid)
                                        .Select(ts => ts.Comment)
                                        .FirstOrDefault(),
                                    value = item.Val
                                })
                        });
                        listObjects.Add(jObject);
                    }
                }
            }
            return listObjects.OrderByDescending(o =>
                DateTime.ParseExact(o.GetValue("dt")?.ToString() ?? string.Empty,
                    "dd.MM.yyyy HH:mm:ss", null)
                )
                .ToArray();
        }
        [HttpGet]
        public string GetUserName()
        {
            return _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Name).Value;
        }
    }
}