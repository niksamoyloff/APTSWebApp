using System;
using System.Collections.Generic;
using System.Text;
using DBRepository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using APTSWebApp.Models;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace DBRepository.Repositories
{
    public class APTSRepository : BaseRepository, IAPTSRepository
    {
        protected IHttpContextAccessor HttpContextAccessor { get; }
        public APTSRepository(string connectionString, IRepositoryContextFactory contextFactory, IHttpContextAccessor httpContextAccessor) 
            : base(connectionString, contextFactory)
        {
            HttpContextAccessor = httpContextAccessor;
        }

        public async Task AddAction(List<OicTs> list, string actionName)
        {
            List<Actions> listActions = new List<Actions>();
            string username = HttpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Name).Value;

            using (var context = ContextFactory.CreateDbContext(ConnectionString))
            {
                if (list?.Count > 0)
                {
                    foreach (var ts in list)
                    {
                        var device = context.Devices.Where(item => item.Shifr == ts.DeviceShifr).FirstOrDefault();
                        var primary = context.PrimaryEquipments
                            .Where(item => 
                                item.Shifr == context.PrimaryEquipmentDevices
                                .Where(p => p.DeviceShifr == device.Shifr)
                                .FirstOrDefault().PrimaryEquipmentShifr
                            ).FirstOrDefault();
                        
                        var obj = context.PowerObjects
                            .Where(item => 
                                item.Id == context.PowerObjectDevices
                                .Where(d => d.DeviceShifr == device.Shifr)
                                .FirstOrDefault().PowerObjectId
                            ).FirstOrDefault();
                        
                        var action = new Actions
                        {
                            ActionName = actionName,
                            Dtime = DateTime.Now,
                            UserName = username,
                            OicTsName = ts.Name,
                            DeviceName = device.Name,
                            PrimaryName = primary.Name,
                            PowerObjectName = obj.Name,
                            TsOicId = ts.OicId.ToString()
                        };
                        listActions.Add(action);
                    }
                    context.Actions.AddRange(listActions);
                    await context.SaveChangesAsync();
                }
            }
                
        }

        public async Task AddAPTS(object data)
        {
            var definition = new[] { new { oicid = "", name = "", device = "", isStatus = "" } };
            var arrDevDes = JsonConvert.DeserializeAnonymousType(data.ToString(), definition);
            int oicId;
            string devId;
            string tsName;
            bool isStatus;

            List<OicTs> listToAdd = new List<OicTs>();

            using (var context = ContextFactory.CreateDbContext(ConnectionString))
            {
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
                        Comment = context.OicTs.Where(item => item.OicId == oicId).FirstOrDefault()?.Comment ?? ""
                    };

                    listToAdd.Add(ts);
                }

                if (listToAdd.Count > 0)
                {
                    await AddAction(listToAdd, "Добавил");
                    context.OicTs.AddRange(listToAdd);
                    await context.SaveChangesAsync();
                }
            }
                
        }

        public async Task DeleteAPTS(object data)
        {
            var definition = new[] { new { id = "" } };
            var arrDevDes = JsonConvert.DeserializeAnonymousType(data.ToString(), definition);
            int devId;
            List<OicTs> listToDelete = new List<OicTs>();

            using (var context = ContextFactory.CreateDbContext(ConnectionString))
            {
                for (int i = 0; i < arrDevDes.Length; i++)
                {
                    devId = int.Parse(arrDevDes[i].id);
                    var tsToDelete = context.OicTs.Find(devId);
                    if (tsToDelete != null)
                        listToDelete.Add(tsToDelete);
                }

                if (listToDelete.Count > 0)
                {
                    await AddAction(listToDelete, "Удалил");
                    context.OicTs.RemoveRange(listToDelete);
                    await context.SaveChangesAsync();
                }
            }
                
        }

        public async Task EditAPTS(object data)
        {
            var definition = new { id = "", status = "", comment = "" };
            var tsDes = JsonConvert.DeserializeAnonymousType(data.ToString(), definition);
            int tsOicId = Convert.ToInt32(tsDes.id);
            bool tsStatus = Convert.ToBoolean(tsDes.status);
            string tsComment = tsDes.comment;

            using (var context = ContextFactory.CreateDbContext(ConnectionString))
            {
                var tsList = context.OicTs.Where(item => item.OicId == tsOicId).ToList();
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
                    await context.SaveChangesAsync();
                }
            }                
        }

        public JObject[] ExportDevTreeAPTS()
        {
            List<JObject> list = new List<JObject>();

            using (var context = ContextFactory.CreateDbContext(ConnectionString))
            {
                if (context.OicTs.Count() > 0)
                {
                    foreach (var ts in context.OicTs.Where(item => !item.IsRemoved).OrderByDescending(item => item.Id))
                    {
                        var device = context.Devices.Where(d => !d.IsRemoved && d.Shifr == ts.DeviceShifr)
                            .FirstOrDefault();
                        var eObj = context.PowerObjects.Where(o => !o.IsRemoved && o.Id == context.PowerObjectDevices
                            .Where(d => device != null && d.DeviceShifr == device.Shifr)
                            .FirstOrDefault().PowerObjectId)
                            .FirstOrDefault();
                        var pSys = context.PowerSystems.Where(s => !s.IsRemoved && eObj != null && s.Id == eObj.PowerSystemId)
                            .FirstOrDefault();
                        var primary = context.PrimaryEquipments.Where(p => !p.IsRemoved && p.Shifr == context.PrimaryEquipmentDevices
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
            }
            return list.ToArray();
        }

        public async Task<JObject[]> ExportDevTreeAPTSAsync()
        {
            return await Task.Run(() => ExportDevTreeAPTS());
        }

        public Task<JObject[]> GetActions()
        {
            throw new NotImplementedException();
        }

        public Task<JObject[]> GetCurrentAPTSList(object data)
        {
            throw new NotImplementedException();
        }

        public Task<JObject[]> GetDeviceAPTSList(object data)
        {
            throw new NotImplementedException();
        }

        public Task<JObject[]> GetTreeOfDevices()
        {
            throw new NotImplementedException();
        }

        public Task<JObject[]> GetTSListFromOIC()
        {
            throw new NotImplementedException();
        }
    }
}
