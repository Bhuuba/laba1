using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewMyApp.Infrastructure.Data;
using System.Data;
using System.Text;

namespace NewMyApp.Web.Controllers
{
    [Authorize] // Доступ для всіх авторизованих користувачів
    public class SqlQueryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SqlQueryController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ExecuteQuery(string query)
        {
            try
            {
                if (string.IsNullOrEmpty(query))
                {
                    return Json(new { error = "Запит не може бути порожнім" });
                }

                // Перевіряємо, що запит є SELECT
                if (!query.Trim().ToLower().StartsWith("select"))
                {
                    return Json(new { error = "Дозволені лише SELECT запити" });
                }

                var result = await _context.Database.ExecuteSqlRawAsync(query);
                
                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = query;
                    command.CommandType = CommandType.Text;

                    await _context.Database.OpenConnectionAsync();

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        var data = new List<Dictionary<string, object>>();
                        var columns = new List<string>();

                        // Отримуємо назви колонок
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            columns.Add(reader.GetName(i));
                        }

                        // Читаємо дані
                        while (await reader.ReadAsync())
                        {
                            var row = new Dictionary<string, object>();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                row[columns[i]] = reader.GetValue(i);
                            }
                            data.Add(row);
                        }

                        return Json(new { success = true, columns, data });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }
    }
}
