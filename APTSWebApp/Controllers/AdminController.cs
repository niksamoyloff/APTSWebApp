﻿using System;
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
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
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
        public async Task<JObject[]> GetEquipmentTreeAsync()
        {
            var list = new List<JObject>();
            var ps = await _context.PowerSystems
                .Where(s => !s.IsRemoved)
                .AsNoTracking()
                .ToListAsync();
            var po = await _context.PowerObjects
                .Where(o => !o.IsRemoved && o.PowerObjectDevices.Any() && o.PrimaryEquipmentPowerObjects.Any())
                .AsNoTracking()
                .ToListAsync();
            var pe = await _context.PrimaryEquipments
                .Where(e => !e.IsRemoved && e.PrimaryEquipmentPowerObjects.Any())
                .Include(e => e.PrimaryEquipmentPowerObjects)
                .AsNoTracking()
                .ToListAsync();
            var dev = await _context.Devices
                .Where(d => !d.IsRemoved && d.PowerObjectDevices.Any())
                .Include(d => d.PowerObjectDevices)
                .AsNoTracking()
                .ToListAsync();

            foreach (var s in ps)
            {
                var jObject = JObject.FromObject(new
                {
                    key = s.Id,
                    label = s.Name,
                    nodes = po.Where(o => o.PowerSystemId == s.Id)
                        .OrderBy(o => o.Name)
                        .Select(o => new
                        {
                            key = o.Id,
                            label = o.Name,
                            nodes = pe.Where(item => item.PrimaryEquipmentPowerObjects
                                        .Any(i => i.PowerObjectId == o.Id)
                                    )
                                    .OrderBy(item => item.Name)
                                    .Select(p => new
                                    {
                                        key = p.Shifr,
                                        label = p.Name,
                                        nodes = dev.Where(item => item.PowerObjectDevices
                                                .Any(i => i.PowerObjectId == o.Id)
                                            )
                                            .OrderBy(item => item.Name)
                                            .Select(d => new
                                            {
                                                key = d.Shifr,
                                                label = d.Name
                                            })
                                    })
                        })
                });
                list.Add(jObject);
            }
            return list.ToArray();
        }

        [HttpPost]
        public async Task<JObject[]> GetAptsListAsync([FromBody] object data)
        {
            var definition = new { id = "" };
            var devDes = JsonConvert.DeserializeAnonymousType(data.ToString(), definition);
            var devId = devDes.id.Split('/')[3];
            var oicTSs = await _context.OicTs
                .Where(item => !item.IsRemoved && item.DeviceShifr == devId)
                .AsNoTracking()
                .ToListAsync();
            var list = new List<JObject>();

            foreach (var s in oicTSs)
            {
                var currentVal = _context.ReceivedTsvalues
                    .Where(v => v.OicTsid == s.Id)
                    .OrderBy(v => v.Id)
                    .AsNoTracking()
                    .Select(v => v.Val)
                    .LastOrDefault()
                    .ToString();
                var jObject = JObject.FromObject(new
                {
                    key = s.Id,
                    oicId = s.OicId,
                    label = s.Name,
                    isStatus = s.IsStatusTs,
                    comment = s.Comment,
                    isOic = s.IsOicTs,
                    currVal = currentVal
                });
                list.Add(jObject);
            }
            return list.ToArray();
        }

        [HttpPost]
        public async Task AddAptsAsync([FromBody] object data)
        {
            var definition = new[] { new { oicid = "", name = "", device = "", isStatus = "", isOic = "" } };
            var arrDevDes = JsonConvert.DeserializeAnonymousType(data.ToString() ?? string.Empty, definition);
            int oicId;

            var listToAdd = new List<OicTs>();

            foreach (var t in arrDevDes)
            {
                oicId = Convert.ToInt32(t.oicid);
                var devId = t.device.Split('/')[3];
                var tsName = t.name;
                var isStatus = Convert.ToBoolean(t.isStatus);
                var isOic = Convert.ToBoolean(t.isOic);

                if (oicId == 0) continue;
                var ts = new OicTs
                {
                    DeviceShifr = devId,
                    Name = tsName,
                    OicId = oicId,
                    IsStatusTs = isStatus,
                    IsOicTs = isOic,
                    Comment = _context.OicTs
                        .AsNoTracking()
                        .FirstOrDefault(item => item.OicId == oicId)?.Comment ?? ""
                };
                listToAdd.Add(ts);
            }

            if (!listToAdd.Any()) return;

            await AddActionAsync(listToAdd, "Добавил");
            await _context.OicTs.AddRangeAsync(listToAdd);
            await _context.SaveChangesAsync();
        }

        [HttpPost]
        public async Task DeleteAptsAsync([FromBody] object data)
        {
            var definition = new[] { new { id = "" } };
            var arrDevDes = JsonConvert.DeserializeAnonymousType(data.ToString() ?? string.Empty, definition);
            var listToDelete = new List<OicTs>();

            foreach (var t in arrDevDes)
            {
                var devId = int.Parse(t.id);
                var tsToDelete = await _context.OicTs.FindAsync(devId);
                if (tsToDelete != null)
                    listToDelete.Add(tsToDelete);
            }

            if (listToDelete.Count > 0)
            {
                await AddActionAsync(listToDelete, "Удалил");
                _context.OicTs.RemoveRange(listToDelete);
                await _context.SaveChangesAsync();
            }
        }

        private async Task AddActionAsync(List<OicTs> list, string actionName)
        {
            var listAction = new List<Actions>();

            if (list?.Count > 0)
            {
                foreach (var ts in list)
                {
                    var device = _context.Devices
                        .AsNoTracking()
                        .FirstOrDefault(item => item.Shifr == ts.DeviceShifr);
                    var primary = _context.PrimaryEquipments
                        .AsNoTracking()
                        .FirstOrDefault(item => item.Shifr == _context.PrimaryEquipmentDevices
                            .FirstOrDefault(p => p.DeviceShifr == device.Shifr)
                            .PrimaryEquipmentShifr
                        );
                    var obj = _context.PowerObjects
                        .AsNoTracking()
                        .FirstOrDefault(item => item.Id == _context.PowerObjectDevices
                            .FirstOrDefault(d => d.DeviceShifr == device.Shifr)
                            .PowerObjectId
                        );
                    var action = new Actions
                    {
                        ActionName = actionName,
                        Dtime = DateTime.Now,
                        UserName = User.Identity.Name,
                        OicTsName = ts.Name,
                        DeviceName = device?.Name,
                        PrimaryName = primary?.Name,
                        PowerObjectName = obj?.Name,
                        TsOicId = ts.OicId.ToString()
                    };
                    listAction.Add(action);
                }
                await _context.Actions.AddRangeAsync(listAction);
            }
        }

        [HttpPost]
        public async Task EditAptsAsync([FromBody] object data)
        {
            var definition = new { id = "", status = "", comment = "", isOic = "" };
            var tsDes = JsonConvert.DeserializeAnonymousType(data.ToString() ?? string.Empty, definition);
            var tsOicId = Convert.ToInt32(tsDes.id);
            var tsStatus = Convert.ToBoolean(tsDes.status);
            var tsOic = Convert.ToBoolean(tsDes.isOic);
            var tsComment = tsDes.comment;

            var tsList = await _context.OicTs
                .Where(item => item.OicId == tsOicId)
                .AsNoTracking()
                .ToListAsync();
            if (tsList.Any())
            {
                foreach (var ts in tsList)
                {
                    ts.IsStatusTs = tsStatus;
                    ts.Comment = tsComment;
                    ts.IsOicTs = tsOic;
                }

                await _context.SaveChangesAsync();
            }
        }

        [HttpGet]
        //[ResponseCache(VaryByHeader = "User-Agent", Duration = 60)]
        public async Task<JObject[]> GetTsListFromOicAsync()
        {
            var apiOic = new Api_OIC(_configuration);
            var tsCollection = apiOic.GetTSFromOIC();
            var list = new List<JObject>();

            if (tsCollection.Count == 0) return list.ToArray();
            
            foreach (DataRow row in tsCollection)
            {
                var tsDb = _context.OicTs
                    .AsNoTracking()
                    .FirstOrDefault(item => !item.IsRemoved && item.OicId == (int)row.ItemArray[0]);
                var jObject = JObject.FromObject(new
                {
                    key = row.ItemArray[0],
                    oicId = row.ItemArray[0],
                    label = row.ItemArray[1],
                    enObj = row.ItemArray[2],
                    isStatus = tsDb != null && tsDb.IsStatusTs,
                    isAdded = tsDb != null,
                    isOic = tsDb != null && tsDb.IsOicTs
                });
                list.Add(jObject);
            }
            return list.ToArray();
        }

        [HttpGet]
        public JObject[] GetActions()
        {
            List<JObject> list = new List<JObject>();

            if (_context.Actions.Any())
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

            if (_context.OicTs.Any())
            {
                foreach (var ts in _context.OicTs.Where(item => !item.IsRemoved).ToList().OrderByDescending(item => item.Id))
                {
                    var device = _context.Devices
                        .FirstOrDefault(d => !d.IsRemoved && d.Shifr == ts.DeviceShifr);

                    var eObj = _context.PowerObjects
                        .FirstOrDefault(o => !o.IsRemoved && o.Id == _context.PowerObjectDevices
                            .FirstOrDefault(d => device != null && d.DeviceShifr == device.Shifr).PowerObjectId);

                    var pSys = _context.PowerSystems
                        .FirstOrDefault(s => !s.IsRemoved && eObj != null && s.Id == eObj.PowerSystemId);

                    var primary = _context.PrimaryEquipments
                        .FirstOrDefault(p => !p.IsRemoved && p.Shifr == _context.PrimaryEquipmentDevices
                            .FirstOrDefault(p => device != null && p.DeviceShifr == device.Shifr).PrimaryEquipmentShifr);

                    var currentVal = _context.ReceivedTsvalues.Where(v => v.OicTsid == ts.Id)
                        .OrderBy(v => v.Id)
                        .Select(v => v.Val)
                        .LastOrDefault().ToString() ?? "";

                    if (device != null && eObj != null && pSys != null && primary != null)
                    {
                        JObject jObject = JObject.FromObject(new
                        {
                            powSys = pSys.Name,
                            enObj = eObj.Name,
                            primary = primary.Name,
                            device = device.Name,
                            tsName = ts.Name,
                            tsId = ts.OicId,
                            isStatus = ts.IsStatusTs ? "Да" : "Нет",
                            comment = ts.Comment,
                            isOic = ts.IsOicTs ? "Да" : "Нет",
                            currVal = currentVal
                        });
                        list.Add(jObject);
                    }
                }
            }
            return list.ToArray();
        }
    }
}