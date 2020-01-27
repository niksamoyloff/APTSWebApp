using System;
using System.Collections.Generic;
using System.Text;
using APTSWebApp.Models;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DBRepository.Interfaces
{
    public interface IAPTSRepository
    {
        Task<JObject[]> GetCurrentAPTSList(object data);
        Task<JObject[]> GetTreeOfDevices();
        Task<JObject[]> GetDeviceAPTSList(object data);
        Task<JObject[]> GetTSListFromOIC();
        Task<JObject[]> GetActions();
        Task<JObject[]> ExportDevTreeAPTS();
        Task AddAPTS(object data);
        Task EditAPTS(object data);
        Task DeleteAPTS(object data);
        Task AddAction(List<OicTs> list, string actionName);
    }
}
