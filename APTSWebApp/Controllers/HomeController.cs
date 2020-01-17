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
        //[ResponseCache(CacheProfileName = "Default30")]
        public JObject[] GetData([FromBody]object data)
        {
            DateTime? startDate;
            DateTime? endDate;
            bool isArchiveMode = false;

            IList<DateTime?> dateList = JsonConvert.DeserializeObject<IList<DateTime?>>(data.ToString(), new JsonSerializerSettings
            {
                DateFormatString = "dd.MM.yyyy"
            });

            if (dateList.Count > 0)
                isArchiveMode = true;

            // Добавляем по дню к датам начала и окончания по причине того, что в объект "data" поступает дата на вчерашний день (специфика работы react datepicker).
            startDate =
                dateList?.FirstOrDefault() != null
                ?
                dateList?.FirstOrDefault().GetValueOrDefault().AddDays(1)
                :
                (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue;
            endDate =
                dateList?.LastOrDefault() != null
                ?
                dateList?.LastOrDefault().GetValueOrDefault().AddDays(1)
                :
                (DateTime)System.Data.SqlTypes.SqlDateTime.MaxValue;

            List<JObject> listObjects = new List<JObject>();

            List<ReceivedTsvalues> listReceivedValues = _context.ReceivedTsvalues.OrderByDescending(ts => ts.Id).ToList();            

            List<OicTs> listTs = _context.OicTs.Where(ts => !ts.IsRemoved && !ts.IsStatusTs).ToList();

            List<OicTs> listStatusTs = _context.OicTs.Where(ts => !ts.IsRemoved && ts.IsStatusTs).ToList();

            List<ReceivedTsvalues> listReceivedTsValues = listReceivedValues.Where(ts =>
                listTs.Select(item => item.Id).Contains(ts.OicTsid)
                && ts.Val == 1
                && ts.Dt.Date >= startDate.Value.Date
                && ts.Dt.Date <= endDate.Value.Date
                ).ToList();

            List<ReceivedTsvalues> listReceivedStatusTsValues = listReceivedValues.Where(ts =>
                listStatusTs.Select(item => item.Id).Contains(ts.OicTsid)
                && ts.Dt.Date >= startDate.Value.Date
                && ts.Dt.Date <= endDate.Value.Date
                ).ToList();

            List<ReceivedTsvalues> summaryList = listReceivedTsValues.Concat(listReceivedStatusTsValues).ToList();

            summaryList = !isArchiveMode
                ?
                summaryList.Take(100).ToList()
                :
                summaryList.Take(10000).ToList();

            foreach (var dev in _context.Devices.Where(d => !d.IsRemoved))
            {
                if (_context.OicTs.Where(item => item.DeviceShifr == dev.Shifr).Count() > 0)
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
                                && item.Dt.Minute == ts.Dt.Minute).OrderByDescending(item => item.Dt).ToList() ?? new List<ReceivedTsvalues> { ts });
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