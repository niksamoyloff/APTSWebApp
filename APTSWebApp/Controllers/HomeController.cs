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
            string department = _configuration.GetSection("userSettings").GetSection("adminGroupAD").Value;

            if (_httpContextAccessor.HttpContext.User.IsInRole(department))
                return 1;
            return 0;
        }

        [HttpPost]
        public JObject[] GetData([FromBody]object data)
        {
            DateTime? startDate;
            DateTime? endDate;
            bool isArchiveMode = false;

            var definition = new { sDate = "", eDate = "", viewTsRZA = "", viewTsOIC = "" };
            var dataList = JsonConvert.DeserializeAnonymousType(data.ToString(), definition);
            var viewTsRZA = Convert.ToBoolean(dataList.viewTsRZA);
            var viewTsOIC = Convert.ToBoolean(dataList.viewTsOIC);

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

            List<JObject> listObjects = new List<JObject>();

            List<ReceivedTsvalues> listReceivedValues = _context.ReceivedTsvalues.ToList();

            List<OicTs> listTs = new List<OicTs>();

            List<OicTs> listStatusTs = new List<OicTs>();

            if (!viewTsRZA && !viewTsOIC)
                return new JObject[] { };
            else if (!viewTsRZA)
            {
                listTs = _context.OicTs.Where(ts => !ts.IsRemoved && !ts.IsStatusTs && ts.IsOicTs).ToList();
                listStatusTs = _context.OicTs.Where(ts => !ts.IsRemoved && ts.IsStatusTs && ts.IsOicTs).ToList();
            }
            else if (!viewTsOIC)
            {
                listTs = _context.OicTs.Where(ts => !ts.IsRemoved && !ts.IsStatusTs && !ts.IsOicTs).ToList();
                listStatusTs = _context.OicTs.Where(ts => !ts.IsRemoved && ts.IsStatusTs && !ts.IsOicTs).ToList();
            }
            else
            {
                listTs = _context.OicTs.Where(ts => !ts.IsRemoved && !ts.IsStatusTs).ToList();
                listStatusTs = _context.OicTs.Where(ts => !ts.IsRemoved && ts.IsStatusTs).ToList();
            }
            
            List<ReceivedTsvalues> listReceivedTsValues = listReceivedValues.Where(ts =>
                listTs.Where(item => item.Id == ts.OicTsid).Any()
                && ts.Val == 1
                && ts.Dt.Date >= startDate.Value.Date
                && ts.Dt.Date <= endDate.Value.Date
                ).ToList();

            List<ReceivedTsvalues> listReceivedStatusTsValues = listReceivedValues.Where(ts =>
                listStatusTs.Where(item => item.Id == ts.OicTsid).Any()
                && ts.Dt.Date >= startDate.Value.Date
                && ts.Dt.Date <= endDate.Value.Date
                ).ToList();

            List<ReceivedTsvalues> summaryList = listReceivedTsValues.Concat(listReceivedStatusTsValues).OrderByDescending(ts => ts.Dt).ToList();

            summaryList = !isArchiveMode
                ?
                summaryList.Take(100).ToList()
                :
                summaryList.Take(10000).ToList();

            foreach (var dev in _context.Devices.Where(d => !d.IsRemoved))
            {
                if (_context.OicTs.Where(item => item.DeviceShifr == dev.Shifr).Any())
                {
                    var tsListOfDevices = _context.OicTs.Where(i => i.DeviceShifr == dev.Shifr).Select(i => i.Id).ToList();

                    List<ReceivedTsvalues> listReceivedValofDev = summaryList.Where(ts => tsListOfDevices.Contains(ts.OicTsid)).ToList();

                    List<List<ReceivedTsvalues>> listOfListsValWithDiffTime = new List<List<ReceivedTsvalues>>();

                    foreach (var ts in listReceivedValofDev)
                    {
                        if (!listOfListsValWithDiffTime.SelectMany(list => list.Select(item => item.Id)).ToList().Contains(ts.Id))
                        {
                            listOfListsValWithDiffTime.Add(listReceivedValofDev.Where(item =>
                                item.Dt.Date == ts.Dt.Date
                                && item.Dt.Hour == ts.Dt.Hour
                                && item.Dt.Minute == ts.Dt.Minute)
                                .OrderByDescending(item => item.Dt).ToList() ?? new List<ReceivedTsvalues> { ts });
                        }
                    }

                    foreach (var subList in listOfListsValWithDiffTime)
                    {
                        JObject jObject = JObject.FromObject(new
                        {
                            dt = subList.FirstOrDefault().Dt.ToString("dd.MM.yyyy HH:mm:ss"),
                            objName = _context.PowerObjects.Where(item => item.PowerObjectDevices.Where(i => i.DeviceShifr == dev.Shifr).Count() > 0).Select(o => o.Name).FirstOrDefault(),
                            //primaryName = _context.PrimaryEquipments.Where(item => item.PrimaryEquipmentDevices.Where(p => p.DeviceShifr == dev.Shifr).Count() > 0).Select(item => item.Name).FirstOrDefault(),
                            tsName = _context.OicTs.Where(ts => ts.Id == subList.FirstOrDefault().OicTsid).Select(ts => ts.Name).FirstOrDefault(),                            
                            devName = dev.Name,
                            isOicTs = _context.OicTs.Where(ts => ts.IsOicTs).Select(ts => ts.Id).ToList().Intersect(subList.Select(rTS => rTS.OicTsid)).Any() ? true : false,
                            tsList = subList
                                .Select(item => new
                                {
                                    key = item.Id,
                                    oicId = _context.OicTs.Where(ts => ts.Id == item.OicTsid).FirstOrDefault().OicId,
                                    dt = item.Dt.ToString("dd.MM.yyyy HH:mm:ss"),
                                    label = _context.OicTs.Where(ts => ts.Id == item.OicTsid).Select(ts => ts.Name).FirstOrDefault(),
                                    comment = _context.OicTs.Where(ts => ts.Id == subList.FirstOrDefault().OicTsid).Select(ts => ts.Comment).FirstOrDefault(),
                                    value = item.Val
                                })
                        });
                        listObjects.Add(jObject);
                    }
                }
            }
            return listObjects?.OrderByDescending(o => DateTime.ParseExact(o.GetValue("dt").ToString(), "dd.MM.yyyy HH:mm:ss", null)).ToArray();
        }
        [HttpGet]
        public string GetUserName()
        {
            return _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Name).Value;
        }
    }
}