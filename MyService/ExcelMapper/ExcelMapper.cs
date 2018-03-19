using DocumentFormat.OpenXml.Packaging;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml;

namespace ExcelMapper
{
    public class ExcelMapper<T> where T: class
    {
        private string path;
        private string fileName;
        private string randomPath;
        private string oldPath;

        public ExcelMapper(string _path = null, string _fileName = null)
        {
            if (_path == null || string.IsNullOrWhiteSpace(_path) || string.IsNullOrEmpty(_path))
            {
                //_path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                _path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                this.path = _path;
            }
            if (_fileName == null || string.IsNullOrWhiteSpace(_fileName) || string.IsNullOrEmpty(_fileName))
            {
                _fileName = "Reporte.xlsx";
                this.fileName = _fileName;
            }            
        }

        public string Path
        {
            get
            {
                return path;
            }

            set
            {
                path = value;
            }
        }

        public string FileName
        {
            get
            {
                return fileName;
            }

            set
            {
                fileName = value;
            }
        }

        public string RandomPath
        {
            get
            {
                return GenerateRandomPath();
            }            
        }

        public string OldPath
        {
            get
            {
                return oldPath;
            }
        }

        private long GetTicks()
        {
            return DateTime.Now.Ticks;
        }

        private string GetDateTimeInString()
        {
            return string.Format("{0:yyyy-MM-dd_hh-mm-ss-tt}", DateTime.Now);
        }

        private string GenerateRandomPath()
        {
            //string _fileName = string.Concat(System.IO.Path.GetFileNameWithoutExtension(this.fileName), "_", this.GetTicks().ToString(), System.IO.Path.GetExtension(this.fileName));            
            //this.randomPath = System.IO.Path.Combine(this.path, _fileName);
            this.randomPath = System.IO.Path.Combine(this.path, this.fileName);
            return this.randomPath;
        }

        public async Task<bool> Export(IEnumerable<T> _list)
        {
            bool exported = false;
            string className = typeof(T).Name;
            this.fileName = string.Concat(System.IO.Path.GetFileNameWithoutExtension(this.fileName), "_", className.ToUpper(), System.IO.Path.GetExtension(this.fileName));
            bool IsFileAvailable = await CheckIfFileCanBeUsed(this.fileName);
            if (IsFileAvailable)
            {
                PropertyInfo[] properties = typeof(T).GetProperties();
                string sheetName = "SHEET " + className.ToUpper();
                string randomPath = this.RandomPath;
                this.oldPath = randomPath;
                using (var workbook = SpreadsheetDocument.Create(randomPath, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook))
                {
                    var workbookPart = workbook.AddWorkbookPart();
                    workbook.WorkbookPart.Workbook = new DocumentFormat.OpenXml.Spreadsheet.Workbook();
                    workbook.WorkbookPart.Workbook.Sheets = new DocumentFormat.OpenXml.Spreadsheet.Sheets();

                    WorkbookStylesPart workStylePart = workbookPart.AddNewPart<WorkbookStylesPart>();
                    workStylePart.Stylesheet = GenerateStyleSheet();

                    var sheetPart = workbook.WorkbookPart.AddNewPart<WorksheetPart>();
                    var sheetData = new DocumentFormat.OpenXml.Spreadsheet.SheetData();
                    sheetPart.Worksheet = new DocumentFormat.OpenXml.Spreadsheet.Worksheet(sheetData);

                    DocumentFormat.OpenXml.Spreadsheet.Sheets sheets = workbook.WorkbookPart.Workbook.GetFirstChild<DocumentFormat.OpenXml.Spreadsheet.Sheets>();
                    string relationshipId = workbook.WorkbookPart.GetIdOfPart(sheetPart);

                    uint sheetId = 1;
                    if (sheets.Elements<DocumentFormat.OpenXml.Spreadsheet.Sheet>().Count() > 0)
                    {
                        sheetId = sheets.Elements<DocumentFormat.OpenXml.Spreadsheet.Sheet>().Select(s => s.SheetId.Value).Max() + 1;
                    }

                    DocumentFormat.OpenXml.Spreadsheet.Sheet sheet = new DocumentFormat.OpenXml.Spreadsheet.Sheet() { Id = relationshipId, SheetId = sheetId, Name = sheetName };
                    sheets.Append(sheet);

                    DocumentFormat.OpenXml.Spreadsheet.Row headerRow = new DocumentFormat.OpenXml.Spreadsheet.Row();
                    headerRow.Height = new DoubleValue() { Value = 32 };
                    foreach (PropertyInfo propertyInfo in properties)
                    {
                        DocumentFormat.OpenXml.Spreadsheet.Cell cell = new DocumentFormat.OpenXml.Spreadsheet.Cell();
                        cell.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
                        cell.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(propertyInfo.Name.ToUpper());
                        cell.StyleIndex = 1;
                        headerRow.AppendChild(cell);
                    }
                    sheetData.AppendChild(headerRow);
                    foreach (T _obj in _list)
                    {
                        DocumentFormat.OpenXml.Spreadsheet.Row newRow = new DocumentFormat.OpenXml.Spreadsheet.Row();
                        foreach (PropertyInfo propertyInfo in properties)
                        {
                            DocumentFormat.OpenXml.Spreadsheet.Cell cell = new DocumentFormat.OpenXml.Spreadsheet.Cell();
                            cell.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
                            object propertyValue = propertyInfo.GetValue(_obj, null);
                            string cellValue = (propertyValue != null ? propertyValue.ToString() : "");
                            cell.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(cellValue);
                            cell.StyleIndex = 0;
                            newRow.AppendChild(cell);
                        }
                        sheetData.AppendChild(newRow);
                    }
                    workbook.Save();
                    exported = true;
                }                
            }
            return exported;            
        }

        private async Task<Stream> GetStreamAsync(string _file)
        {
            try
            {                
                return new FileStream(_file, FileMode.Create, FileAccess.Write);
            }
            catch
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                return await GetStreamAsync(_file);
            }
        }

        private async Task<bool> CheckIfFileCanBeUsed(string _file)
        {
            try
            {
                FileStream fs = new FileStream(_file, FileMode.Create, FileAccess.Write);
                return true;
            }
            catch
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                return await CheckIfFileCanBeUsed(_file);
            }
        }

        private Stylesheet GenerateStyleSheet()
        {
            return new Stylesheet(
                new Fonts(
                    new Font(                                                               // Index 0 - The default font.
                        new FontSize() { Val = 11 },
                        new Color() { Rgb = new HexBinaryValue() { Value = "000000" } },
                        new FontName() { Val = "Calibri" }),
                    new Font(                                                               // Index 1 - The bold font.
                        new Bold(),
                        new FontSize() { Val = 11 },
                        new Color() { Rgb = new HexBinaryValue() { Value = "000000" } },
                        new FontName() { Val = "Calibri" }),
                    new Font(                                                               // Index 2 - The Italic font.
                        new Italic(),
                        new FontSize() { Val = 11 },
                        new Color() { Rgb = new HexBinaryValue() { Value = "000000" } },
                        new FontName() { Val = "Calibri" }),
                    new Font(                                                               // Index 2 - The Times Roman font. with 16 size
                        new FontSize() { Val = 16 },
                        new Color() { Rgb = new HexBinaryValue() { Value = "000000" } },
                        new FontName() { Val = "Times New Roman" })
                ),
                new Fills(
                    new Fill(                                                           // Index 0 - The default fill.
                        new PatternFill() { PatternType = PatternValues.None }),
                    new Fill(                                                           // Index 1 - The default fill of gray 125 (required)
                        new PatternFill() { PatternType = PatternValues.Gray125 }),
                    new Fill(                                                           // Index 2 - The yellow fill.
                        new PatternFill(
                            new ForegroundColor() { Rgb = new HexBinaryValue() { Value = "FFFFFF00" } }
                        )
                        { PatternType = PatternValues.Solid })
                ),
                new Borders(
                    new Border(                                                         // Index 0 - The default border.
                        new LeftBorder(),
                        new RightBorder(),
                        new TopBorder(),
                        new BottomBorder(),
                        new DiagonalBorder()),
                    new Border(                                                         // Index 1 - Applies a Left, Right, Top, Bottom border to a cell
                        new LeftBorder(
                            new Color() { Auto = true }
                        )
                        { Style = BorderStyleValues.Thin },
                        new RightBorder(
                            new Color() { Auto = true }
                        )
                        { Style = BorderStyleValues.Thin },
                        new TopBorder(
                            new Color() { Auto = true }
                        )
                        { Style = BorderStyleValues.Thin },
                        new BottomBorder(
                            new Color() { Auto = true }
                        )
                        { Style = BorderStyleValues.Thin },
                        new DiagonalBorder())
                ),
                new CellFormats(
                    new CellFormat() { FontId = 0, FillId = 0, BorderId = 0 },                          // Index 0 - The default cell style.  If a cell does not have a style index applied it will use this style combination instead
                    new CellFormat()
                    {
                        FontId = 1,
                        FillId = 0,
                        BorderId = 0,
                        ApplyFont = true,
                        Alignment = new Alignment()
                        {
                            Horizontal = HorizontalAlignmentValues.Center,
                            Vertical = VerticalAlignmentValues.Center
                        }
                    },       // Index 1 - Bold 
                    new CellFormat() { FontId = 2, FillId = 0, BorderId = 0, ApplyFont = true },       // Index 2 - Italic
                    new CellFormat() { FontId = 3, FillId = 0, BorderId = 0, ApplyFont = true },       // Index 3 - Times Roman
                    new CellFormat() { FontId = 0, FillId = 2, BorderId = 0, ApplyFill = true },       // Index 4 - Yellow Fill
                    new CellFormat(                                                                   // Index 5 - Alignment
                        new Alignment() { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center }
                    )
                    { FontId = 0, FillId = 0, BorderId = 0, ApplyAlignment = true },
                    new CellFormat() { FontId = 0, FillId = 0, BorderId = 1, ApplyBorder = true }      // Index 6 - Border
                )
            );
        }
    }
}