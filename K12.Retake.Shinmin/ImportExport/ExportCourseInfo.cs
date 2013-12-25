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
    class ExportCourseInfo
    {
        BackgroundWorker _bgWork;
        List<string> _CourseIDList; //課程id清單
        List<UDTCourseDef> _CourseDefList; //課程清單
        Dictionary<string, string> _TeacherDic; //教師名稱字典

        public ExportCourseInfo(List<string> courseIDList)
        {
            //初始化
            _CourseIDList = courseIDList;
            _CourseDefList = new List<UDTCourseDef>();
            _TeacherDic = new Dictionary<string, string>();

            _bgWork = new BackgroundWorker();
            _bgWork.DoWork += new DoWorkEventHandler(_bgWork_DoWork);
            _bgWork.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_bgWork_RunWorkerCompleted);
            _bgWork.WorkerReportsProgress = true;
            _bgWork.ProgressChanged += new ProgressChangedEventHandler(_bgWork_ProgressChanged);
            _bgWork.RunWorkerAsync();
        }

        private void _bgWork_DoWork(object sender, DoWorkEventArgs e)
        {
            //取得教師
            List<TeacherRecord> teachers = Teacher.SelectAll();
            foreach(TeacherRecord teacher in teachers)
            {
                if(!_TeacherDic.ContainsKey(teacher.ID))
                {
                    //建立教師字典
                    _TeacherDic.Add(teacher.ID, teacher.Name);
                }
            }

            //取得課程資料
            _CourseDefList.Clear();
            foreach (UDTCourseDef rec in UDTTransfer.UDTCourseSelectUIDs(_CourseIDList))
            {
                _CourseDefList.Add(rec);
            }

            //排序
            _CourseDefList.Sort(SortCourse);

            //輸出
            Workbook wb = new Workbook();
            wb.Open(new MemoryStream(Properties.Resources.課程基本資料));

            int row = 1;
            foreach(UDTCourseDef elem in _CourseDefList)
            {
                wb.Worksheets[0].Cells[row, 0].PutValue(elem.CourseName);
                wb.Worksheets[0].Cells[row, 1].PutValue(elem.SchoolYear);
                wb.Worksheets[0].Cells[row, 2].PutValue(elem.Semester);
                wb.Worksheets[0].Cells[row, 3].PutValue(elem.Month);
                wb.Worksheets[0].Cells[row, 4].PutValue(elem.SubjectType);
                wb.Worksheets[0].Cells[row, 5].PutValue(elem.DeptName);
                wb.Worksheets[0].Cells[row, 6].PutValue(elem.SubjectName);
                wb.Worksheets[0].Cells[row, 7].PutValue(elem.Credit);
                wb.Worksheets[0].Cells[row, 8].PutValue(elem.SubjectLevel);
                string teacherName;
                try
                {
                    teacherName = _TeacherDic[elem.RefTeacherID.ToString()];
                }
                catch
                {
                    teacherName = "";
                }
                wb.Worksheets[0].Cells[row, 9].PutValue(teacherName);
                row++;
            }

            wb.Worksheets[0].AutoFitColumns();
            e.Result = wb;
        }

        //排序方法
        private int SortCourse(UDTCourseDef x, UDTCourseDef y)
        {
            string xx = x.SchoolYear.ToString().PadLeft(4, '0');
            xx += x.Semester.ToString().PadLeft(2, '0');
            xx += x.Month.ToString().PadLeft(3, '0');
            xx += x.CourseName.PadLeft(20, '0');

            string yy = y.SchoolYear.ToString().PadLeft(4, '0');
            yy += y.Semester.ToString().PadLeft(2, '0');
            yy += y.Month.ToString().PadLeft(3, '0');
            yy += y.CourseName.PadLeft(20, '0');

            return xx.CompareTo(yy);
        }

        private void _bgWork_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                Workbook wb = (Workbook)e.Result;
                Utility.CompletedXls("課程基本資料", wb);
            }
        }

        private void _bgWork_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
        }
    }
}
