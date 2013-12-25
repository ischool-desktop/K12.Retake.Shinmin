using Aspose.Cells;
using K12.Data;
using K12.Retake.Shinmin.DAO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;

namespace K12.Retake.Shinmin.ImportExport
{
    class ExportSCAttend
    {
        BackgroundWorker _bgWork;
        List<string> _CourseIDList; //課程id清單
        Dictionary<int, StudentRecord> _StudDict; //學生Record字典
        Dictionary<int, UDTCourseDef> _CourseDic; //課程字典
        List<UDTScselectDef> _CourseStudents; //修課學生清單

        public ExportSCAttend(List<string> courseIDList)
        {
            _CourseIDList = courseIDList;
            
            _bgWork = new BackgroundWorker();
            _bgWork.DoWork += new DoWorkEventHandler(_bgWork_DoWork);
            _bgWork.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_bgWork_RunWorkerCompleted);
            _bgWork.WorkerReportsProgress = true;
            _bgWork.ProgressChanged += new ProgressChangedEventHandler(_bgWork_ProgressChanged);
            _bgWork.RunWorkerAsync();
        }

        private void _bgWork_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
        }

        private void _bgWork_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                Workbook wb = (Workbook)e.Result;
                Utility.CompletedXls("修課學生資料", wb);
            }
        }

        private void _bgWork_DoWork(object sender, DoWorkEventArgs e)
        {
            //取得課程資料
            _CourseDic = new Dictionary<int, UDTCourseDef>();
            foreach (UDTCourseDef rec in UDTTransfer.UDTCourseSelectUIDs(_CourseIDList))
            {
                int uid = int.Parse(rec.UID);
                if(!_CourseDic.ContainsKey(uid))
                {
                    _CourseDic.Add(uid, rec);
                }
            }

            //取得修課學生資料
            _CourseStudents = new List<UDTScselectDef>();
            foreach (UDTScselectDef data in UDTTransfer.UDTSCSelectByCourseIDList(_CourseIDList))
            {
                _CourseStudents.Add(data);
            }

            //排序
            _CourseStudents.Sort(SortCourseStudents);

            //建立學生record字典
            _StudDict = new Dictionary<int, StudentRecord>();
            List<string> sidList = (from data in _CourseStudents select data.StudentID.ToString()).Distinct().ToList();
            foreach (StudentRecord rec in Student.SelectByIDs(sidList))
            {
                int id = int.Parse(rec.ID);
                if (!_StudDict.ContainsKey(id))
                    _StudDict.Add(id, rec);
            }

            //輸出
            Workbook wb = new Workbook();
            wb.Open(new MemoryStream(Properties.Resources.匯出修課學生));

            int row = 1;
            foreach (UDTScselectDef student in _CourseStudents)
            {
                int courseID = student.CourseID;
                int studentID = student.StudentID;
                wb.Worksheets[0].Cells[row, 0].PutValue(_CourseDic[courseID].CourseName);
                wb.Worksheets[0].Cells[row, 1].PutValue(_CourseDic[courseID].SchoolYear);
                wb.Worksheets[0].Cells[row, 2].PutValue(_CourseDic[courseID].Semester);
                wb.Worksheets[0].Cells[row, 3].PutValue(_CourseDic[courseID].Month);
                wb.Worksheets[0].Cells[row, 4].PutValue(_StudDict[studentID].StudentNumber);
                wb.Worksheets[0].Cells[row, 5].PutValue(_StudDict[studentID].Name);
                wb.Worksheets[0].Cells[row, 6].PutValue(student.SeatNo);
                wb.Worksheets[0].Cells[row, 7].PutValue(student.Type);
                row++;
            }

            wb.Worksheets[0].AutoFitColumns();
            e.Result = wb;
        }

        private int SortCourseStudents(UDTScselectDef x, UDTScselectDef y)
        {
            string xx = _CourseDic[x.CourseID].SchoolYear.ToString().PadLeft(4, '0');
            xx += _CourseDic[x.CourseID].Semester.ToString().PadLeft(2, '0');
            xx += _CourseDic[x.CourseID].Month.ToString().PadLeft(3, '0');
            xx += _CourseDic[x.CourseID].CourseName.PadLeft(20, '0');
            xx += x.SeatNo.ToString().PadLeft(3, '0');

            string yy = _CourseDic[y.CourseID].SchoolYear.ToString().PadLeft(4, '0');
            yy += _CourseDic[y.CourseID].Semester.ToString().PadLeft(2, '0');
            yy += _CourseDic[y.CourseID].Month.ToString().PadLeft(3, '0');
            yy += _CourseDic[y.CourseID].CourseName.PadLeft(20, '0');
            yy += y.SeatNo.ToString().PadLeft(3, '0');

            return xx.CompareTo(yy);
        }
    }
}
