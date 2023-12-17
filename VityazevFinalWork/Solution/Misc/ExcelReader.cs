using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Linq;
using VityazevFinalWork.Solution.Models;
using System.Globalization;

namespace VityazevFinalWork.Solution.Misc
{
    internal class ExcelReader
    {
        public static List<TData> ReadBigExcel()
        {
            var result = new List<TData>();

            using (var package = new ExcelPackage(new FileInfo(@"D:\Downloads\Stat_morgage_tables_10 (1).xlsx")))
            {
                var worksheet = package.Workbook.Worksheets[5];
                // 2009 - 2013
                result.AddRange(ReadTables(worksheet, 139, 1, 5));
                // 2014 - 2017
                result.AddRange(ReadTables(worksheet, 86, 1, 4));
                // 2018 - 2020 (оставляю данные за 2021 год для тестов)
                result.AddRange(ReadTables(worksheet, 5, 1, 4));
            }

            return result.OrderBy(d=>d.date).ToList();
        }

        private static List<TData> ReadTables(ExcelWorksheet worksheet, int startI, int startJ, int count)
        {
            var result = new List<TData>();
            for (int n=0; n<count; n++)
            {
                result.AddRange( ReadTable(worksheet, startI, startJ));
                startI += 13;
            }
            return result;
        }

        private static List<TData> ReadTable(ExcelWorksheet worksheet,int startI, int startJ)
        {
            string year = worksheet.Cells[startI, startJ].Value.ToString().Replace(" г.", "");
            Debug.WriteLine($"Got year = {year}");
            startI += 2;

            var result = new List<TData>
            {
                new TData
                {
                    date = DateTime.ParseExact($"{worksheet.Cells[startI, startJ].Value}.{year}", "dd.MM.yyyy", CultureInfo.InvariantCulture),
                    amount = Convert.ToDouble(worksheet.Cells[startI, startJ + 1].Value)
                }
            };
            for (int i = startI + 1; i < startI + 1 + 10; i++)
            {
                double am = (double)worksheet.Cells[i, startJ + 1].Value - (double)worksheet.Cells[i - 1, startJ + 1].Value;
                result.Add(new TData
                {
                    date = DateTime.ParseExact($"{worksheet.Cells[i, startJ].Value}.{year}", "dd.MM.yyyy", CultureInfo.InvariantCulture),
                    amount = am
                });
            }
            return result;
        }
    }
}
