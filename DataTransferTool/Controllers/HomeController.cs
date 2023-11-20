using DataTransferTool.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using System.Runtime.Intrinsics.X86;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.CodeAnalysis;
using System.ComponentModel;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Dapper;

namespace DataTransferTool.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        /// <summary>
        /// وارد کردن کانکشن استرینگ ها
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [HttpPost]
        public IActionResult GetTabels(ConnectionStringsViewModel model)
        {
            // رشته‌های اتصال به دیتابیس مبدا و مقصد
            string sourceConnectionString = model.SourceConnectionString;
            string destinationConnectionString = model.DestinationConnectionString;

            ViewBag.SourceConnectionString = model.SourceConnectionString;
            ViewBag.DestinationConnectionString = model.DestinationConnectionString;
            // ایجاد اتصال به دیتابیس مبدا و مقصد
            using (SqlConnection sourceConnection = new SqlConnection(sourceConnectionString))
            using (SqlConnection destinationConnection = new SqlConnection(destinationConnectionString))
            {
                // باز کردن اتصال به دیتابیس مبدا و مقصد
                sourceConnection.Open();
                destinationConnection.Open();

                // نام دیتابیس مبدا
                string sourceDatabaseName = sourceConnection.Database;
                // نام دیتابیس مقصد
                string destinationDatabaseName = destinationConnection.Database;

                // کوئری برای برگرداندن نام جدول‌های موجود در دیتابیس مبدا
                string sourcequery = $"SELECT table_name FROM information_schema.tables WHERE table_type = 'BASE TABLE' AND table_catalog = '{sourceDatabaseName}'";

                List<Table> SourceTables = new List<Table>();
                using (SqlCommand sourceCommand = new SqlCommand(sourcequery, sourceConnection))
                using (SqlDataReader reader = sourceCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string tableName = reader["table_name"].ToString();
                        SourceTables.Add(new Table
                        {
                            TableName = tableName
                        });
                        Console.WriteLine($"نام جدول: {tableName}");
                    }
                }


                // کوئری برای برگرداندن نام جدول‌های موجود در دیتابیس مقصد
                string destinstionquery = $"SELECT table_name FROM information_schema.tables WHERE table_type = 'BASE TABLE' AND table_catalog = '{destinationDatabaseName}'";

                List<Table> destinationTables = new List<Table>();
                using (SqlCommand destinationCommand = new SqlCommand(destinstionquery, destinationConnection))
                using (SqlDataReader reader = destinationCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string tableName = reader["table_name"].ToString();
                        destinationTables.Add(new Table
                        {
                            TableName = tableName
                        });
                        Console.WriteLine($"نام جدول: {tableName}");
                    }
                }
                ViewBag.SourceTables = SourceTables;
                ViewBag.destinationTables = destinationTables;
                return View();
            }
        }
        [HttpPost]
        public IActionResult Selectfields(SharedModel model)
        {
            // رشته‌های اتصال به دیتابیس مبدا و مقصد
            string sourceConnectionString = model.SourceConnectionString;
            string destinationConnectionString = model.DestinationConnectionString;

            ViewBag.SourceConnectionString = model.SourceConnectionString;
            ViewBag.DestinationConnectionString = model.DestinationConnectionString;
            // ایجاد اتصال به دیتابیس مبدا و مقصد
            using (SqlConnection sourceConnection = new SqlConnection(sourceConnectionString))
            using (SqlConnection destinationConnection = new SqlConnection(destinationConnectionString))
            {
                // باز کردن اتصال به دیتابیس مبدا و مقصد
                sourceConnection.Open();
                destinationConnection.Open();

                // نام دیتابیس مبدا
                string sourceDatabaseName = sourceConnection.Database;
                // نام دیتابیس مقصد
                string destinationDatabaseName = destinationConnection.Database;

                // نام جدول مبدا مورد نظر
                string SourceTables = model.SourceTables;
                ViewBag.SourceTables = SourceTables;
                // کوئری برای گرفتن نام فیلدها از جدول مورد نظر
                string sourcefieldsQuery = $"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{SourceTables}'";

                List<Field> sourceTableFields = new List<Field>();
                using (SqlCommand fieldsCommand = new SqlCommand(sourcefieldsQuery, sourceConnection))
                using (SqlDataReader fieldsReader = fieldsCommand.ExecuteReader())
                {
                    while (fieldsReader.Read())
                    {
                        // خواندن نام فیلدها و اضافه کردن به لیست
                        string fieldName = fieldsReader["COLUMN_NAME"].ToString();
                        sourceTableFields.Add(new Field
                        {
                            FieldName = fieldName
                        });

                    }
                }

                ViewBag.sourceTableFields = sourceTableFields; // ارسال نتایج به ویو


                // نام جدول مقصد مورد نظر
                string DistinationTables = model.DestinationTables;
                ViewBag.DistinationTables = DistinationTables;
                // کوئری برای گرفتن نام فیلدها از جدول مورد نظر
                string distinationfieldsQuery = $"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{DistinationTables}'";

                List<Field> distinationtableFields = new List<Field>();

                using (SqlCommand fieldsCommand = new SqlCommand(distinationfieldsQuery, destinationConnection))
                using (SqlDataReader fieldsReader = fieldsCommand.ExecuteReader())
                {
                    while (fieldsReader.Read())
                    {
                        // خواندن نام فیلدها و اضافه کردن به لیست
                        string fieldName = fieldsReader["COLUMN_NAME"].ToString();
                        distinationtableFields.Add(new Field
                        {
                            FieldName = fieldName
                        });
                    }
                }

                ViewBag.distinationtableFields = distinationtableFields; // ارسال نتایج به ویو
                return View();

            }
        }
        [HttpPost]
        public IActionResult ShowfieldsData(Selectfields model)
        {
            // رشته‌های اتصال به دیتابیس مبدا و مقصد
            string sourceConnectionString = model.SourceConnectionString;
            string destinationConnectionString = model.DestinationConnectionString;

            ViewBag.SourceConnectionString = model.SourceConnectionString;
            ViewBag.DestinationConnectionString = model.DestinationConnectionString;
            // ایجاد اتصال به دیتابیس مبدا و مقصد
            using (SqlConnection sourceConnection = new SqlConnection(sourceConnectionString))
            using (SqlConnection destinationConnection = new SqlConnection(destinationConnectionString))
            {
                // باز کردن اتصال به دیتابیس مبدا و مقصد
                sourceConnection.Open();
                destinationConnection.Open();

                // نام دیتابیس مبدا
                string sourceDatabaseName = sourceConnection.Database;
                // نام دیتابیس مقصد
                string destinationDatabaseName = destinationConnection.Database;

                // نام جدول مبدا مورد نظر
                string SourceTable = model.SourceTable;
                ViewBag.SourceTable = SourceTable;
                // نام دو فیلد مورد نظر
                string fieldcommonName = model.CommonfieldSourceTable;
                ViewBag.fieldcommonName = fieldcommonName;
                string fieldtargetName = model.TargetfieldSourceTable;
                ViewBag.fieldtargetName = fieldtargetName;
                // کوئری برای گرفتن اطلاعات از دو فیلد در جدول مورد نظر
                string dataQuery = $"SELECT {fieldcommonName}, {fieldtargetName} FROM {SourceTable}";

                List<Tuple<string, string>> tableData = new List<Tuple<string, string>>();

                using (SqlCommand dataCommand = new SqlCommand(dataQuery, sourceConnection))
                using (SqlDataReader dataReader = dataCommand.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        // خواندن اطلاعات از دو فیلد و اضافه کردن به لیست
                        string fieldcommonValue = dataReader[fieldcommonName].ToString();
                        string fieldtargetValue = dataReader[fieldtargetName].ToString();
                        tableData.Add(new Tuple<string, string>(fieldcommonValue, fieldtargetValue));
                    }
                }

                ViewBag.TableData = tableData; // ارسال نتایج به ویو

                // نام جدول مقصد مورد نظر
                string DestinationTable = model.DestinationTable;
                ViewBag.DestinationTable = DestinationTable;
                // نام دو فیلد مورد نظر
                string CommonfieldDestinationTable = model.CommonfieldDestinationTable;
                ViewBag.CommonfieldDestinationTable = CommonfieldDestinationTable;
                string TargetfieldDestinationTable = model.TargetfieldDestinationTable;
                ViewBag.TargetfieldDestinationTable = TargetfieldDestinationTable;
                // کوئری برای گرفتن اطلاعات از دو فیلد در جدول مورد نظر
                string dataQuery2 = $"SELECT {CommonfieldDestinationTable}, {TargetfieldDestinationTable} FROM {DestinationTable}";

                List<Tuple<string, string>> tableData2 = new List<Tuple<string, string>>();

                using (SqlCommand dataCommand = new SqlCommand(dataQuery2, destinationConnection))
                using (SqlDataReader dataReader = dataCommand.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        // خواندن اطلاعات از دو فیلد و اضافه کردن به لیست
                        string fieldcommonValue = dataReader[CommonfieldDestinationTable].ToString();
                        string fieldtargetValue = dataReader[TargetfieldDestinationTable].ToString();
                        tableData2.Add(new Tuple<string, string>(fieldcommonValue, fieldtargetValue));
                    }
                }

                ViewBag.TableData2 = tableData2; // ارسال نتایج به ویو

                return View();

            }
        }
        [HttpPost]
        public IActionResult MatchfieldsData(Selectfields model)
        {
            // رشته‌های اتصال به دیتابیس مبدا و مقصد
            string sourceConnectionString = model.SourceConnectionString;
            string destinationConnectionString = model.DestinationConnectionString;

            ViewBag.SourceConnectionString = model.SourceConnectionString;
            ViewBag.DestinationConnectionString = model.DestinationConnectionString;
            // ایجاد اتصال به دیتابیس مبدا و مقصد
            using (SqlConnection sourceConnection = new SqlConnection(sourceConnectionString))
            using (SqlConnection destinationConnection = new SqlConnection(destinationConnectionString))
            {
                // باز کردن اتصال به دیتابیس مبدا و مقصد
                sourceConnection.Open();
                destinationConnection.Open();

                // نام دیتابیس مبدا
                string sourceDatabaseName = sourceConnection.Database;
                // نام دیتابیس مقصد
                string destinationDatabaseName = destinationConnection.Database;

                // نام جدول مبدا مورد نظر
                string SourceTable = model.SourceTable;
                ViewBag.SourceTable = SourceTable;
                // نام دو فیلد مورد نظر
                string fieldcommonName = model.CommonfieldSourceTable;
                ViewBag.fieldcommonName = fieldcommonName;
                string fieldtargetName = model.TargetfieldSourceTable;
                ViewBag.fieldtargetName = fieldtargetName;
                // کوئری برای گرفتن اطلاعات از دو فیلد در جدول مورد نظر
                string dataQuery = $"SELECT {fieldcommonName}, {fieldtargetName} FROM {SourceTable}";

                List<Tuple<string, string>> tableData = new List<Tuple<string, string>>();

                using (SqlCommand dataCommand = new SqlCommand(dataQuery, sourceConnection))
                using (SqlDataReader dataReader = dataCommand.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        // خواندن اطلاعات از دو فیلد و اضافه کردن به لیست
                        string fieldcommonValue = dataReader[fieldcommonName].ToString();
                        string fieldtargetValue = dataReader[fieldtargetName].ToString();
                        tableData.Add(new Tuple<string, string>(fieldcommonValue, fieldtargetValue));
                    }
                }

                ViewBag.TableData = tableData; // ارسال نتایج به ویو

                // نام جدول مقصد مورد نظر
                string DestinationTable = model.DestinationTable;
                ViewBag.DestinationTable = DestinationTable;
                // نام دو فیلد مورد نظر
                string CommonfieldDestinationTable = model.CommonfieldDestinationTable;
                ViewBag.CommonfieldDestinationTable = CommonfieldDestinationTable;
                string TargetfieldDestinationTable = model.TargetfieldDestinationTable;
                ViewBag.TargetfieldDestinationTable = TargetfieldDestinationTable;
                // کوئری برای گرفتن اطلاعات از دو فیلد در جدول مورد نظر
                string dataQuery2 = $"SELECT {CommonfieldDestinationTable}, {TargetfieldDestinationTable} FROM {DestinationTable}";

                List<Tuple<string, string>> tableData2 = new List<Tuple<string, string>>();

                using (SqlCommand dataCommand = new SqlCommand(dataQuery2, destinationConnection))
                using (SqlDataReader dataReader = dataCommand.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        // خواندن اطلاعات از دو فیلد و اضافه کردن به لیست
                        string fieldcommonValue = dataReader[CommonfieldDestinationTable].ToString();
                        string fieldtargetValue = dataReader[TargetfieldDestinationTable].ToString();
                        tableData2.Add(new Tuple<string, string>(fieldcommonValue, fieldtargetValue));
                    }
                }

                ViewBag.TableData2 = tableData2; // ارسال نتایج به ویو


                // تطابق داده‌ها بر اساس فیلد مشترک و تغییر فیلد هدف مقصد
                var matchedData = new List<Tuple<string, string>>();
                foreach (var item2 in tableData2)
                {
                    bool matchFound = false; // برای ذخیره آیا تطابق پیدا شده یا نه
                    string updatedFieldTarget = item2.Item2; // مقدار پیش‌فرض

                    foreach (var item1 in tableData)
                    {
                        if (item1.Item1 == item2.Item1) // تطابق بر اساس فیلد مشترک
                        {
                            matchFound = true;
                            updatedFieldTarget = item1.Item2; // اگر تطابق پیدا شد، مقدار فیلد هدف را به مقدار جدول مقصد تنظیم کنید
                            break; // چون تطابق پیدا شده است، می‌توانید حلقه داخلی را ترک کنید
                        }
                    }

                    // افزودن تطابق به لیست
                    matchedData.Add(new Tuple<string, string>(item2.Item1, updatedFieldTarget));
                }


                ViewBag.MatchedData = matchedData; // ارسال نتایج تطابق به ویو


                //foreach (var match in matchedData)
                //{
                //    string commonFieldValue = match.Item1; // مقدار فیلد مشترک
                //    string updatedFieldTarget = match.Item2; // مقدار جدید فیلد هدف مقصد

                //    // ساخت کوئری UPDATE برای به‌روزرسانی داده در دیتابیس مقصد
                //    string updateQuery = $"UPDATE {DestinationTable} SET {TargetfieldDestinationTable} = '{updatedFieldTarget}' WHERE {CommonfieldDestinationTable} = '{commonFieldValue}'";

                //    using (SqlCommand updateCommand = new SqlCommand(updateQuery, destinationConnection))
                //    {
                //        updateCommand.ExecuteNonQuery(); // اجرای کوئری UPDATE برای به‌روزرسانی دیتابیس مقصد
                //    }
                //}

                foreach (var match in matchedData)
                {
                    string commonFieldValue = match.Item1; // مقدار فیلد مشترک
                    string updatedFieldTarget = match.Item2; // مقدار جدید فیلد هدف مقصد

                    string updateQuery = $"UPDATE {DestinationTable} SET {TargetfieldDestinationTable} = @updatedFieldTarget WHERE {CommonfieldDestinationTable} = @commonFieldValue";
                    destinationConnection.Execute(updateQuery, new { updatedFieldTarget, commonFieldValue });
                }

                return RedirectToAction("index");

            }
        }
    }

}

