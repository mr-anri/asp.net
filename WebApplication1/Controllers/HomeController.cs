using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;
using System.Threading;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {

        [HttpGet]
        public ActionResult Index()
        {
            ViewBag.role = new List<Role>();
            return View();
        }



        public async Task<ActionResult> Get_Roles(Auth Auth1)
        {
            var json_param = "";
            var list = new List<Role>();
            HttpClient authorization_obj = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage();
            request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Auth1.username_ist + '@' + Auth1.companyId_ist}:{Auth1.password_ist}")));
            request.Headers.Add("Accept", "application/json");
            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri("https://api18preview.sapsf.com/odata/v2/RBPRole?$expand=permissions&$select=permissions/permissionId,permissions/permissionStringValue,roleId,roleName");

            HttpResponseMessage response = await authorization_obj.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                HttpContent responseContent = response.Content;
                json_param = await responseContent.ReadAsStringAsync();
            }
            ViewBag.json_param = json_param;

            int iii = 0;
            if (json_param != "")
            { 
            var respons_api = JObject.Parse(json_param);
            
            JArray rols = (JArray)respons_api["d"]["results"];
            
            
            foreach (JObject role in rols)
            {
                string role_id = (string)rols[iii]["roleId"];
                string role_name = (string)rols[iii]["roleName"];
                var Permissions = new List<Permissions>();

                int ii = 0;
                JArray role_permissiions = (JArray)rols[iii]["permissions"]["results"];
                foreach (JObject item in role_permissiions)
                {
                    var permission_id = (string)role_permissiions[ii]["permissionId"];
                    var permissionStringValue = (string)role_permissiions[ii]["permissionStringValue"];
                        Permissions.Add(new Permissions { Id = permission_id, PermissionStringValue = permissionStringValue});
                    ii++;
                }
                list.Add(new Role { Id = role_id, Name = role_name, Permission = Permissions});
                iii++;
            }
            ViewBag.role = list; 
            }
            TempData["list_of_role"] = list;
            TempData["Auth"] = Auth1;
            return View(list);
        }

        [HttpPost]
        public string Get_list_of_permission_into_role (Role_search Role_from_view)
        {
            //var list_of_roles_that_was_checkd = new List<Role>();
            var id_role = Role_from_view.Id;                                    //добавили в переменную список Id!!!!!! выбранных в окошке ролей
            List<Role> list_of_role = TempData["list_of_role"] as List<Role>;   //забрали полученные в первом запросе роли и информацию по ним
            TempData["list_of_role"] = list_of_role;
            int i = 0;
            int nomer_roli = 0;
            
            var list_of_find_role = new List<Role>();                           //сделали новый список, в который будем складывать сами роли по полученным id
            var result = new List<Mapping_permissions>();
            Auth Auth = TempData["Auth"] as Auth;
            TempData["Auth"] = Auth;
            foreach (string Id in id_role)                                      //перебираем полученные id ролей и находим всю информацию по ним
            { 
                Role find_role = list_of_role.Find(x => x.Id == id_role[i]);
                list_of_find_role.Add(find_role);
                i++;
            }

            foreach (Role role in list_of_find_role)                                      //перебираем полученные id ролей и находим всю информацию по ним
            {
                int nomer_perm = 0;
                var list_permission_of_select_role = new List<Permissions>();
                var list_of_mapping_permissions = new List<Mapping_permissions>();
                list_permission_of_select_role = list_of_find_role[nomer_roli].Permission;

                foreach (Permissions perm in list_permission_of_select_role)                //для каждого полномочия, входящего в рассматриваемую роль, находим соответствующий лномочия в системе назначения
                {
                    var id_perm_in_naz_task = Permission_id_in_naz_system(list_permission_of_select_role[nomer_perm].Id, list_permission_of_select_role[nomer_perm].PermissionStringValue, Auth);
                    var id_perm_in_naz = id_perm_in_naz_task.Result;
                    list_of_mapping_permissions.Add(new Mapping_permissions {Id_ist = list_permission_of_select_role[nomer_perm].Id, Id_naz = id_perm_in_naz, PermissionStringValue = list_permission_of_select_role[nomer_perm].PermissionStringValue});
                    result = list_of_mapping_permissions;
                    nomer_perm++;
                }
                nomer_roli++;
            }
            //return JsonConvert.SerializeObject(list_of_find_role);
            return JsonConvert.SerializeObject(result);
        }

        [HttpGet]
        public async Task<string> Permission_id_in_naz_system (string id, string permissionStringValue, Auth Auth1)
        {
            await Task.Delay(1000).ConfigureAwait(false);
            
            var json_param = "";
            string permission_id = "";
            HttpClient authorization_obj_for_naz = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage();
            request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Auth1.username_naz + '@' + Auth1.companyId_naz}:{Auth1.password_naz}")));
            request.Headers.Add("Accept", "application/json");
            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri("https://api18preview.sapsf.com/odata/v2/RBPBasicPermission?$filter=permissionStringValue eq '" + permissionStringValue + "'");

            HttpResponseMessage response = await authorization_obj_for_naz.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                HttpContent responseContent = response.Content;
                json_param = await responseContent.ReadAsStringAsync();
            }

            ViewBag.json_param = json_param;
            
            if (json_param != "")
            {
                var respons_api = JObject.Parse(json_param);
                JArray permissions = (JArray)respons_api["d"]["results"];
                permission_id = (string)permissions[0]["permissionId"];
            }
            return permission_id;
        }
    }
}