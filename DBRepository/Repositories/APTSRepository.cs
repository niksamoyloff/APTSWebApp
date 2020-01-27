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

        public Task DeleteAPTS(object data)
        {
            throw new NotImplementedException();
        }

        public Task EditAPTS(object data)
        {
            throw new NotImplementedException();
        }

        public Task<JObject[]> ExportDevTreeAPTS()
        {
            throw new NotImplementedException();
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
