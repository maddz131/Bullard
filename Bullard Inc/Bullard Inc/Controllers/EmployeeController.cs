﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bullard_Inc.Models;
using System.Web.Mvc;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Bullard_Inc.Controllers
{
    public class EmployeeController : Controller
    {
        private List<ActivityCode> activityCodes;
        HttpClient client;
        string url = "http://bullardapi.azurewebsites.net/api/"; // The URL of the WEB API Service

        //Set the base address and the Header Formatter
        public EmployeeController()
        {
            client = new HttpClient();
            client.BaseAddress = new Uri(url);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<ActionResult> Index()
        {
            HttpResponseMessage responseMessage = await client.GetAsync("employees");
            if (responseMessage.IsSuccessStatusCode)
            {
                var responseData = responseMessage.Content.ReadAsStringAsync().Result;

                List<Employee> employees = JsonConvert.DeserializeObject<List <Employee>>(responseData);
                
                return View(employees);
            }
            return View("Error");
        }

        // GET: EmployeeInfo
        [Route("employee/{id}/timesheets")]
        public async Task<ActionResult> EmpTimesheetsView(string id)
        {
            EmpTimesheetsView result = new Models.EmpTimesheetsView();
           
            HttpResponseMessage responseMessage = await client.GetAsync("employees/" + id);
            var responseData = responseMessage.Content.ReadAsStringAsync().Result;
            if (responseMessage.IsSuccessStatusCode)
            {               
                result = (EmpTimesheetsView)JsonConvert.DeserializeObject<EmpTimesheetsView>(responseData);
                Debug.WriteLine(result.FirstName);
                responseMessage = await client.GetAsync("timesheets/employee/" + id);
                if (responseMessage.IsSuccessStatusCode)
                {
                    responseData = responseMessage.Content.ReadAsStringAsync().Result;

                    List<Timesheet> timesheets = JsonConvert.DeserializeObject<List<Timesheet>>(responseData);
                    Debug.WriteLine(timesheets);
                    result.Timesheets = timesheets;
                    responseMessage = await client.GetAsync("timesheets/employee/" + id);
                    if (responseMessage.IsSuccessStatusCode)
                    {
                        responseData = responseMessage.Content.ReadAsStringAsync().Result;

                        List<ActivityCode> ac = JsonConvert.DeserializeObject<List<ActivityCode>>(responseData);
                    //Debug.WriteLine(timesheets);
                        activityCodes = ac;
                        result.ActivityCodes = ac;
                        return View(result);
                    }
                }               
            }    
            return View("Error");
        }

        // EDIT EMPLOYEE ACTION
        [Route("employee/empedit/{emp_Id}/")]
        public async Task<ActionResult> EmpEdit(int emp_Id)
        {
            // custom url
            string empEditURL = url + "employees/" + emp_Id;

            Employee employee;

            HttpResponseMessage response = await client.GetAsync(empEditURL);
            var responseData = response.Content.ReadAsStringAsync().Result;
            if (response.IsSuccessStatusCode)
            {
                employee = JsonConvert.DeserializeObject<Employee>(responseData);
                return View(employee);
            }
            // if api call fails, return error
            return RedirectToAction("Error " + response);
        }

        // UPDATE EMPLOYEE ACTION 
        [Route("employee/empedit/empupdate/")]
        public async Task<ActionResult> EmpUpdate([Bind(Include = "Emp_Id,AccountName,FirstName,LastName,Email,Phone,")] Employee employee)
        {
            // custom url
            string empJobUpdateURL = url + "/employees/" + employee.Emp_Id;

            HttpResponseMessage responseMessage = await client.PutAsJsonAsync(empJobUpdateURL, employee);
            System.Net.HttpStatusCode response = responseMessage.StatusCode;
            if (responseMessage.IsSuccessStatusCode)
            {
                return RedirectToAction("index");
            }
            return RedirectToAction("Error " + response);
        }

        /*
        [HttpPost]
        [Route("employee/timesheets")]
        public JsonResult Index(string Prefix)
        {
            
            //Searching records from list using LINQ query  
            var actDes = (from N in activityCodes
                            where N.ActivityDescription.StartsWith(Prefix)
                            select new { N.ActivityDescription });
            return Json(actDes, JsonRequestBehavior.AllowGet);
        }
        */

    }
}