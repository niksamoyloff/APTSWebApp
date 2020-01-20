using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using APTSWebApp.API;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using APTSWebApp.Models;
using Microsoft.Extensions.Caching.Memory;

namespace APTSWebApp.Controllers
{
    //[Route("[controller]/[action]")]
    public class AdminController : Controller
    {
        private readonly APTS_RZA_Context _context;
        private readonly IConfiguration _configuration;

        public AdminController(APTS_RZA_Context context, IConfiguration configuration, IMemoryCache memoryCache)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: Home
        [HttpGet]
        [ResponseCache(VaryByHeader = "User-Agent", Duration = 300)]
        public JObject[] GetTree()
        {
            List<JObject> list = new List<JObject>();

            foreach (var s in _context.PowerSystems.Where(item => !item.IsRemoved))
            {
                JObject jObject = JObject.FromObject(new
                {
                    key = s.Id,
                    label = s.Name,
                    nodes =
                    from o in _context.PowerObjects.Where(item => !item.IsRemoved && item.PowerSystemId == s.Id && item.PowerObjectDevices.Count > 0).OrderBy(item => item.Name).ToList()
                    select new
                    {
                        key = o.Id,
                        label = o.Name,
                        nodes =
                            from p in _context.PrimaryEquipments.Where(item => !item.IsRemoved && item.PrimaryEquipmentPowerObjects.Select(i => i.PowerObjectId).Contains(o.Id)).OrderBy(item => item.Name).ToList()
                            select new
                            {
                                key = p.Shifr,
                                label = p.Name,
                                nodes =
                                    from d in _context.Devices.Where(item => !item.IsRemoved &&
                                        item.PrimaryEquipmentDevices.Select(i => i.PrimaryEquipmentShifr).FirstOrDefault() == p.Shifr &&
                                        item.PowerObjectDevices.Select(i => i.PowerObjectId).FirstOrDefault() == o.Id).OrderBy(item => item.Name).ToList()
                                    select new
                                    {
                                        key = d.Shifr,
                                        label = d.Name
                                    }
                            }
                    }
                });
                list.Add(jObject);
            }
            return list.ToArray();
        }

        [HttpPost]
        public JObject[] GetAPTSList([FromBody]object data)
        {
            var definition = new { id = "" };
            var devDes = JsonConvert.DeserializeAnonymousType(data.ToString(), definition);
            string devId = devDes.id.Split('/')[3];
            var oicTSs = _context.OicTs.Where(item => !item.IsRemoved && item.DeviceShifr == devId).ToList();
            List<JObject> list = new List<JObject>();

            foreach (var s in oicTSs)
            {
                JObject jObject = JObject.FromObject(new
                {
                    key = s.Id,
                    oicId = s.OicId,
                    label = s.Name,
                    isStatus = s.IsStatusTs,
                    comment = s.Comment
                });
                list.Add(jObject);
            }
            return list.ToArray();
        }

        [HttpPost]
        public void AddAPTS([FromBody]object data)
        {
            var definition = new[] { new { oicid = "", name = "", device = "", isStatus = "" } };
            var arrDevDes = JsonConvert.DeserializeAnonymousType(data.ToString(), definition);
            int oicId;
            string devId;
            string tsName;
            bool isStatus;

            List<OicTs> listToAdd = new List<OicTs>();

            for (int i = 0; i < arrDevDes.Length; i++)
            {
                oicId = Convert.ToInt32(arrDevDes[i].oicid);
                devId = arrDevDes[i].device.Split('/')[3];
                tsName = arrDevDes[i].name;
                isStatus = Convert.ToBoolean(arrDevDes[i].isStatus);

                OicTs ts = new OicTs
                {
                    DeviceShifr = devId,
                    Name = tsName,
                    OicId = oicId,
                    IsStatusTs = isStatus,
                    Comment = _context.OicTs.Where(item => item.OicId == oicId).FirstOrDefault()?.Comment ?? ""
                };

                listToAdd.Add(ts);
            }

            if (listToAdd.Count > 0)
            {
                AddAction(listToAdd, "Добавил");
                _context.OicTs.AddRange(listToAdd);
                _context.SaveChanges();
            }
        }

        [HttpPost]
        public void DeleteAPTS([FromBody]object data)
        {
            var definition = new[] { new { id = "" } };
            var arrDevDes = JsonConvert.DeserializeAnonymousType(data.ToString(), definition);
            int devId;
            List<OicTs> listToDelete = new List<OicTs>();

            for (int i = 0; i < arrDevDes.Length; i++)
            {
                devId = int.Parse(arrDevDes[i].id);
                var tsToDelete = _context.OicTs.Find(devId);
                if (tsToDelete != null)
                    listToDelete.Add(tsToDelete);
            }

            if (listToDelete.Count > 0)
            {
                AddAction(listToDelete, "Удалил");
                _context.OicTs.RemoveRange(listToDelete);
                _context.SaveChanges();
            }
        }

        private void AddAction(List<OicTs> list, string actionName)
        {
            List<Actions> listActions = new List<Actions>();

            if (list?.Count > 0)
            {
                foreach (var ts in list)
                {
                    var device = _context.Devices.Where(item => item.Shifr == ts.DeviceShifr).FirstOrDefault();
                    var primary = _context.PrimaryEquipments.Where(item => item.Shifr == _context.PrimaryEquipmentDevices.Where(p => p.DeviceShifr == device.Shifr).FirstOrDefault().PrimaryEquipmentShifr).FirstOrDefault();
                    var obj = _context.PowerObjects.Where(item => item.Id == _context.PowerObjectDevices.Where(d => d.DeviceShifr == device.Shifr).FirstOrDefault().PowerObjectId).FirstOrDefault();
                    var action = new Actions
                    {
                        ActionName = actionName,
                        Dtime = DateTime.Now,
                        UserName = User.Identity.Name,
                        OicTsName = ts.Name,
                        DeviceName = device.Name,
                        PrimaryName = primary.Name,
                        PowerObjectName = obj.Name,
                        TsOicId = ts.OicId.ToString()
                    };
                    listActions.Add(action);
                }
                _context.Actions.AddRange(listActions);
            }
        }

        [HttpPost]
        public void EditAPTS([FromBody]object data)
        {
            var definition = new { id = "", status = "", comment = "" };
            var tsDes = JsonConvert.DeserializeAnonymousType(data.ToString(), definition);
            int tsOicId = Convert.ToInt32(tsDes.id);
            bool tsStatus = Convert.ToBoolean(tsDes.status);
            string tsComment = tsDes.comment;

            var tsList = _context.OicTs.Where(item => item.OicId == tsOicId).ToList();
            if (tsList.Count > 0)
            {
                foreach (var ts in tsList)
                {
                    if (ts != null)
                    {
                        ts.IsStatusTs = tsStatus;
                        ts.Comment = tsComment;
                    }
                }
                _context.SaveChanges();
            }
        }

        [HttpGet]
        [ResponseCache(VaryByHeader = "User-Agent", Duration = 300)]
        public JObject[] GetTSListFromOIC()
        {
            Api_OIC apiOIC = new Api_OIC(_configuration);
            DataRowCollection tsCollection = apiOIC.GetTSFromOIC();
            List<JObject> list = new List<JObject>();

            if (tsCollection.Count > 0)
            {
                foreach (DataRow row in tsCollection)
                {
                    var tsDB = _context.OicTs.Where(item => !item.IsRemoved && item.OicId == (int)row.ItemArray[0]).FirstOrDefault();
                    JObject jObject = JObject.FromObject(new
                    {
                        key = row.ItemArray[0],
                        oicId = row.ItemArray[0],
                        label = row.ItemArray[1],
                        enObj = row.ItemArray[2],
                        isStatus = tsDB != null ? tsDB.IsStatusTs : false
                    });
                    list.Add(jObject);
                }
            }
            return list.ToArray();
        }

        [HttpGet]
        public JObject[] GetActions()
        {
            List<JObject> list = new List<JObject>();

            if (_context.Actions.Count() > 0)
            {
                foreach (var action in _context.Actions.OrderByDescending(item => item.Id))
                {
                    JObject jObject = JObject.FromObject(new
                    {
                        key = action.Id,
                        dt = action.Dtime.ToString("dd.MM.yyyy HH:mm:ss"),
                        userName = action.UserName.Split('\\')[1],
                        actionName = action.ActionName,
                        tsOicId = action.TsOicId,
                        tsName = action.OicTsName,
                        devName = action.DeviceName,
                        objName = action.PowerObjectName
                    });
                    list.Add(jObject);

                    if (list?.Count == 10000)
                        break;
                }
            }
            return list.ToArray();
        }
        [HttpGet]
        public JObject[] ExportDevTreeAPTS()
        {
            List<JObject> list = new List<JObject>();

            if (_context.OicTs.Count() > 0)
            {
                foreach (var ts in _context.OicTs.Where(item => !item.IsRemoved).OrderByDescending(item => item.Id))
                {
                    var device = _context.Devices.Where(d => !d.IsRemoved && d.Shifr == ts.DeviceShifr)
                        .FirstOrDefault();
                    var eObj = _context.PowerObjects.Where(o => !o.IsRemoved && o.Id == _context.PowerObjectDevices
                        .Where(d => device != null && d.DeviceShifr == device.Shifr)
                        .FirstOrDefault().PowerObjectId)
                        .FirstOrDefault();
                    var pSys = _context.PowerSystems.Where(s => !s.IsRemoved && eObj != null && s.Id == eObj.PowerSystemId)
                        .FirstOrDefault();
                    var primary = _context.PrimaryEquipments.Where(p => !p.IsRemoved && p.Shifr == _context.PrimaryEquipmentDevices
                        .Where(p => device != null && p.DeviceShifr == device.Shifr)
                        .FirstOrDefault().PrimaryEquipmentShifr)
                        .FirstOrDefault();
                    if (device != null && eObj != null && pSys != null && primary != null)
                    {
                        JObject jObject = JObject.FromObject(new
                        {
                            powSys = pSys.Name,
                            enObj = eObj.Name,
                            primary = primary.Name,
                            dev = device.Name,
                            tsName = ts.Name,
                            tsId = ts.OicId
                        });
                        list.Add(jObject);
                    }
                }
            }
            return list.ToArray();
        }
    }
}