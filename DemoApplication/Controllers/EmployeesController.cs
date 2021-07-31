using DemoApplication.Models;
using DemoApplication.Paging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DemoApplication.Controllers
{
    public class EmployeesController : Controller
    {
        EFDataContext _dbContext = new EFDataContext();

        public IActionResult Index(string sortField, string currentSortField, string currentSortOrder, string currentFilter, string SearchString,
        int pageNo)
        {
            var employees = this.GetEmployeeList();
            if (SearchString != null)
            {
                pageNo = 1;
            }
            else
            {
                SearchString = currentFilter;
            }
            ViewData["CurrentSort"] = sortField;
            ViewBag.CurrentFilter = SearchString;
            if (!String.IsNullOrEmpty(SearchString))
            {
                employees = employees.Where(s => s.EmployeeName.Contains(SearchString)).ToList();
            }
            employees = this.SortEmployeeData(employees, sortField, currentSortField, currentSortOrder);
            int pageSize = 10;
            return View(PagingList<Employee>.CreateAsync(employees.AsQueryable<Employee>(), pageNo,pageSize));

        }
        private List<Employee> GetEmployeeList()
        {
            return (from employee in this._dbContext.Employees
                    join desig in this._dbContext.Designations on employee.DepartmentId equals desig.DesignationId
                    join dept in this._dbContext.Departments on employee.DepartmentId equals dept.DepartmentId
                    select new Employee
                    {
                        Id = employee.Id,
                        EmployeeCode = employee.EmployeeCode,
                        EmployeeName = employee.EmployeeName,
                        DateOfBirth = employee.DateOfBirth,
                        JoinDate = employee.JoinDate,
                        Salary = employee.Salary,
                        Address = employee.Address,
                        State = employee.State,
                        City = employee.City,
                        ZipCode = employee.ZipCode,
                        DepartmentId = employee.DepartmentId,
                        DepartmentName = dept.DepartmentName,
                        DesignationId = employee.DesignationId,
                        DesignationName = desig.DesignationName
                    }).AsNoTracking().ToList();
        }
               
        private List<Employee> SortEmployeeData(List<Employee> employees, string sortField, string currentSortField, string currentSortOrder)
        {
            if (string.IsNullOrEmpty(sortField))
            {
                ViewBag.SortField = "EmployeeCode";
                ViewBag.SortOrder = "Asc";
            }
            else
            {
                if (currentSortField == sortField)
                {
                    ViewBag.SortOrder = currentSortOrder == "Asc" ? "Desc" : "Asc";
                }
                else
                {
                    ViewBag.SortOrder = "Asc";
                }
                ViewBag.SortField = sortField;
            }

            var propertyInfo = typeof(Employee).GetProperty(ViewBag.SortField);
            if (ViewBag.SortOrder == "Asc")
            {
                employees = employees.OrderBy(s => propertyInfo.GetValue(s, null)).ToList();
            }
            else
            {
                employees = employees.OrderByDescending(s => propertyInfo.GetValue(s, null)).ToList();
            }
            return employees;
        }
        [HttpPost]
        public IActionResult Create(Employee model)
        {
            if (ModelState.IsValid)
            {
                _dbContext.Employees.Add(model);
                _dbContext.SaveChanges();
                return RedirectToAction("Index");
            }
            this.GetModelData();
            return View();
        }
        public IActionResult Create()
        {
            this.GetModelData();
            return View();
        }
        public IActionResult Delete(int id)
        {
            Employee data = this._dbContext.Employees.Where(p => p.Id == id).FirstOrDefault();
            if (data != null)
            {
                _dbContext.Employees.Remove(data);
                _dbContext.SaveChanges();
            }
            return RedirectToAction("Index");
        }
        [HttpPost]
        public IActionResult Edit(Employee model)
        {
            if (ModelState.IsValid)
            {
                _dbContext.Employees.Update(model);
                _dbContext.SaveChanges();
                return RedirectToAction("Index");
            }
            this.GetModelData();
            return View("Create", model);
        }
        public IActionResult Edit(int id)
        {
            Employee data = this._dbContext.GetEmployeeById(id);
            this.GetModelData();
            return View("Create", data);
        }
        private void GetModelData()
        {
            ViewBag.Departments = this._dbContext.Departments.ToList();
            ViewBag.Designations = this._dbContext.Designations.ToList();
        }

        public IActionResult ExportToExcel()
        {
            var employees = this.GetEmployeeList();
            byte[] fileContents;

            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("EmployeeInfo");
            Sheet.Cells["A1"].Value = "Employee Code";
            Sheet.Cells["B1"].Value = "Employee Name";
            Sheet.Cells["C1"].Value = "Join Date";
            Sheet.Cells["D1"].Value = "Department";
            Sheet.Cells["E1"].Value = "Designation";
            Sheet.Cells["F1"].Value = "Salary";
            Sheet.Cells["G1"].Value = "City";

            int row = 2;
            foreach (var item in employees)
            {
                Sheet.Cells[string.Format("A{0}", row)].Value = item.EmployeeCode;
                Sheet.Cells[string.Format("B{0}", row)].Value = item.EmployeeName;
                Sheet.Cells[string.Format("C{0}", row)].Value = item.JoinDate;
                Sheet.Cells[string.Format("D{0}", row)].Value = item.DepartmentName;
                Sheet.Cells[string.Format("E{0}", row)].Value = item.DesignationName;
                Sheet.Cells[string.Format("F{0}", row)].Value = item.Salary;
                Sheet.Cells[string.Format("G{0}", row)].Value = item.City;
                row++;
            }


            Sheet.Cells["A:AZ"].AutoFitColumns();
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            fileContents = Ep.GetAsByteArray();

            if (fileContents == null || fileContents.Length == 0)
            {
                return NotFound();
            }

            return File(
                fileContents: fileContents,
                contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileDownloadName: "Employee.xlsx"
            );
        }

    }
}
