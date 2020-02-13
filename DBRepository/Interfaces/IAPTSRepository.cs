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
        JObject[] GetCurrentAPTSList(object data);
        JObject[] GetTreeOfDevices();
        JObject[] GetDeviceAPTSList(object data);
        JObject[] GetTSListFromOIC();
        JObject[] GetActions();
        JObject[] ExportDevTreeAPTS();
        Task<JObject[]> GetCurrentAPTSListAsync();
        Task<JObject[]> GetTreeOfDevicesAsync();
        Task<JObject[]> GetDeviceAPTSListAsync();
        Task<JObject[]> GetTSListFromOICAsync();
        Task<JObject[]> GetActionsAsync();
        Task<JObject[]> ExportDevTreeAPTSAsync();
        Task AddAPTS(object data);
        Task EditAPTS(object data);
        Task DeleteAPTS(object data);
        Task AddAction(List<OicTs> list, string actionName);
    }
}
