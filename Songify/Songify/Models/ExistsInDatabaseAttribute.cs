using Microsoft.EntityFrameworkCore;
using Songify.Data;
using System.ComponentModel.DataAnnotations;

namespace Songify.Models
{
    public class ExistsInDatabaseAttribute : ValidationAttribute
    {
        private readonly string _tableName;
        private readonly string _columnName;
        public ExistsInDatabaseAttribute(string tableName, string columnName)
        {
            _tableName = tableName;
            _columnName = columnName;
        }
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return new ValidationResult($"{_columnName} is required.");
            }
            var dbContext = (ApplicationDbContext)validationContext.GetService(typeof(ApplicationDbContext));
            if (dbContext == null)
            {
                return new ValidationResult("Database context is unavailable.");
            }
            var exists = dbContext.Database.ExecuteSqlRaw(
                $"SELECT COUNT(1) FROM {_tableName} WHERE Id = {{0}}", value) > 0;
            return exists ? ValidationResult.Success : new ValidationResult($"{_columnName} does not exist.");
        }
    }
}
