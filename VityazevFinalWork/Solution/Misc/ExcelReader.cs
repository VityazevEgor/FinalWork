using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VityazevFinalWork.Solution.Models;
using System.Globalization;

namespace VityazevFinalWork.Solution.Misc
{
    internal class ExcelReader
    {
        private const string format = "MMMM yyyy";
        private const int startColumn = 2; // Колонка B
        private const int endColumn = 58; // Колонка BF
        private const int row = 47; // Строка 47
        private static CultureInfo provider = new CultureInfo("ru-RU");

        public static List<TData> ReadExcel(string filePath)
        {
            var result = new List<TData>();
            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets[0];
                for (int column = startColumn; column <= endColumn; column++)
                {
                    //Debug.WriteLine(worksheet.Cells[row, column].Value);
                    result.Add(new TData { 
                        amount = Convert.ToDouble(worksheet.Cells[row,column].Value),
                        date = DateTime.ParseExact(worksheet.Cells[row-1, column].Value.ToString(), format, provider)
                    });
                }
            }
            return result;
        }


        public static List<TData> ReadSmallData()
        {
            var result = new List<TData>();
            using (var package = new ExcelPackage(new FileInfo(@"D:\Учёба\Прикладной статистический анализ\FinalWork\02_01_All_Borrowers_info.xlsx")))
            {
                var worksheet = package.Workbook.Worksheets[0];
                for (int column = 2; column <= endColumn-13; column++)
                {
                    //Debug.WriteLine(worksheet.Cells[row, column].Value);
                    result.Add(new TData
                    {
                        amount = Convert.ToDouble(worksheet.Cells[row, column].Value),
                        date = DateTime.ParseExact(worksheet.Cells[row - 1, column].Value.ToString(), format, provider)
                    });
                }
            }
            return result;
        }

        public static List<TData> ReadSmallTestData()
        {
            var result = new List<TData>();
            using (var package = new ExcelPackage(new FileInfo(@"D:\Учёба\Прикладной статистический анализ\FinalWork\02_01_All_Borrowers_info.xlsx")))
            {
                var worksheet = package.Workbook.Worksheets[0];
                for (int column = endColumn-12; column <= endColumn; column++)
                {
                    //Debug.WriteLine(worksheet.Cells[row, column].Value);
                    result.Add(new TData
                    {
                        amount = Convert.ToDouble(worksheet.Cells[row, column].Value),
                        date = DateTime.ParseExact(worksheet.Cells[row - 1, column].Value.ToString(), format, provider)
                    });
                }
            }
            return result;
        }

        public static List<TData> MakeLegitData(List<TData> data)
        {
            data.ForEach(d => {
                d.amount /= 0.995; // получаем 100% с учётом того, что юридическим лицам отказывают только в 0.5% случаев
            });
            return data;
        }

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
                // 2018 - 2021 (оставляю данные за 2022 год для тестов)
                result.AddRange(ReadTables(worksheet, 5, 1, 3));
            }

            return result;
        }

        public static List<TData> ReadTestData()
        {
            using (var package = new ExcelPackage(new FileInfo(@"D:\Downloads\Stat_morgage_tables_10 (1).xlsx")))
            {
                var worksheet = package.Workbook.Worksheets[5];
                return ReadTable(worksheet, 44, 1);
            }
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

            var result = new List<TData>();
            result.Add(new TData
            {
                date = DateTime.ParseExact($"{worksheet.Cells[startI, startJ].Value}.{year}", "dd.MM.yyyy", CultureInfo.InvariantCulture),
                amount = Convert.ToDouble(worksheet.Cells[startI, startJ + 1].Value)
            });
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
