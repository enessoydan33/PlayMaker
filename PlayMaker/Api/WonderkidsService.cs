using PlayMaker.Models;
using System.ComponentModel;
using OfficeOpenXml; // EPPlus kütüphanesi
using System.IO;
using System.Collections.Generic;
using LicenseContext = OfficeOpenXml.LicenseContext;

namespace PlayMaker.Api
{
    public class WonderkidsService
    {
        public List<WonderkidsPlayerData> ReadExcel(string filePath)
        {
            // EPPlus 8.0.5 için lisans ayarı
            ExcelPackage.License.SetNonCommercialPersonal("Playmaker"); // Buraya ismini yazabilirsin

            var playerList = new List<WonderkidsPlayerData>();

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension.Rows;

                for (int row = 2; row <= rowCount; row++)
                {
                    var player = new WonderkidsPlayerData
                    {
                        UID = worksheet.Cells[row, 1].Text,
                        Name_2023 = worksheet.Cells[row, 2].Text,
                        Club_2023 = worksheet.Cells[row, 3].Text,
                        Position_2023 = worksheet.Cells[row, 4].Text,
                        Age_2024 = int.TryParse(worksheet.Cells[row, 7].Text, out var age) ? age : 0,
                        Predicted_2024_Growth = double.TryParse(worksheet.Cells[row, 8].Text, out var growth) ? growth : 0,
                        Feature_Score_2023 = double.TryParse(worksheet.Cells[row, 9].Text, out var score) ? score : 0,
                        Role= worksheet.Cells[row, 5].Text,

                    };

                    playerList.Add(player);
                }
            }

            return playerList;
        }
    }

      

    }
