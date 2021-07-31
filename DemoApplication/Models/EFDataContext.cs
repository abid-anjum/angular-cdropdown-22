using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DemoApplication.Models
{
    public class EFDataContext : DbContext
    {
        public DbSet<Department> Departments { get; set; }

        public DbSet<Designation> Designations { get; set; }

        public DbSet<Employee> Employees { get; set; }

        public Employee GetEmployeeById(int employeeId)
        {
            IQueryable<Employee> data = this.Employees.FromSqlRaw<Employee>(
                "Exec [dbo].uspGetEmployee " +
                    "@p_EmployeeId", new SqlParameter("p_EmployeeId", employeeId));

            if (data != null)

                return data.ToList().FirstOrDefault();

            else
                return new Employee();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"data source=localhost; initial catalog=EFCrudDemo;Integrated Security=True;");
        }
    }
}
